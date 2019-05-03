using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizerItem : MonoBehaviour
{
    public Image image;
    public int ID = 0;

    public void Hover()
    {
        CustomizeShip.current.DisplayDetails(GameManager.current.allParts[ID]);
    }

    public void Unhover()
    {
        CustomizeShip.current.HideDetails();
    }

    public void Select()
    {
        CustomizeShip.current.Select(ID);
    }
}
