using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //Health and movement speed of enemy
    public float health;
    private float maxHealth;
    public float movementSpeed;
    public float destructionValue = 2;
    public float destructionTimeValue = 0.25f;
    //Prefab of bullet to shoot
    public GameObject bullet;

    //X and Y will be the direction of movement. Z will be the length of time it will move in that direction
    //W will be if the enemy should start shooting
    public Vector4[] enemyMovements;

    //Top right and Bottom left portions of the screen
    private Vector3 topRight;
    private Vector3 bottomLeft;

    //Which Vector4 of movement the enemy is currently on
    private Vector4 currentMovement;
    private Color oldEmissionColor;
    private Material material;
    private Cutscene cutscene;

    public Collect[] droppables;
    private void Start()
    {
        material = GetComponent<Renderer>().material;
        material.EnableKeyword("_EMISSION");
        oldEmissionColor = material.GetColor("_EmissionColor");
        maxHealth = health;
        //Sets up where the Top right and Bottom left are
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));

        if (!FindObjectOfType<Cutscene>())
        {
            //Starts moving and shooting
            StartCoroutine(Move());
            StartCoroutine(ShouldWeShoot());
        }
        else
        {
            Cutscene[] cutscenes;
            cutscenes = FindObjectsOfType<Cutscene>();
            bool atStart = false;
            for(int i=0; i<cutscenes.Length; i+=1)
            {
                if(cutscenes[i].beginning)
                {
                    atStart = true;
                    cutscene = cutscenes[i];
                    StartCoroutine(WaitForEndOfCutscene());
                }
            }
            if(!atStart)
            {
                StartCoroutine(Move());
                StartCoroutine(ShouldWeShoot());
            }
        }
    }
    public void Hurt()
    {
        StartCoroutine(Flash());
    }

    IEnumerator WaitForEndOfCutscene()
    {
        while(cutscene)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(Move());
        StartCoroutine(ShouldWeShoot());
        yield return null;
    }

    IEnumerator Flash()
    {
        Color hurtColor = new Color(oldEmissionColor.r+(1 - (health / maxHealth)), oldEmissionColor.g, oldEmissionColor.b);
        material.SetColor("_EmissionColor", hurtColor);
        yield return new WaitForSeconds(0.1f);
        material.SetColor("_EmissionColor", oldEmissionColor);
        yield return new WaitForSeconds(0.1f);
        material.SetColor("_EmissionColor", hurtColor);
        yield return new WaitForSeconds(0.1f);
        material.SetColor("_EmissionColor", oldEmissionColor);
        yield return null;
    }

    //Causes a separate thread to run that checks if enemy is able to shoot
    IEnumerator ShouldWeShoot()
    {
        //Keep repeating all this code until the gameObject is destroyed
        while(gameObject)
        {
            //If enemy can shoot 1 or more times during this movement phase
            if(currentMovement.w > 0)
            {
                //Calculate how much time needs to pass between shots to shoot evenly at this point
                float timeBetweenShots = (currentMovement.z / currentMovement.w) - (currentMovement.z / currentMovement.w)/2;

                //Create a bullet tagged with the enemy team name
                GameObject b = Instantiate(bullet, transform.position + transform.forward/2, Quaternion.identity);
                Bullet bulletScript = b.GetComponent<Bullet>();
                bulletScript.team = "Enemy";
                bulletScript.direction = transform.forward;

                //Wait for the correct time to pass
                yield return new WaitForSeconds(timeBetweenShots);
            }

            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    //Causes a separate thread to run that moves the enemy
    IEnumerator Move()
    {
        //Loops through all the enemy movement patterns
        for (int i=0; i<enemyMovements.Length; i+=1)
        {
            //Sets currentMovement for the Shoot thread
            currentMovement = enemyMovements[i];

            //o represents how long the enemy should be moving in this given direction for
            float o = enemyMovements[i].z;

            //While enemy should still be moving
            while (o > 0)
            {
                //Move enemy
                transform.position += new Vector3(enemyMovements[i].x, 0, enemyMovements[i].y) * movementSpeed * Time.deltaTime;

                //If enemy is beyond left/right boundaries, move it back inside
                if(transform.position.x < bottomLeft.x+1 || transform.position.x > topRight.x-1)
                {
                    transform.position -= new Vector3(enemyMovements[i].x, 0, 0) * movementSpeed * Time.deltaTime;
                }
                //If enemy is further than the bottom of players screen, KILL IT
                if(transform.position.z < bottomLeft.z-1)
                {
                    if(GameManager.current.CurrentState != GameManager.State.NumberOfEnemies)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {

                    }
                }
                o -= Time.deltaTime;

                //If enemy is dead, cause explosion, and destroy the enemy
                if(health <= 0)
                {
                    //GO THROUGH DROPPABLES TO DROP ITEMS???
                    for(int g=0; g<droppables.Length; g+=1)
                    {
                        int value = Random.Range(0, 100);
                        if(value < 50)
                        {
                            Collect collect = Instantiate(droppables[g].gameObject, transform.position + new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)), Quaternion.identity).GetComponent<Collect>();
                            collect.SetState(movementSpeed);
                        }
                    }

                    CameraShake.current.Shake(destructionValue,destructionTimeValue);
                    GetComponent<Hazard>().playerDestroyed = true;
                    GameManager.current.SpawnExplosion(transform.position);
                    Destroy(gameObject);
                }

                yield return new WaitForEndOfFrame();
            }

            //Reset the movement loop
            if(i == enemyMovements.Length-1)
            {
                i = -1;
            }
        }

        yield return null;
    }
}
