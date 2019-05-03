using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Hazard
{
    public Vector3 direction;
    public float movementSpeed;
    public float lifeTime;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * movementSpeed * Time.deltaTime;

        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2);
        foreach(Collider collider in colliders)
        {
            if(gameObject != collider.gameObject && team != collider.tag && collider.GetComponent<Hazard>() && collider.GetComponent<Hazard>().team != team)
            {
                CameraShake.current.Shake(shakeAmount, timeShake);
                if (collider.tag != "Enemy")
                {
                    GameManager.current.SpawnExplosion(transform.position);
                    Destroy(collider.gameObject);
                    Destroy(gameObject);
                }
                else
                {
                    Enemy enemy = collider.GetComponent<Enemy>();
                    if(enemy)
                    {
                        enemy.health -= damage;
                        enemy.Hurt();
                    }
                    GameManager.current.SpawnExplosion(transform.position);
                    Destroy(gameObject);
                }
            }
        }
    }
}
