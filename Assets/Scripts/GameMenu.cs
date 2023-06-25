using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameMenu : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text highScoreText;
    public TMP_Text highScoreText2;
    public TMP_Text highScoreText3;

    void Start() 
    {
        if (levelText != null)
            levelText.text = "0";

        if (highScoreText != null)
            highScoreText.text = PlayerPrefs.GetInt("highscore").ToString();
            
        if (highScoreText2 != null)
            highScoreText2.text = PlayerPrefs.GetInt("highscore2").ToString();

        if (highScoreText3 != null)
            highScoreText3.text = PlayerPrefs.GetInt("highscore3").ToString();
    }

    public void PlayGame()
    {
        if (Game.startingLevel == 0)
        {
            Game.startingAtLevel0 = true;
        }
        else
        {
            Game.startingAtLevel0 = false;
        }

        SceneManager.LoadScene("Level");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangedValue(float value)
    {
        Game.startingLevel = (int)value;
        levelText.text = value.ToString();
    }

}
