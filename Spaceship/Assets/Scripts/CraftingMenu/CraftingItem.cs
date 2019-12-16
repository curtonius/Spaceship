using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingItem : MonoBehaviour
{
    public Image image;
    public int ID = 0;

    public void Hover()
    {
        CraftingPage.current.DisplayDetails(GameManager.current.allParts[ID]);
    }

    public void Unhover()
    {
        CraftingPage.current.HideDetails();
    }

    public void Select()
    {
        Part p = GameManager.current.allParts[ID];
        if(p.levelRequirement <= MiscData.level && p.scrapRequirement <= MiscData.Scrap)
        {
            MiscData.Scrap -= p.scrapRequirement;
            MiscData.unlockedItems.Add(ID);
            if (p is Hull)
            {
                CraftingPage.current.DisplayHulls();
            }
            else if (p is Thruster)
            {
                CraftingPage.current.DisplayThrusters();
            }
            if (p is Weapon)
            {
                CraftingPage.current.DisplayWeapons();
            }
        }
        Unhover();
    }
}
