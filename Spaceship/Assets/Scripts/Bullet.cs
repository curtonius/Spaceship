﻿using System.Collections;
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
        StartCoroutine(Move());
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (gameObject != collider.gameObject && team != collider.tag && collider.GetComponent<Hazard>() && collider.GetComponent<Hazard>().team != team)
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
                if (enemy)
                {
                    enemy.health -= damage+GameManager.current.boosts["Damage Increase"];
                    enemy.Hurt();
                    GameManager.current.AddHit();
                }
                GameManager.current.SpawnExplosion(transform.position);
                Destroy(gameObject);
            }
        }
    }

    // Update is called once per frame
    IEnumerator Move()
    {
        while(gameObject)
        {
            transform.position += direction * movementSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
