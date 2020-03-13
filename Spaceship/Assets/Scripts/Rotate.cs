using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 rotate;
    public float rotationSpeed = 1;
    public bool randomizeX;
    public bool randomizeY;
    public bool randomizeZ;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomized = rotate;
        if (randomizeX)
            randomized.x = Random.Range(-2, 2);
        if (randomizeY)
            randomized.y = Random.Range(-2, 2);
        if (randomizeZ)
            randomized.z = Random.Range(-2, 2);

        rotate = randomized.normalized;

        StartCoroutine(DoRotate());
    }

    // Update is called once per frame
    IEnumerator DoRotate()
    {
        while (gameObject)
        {
            transform.Rotate(rotate * rotationSpeed);
            yield return new WaitForEndOfFrame();
        }
    }
}
