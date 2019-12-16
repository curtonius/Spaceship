//These are all used for XML Serialization (Saving/Loading of data)
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;

[XmlRoot("SaveFile")]
public class SaveFile
{
    [XmlAttribute("Highscore")]
    public int score;
    [XmlAttribute("Scrap")]
    public int scrap;
    [XmlAttribute("Level")]
    public int level;
    [XmlAttribute("RepairKits")]
    public int repairKits;

    [XmlArray("UnlockedItems")]
    public int[] itemIds;

    [XmlAttribute("Hull")]
    public int hull;
    [XmlAttribute("Thruster")]
    public int thruster;
    [XmlArray("Weapons")]
    public int[] weapons;

    [XmlAttribute("HullLight")]
    public int hullLight;
    [XmlAttribute("ThrusterLight")]
    public int thrusterLight;
    [XmlAttribute("WeaponLight")]
    public int weaponLight;

    public SaveFile()
    {
        score = scrap = repairKits= 0;
        level = 1;
    }
    public SaveFile(int s, int sc, int lvl, int rk, int h, int thr, int[] weps, int[] ids)
    {
        score = s;
        scrap = sc;
        level = lvl;
        itemIds = ids;
        repairKits = rk;
        hull = h;
        thruster = thr;
        weapons = weps;
    }

}

public static class MiscData
{
    private static int scrap;
    public static int highscore;
    public static int Scrap { get { return scrap; } set { scrap = value; EventManager.Instance.Raise<int>("UpdateScrap", scrap);} }
    public static int level=1;
    public static int repairKits;
    public static List<int> unlockedItems=new List<int>(new int[3] { 0, 1, 2 });

    public static int currentHull = 0;
    public static int currentThruster = 1;
    public static int[] currentWeapons = new int[1] { 2 };

    public static int hullLight=1;
    public static int thrusterLight=1;
    public static int weaponLight=1;

    private static SaveFile file;

    public static void SaveGame()
    {
        file = new SaveFile(highscore, scrap, level, repairKits,currentHull,currentThruster,currentWeapons, unlockedItems.ToArray());

        //Creates a serializer that serializes a specific class, in this case 'SaveFile'
        XmlSerializer serializer = new XmlSerializer(typeof(SaveFile));
        
        //Opens a path to the directory you'd like to save to AND names the save file
        FileStream stream = new FileStream(Path.Combine(UnityEngine.Application.dataPath, "SaveFile.xml"), FileMode.Create);

        //Saves and closes the file
        serializer.Serialize(stream, file);
        stream.Close();
    }

    public static void LoadGame()
    {
        //Checks to make sure the SaveFile exists in our directory
        if (File.Exists(Path.Combine(UnityEngine.Application.dataPath, "SaveFile.xml")))
        {
            //Creates a serializer that serializes a specific class, in this case 'SaveFile'
            XmlSerializer serializer = new XmlSerializer(typeof(SaveFile));

            //Opens a path to the directory you would like to load from AND the name of the file you are loading
            FileStream stream = new FileStream(Path.Combine(UnityEngine.Application.dataPath, "SaveFile.xml"), FileMode.Open);

            //Loads and closes the file
            file = serializer.Deserialize(stream) as SaveFile;
            stream.Close();
        }

        if(file!=null)
        {
            highscore = file.score;
            scrap = file.scrap;
            unlockedItems = new List<int>(file.itemIds);
            repairKits = file.repairKits;
            currentHull = file.hull;
            currentThruster = file.thruster;
            currentWeapons = file.weapons;
        }
    }
}
