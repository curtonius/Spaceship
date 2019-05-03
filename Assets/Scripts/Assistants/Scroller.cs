using UnityEngine;
using System.Collections;

public class Scroller : MonoBehaviour
{
	public float scrollSpeed;
	public float tileSize;
    public Vector3 direction;
	private Vector3 startPosition;

	void Start ()
	{
		startPosition = transform.position;
	}

	void Update ()
	{
		float newPosition = Mathf.Repeat(Time.time * scrollSpeed, tileSize);
		transform.position = startPosition + direction * newPosition;
	}
}