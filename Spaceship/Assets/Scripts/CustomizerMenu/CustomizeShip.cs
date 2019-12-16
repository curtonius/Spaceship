using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeShip : MonoBehaviour
{
    public static CustomizeShip current;

    public List<Image> imageButtons = new List<Image>();
    public Scroller background1;
    public Scroller background2;

    //Tabs
    public Image hullTab;
    public Image thrusterTab;
    public Image weaponTab;

    public Color selectedColor;
    public Color deselectedColor;

    public GameObject buttonToClone;
    public WeaponPort weaponPortGameObject;
    private List<WeaponPort> clonedPorts= new List<WeaponPort>();

    public GameObject background;

    //Details tabs information
    public GameObject detailsTab;
    public Text levelReq;
    public Text scrapReq;
    public Text modifier;
    public Text special;

    private float spacing = 10;
    private float yPos = 10;
    private float buttonHeight;
    private Hull hull;
    private Thruster thruster;
    private Weapon[] weapons;

    private Weapon selectedWeapon;
    private WeaponPort selectedPort;

    public Color selectedPortColorPrimary;
    public Color selectedPortColorSecondary;
    public Color deselectedPortColorPrimary;
    public Color deselectedPortColorSecondary;

    public float scrollSpeed;
    private float scrollAmount;

    private void Start()
    {
        EventManager.Instance.AddEventListener<Vector3>("MovedMouse", MovedMouse);
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 15));
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 15));
        //Gets distance between left side of the screen and right side of the screen
        float sizeVariable = Mathf.Abs(topRight.x - bottomLeft.x);

        background1.transform.localScale = background2.transform.localScale = new Vector3(sizeVariable, 1, sizeVariable);
        background2.transform.position = background1.transform.position + new Vector3(0, 0, sizeVariable);
        background1.tileSize = background2.tileSize = sizeVariable;

        current = this;
        buttonHeight = buttonToClone.GetComponent<RectTransform>().rect.height;

        DisplayHulls();
        LoadShip();
        GameManager.current.Clear();
    }

    private void MovedMouse(Vector3 mousePosition)
    {
        RectTransform rectTransform = background.GetComponent<RectTransform>();
        Vector3 topPosition = rectTransform.position + new Vector3(0, rectTransform.rect.height / 3, 0);
        Vector3 bottomPosition = rectTransform.position - new Vector3(0, rectTransform.rect.height / 3, 0);

        if (mousePosition.x > topPosition.x - rectTransform.rect.width / 2 && mousePosition.x < topPosition.x + rectTransform.rect.width / 2 && mousePosition.y > topPosition.y && scrollAmount > 0)
        {
            scrollAmount -= Time.deltaTime * scrollSpeed;
            for(int i=0; i<imageButtons.Count; i+=1)
            {
                imageButtons[i].transform.position -= new Vector3(0, Time.deltaTime * scrollSpeed, 0);
            }
        }
        else if (mousePosition.x > topPosition.x - rectTransform.rect.width / 2 && mousePosition.x < topPosition.x + rectTransform.rect.width / 2 && mousePosition.y < bottomPosition.y && scrollAmount < (imageButtons.Count-1) * (buttonHeight+spacing))
        {
            scrollAmount += Time.deltaTime * scrollSpeed;
            for (int i = 0; i < imageButtons.Count; i += 1)
            {
                imageButtons[i].transform.position += new Vector3(0, Time.deltaTime * scrollSpeed, 0);
            }
        }
    }

    public void HideTabs()
    {
        selectedWeapon = null;
        selectedPort = null;
        hullTab.color = thrusterTab.color = weaponTab.color = deselectedColor;
        for (int i = 0; i < imageButtons.Count; i += 1)
        {
            Destroy(imageButtons[i].gameObject);
        }
        for (int i = 0; i < clonedPorts.Count; i += 1)
        {
            Destroy(clonedPorts[i].gameObject);
        }
        clonedPorts.Clear();
        imageButtons.Clear();
        yPos = spacing;
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

    public void SetupPortImages()
    {
        for(int i=0; i<hull.weaponPorts.Length; i+=1)
        {
            Vector3 position = Camera.main.WorldToScreenPoint(hull.weaponPorts[i].position);
            WeaponPort newPort = Instantiate(weaponPortGameObject.gameObject, position, Quaternion.identity).GetComponent<WeaponPort>();
            clonedPorts.Add(newPort);
            newPort.weaponPortIndex = i;
            newPort.transform.SetParent(weaponPortGameObject.transform.parent);
            newPort.gameObject.SetActive(true);
        }
    }

    public void SelectPort(WeaponPort wp)
    {
        if (selectedPort && selectedPort == wp && weapons[selectedPort.weaponPortIndex])
        {
            Destroy(weapons[selectedPort.weaponPortIndex].gameObject);
            weapons[selectedPort.weaponPortIndex] = null;

            MiscData.currentWeapons = new int[weapons.Length];
            for (int i = 0; i < weapons.Length; i += 1)
            {
                if (weapons[i] != null)
                {
                    MiscData.currentWeapons[i] = weapons[i].id;
                }
                else
                {
                    MiscData.currentWeapons[i] = -1;
                }
            }
        }
        else
        {
            selectedPort = wp;
            if (selectedWeapon)
            {
                if (weapons[selectedPort.weaponPortIndex])
                {
                    Destroy(weapons[selectedPort.weaponPortIndex].gameObject);
                }
                weapons[selectedPort.weaponPortIndex] = Instantiate(selectedWeapon.gameObject, hull.weaponPorts[selectedPort.weaponPortIndex].transform.position, hull.weaponPorts[selectedPort.weaponPortIndex].transform.rotation).GetComponent<Weapon>();
                weapons[selectedPort.weaponPortIndex].barrel = hull.weaponPorts[selectedPort.weaponPortIndex];
            }

            //Recolors port images
            for (int i = 0; i < clonedPorts.Count; i += 1)
            {
                if (clonedPorts[i] != selectedPort)
                {
                    clonedPorts[i].GetComponent<Image>().color = deselectedPortColorPrimary;
                    clonedPorts[i].transform.Find("FirstBox").GetComponent<Image>().color = deselectedPortColorSecondary;
                    clonedPorts[i].transform.Find("FirstBox").Find("SecondBox").GetComponent<Image>().color = deselectedPortColorPrimary;
                }
            }

            selectedPort.GetComponent<Image>().color = selectedPortColorPrimary;
            selectedPort.transform.Find("FirstBox").GetComponent<Image>().color = selectedPortColorSecondary;
            selectedPort.transform.Find("FirstBox").Find("SecondBox").GetComponent<Image>().color = selectedPortColorPrimary;
        }
    }

    public void Select(int id)
    {
        Part p = GameManager.current.allParts[id];
        if(p is Hull)
        {
            Destroy(hull.gameObject);
            hull = Instantiate(GameManager.current.allParts[id].gameObject, new Vector3(), Quaternion.identity).GetComponent<Hull>();
            thruster.transform.position = hull.thrusterLocation.position;

            for(int i=0; i<weapons.Length; i+=1)
            {
                if(weapons[i])
                    Destroy(weapons[i].gameObject);
            }
            weapons = new Weapon[hull.weaponPorts.Length];
        }
        else if (p is Thruster)
        {
            Destroy(thruster.gameObject);
            thruster = Instantiate(GameManager.current.allParts[id].gameObject, hull.thrusterLocation.position, hull.thrusterLocation.rotation).GetComponent<Thruster>();
        }
        else if(p is Weapon)
        {
            selectedWeapon = p as Weapon;
            if (selectedPort)
            {
                if (weapons[selectedPort.weaponPortIndex])
                {
                    Destroy(weapons[selectedPort.weaponPortIndex].gameObject);
                }
                weapons[selectedPort.weaponPortIndex] = Instantiate(selectedWeapon.gameObject, hull.weaponPorts[selectedPort.weaponPortIndex].transform.position, hull.weaponPorts[selectedPort.weaponPortIndex].transform.rotation).GetComponent<Weapon>();
                weapons[selectedPort.weaponPortIndex].barrel = hull.weaponPorts[selectedPort.weaponPortIndex];
            }
        }

        MiscData.currentHull = hull.id;
        MiscData.currentThruster = thruster.id;
        MiscData.currentWeapons = new int[weapons.Length];
        for(int i=0; i<weapons.Length; i +=1)
        {
            if (weapons[i] != null)
            {
                MiscData.currentWeapons[i] = weapons[i].id;
            }
            else
            {
                MiscData.currentWeapons[i] = -1;
            }
        }

    }

    public void LoadShip()
    {
        //Generate players ship
        GameObject hullObj = Instantiate(GameManager.current.allParts[MiscData.currentHull].gameObject, new Vector3(), Quaternion.identity);
        hull = hullObj.GetComponent<Hull>();

        if(!hull)
        {
            Destroy(hullObj);
        }

        GameObject thrusterObj = Instantiate(GameManager.current.allParts[MiscData.currentThruster].gameObject, hull.thrusterLocation.position, hull.thrusterLocation.rotation);
        thruster = thrusterObj.GetComponent<Thruster>();

        if (!thruster)
        {
            Destroy(thrusterObj);
        }

        weapons = new Weapon[hull.weaponPorts.Length];
        for (int i = 0; i < MiscData.currentWeapons.Length; i += 1)
        {
            if(MiscData.currentWeapons[i] >= 0)
            {
                Weapon weapon = Instantiate(GameManager.current.allParts[MiscData.currentWeapons[i]].gameObject, hull.weaponPorts[i].position, hull.weaponPorts[i].rotation).GetComponent<Weapon>();
                weapon.barrel = hull.weaponPorts[i];
                weapons[i] = weapon;
            }
        }
    }

    public void CreateButton(int id)
    {
        CustomizerItem newButton = Instantiate(buttonToClone.gameObject, buttonToClone.transform.parent).GetComponent<CustomizerItem>();
        newButton.gameObject.SetActive(true);
        newButton.image.sprite = GameManager.current.allParts[id].sprite;
        newButton.transform.localPosition = buttonToClone.transform.localPosition + new Vector3(0, -yPos, 0);
        newButton.ID = id;
        imageButtons.Add(newButton.GetComponent<Image>());

        yPos += buttonHeight + spacing;
    }

    public void Display(System.Type type)
    {
        if (type == typeof(Hull))
        {
            Part[] allParts = GameManager.current.allParts;
            for (int i = 0; i < allParts.Length; i += 1)
            {
                if (allParts[i].GetType() == typeof(Hull) && MiscData.unlockedItems.Contains(i))
                {
                    CreateButton(allParts[i].id);
                }
            }
        }
        else if (type == typeof(Thruster))
        {
            Part[] allParts = GameManager.current.allParts;
            for (int i = 0; i < allParts.Length; i += 1)
            {
                if (allParts[i].GetType() == typeof(Thruster) && MiscData.unlockedItems.Contains(i))
                {
                    CreateButton(allParts[i].id);
                }
            }
        }
        else if (type == typeof(Weapon))
        {
            Part[] allParts = GameManager.current.allParts;
            for (int i = 0; i < allParts.Length; i += 1)
            {
                if (allParts[i].GetType() == typeof(Weapon) && MiscData.unlockedItems.Contains(i))
                {
                    CreateButton(allParts[i].id);
                }
            }
            SetupPortImages();
        }
    }

    //Display specific type of ship part
    public void DisplayHulls()
    {
        HideTabs();
        hullTab.color = selectedColor;
        Display(typeof(Hull));
    }
    public void DisplayThrusters()
    {
        HideTabs();
        thrusterTab.color = selectedColor;
        Display(typeof(Thruster));
    }
    public void DisplayWeapons()
    {
        HideTabs();
        weaponTab.color = selectedColor;
        Display(typeof(Weapon));
    }
}
