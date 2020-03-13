using System.Collections;
using UnityEngine;

public class Collect : MonoBehaviour
{
    private float movementSpeed;

    private void Start()
    {
        StartCoroutine(Move());
    }
    public virtual void DoThing()
    {

    }

    public void SetState(float speed)
    {
        movementSpeed = speed;
    }

    private IEnumerator Move()
    {
        while (gameObject)
        {
            transform.position += new Vector3(0, 0, -1) * movementSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
