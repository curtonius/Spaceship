using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    //How much damage the hazard will do if it hits the opposite team
    public float damage;
    //Which team does this hazard belong to
    public string team;
    //How many points do you get for destroying this hazard
    public int score = 10;
    public float shakeAmount = 0.5f;
    public float timeShake = 0.125f;
    public bool playerDestroyed;

    //When the gameObject this script is attached to is Destroyed, this function will be called
    private void OnDestroy()
    {
        //Adds points to the score
        if (playerDestroyed)
        {
            GameManager.current.AddToScore(score);
        }
    }
}
