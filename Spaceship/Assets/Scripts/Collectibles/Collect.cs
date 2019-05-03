using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collect : MonoBehaviour
{
    private float movementSpeed;
    public virtual void DoThing()
    {

    }

    public void SetState(float speed)
    {
        movementSpeed = speed;
    }

    private void Update()
    {
        transform.position += new Vector3(0, 0, -1) * movementSpeed * Time.deltaTime;
    }
}
