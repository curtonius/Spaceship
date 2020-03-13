using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    Material mat;
    public float fieldSpeed;
    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        StartCoroutine(DoWarp());
    }
    // Update is called once per frame
    IEnumerator DoWarp()
    {
        while (gameObject)
        {
            mat.SetFloat("_FresnelWidth", 0.75f + (0.25f * Mathf.Sin(Time.time * fieldSpeed)));
            yield return new WaitForEndOfFrame();
        }
    }
}
