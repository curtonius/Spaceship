using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 rotate;
    public float rotationSpeed = 1;
    public bool randomize;
    // Start is called before the first frame update
    void Start()
    {
        if (randomize)
            rotate = new Vector3(Random.Range(-2, 2), Random.Range(-1, 1), Random.Range(-2, 2));
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotate * rotationSpeed);
    }
}
