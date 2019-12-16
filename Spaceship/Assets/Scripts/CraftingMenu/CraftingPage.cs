using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingPage : MonoBehaviour
{
    public List<Image> imageButtons = new List<Image>();
    public static CraftingPage current;

    //Tab images to change their colors on select/deselect
    public Image hullTab;
    public Image thrusterTab;
    public Image weaponTab;

    //Colors for the tab images to change colors on select/deselect
    public Color selectedColor;
    public Color deselectedColor;

    //Details tabs information
    public GameObject detailsTab;
    public Text levelReq;
    public Text scrapReq;
    public Text modifier;
    public Text special;

    //Button to create
    public CraftingItem buttonToClone;

    public Text scrapText;
    public RectTransform background;

    public Image[] upgradeLights;
    public Color litColor;
    public Color darkColor;

    private float backgroundWidth;
    private float buttonWidth;
    private float spacing = 10;
    private float howManyPerLine;
    private float xPos;
    private float yPos;
    private float currentNumber=1;
    private string currentPartType = "Hull";
    private void Start()
    {
        current = this;
        EventManager.Instance.AddEventListener<int>("UpdateScrap", UpdateScrapText);
        backgroundWidth = background.rect.width;
        buttonWidth = buttonToClone.GetComponent<RectTransform>().rect.width;

        howManyPerLine = Mathf.Floor(backgroundWidth / (buttonWidth+ spacing));

        xPos = yPos = spacing;

        DisplayHulls();
        UpdateLights();
        GameManager.current.Clear();
    }

    public void HideTabs()
    {
        hullTab.color = thrusterTab.color = weaponTab.color = deselectedColor;
        for(int i=0; i< imageButtons.Count; i+=1)
        {
            Destroy(imageButtons[i].gameObject);
        }
        imageButtons.Clear();
        xPos = yPos = spacing;
    }

    public void DisplayDetails(Part craftingItem)
    {
        levelReq.text = "Level Req. " + craftingItem.levelRequirement;
        scrapReq.text = "Scrap Req. " + craftingItem.scrapRequirement;
        if (craftingItem is Hull)
        {
            modifier.text = "HP +" + craftingItem.GetComponent<Hull>().healthMod;
        }
        else if (craftingItem is Thruster)
        {
            modifier.text = "Speed +" + craftingItem.GetComponent<Thruster>().speedMod;
        }
        else if (craftingItem is Weapon)
        {
            modifier.text = "Dmg +" + craftingItem.GetComponent<Weapon>().damageMod;
        }
        special.text = craftingItem.special;
        detailsTab.SetActive(true);
    }

    public void HideDetails()
    {
        detailsTab.SetActive(false);
    }

    public void CreateButton(int id)
    {
        CraftingItem newButton = Instantiate(buttonToClone.gameObject, background.transform).GetComponent<CraftingItem>();
        newButton.gameObject.SetActive(true);
        newButton.image.sprite = GameManager.current.allParts[id].sprite;
        newButton.transform.position = buttonToClone.transform.position + new Vector3(xPos, -yPos, 0);
        newButton.ID = id;
        imageButtons.Add(newButton.GetComponent<Image>());

        xPos += buttonWidth + spacing;
        currentNumber += 1;

        if(currentNumber > howManyPerLine)
        {
            currentNumber = 1;
            xPos = spacing;
            yPos += buttonWidth + spacing;
        }
    }

    public void Display(System.Type type)
    {
        if(type == typeof(Hull))
        {
            Part[] allParts = GameManager.current.allParts;
            for(int i=0; i<allParts.Length; i+=1)
            {
                if(allParts[i].GetType() == typeof(Hull) && !MiscData.unlockedItems.Contains(i))
                {
                    CreateButton(allParts[i].id);
                }
            }
        }
        else if(type == typeof(Thruster))
        {
            Part[] allParts = GameManager.current.allParts;
            for (int i = 0; i < allParts.Length; i += 1)
            {
                if (allParts[i].GetType() == typeof(Thruster) && !MiscData.unlockedItems.Contains(i))
                {
                    CreateButton(allParts[i].id);
                }
            }
        }
        else if(type == typeof(Weapon))
        {
            Part[] allParts = GameManager.current.allParts;
            for (int i = 0; i < allParts.Length; i += 1)
            {
                if (allParts[i].GetType() == typeof(Weapon) && !MiscData.unlockedItems.Contains(i))
                {
                    CreateButton(allParts[i].id);
                }
            }
        }
    }

    //Display specific type of ship part
    public void DisplayHulls()
    {
        currentPartType = "Hull";
        HideTabs();
        UpdateLights();
        hullTab.color = selectedColor;
        Display(typeof(Hull));
    }
    public void DisplayThrusters()
    {
        currentPartType = "Thruster";
        HideTabs();
        UpdateLights();
        thrusterTab.color = selectedColor;
        Display(typeof(Thruster));
    }
    public void DisplayWeapons()
    {
        currentPartType = "Weapon";
        HideTabs();
        UpdateLights();
        weaponTab.color = selectedColor;
        Display(typeof(Weapon));
    }

    public void RepairKitHover(bool enter)
    {
        if (enter)
        {
            detailsTab.SetActive(true);
            levelReq.text = "";
            modifier.text = "";
            special.text = "";
            scrapReq.text = "Scrap Req. 5";
        }
        else
        {
            detailsTab.SetActive(false);
        }
    }

    public void UpgradeHover(bool enter)
    {
        if (enter)
        {
            detailsTab.SetActive(true);
            levelReq.text = "";
            modifier.text = "";
            special.text = "";
            if(currentPartType=="Hull")
                scrapReq.text = "Scrap Req. " + 2*Mathf.Pow(MiscData.hullLight,2);
            else if (currentPartType == "Thruster")
                scrapReq.text = "Scrap Req. " + 2 * Mathf.Pow(MiscData.thrusterLight, 2);
            else if (currentPartType == "Weapon")
                scrapReq.text = "Scrap Req. " + 2 * Mathf.Pow(MiscData.weaponLight, 2);
        }
        else
        {
            detailsTab.SetActive(false);
        }
    }

    public void UpgradeButton()
    {
        float scrapReq=0;
        if (currentPartType == "Hull")
            scrapReq = 2 * Mathf.Pow(MiscData.hullLight, 2);
        else if (currentPartType == "Thruster")
            scrapReq = 2 * Mathf.Pow(MiscData.thrusterLight, 2);
        else if (currentPartType == "Weapon")
            scrapReq = 2 * Mathf.Pow(MiscData.weaponLight, 2);
        
        if(MiscData.Scrap >= scrapReq)
        {
            MiscData.Scrap -= (int)scrapReq;
            if (currentPartType == "Hull")
                MiscData.hullLight += 1;
            else if (currentPartType == "Thruster")
                MiscData.thrusterLight += 1;
            else if (currentPartType == "Weapon")
                MiscData.weaponLight += 1;
        }

        UpdateLights();
    }

    public void UpdateLights()
    {
        if (currentPartType == "Hull")
        {
            for (int i = 0; i < upgradeLights.Length; i += 1)
            {
                if (i < MiscData.hullLight)
                    upgradeLights[i].color = litColor;
                else
                    upgradeLights[i].color = darkColor;
            }
        }
        else if (currentPartType == "Thruster")
        {
            for (int i = 0; i < upgradeLights.Length; i += 1)
            {
                if (i < MiscData.thrusterLight)
                    upgradeLights[i].color = litColor;
                else
                    upgradeLights[i].color = darkColor;
            }
        }
        else if (currentPartType == "Weapon")
        {
            for (int i = 0; i < upgradeLights.Length; i += 1)
            {
                if (i < MiscData.weaponLight)
                    upgradeLights[i].color = litColor;
                else
                    upgradeLights[i].color = darkColor;
            }
        }
    }

    public void CraftRepairKit()
    {
        if(MiscData.Scrap >= 5)
        {
            MiscData.Scrap -= 5;
            MiscData.repairKits += 1;
        }
    }

    private void UpdateScrapText(int scrap)
    {
        scrapText.text = scrap.ToString();
    }
}
