using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessMode : MonoBehaviour
{
    public float minTime;
    public float maxTime;
    public List<Enemy> enemies = new List<Enemy>();
    private float x;
    private float lastX;
    private float timeUntilSpawn;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));

        transform.localScale = new Vector3(topRight.x - bottomLeft.x - 2, 1, 1);
        x = transform.localScale.x/2;
        timeUntilSpawn = Random.Range(minTime, maxTime);

        StartCoroutine(Spawner());
    }

    IEnumerator Spawner()
    {
        while(gameObject)
        {
            yield return new WaitForSeconds(timeUntilSpawn);
            timeUntilSpawn = Random.Range(minTime, maxTime);

            Vector3 enemyPosition = transform.position + new Vector3(Random.Range(-x, x), 0, 0);
            while(Mathf.Abs(enemyPosition.x - lastX) < 1)
            {
                enemyPosition = transform.position + new Vector3(Random.Range(-x, x), 0, 0);
            }
            Enemy enemy = enemies[Random.Range(0, enemies.Count)];
            GameObject enemyObject = Instantiate(enemy.gameObject, enemyPosition, transform.rotation);
            lastX = enemyPosition.x;
        }
        yield return null;
    }
}
