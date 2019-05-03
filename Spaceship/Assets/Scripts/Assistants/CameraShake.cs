using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake current;
    public Quaternion originalrotation;
    public float timeToShakeFor;
    private void Start()
    {
        current = this;
        originalrotation = transform.rotation;
    }

    public void Shake(float shakeAmount, float timeShake)
    {
        timeToShakeFor += timeShake;
        StartCoroutine(DoShake(shakeAmount));
    }

    IEnumerator DoShake(float shakeAmount)
    {
        Quaternion lastRot = transform.rotation;
        float amountToReduceBy = (shakeAmount / timeToShakeFor);
        while (timeToShakeFor > 0)
        {
            float x = Random.value * shakeAmount;
            float y = Random.value * shakeAmount;
            float z = Random.value * shakeAmount;
            transform.rotation = lastRot * Quaternion.Euler(x, 0, z);
            yield return new WaitForEndOfFrame();
            transform.rotation = lastRot;
            shakeAmount -= Time.deltaTime * amountToReduceBy;
            timeToShakeFor -= Time.deltaTime;
            lastRot = transform.rotation;
            yield return new WaitForEndOfFrame();
        }

        float i = 0;
        while(transform.rotation != originalrotation)
        {
            transform.rotation = Quaternion.Slerp(lastRot, originalrotation, i);
            i += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
