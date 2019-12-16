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
    private AudioSource shootingSFX;
    private bool spaceDown;

    private void Start()
    {
        lastTimeFired = Time.time;
        shootingSFX = GetComponent<AudioSource>();

        EventManager.Instance.AddEventListener<float>("UpdateFiring", UpdateFiring);
    }

    public void UpdateFiring(float updated)
    {
        if (updated == 1)
        {
            spaceDown = true;
            StartCoroutine(SetupFire());
        }
        else
            spaceDown = false;
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
        shootingSFX.Play();
    }

    IEnumerator SetupFire()
    {
        while (spaceDown)
        {
            while (spaceDown && charge)
            {
                chargeValue = Mathf.Clamp(chargeValue, 0, maxCharge);

                if (charging)
                    chargeValue += Time.deltaTime * chargeSpeed;
                yield return new WaitForEndOfFrame();
            }
            if (Time.time - lastTimeFired >= fireRate)
            {
                Fire();
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
