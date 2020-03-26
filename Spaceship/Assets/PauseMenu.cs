using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    public RectTransform settingsMenu;
    public GameObject pauseText;
    public float blinkSpeed;
    
    PauseMenu()
    {
        Instance = this;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        if (pauseText && blinkSpeed != 0)
        {
            StartCoroutine(BlinkText());
        }
    }

    IEnumerator BlinkText()
    {
        while(gameObject.activeSelf)
        {
            pauseText.SetActive(!pauseText.activeSelf);
            yield return new WaitForSecondsRealtime(blinkSpeed);
        }
        yield return null;
    }

    public void Close()
    {
        gameObject.SetActive(false);

        if (settingsMenu)
        {
            settingsMenu.gameObject.SetActive(false);
        }
    }

    public void Resume()
    {
        if(EventManager.Instance && EventManager.Instance.IsEventRegistered("UpdatePause"))
        {
            EventManager.Instance.Raise<bool>("UpdatePause", false);
        }
    }

    public void Settings()
    {
        if (settingsMenu != null)
        {
            settingsMenu.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void ExitLevel()
    {
        Resume();
        if (GameManager.current)
        {
            GameManager.current.PlayerDied();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
