using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController current;

    public float Health {get { return health; } set { health = Mathf.Clamp(value, 0, maxHealth); UpdateHealthBar(); } }
    private float health;
    public float maxHealth;
    public float movementSpeed;
    public float timeToHeal = 10;
    private float currentTime;

    public Scroller background1;
    public Scroller background2;
    public Transform starEmitter;

    private Vector3 lastPosition;

    private Vector3 topRight;
    private Vector3 bottomLeft;

    private Image healthBar;
    
    private float healthBarSize;
    private float lastTimeHit;
    private float scrapWait;
    public bool waitAtStart;
    private Cutscene cutscene;

    private void UpdateHealthBar()
    {
        float percentage = (float)health / (float)maxHealth;
        healthBar.rectTransform.sizeDelta = new Vector2(healthBarSize * percentage, healthBar.rectTransform.rect.height);
        healthBar.rectTransform.position = new Vector3(Screen.width - healthBarSize / 2 + (healthBarSize * (1 - percentage) / 2) - 5, healthBar.rectTransform.position.y, healthBar.rectTransform.position.z);

        if (percentage >= 0.6f)
        {
            //Green bar to Yellow bar
            healthBar.color = new Color((1 - percentage) * 2.5f, 1, 0);
        }
        else
        {
            //Yellow bar to red bar
            healthBar.color = new Color(1, percentage / 0.6f, 0);
        }

        if (health < maxHealth )//AND NOT ALREADY HEALING
            StartCoroutine(HealAfterTime());
    }

    private IEnumerator HealAfterTime()
    {
        while (health < maxHealth)
        {
            if (Time.time - lastTimeHit >= timeToHeal && MiscData.scrap >= 1)
            {
                Health += movementSpeed * Time.deltaTime;
                scrapWait += Time.deltaTime;
                if (scrapWait >= 1)
                {
                    scrapWait = 0;
                    MiscData.scrap -= 1;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    private void Start()
    {
        current = this;

        //TopRight/BottomLeft for Background objects
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 15));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 15));
        //Gets distance between left side of the screen and right side of the screen
        float sizeVariable = Mathf.Abs(topRight.x - bottomLeft.x);

        background1.transform.localScale = background2.transform.localScale = new Vector3(sizeVariable, 1, sizeVariable * 2);
        background2.transform.position = background1.transform.position + new Vector3(0, 0, sizeVariable * 2);

        starEmitter.localScale = new Vector3(sizeVariable, 1, 1);
        starEmitter.GetComponent<ParticleSystem>().Stop();
        starEmitter.GetComponent<ParticleSystem>().Play();
        starEmitter.position = new Vector3(0, -2, background1.transform.position.z + Mathf.Abs(topRight.z - bottomLeft.z));

        background1.tileSize = background2.tileSize =  sizeVariable * 2;

        //TopRight/BottomLeft for player boundaries
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0,0, 10));
        healthBar = GameManager.current.healthBar;
        healthBarSize = healthBar.rectTransform.rect.width;

        lastPosition = new Vector3(0,0,-10);

        PlayerInputManager fake = PlayerInputManager.Instance;

        if (FindObjectOfType<Cutscene>())
        {
            Cutscene[] cutscenes;
            cutscenes = FindObjectsOfType<Cutscene>();
            
            for (int i = 0; i < cutscenes.Length; i += 1)
            {
                if (cutscenes[i].beginning)
                {
                    waitAtStart = true;
                    cutscene = cutscenes[i];
                }
            }
        }
    }

    public void EndLevel(Cutscene scene)
    {
        waitAtStart = true;
        cutscene = scene;
    }

    private void Update()
    {
        if(waitAtStart && cutscene != null)
        {
            return;
        }

        currentTime += Time.deltaTime;
        //If the health reaches 0 or below, player is dead
        if(health <= 0)
        {
            GameManager.current.PlayerDied();
            GameManager.current.SpawnExplosion(transform.position);
            Destroy(gameObject);
        }

        float moveByTime = movementSpeed * Time.deltaTime;
        //Choose direction to move towards based on previously inputted direction
        lastPosition += (Vector3.forward * Input.GetAxisRaw("Vertical"))*moveByTime + (Vector3.right * Input.GetAxisRaw("Horizontal"))* moveByTime;
        bool hadToStop = false;

        if (lastPosition.x < bottomLeft.x + 1 || lastPosition.x > topRight.x - 1)
        {
            lastPosition -= Vector3.right * Input.GetAxisRaw("Horizontal") * moveByTime;
            hadToStop = true;
        }
        if (lastPosition.z < bottomLeft.z + 0.5f || lastPosition.z > topRight.z - 0.5f)
        {
            lastPosition -= Vector3.forward * Input.GetAxisRaw("Vertical") * moveByTime;
        }


        //Move in that direction floatily
        transform.position = Vector3.Lerp(transform.position, lastPosition, 1/movementSpeed);

        if(Input.GetAxisRaw("Horizontal") == 0 || hadToStop)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0,0, 0), 0.1f);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, -25 * Input.GetAxisRaw("Horizontal")), 0.1f);


        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2);
        foreach(Collider collider in colliders)
        {
            if(collider.GetComponent<Hazard>() && collider.GetComponent<Hazard>().team != gameObject.tag)
            {
                Hazard hazard = collider.GetComponent<Hazard>();
                CameraShake.current.Shake(hazard.shakeAmount, hazard.timeShake);
                lastTimeHit = Time.time;
                Health -= hazard.damage;
                GameManager.current.SpawnExplosion(collider.transform.position);
                Destroy(collider.gameObject);
            }

            if(collider.GetComponent<Collect>())
            {
                collider.GetComponent<Collect>().DoThing();
            }
        }
    }
}
