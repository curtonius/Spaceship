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
    public State currentState = State.MainMenu;

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
    public Image background1;
    public Image background2;

    //UI Elements for cutscenes
    public RectTransform cutsceneUI;
    public Image characterImage;
    public Text dialogBox;

    //Prefab containing the explosion particle effect
    public GameObject explosionPrefab;
    //Image for the healthbar
    public Image healthBar;

    private int sceneIndex = 4;
    private int numberOfEnemies;
    private Transform lastEnemy;
    private int currentLevelMax = 1;
    private Vector3 topRight;
    private Vector3 bottomLeft;

    // Start is called before the first frame update
    void Start()
    {
        if(current != null)
        {
            Destroy(current.gameObject);
        }

        current = this;

        DontDestroyOnLoad(gameObject);

        MiscData.LoadGame();
        currentLevelMax = MiscData.level;
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
        highscoreText.text = "Highscore: " + MiscData.highscore.ToString();
        finishLogo.rectTransform.position -= new Vector3(-(Screen.width / 2 + finishLogo.rectTransform.rect.width / 2), 0, 0);

        background1.rectTransform.sizeDelta = background2.rectTransform.sizeDelta = new Vector2(Screen.height, Screen.height*2);
        background2.rectTransform.anchoredPosition = background1.rectTransform.anchoredPosition + new Vector2(0, Screen.height * 2);
        background1.GetComponent<Scroller>().tileSize = Screen.height * 2;
        background2.GetComponent<Scroller>().tileSize = Screen.height * 2;

        menuUI.sizeDelta = new Vector2(Screen.width,Screen.height);
    }

    //Create explosion at position, and destroy it after half a second
    public void SpawnExplosion(Vector3 position)
    {
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.GetComponent<ParticleSystem>().Emit(1);
        Destroy(explosion, 0.5f);
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
        currentState = State.Endless;
        menuUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);
        SceneManager.LoadScene(3);
    }

    public void Clear()
    {
        Destroy(background1);
        Destroy(background2);
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
                Weapon weapon = Instantiate(allParts[MiscData.currentWeapons[i]].gameObject, hull.weaponPorts[i].position, hull.weaponPorts[i].rotation).GetComponent<Weapon>();
                weapon.transform.SetParent(playerControllerTransform);
                weapon.barrel = hull.weaponPorts[i];
            }
        }
        //Activate modifers
        PlayerController.current.maxHealth += hull.healthMod + (MiscData.hullLight * 10);
        PlayerController.current.health = PlayerController.current.maxHealth;
        PlayerController.current.movementSpeed += thruster.speedMod + MiscData.thrusterLight;

        Destroy(background1);
        Destroy(background2);


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
                Weapon weapon = Instantiate(allParts[MiscData.currentWeapons[i]].gameObject, hull.weaponPorts[i].position, hull.weaponPorts[i].rotation).GetComponent<Weapon>();
                weapon.transform.SetParent(playerControllerTransform);
                weapon.barrel = hull.weaponPorts[i];
            }
        }
        //Activate modifers
        PlayerController.current.maxHealth += hull.healthMod + (MiscData.hullLight*10);
        PlayerController.current.health = PlayerController.current.maxHealth;
        PlayerController.current.movementSpeed += thruster.speedMod + MiscData.thrusterLight;


        Destroy(background1);
        Destroy(background2);
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
            currentState = State.NumberOfEnemies;
        }
        else
        {
            currentState = State.SurviveUntilEnd;
        }

        Vector3 oldGameUIPosition = gameUI.position;
        Vector3 oldCutsceneUIPosition = cutsceneUI.position;
        gameUI.position += new Vector3(gameUI.rect.width,0,0);
        gameUI.gameObject.SetActive(true);
        Cutscene scene = null;
        if (FindObjectOfType<Cutscene>())
        {
            Cutscene[] cutscenes;
            cutscenes = FindObjectsOfType<Cutscene>();

            for (int i = 0; i < cutscenes.Length; i += 1)
            {
                if (cutscenes[i].beginning)
                {
                    cutsceneUI.position -= new Vector3(cutsceneUI.rect.width, 0, 0);
                    cutsceneUI.gameObject.SetActive(true);
                    scene = cutscenes[i];
                    scene.characterImage.sprite = scene.parts[0].sprite;
                    break;
                }
            }
        }
        Vector3 newGameUIPosition = gameUI.position;
        Vector3 newCutsceneUIPosition = cutsceneUI.position;
        Vector3 playerPosition = PlayerController.current.transform.position;

        float j = 0;
        while(gameUI.position != oldGameUIPosition)
        {
            gameUI.position = Vector3.Lerp(newGameUIPosition, oldGameUIPosition, j);
            if (scene)
                cutsceneUI.position = Vector3.Lerp(newCutsceneUIPosition, oldCutsceneUIPosition, j);

            PlayerController.current.transform.position = Vector3.Lerp(playerPosition, new Vector3(0, 0, -10), j);

            j += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if(scene!=null)
        {
            scene.Play();
        }

        yield return null;
    }

    IEnumerator DragIn()
    {
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

    private void Update()
    {
        scrapText.text = MiscData.scrap.ToString();
        if(currentState != State.MainMenu)
        {
            if(currentState == State.SurviveUntilEnd || currentState == State.NumberOfEnemies)
            {
                List<Hazard> hazards = new List<Hazard>(FindObjectsOfType<Hazard>());
                hazards.RemoveAll(x => x.name.Contains("Bullet"));
                if(hazards.Count == 0)
                {
                    if(SceneManager.GetActiveScene().buildIndex == currentLevelMax+2 && SceneManager.sceneCountInBuildSettings > currentLevelMax+3)
                    {
                        currentLevelMax += 1;
                        MiscData.level = currentLevelMax;
                    }
                    StartCoroutine(EndLevel(true,false));
                }
                Bullet[] bullets = FindObjectsOfType<Bullet>();
                for (int i = 0; i < bullets.Length; i += 1)
                {
                    if (bullets[i].transform.position.z > topRight.z)
                    {
                        Destroy(bullets[i].gameObject);
                    }
                }
            }
        }
    }

    public void PlayerDied()
    {
        if (currentState == State.Endless)
        {
            StartCoroutine(EndLevel(false, true));
        }
        else
        {
            StartCoroutine(EndLevel(false, false));
        }
    }

    private IEnumerator EndLevel(bool cleared, bool endless)
    {
        if (!cleared)
            finishLogo.text = "Failed!";

        if(endless)
        {
            finishLogo.text = "Final Score: " + score + "!";
        }
        currentState = State.MainMenu;
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
            Cutscene scene = null;
            if (FindObjectOfType<Cutscene>())
            {
                Cutscene[] cutscenes;
                cutscenes = FindObjectsOfType<Cutscene>();

                for (int i = 0; i < cutscenes.Length; i += 1)
                {
                    if (cutscenes[i].ending)
                    {
                        PlayerController.current.EndLevel(cutscenes[i]);
                        scene = cutscenes[i];
                        scene.Clear();
                        scene.characterImage.sprite = scene.parts[0].sprite;
                    }
                }
            }

            Vector3 oldPosition = cutsceneUI.transform.position;
            Vector3 newPosition = cutsceneUI.transform.position + new Vector3(cutsceneUI.GetComponent<RectTransform>().rect.width, 0, 0);

            float j = 0;
            while (cutsceneUI.position != newPosition)
            {
                cutsceneUI.transform.position = Vector3.Lerp(oldPosition, newPosition, j);

                j += Time.deltaTime;
                yield return null;
            }

            scene.Play();

            while (scene != null)
            {
                yield return new WaitForEndOfFrame();
            }
            Transform playerTrans = PlayerController.current.transform;
            Destroy(PlayerController.current);
            oldPosition = playerTrans.position;
            newPosition = playerTrans.position + new Vector3(0, 0, 10);

            j = 0;
            while (playerTrans.position != newPosition)
            {
                playerTrans.position = Vector3.Lerp(oldPosition, newPosition, j);

                j += Time.deltaTime;
                yield return null;
            }
        }
        yield return new WaitForSeconds(1);
        currentState = State.MainMenu;
        MiscData.SaveGame();
        SceneManager.LoadScene(0);
        yield return null;
    }
}
