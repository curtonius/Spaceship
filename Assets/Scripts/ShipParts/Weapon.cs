using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Part
{
    public float damageMod;

    public float fireRate;
    public Transform barrel;
    public GameObject bullet;
    public bool charge;
    public bool laser;
    private float lastTimeFired;
    private bool charging;
    private float chargeValue;
    public float chargeSpeed;
    public float maxCharge;

    private void Start()
    {
        lastTimeFired = Time.time;
    }

    public void Fire()
    {
        charging = false;
        lastTimeFired = Time.time;

        if(!laser)
        {
            GameObject b = Instantiate(bullet, barrel.position, Quaternion.identity);
            Bullet bulletScript = b.GetComponent<Bullet>();
            bulletScript.team = transform.parent.tag;
            bulletScript.direction = transform.forward;
            bulletScript.damage += damageMod + (MiscData.weaponLight*5);
            if(charge)
            {
                bulletScript.damage *= chargeValue;
            }
            chargeValue = 0;
        }
        else
        {
            float distance = Mathf.Abs(barrel.position.z - Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 10)).z);
            GameObject b = Instantiate(bullet, barrel.position+new Vector3(0,0,distance/2), Quaternion.identity);
            b.transform.localScale = new Vector3(b.transform.localScale.x, b.transform.localScale.y, distance / 2);
            Bullet bulletScript = b.GetComponent<Bullet>();
            bulletScript.team = gameObject.tag;
            bulletScript.damage += damageMod + (MiscData.weaponLight * 5);
            if (charge)
            {
                bulletScript.damage *= chargeValue;
            }
            chargeValue = 0;
        }
    }

    private void Update()
    {
        if (!PlayerController.current || (PlayerController.current && PlayerController.current.waitAtStart))
            return;

        chargeValue = Mathf.Clamp(chargeValue, 0, maxCharge);
        if (Time.time - lastTimeFired >= fireRate)
        {
            if (Input.GetAxisRaw("Jump") != 0)
            {
                if (charge)
                    charging = true;
                else
                    Fire();
            }
            else
            {
                if (charging)
                    Fire();
            }
        }
        if (charging)
            chargeValue += Time.deltaTime*chargeSpeed;
    }
}
