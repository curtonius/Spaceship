using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Singleton for GameManager
    public static GameManager current;
    public enum State { NumberOfEnemies, SurviveUntilEnd, Endless, MainMenu}
    public State CurrentState { get{ return currentState; } set { currentState = value; CheckState(); } }
    private State currentState = State.MainMenu;

    public Part[] allParts;

    //Values for the current game score and the highscore
    public int score;

    //GameObjects containing UI elements
    public RectTransform gameUI;
    public RectTransform menuUI;
    public GameObject levelSelectionUI;

    //UI Element for the In-Game score
    public Text scoreText;
    //UI Element for the Menu highscore
    public Text highscoreText;
    //UI Element for the Scrap menu
    public Text scrapText;
    //UI Element for the Level Selection button
    public Text levelSelectionButton;
    //UI Element for the Finish logo
    public Text finishLogo;
    //UI Element for Background
    public GameObject background;

    //UI Elements for cutscenes
    public RectTransform cutsceneUI;
    public Image characterImage;
    public Text dialogBox;

    //Prefab containing the explosion particle effect
    public GameObject explosionPrefab;
    public GameObject forceFieldPrefab;
    //Image for the healthbar
    public Image healthBar;
    public Image[] impactShields;
    public Image repairKit;

    private int sceneIndex = 4;
    private int numberOfEnemies;
    private Transform lastEnemy;
    private int currentLevelMax = 1;
    private Vector3 topRight;
    private Vector3 bottomLeft;

    public Dictionary<string, int> boosts = new Dictionary<string, int>();
    private int hits;
    private int hitsThreshold;
    private int boostNumber = 1;
    public Text boostLogo;

    // Start is called before the first frame update
    void Start()
    {
        if(current != null)
        {
            Destroy(current.gameObject);
        }

        current = this;

        DontDestroyOnLoad(gameObject);
        EventManager.Instance.AddEventListener<int>("UpdateScrap", UpdateScrapText);
        MiscData.LoadGame();
        currentLevelMax = MiscData.level;
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
        highscoreText.text = "Highscore: " + MiscData.highscore.ToString();

        finishLogo.rectTransform.position -= new Vector3(-(Screen.width / 2 + finishLogo.rectTransform.rect.width / 2), 0, 0);
        boostLogo.rectTransform.position = finishLogo.rectTransform.position;

        menuUI.sizeDelta = new Vector2(Screen.width,Screen.height);

        boosts.Add("Damage Increase", 0);
        boosts.Add("Extra Repair Kit", 0);
        boosts.Add("Extra Impact Shield", 0);
        boosts.Add("Health Increase", 0);
        boosts.Add("Speed Increase", 0);
        boosts.Add("Scrap Collection Increase", 0);
        hitsThreshold = 10;
    }
    private void OnDestroy()
    {
        EventManager.Instance.RemoveEventListener<int>("UpdateScrap", UpdateScrapText);
    }

    public void AddHit()
    {
        hits += 1;
        Debug.Log(hits);
        if(hits == hitsThreshold)
        {
            hits = 0;
            boostNumber += 1;
            hitsThreshold = 5 * (int)Mathf.Pow(2,boostNumber);
            DoBoost();
        }
    }

    public void ClearHits()
    {
        hits = 0;
    }

    public void DoBoost()
    {
        int value = Random.Range(1, 6);
        if (value == 1)
        {
            boosts["Damage Increase"] += 3;
            StartCoroutine(DisplayBoost("Damage Increase"));
        }
        else if (value == 2)
        {
            PlayerController.current.AddRepairKit();
            StartCoroutine(DisplayBoost("Extra Repair Kit"));
        }
        else if (value == 3)
        {
            PlayerController.current.AddImpactShield();
            AddImpactShield();
            StartCoroutine(DisplayBoost("Extra Impact Shield"));
        }
        else if (value == 4)
        {
            PlayerController.current.maxHealth += 10;
            PlayerController.current.Health += 20;
            StartCoroutine(DisplayBoost("Health Increase"));
        }
        else if (value == 5)
        {
            PlayerController.current.movementSpeed += 2;
            StartCoroutine(DisplayBoost("Speed Increase"));
        }
        else if (value == 6)
        {
            boosts["Scrap Collection Increase"] += 1;
            StartCoroutine(DisplayBoost("Scrap Collection Increase"));
        }
    }

    //Create explosion at position, and destroy it after half a second
    public void SpawnExplosion(Vector3 position)
    {
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.GetComponent<ParticleSystem>().Emit(1);
        Destroy(explosion, 0.5f);
    }

    public void AddImpactShield()
    {
        foreach (Image image in impactShields)
        {
            if (image.color != new Color(1, 1, 0))
            {
                image.color = new Color(1, 1, 0);
                break;
            }
        }
    }

    public GameObject EnableImpactField()
    {
        GameObject field = Instantiate(forceFieldPrefab, PlayerController.current.transform);
        foreach (Image image in impactShields)
        {
            if (image.color == new Color(1, 1, 0))
            {
                image.color = new Color(0.25f, 0.25f, 0.25f);
                break;
            }
        }
        return field;
    }

    public void UseRepairKit()
    {
        repairKit.gameObject.SetActive(false);
    }

    //Add points to the total score
    public void AddToScore(int points)
    {
        score += points;
        if(score > MiscData.highscore)
        {
            MiscData.highscore = score;            
        }
        scoreText.text = score.ToString();
    }

    //Click on Level selection button to display levels 1 by 1
    public void LevelSelection()
    {
        sceneIndex = 4;
        menuUI.gameObject.SetActive(false);
        levelSelectionUI.SetActive(true);
        levelSelectionButton.text = "Level 1";
    }

    //Click on endless button to start endless mode
    public void EndlessButton()
    {
        StartCoroutine(StartEndless());
        CurrentState = State.Endless;
        menuUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);
        SceneManager.LoadScene(3);
    }

    public void Clear()
    {
        background.SetActive(false);
        menuUI.gameObject.SetActive(false);
    }

    //Click on craft parts button to open crafting menu
    public void CraftButton()
    {
        SceneManager.LoadScene(1);
    }

    //Click on customize ship button to open customization menu
    public void CustomizeButton()
    {
        SceneManager.LoadScene(2);
    }

    //Click on Quit button to end the game
    public void QuitButton()
    {
        Application.Quit();
    }

    //Click on Level button to select this level
    public void SelectLevel()
    {
        levelSelectionUI.SetActive(false);
        
        SceneManager.LoadScene(sceneIndex);
        StartCoroutine(StartGame());
    }

    public void Back()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            menuUI.gameObject.SetActive(true);
            levelSelectionUI.SetActive(false);
            return;
        }
        MiscData.SaveGame();
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    IEnumerator StartEndless()
    {
        while(!PlayerController.current)
        {
            yield return new WaitForEndOfFrame();
        }
        //Generate players ship
        Transform playerControllerTransform = PlayerController.current.transform;
        Hull hull = Instantiate(allParts[MiscData.currentHull].gameObject, playerControllerTransform.position, playerControllerTransform.rotation).GetComponent<Hull>();
        hull.transform.SetParent(playerControllerTransform);
        Thruster thruster = Instantiate(allParts[MiscData.currentThruster].gameObject, hull.thrusterLocation.position, hull.thrusterLocation.rotation).GetComponent<Thruster>();
        thruster.transform.SetParent(playerControllerTransform);
        for (int i = 0; i < MiscData.currentWeapons.Length; i += 1)
        {
            if (MiscData.currentWeapons[i] >= 0)
            {
                GameObject weaponBase = Instantiate(PlayerController.current.weaponBase, hull.weaponPorts[i].position, hull.weaponPorts[i].rotation);
                weaponBase.transform.parent = playerControllerTransform;

                Weapon weapon = Instantiate(allParts[MiscData.currentWeapons[i]].gameObject, weaponBase.transform.position + weaponBase.transform.up * 0.05f, Quaternion.identity).GetComponent<Weapon>();
                weapon.transform.SetParent(playerControllerTransform);
                weapon.barrel = hull.weaponPorts[i];
            }
        }
        //Activate modifers
        PlayerController.current.maxHealth += hull.healthMod + (MiscData.hullLight * 10);
        PlayerController.current.Health = PlayerController.current.maxHealth;
        PlayerController.current.movementSpeed += thruster.speedMod + MiscData.thrusterLight;

        background.SetActive(false);


        yield return null;
    }

    IEnumerator StartGame()
    {
        List<Hazard> hazards = new List<Hazard>(FindObjectsOfType<Hazard>());
        hazards.RemoveAll(x => x.name.Contains("Bullet"));

        while (hazards.Count == 0)
        {
            hazards = new List<Hazard>(FindObjectsOfType<Hazard>());
            yield return null;
        }

        //Generate players ship
        Transform playerControllerTransform = PlayerController.current.transform;
        Hull hull = Instantiate(allParts[MiscData.currentHull].gameObject, playerControllerTransform.position, playerControllerTransform.rotation).GetComponent<Hull>();
        hull.transform.SetParent(playerControllerTransform);
        Thruster thruster = Instantiate(allParts[MiscData.currentThruster].gameObject, hull.thrusterLocation.position, hull.thrusterLocation.rotation).GetComponent<Thruster>();
        thruster.transform.SetParent(playerControllerTransform);
        for (int i = 0; i < MiscData.currentWeapons.Length; i += 1)
        {
            if (MiscData.currentWeapons[i] >= 0)
            {
                GameObject weaponBase = Instantiate(PlayerController.current.weaponBase, hull.weaponPorts[i].position, hull.weaponPorts[i].rotation);
                weaponBase.transform.parent = playerControllerTransform;

                Weapon weapon = Instantiate(allParts[MiscData.currentWeapons[i]].gameObject, weaponBase.transform.position + weaponBase.transform.up*0.05f, Quaternion.identity).GetComponent<Weapon>();
                weapon.transform.SetParent(playerControllerTransform);
                weapon.barrel = hull.weaponPorts[i];
            }
        }
        //Activate modifers
        PlayerController.current.maxHealth += hull.healthMod + (MiscData.hullLight*10);
        PlayerController.current.Health = PlayerController.current.maxHealth;
        PlayerController.current.movementSpeed += thruster.speedMod + MiscData.thrusterLight;


        background.SetActive(false);
        numberOfEnemies = hazards.Count;
        Hazard hazard = hazards[0];
        lastEnemy = hazard.transform;
        for (int i=0; i<numberOfEnemies; i+=1)
        {
            if(hazard && hazards[i].transform.position.z > hazard.transform.position.z)
            {
                hazard = hazards[i];
            }
        }
        
        if(hazard)
            lastEnemy = hazard.transform;

        if (SceneManager.GetActiveScene().buildIndex % 5 == 0)
        {
            CurrentState = State.NumberOfEnemies;
        }
        else
        {
            CurrentState = State.SurviveUntilEnd;
        }

        Vector3 oldGameUIPosition = gameUI.position;
        gameUI.position += new Vector3(gameUI.rect.width,0,0);
        gameUI.gameObject.SetActive(true);
        Vector3 newGameUIPosition = gameUI.position;
        Vector3 playerPosition = PlayerController.current.transform.position;

        float j = 0;
        while(gameUI.position != oldGameUIPosition)
        {
            gameUI.position = Vector3.Lerp(newGameUIPosition, oldGameUIPosition, j);

            

            j += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        yield return null;
    }


    //Click on Next button to change which level is being selected
    public void NextLevel()
    {
        if (sceneIndex >= SceneManager.sceneCountInBuildSettings-1 || sceneIndex >= currentLevelMax+3)
            sceneIndex = 4;
        else
            sceneIndex += 1;

        if (sceneIndex % 5 != 0)
            levelSelectionButton.text = "Level " + (sceneIndex-3);
        else
            levelSelectionButton.text = "Fleet " + ((sceneIndex - 3) / 5);
    }

    //Click on Prev button to change which level is being selected
    public void PrevLevel()
    {
        if (sceneIndex == 4)
            sceneIndex = currentLevelMax+3;
        else
            sceneIndex -= 1;

        if (sceneIndex % 5 != 0)
            levelSelectionButton.text = "Level " + (sceneIndex - 3);
        else
            levelSelectionButton.text = "Fleet " + ((sceneIndex - 3) / 5);
    }

    public void UpdateLastEnemy(Transform enemy)
    {
        enemy.position = lastEnemy.position + new Vector3(0, 0, Mathf.Abs(topRight.z - bottomLeft.z));
        
        lastEnemy = enemy;
    }

    private void UpdateScrapText(int scrap)
    {
        scrapText.text = scrap.ToString();
    }

    void CheckState()
    {
        if (CurrentState == State.SurviveUntilEnd || CurrentState == State.NumberOfEnemies)
        {
            StartCoroutine(PlayStandardGame());
        }
        else if(CurrentState == State.Endless)
        {
            StartCoroutine(PlayEndlessGame());
        }
    }

    IEnumerator PlayEndlessGame()
    {
        bool playing = true;
        while (playing)
        {
            Bullet[] bullets = FindObjectsOfType<Bullet>();
            for (int i = 0; i < bullets.Length; i += 1)
            {
                if (bullets[i].transform.position.z > topRight.z)
                {
                    Destroy(bullets[i].gameObject);
                }
            }
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    IEnumerator PlayStandardGame()
    {
        bool playing = true;
        while (playing)
        {
            List<Hazard> hazards = new List<Hazard>(FindObjectsOfType<Hazard>());
            hazards.RemoveAll(x => x.name.Contains("Bullet"));
            if (hazards.Count == 0)
            {
                if (SceneManager.GetActiveScene().buildIndex == currentLevelMax + 2 && SceneManager.sceneCountInBuildSettings > currentLevelMax + 3)
                {
                    currentLevelMax += 1;
                    MiscData.level = currentLevelMax;
                }
                StartCoroutine(EndLevel(true, false));
                playing = false;
            }
            Bullet[] bullets = FindObjectsOfType<Bullet>();
            for (int i = 0; i < bullets.Length; i += 1)
            {
                if (bullets[i].transform.position.z > topRight.z)
                {
                    Destroy(bullets[i].gameObject);
                }
            }
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    public void PlayerDied()
    {
        if (CurrentState == State.Endless)
        {
            StartCoroutine(EndLevel(false, true));
        }
        else
        {
            StartCoroutine(EndLevel(false, false));
        }
    }

    private IEnumerator DisplayBoost(string boostLabel)
    {
        boostLogo.text = "Boost: " + boostLabel;
        RectTransform rectTransform = boostLogo.rectTransform;
        while (rectTransform.position.x > Screen.width / 2)
        {
            rectTransform.position -= new Vector3(50, 0, 0);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        while (rectTransform.position.x < Screen.width + rectTransform.rect.width / 2)
        {
            rectTransform.position += new Vector3(50, 0, 0);
            yield return null;
        }
        yield return null;
    }

    private IEnumerator EndLevel(bool cleared, bool endless)
    {
        if (!cleared)
            finishLogo.text = "Failed!";

        if(endless)
        {
            finishLogo.text = "Final Score: " + score + "!";
        }
        CurrentState = State.MainMenu;
        RectTransform rectTransform = finishLogo.rectTransform;
        while(rectTransform.position.x > Screen.width/2)
        {
            rectTransform.position -= new Vector3(50, 0, 0);
            yield return null;
        }
        yield return new WaitForSeconds(2);
        while (rectTransform.position.x < Screen.width + rectTransform.rect.width/2)
        {
            rectTransform.position += new Vector3(50, 0, 0);
            yield return null;
        }

        if (cleared)
        {
            //Pull up cutscene, add cutscene to player, play cutscene, finish
            PlayerController.current.EndLevel();

          
            while (CutsceneManager.Instance.playingCutscene)
            {
                yield return new WaitForEndOfFrame();
            }

            Transform playerTrans = PlayerController.current.transform;
            Destroy(PlayerController.current);
            Vector3 oldPosition = playerTrans.position;
            Vector3 newPosition = playerTrans.position + new Vector3(0, 0, 15);

            float j = 0;
            while (playerTrans.position != newPosition)
            {
                playerTrans.position = Vector3.Lerp(oldPosition, newPosition, j);

                j += Time.deltaTime;
                yield return null;
            }
        }
        yield return new WaitForSeconds(1);
        CurrentState = State.MainMenu;
        MiscData.SaveGame();
        SceneManager.LoadScene(0);
        yield return null;
    }
}
