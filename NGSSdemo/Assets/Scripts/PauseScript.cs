using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour
{

    public InputField textBox;
    public static int score;
    private HighscoreTable highscoreTable;
    public Text scoreText;
    public Text saveScoreText;
    public GameObject loadingScreen;
    public Slider loadingBar;

    private void Start()
    {
        highscoreTable = new HighscoreTable();
    }


    private void Update()
    {
        if(scoreText.text != null )
        {
            scoreText.text = "Score: " + score.ToString();
            
        }
        if(saveScoreText != null)
        {
            saveScoreText.text = "Score: " + score.ToString();
        }
        
    }
    public void clickSaveButton()
    {
        if(textBox.text != null)
        {
            highscoreTable.AddHighscoreEntry(score, textBox.text);

            
        }

        
    }


    public void LoadScreen(int index)
    {

        StartCoroutine(loadAsync(index));

    }

    IEnumerator loadAsync(int levelIndex)
    {
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(levelIndex);
        while (!operation.isDone)
        {
            loadingScreen.SetActive(true);
            loadingBar.value = operation.progress;
            Debug.Log(operation.progress);
            yield return null;
        }
    }


    public void exitScreen()
    {
        Application.Quit();
    }




}
