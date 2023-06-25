using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuSystem : MonoBehaviour
{
    public TMP_Text lastScore;

    void Start() 
    {
        if (lastScore != null)
            lastScore.text = Game.currentScore.ToString();
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("Level");
    }
    
    public void LaunchGameMenu()
    {
        SceneManager.LoadScene("GameMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
