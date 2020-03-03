using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;
    private RectTransform cutsceneUI;
    private Image characterImage;
    private Text dialogBox;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        cutsceneUI = GameManager.current.cutsceneUI.GetComponent<RectTransform>() ;
        characterImage = GameManager.current.characterImage;
        dialogBox = GameManager.current.dialogBox;
    }

    private List<string> TextAssetToList(TextAsset ta)
    {
        List<string> listToReturn = new List<string>();
        string[] arrayString = ta.text.Split('\n');
        listToReturn.AddRange(arrayString);
        return listToReturn;
    }

    bool runningMoveTo;
    IEnumerator MoveTo(string line)
    {
        runningMoveTo = true;
        int firstComma = line.IndexOf(",");
        string gameObjectName = line.Substring(7, firstComma - 7);
        if (GameObject.Find(gameObjectName))
        {
            Transform gameObjectTransform = GameObject.Find(gameObjectName).transform;
            Vector3 originalPosition = gameObjectTransform.position;
            Vector3 position;
            float timeToMove;
            string nextPart = line.Substring(firstComma + 1);

            int nextComma = nextPart.IndexOf(",");
            position.x = float.Parse(nextPart.Substring(0, nextComma));

            nextPart = nextPart.Substring(nextComma + 1);
            nextComma = nextPart.IndexOf(",");
            position.y = float.Parse(nextPart.Substring(0, nextComma));

            nextPart = nextPart.Substring(nextComma + 1);
            nextComma = nextPart.IndexOf(",");
            position.z = float.Parse(nextPart.Substring(0, nextComma));

            nextPart = nextPart.Substring(nextComma + 1);
            timeToMove = float.Parse(nextPart.Substring(0, nextPart.Length-1));

            float currentTime = 0;
            while (gameObjectTransform.position != position)
            {
                currentTime += Time.deltaTime;
                gameObjectTransform.position = Vector3.Lerp(originalPosition, position, currentTime / timeToMove);
                yield return new WaitForEndOfFrame();
            }
        }
        runningMoveTo = false;

        yield return null;
    }

    bool runningFaceDirection;
    IEnumerator FaceDirection(string line)
    {
        runningFaceDirection = true;
        int firstComma = line.IndexOf(",");
        string gameObjectName = line.Substring(14, firstComma - 14);
        if (GameObject.Find(gameObjectName))
        {
            Transform gameObjectTransform = GameObject.Find(gameObjectName).transform;
            Quaternion originalRotation = gameObjectTransform.rotation;
            Vector3 rotBeforeEuler;
            float timeToMove;
            string nextPart = line.Substring(firstComma + 1);
            int nextComma = nextPart.IndexOf(",");
            rotBeforeEuler.x = float.Parse(nextPart.Substring(0, nextComma));

            nextPart = nextPart.Substring(nextComma + 1);
            nextComma = nextPart.IndexOf(",");
            rotBeforeEuler.y = float.Parse(nextPart.Substring(0, nextComma));

            nextPart = nextPart.Substring(nextComma + 1);
            nextComma = nextPart.IndexOf(",");
            rotBeforeEuler.z = float.Parse(nextPart.Substring(0, nextComma));

            nextPart = nextPart.Substring(nextComma + 1);
            timeToMove = float.Parse(nextPart.Substring(0, nextPart.Length-1));

            Quaternion rotation = Quaternion.Euler(rotBeforeEuler);
            float currentTime = 0;
            while (gameObjectTransform.rotation != rotation)
            {
                currentTime += Time.deltaTime;
                gameObjectTransform.rotation = Quaternion.Lerp(originalRotation, rotation, currentTime / timeToMove);
                yield return new WaitForEndOfFrame();
            }
        }
        runningFaceDirection = false;
        yield return null;
    }

    bool playingText;
    IEnumerator PlayText(string line)
    {
        playingText = true;
        int firstComma = line.IndexOf(",");
        string characterImageName = line.Substring(0, firstComma);
        Sprite characterImageFile = (Sprite)Resources.Load(characterImageName, typeof(Sprite));
        if (characterImageFile)
        {
            if (!cutsceneUI.gameObject.activeInHierarchy)
            {
                Vector3 oldCutsceneUIPosition = cutsceneUI.position;
                cutsceneUI.position -= new Vector3(cutsceneUI.rect.width, 0, 0);
                cutsceneUI.gameObject.SetActive(true);
                characterImage.sprite = characterImageFile;
                Vector3 newCutsceneUIPosition = cutsceneUI.position;

                float j = 0;
                while (cutsceneUI.position != oldCutsceneUIPosition)
                {
                    cutsceneUI.position = Vector3.Lerp(newCutsceneUIPosition, oldCutsceneUIPosition, j);
                    j += Time.deltaTime;
                    yield return null;
                }
            }

            string finalText = line.Substring(firstComma + 1, line.Length - (firstComma + 2));
            while (Input.GetAxisRaw("Fire") == 0)
            {
                characterImage.sprite = characterImageFile;

                if (dialogBox.text != finalText)
                {
                    for (int o = 0; o <= finalText.Length; o += 1)
                    {
                        if (Input.GetAxisRaw("Fire") != 0)
                        {
                            break;
                        }
                        dialogBox.text = finalText.Substring(0, o);
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
        playingText = false;
        yield return null;
    }

    bool closingText;
    IEnumerator CloseText()
    {
        Vector3 oldCutsceneUIPosition = cutsceneUI.position;
        Vector3 newCutsceneUIPosition = cutsceneUI.position + new Vector3(cutsceneUI.rect.width, 0, 0); ;

        float j = 0;
        while (cutsceneUI.position != newCutsceneUIPosition)
        {
            cutsceneUI.position = Vector3.Lerp(oldCutsceneUIPosition, newCutsceneUIPosition, j);
            j += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    public bool playingCutscene;
    public IEnumerator ReadCutscene(bool atStart)
    {
        PlayerController.current.waitAtStart = true;
        playingCutscene = true;
        TextAsset SourceFile = (TextAsset)Resources.Load<TextAsset>("Cutscenes");
        if(SourceFile)
        {
            List<string> lines = TextAssetToList(SourceFile);
            if(lines.Count > 0)
            {
                bool start = false;
                int skip = 0;
                for(int i=0; i<lines.Count; i+=1)
                {
                    string addOn = "AtStart";
                    if (!atStart)
                        addOn = "AtEnd";
                    
                    string line = lines[i];
                    line = line.Replace("\r", "");

                    if (!start && line == "[[" + SceneManager.GetActiveScene().name + "]] " + addOn)
                    {
                        cutsceneUI.gameObject.SetActive(false);
                        start = true;
                    }
                    else if (start && line.Length > 7 && line.Substring(0, 7) == "MoveTo(")
                    {
                        StartCoroutine(MoveTo(line));
                        while (runningMoveTo)
                            yield return new WaitForEndOfFrame();
                    }
                    else if(start && line.Length > 14 && line.Substring(0,14) == "FaceDirection(")
                    {
                        StartCoroutine(FaceDirection(line));
                        while (runningFaceDirection)
                            yield return new WaitForEndOfFrame();
                    }
                    else if(start && line.Length > 5 && line.Substring(0,5) == "Wait(")
                    {
                        float waitTime = float.Parse(line.Substring(5, line.IndexOf(")") - 5));
                        
                        float currentTime = 0;
                        while (currentTime < waitTime)
                        {
                            currentTime += Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }
                    }
                    else if(start && line == "Rep")
                    {
                        List<string> doLines = new List<string>();
                        skip = 1;
                        for(int j=i+1; j<lines.Count; j+=1)
                        {
                            string newLine = lines[j].Replace("\r", "");
                            if(newLine == "}")
                            {
                                break;
                            }
                            else
                            {
                                doLines.Add(newLine);
                                skip += 1;
                            }
                        }

                        bool fired = false;
                        while(!fired)
                        {
                            for(int j=0; j<doLines.Count; j+=1)
                            {
                                if (start && doLines[j].Length > 7 && doLines[j].Substring(0, 7) == "MoveTo(")
                                {
                                    StartCoroutine(MoveTo(doLines[j]));
                                    while (runningMoveTo)
                                    {
                                        if (Input.GetAxisRaw("Fire") == 1)
                                            fired = true;
                                        yield return new WaitForEndOfFrame();
                                    }
                                }
                                else if (start && doLines[j].Length > 14 && doLines[j].Substring(0, 14) == "FaceDirection(")
                                {
                                    StartCoroutine(FaceDirection(doLines[j]));
                                    while (runningFaceDirection)
                                    {
                                        if (Input.GetAxisRaw("Fire") == 1)
                                            fired = true;
                                        yield return new WaitForEndOfFrame();
                                    }
                                }
                                else if (start && doLines[j].Length > 5 && doLines[j].Substring(0, 5) == "Wait(")
                                {
                                    float waitTime = float.Parse(doLines[j].Substring(5, doLines[j].IndexOf(")") - 5));
                                    float currentTime = 0;
                                    while (currentTime < waitTime)
                                    {
                                        if (Input.GetAxisRaw("Fire") == 1)
                                            fired = true;
                                        currentTime += Time.deltaTime;
                                        yield return new WaitForEndOfFrame();
                                    }
                                }
                            }
                            yield return new WaitForEndOfFrame();
                        }
                        i += skip;
                    }
                    else if(start && line.Length > 9 && line.Substring(0,9) == "PlayText(")
                    {
                        string restOfText = line.Substring(9, line.Length - 10);
                        StartCoroutine(PlayText(restOfText));
                        while (playingText)
                            yield return new WaitForEndOfFrame();
                    }
                    else if(start && line == "End Scene: Release Player")
                    {
                        Vector3 oldCutsceneUIPosition = cutsceneUI.transform.position;
                        Vector3 newCutsceneUIPosition = cutsceneUI.transform.position - new Vector3(cutsceneUI.GetComponent<RectTransform>().rect.width, 0, 0);

                        float j = 0;
                        while (cutsceneUI.transform.position != newCutsceneUIPosition)
                        {
                            cutsceneUI.transform.position = Vector3.Lerp(oldCutsceneUIPosition, newCutsceneUIPosition, j);

                            j += Time.deltaTime;
                            yield return null;
                        }
                        cutsceneUI.gameObject.SetActive(false);
                        break;
                    }
                    else if(start && line == "End Scene: Close")
                    {
                        Vector3 oldCutsceneUIPosition = cutsceneUI.transform.position;
                        Vector3 newCutsceneUIPosition = cutsceneUI.transform.position - new Vector3(cutsceneUI.GetComponent<RectTransform>().rect.width, 0, 0);

                        float j = 0;
                        while (cutsceneUI.transform.position != newCutsceneUIPosition)
                        {
                            cutsceneUI.transform.position = Vector3.Lerp(oldCutsceneUIPosition, newCutsceneUIPosition, j);

                            j += Time.deltaTime;
                            yield return null;
                        }
                        cutsceneUI.gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }
        PlayerController.current.waitAtStart = false;
        playingCutscene = false;
        yield return null;
    }
}
