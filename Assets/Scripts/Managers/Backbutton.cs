using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backbutton : MonoBehaviour
{
    public void BackToMainMenu()
    {
        GameManager.current.Back();
    }
}
