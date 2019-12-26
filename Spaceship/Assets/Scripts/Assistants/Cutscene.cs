using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CutscenePart
{
    public Sprite sprite;
    public string dialog;
}

public class Cutscene : MonoBehaviour
{
    public bool beginning;
    public bool ending;
    public CutscenePart[] parts;
    private GameObject cutsceneUI;
    public Image characterImage;
    private Text dialogBox;

    private void Start()
    {
        cutsceneUI = GameManager.current.cutsceneUI.gameObject;
        characterImage = GameManager.current.characterImage;
        dialogBox = GameManager.current.dialogBox;
    }

    public void Play()
    {
        StartCoroutine(PlayCutscene());
    }

    public void Clear()
    {
        dialogBox.text = "";
        characterImage.sprite = null;
    }

    IEnumerator PlayCutscene()
    {
        cutsceneUI.SetActive(true);
        for (int i=0; i<parts.Length; i+=1)
        {
            while(Input.GetAxisRaw("Fire") == 0)
            {
                characterImage.sprite = parts[i].sprite;

                if (dialogBox.text != parts[i].dialog)
                {
                    for (int o = 0; o <= parts[i].dialog.Length; o += 1)
                    {
                        if(Input.GetAxisRaw("Fire") != 0)
                        {
                            break;
                        }
                        dialogBox.text = parts[i].dialog.Substring(0, o);
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();
                    }
                }
                yield return new WaitForEndOfFrame();
            }

            while(Input.GetAxisRaw("Fire") != 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        Vector3 oldCutsceneUIPosition = cutsceneUI.transform.position;
        Vector3 newCutsceneUIPosition = cutsceneUI.transform.position - new Vector3(cutsceneUI.GetComponent<RectTransform>().rect.width,0,0);

        float j = 0;
        while (cutsceneUI.transform.position != newCutsceneUIPosition)
        {
            cutsceneUI.transform.position = Vector3.Lerp(oldCutsceneUIPosition, newCutsceneUIPosition, j);

            j += Time.deltaTime;
            yield return null;
        }
        PlayerController.current.waitAtStart = false;
        Destroy(gameObject);
        yield return null;
    }
}
