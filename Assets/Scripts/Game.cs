using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.ImageEffects;

public class Game : MonoBehaviour
{
    public static bool startingAtLevel0;
    public static int startingLevel;

    public static float fallSpeed = 1.0f;

    public int currentLevel = 0;
    private int numLinesCleared = 0;

    public static int gridHeight = 20;
    public static int gridWidth = 10;

    public static Transform[,] grid = new Transform[gridWidth, gridHeight];

    public int[] PointsForLinesCleared = new int[4] {100, 250, 600, 1400};
    private int numberOfRowsDeleted = 0;

    public TMP_Text hudScore;
    public TMP_Text hudLevel;
    public TMP_Text hudLines;

    public static int currentScore = 0;
    private int startingHighScore;
    private int startingHighScore2;
    private int startingHighScore3;

    private AudioSource audioSource;
    public AudioClip lineClearedSound;

    private GameObject previewTetrimino;
    private GameObject nextTetrimino;
    private GameObject savedTetrimino;
    private GameObject ghostTetrimino;

    private bool gameStarted = false;

    private Vector2 previewTetriminoPosition = new Vector2 (-6.5f, 16);
    private Vector2 savedTetriminoPosition = new Vector2 (-6.5f, 10);

    public int maxSwaps = 2;
    private int currentSwaps = 0;

    public static bool isPaused = false;
    public Canvas hudCanvas;
    public Canvas pauseCanvas;

    void Start()
    {
        currentScore = 0;
        hudScore.text = "0";
        hudLevel.text = currentLevel.ToString();
        hudLines.text = "0";
        currentLevel = startingLevel;

        SpawnNextTetrimino();

        audioSource = GetComponent<AudioSource>();

        startingHighScore = PlayerPrefs.GetInt("highscore");
        startingHighScore2 = PlayerPrefs.GetInt("highscore2");
        startingHighScore3 = PlayerPrefs.GetInt("highscore3");
    }

    void Update() 
    {
        if (hudScore != null)
        {
            UpdateScore();
            UpdateUI();
            UpdateLevel();
            UpdateSpeed();
            CheckUserInput();
        }
    }

    void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (Time.timeScale == 1)
                PauseGame();
            else
                ResumeGame();
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            GameObject tempNextTetrimino = GameObject.FindGameObjectWithTag("CurrentActiveTetrimino");
            SaveTetrimino(tempNextTetrimino.transform);
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        audioSource.Pause();
        hudCanvas.enabled = false;
        pauseCanvas.enabled = true;
        Camera.main.GetComponent<Blur>().enabled = true;
        isPaused = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
        audioSource.Play();
        hudCanvas.enabled = true;
        pauseCanvas.enabled = false;
        Camera.main.GetComponent<Blur>().enabled = false;
        isPaused = false;
    }

    void UpdateLevel()
    {
        if (startingAtLevel0 || (!startingAtLevel0 && numLinesCleared / 10 > startingLevel))
        {
            currentLevel = numLinesCleared / 10;
        }
    }

    void UpdateSpeed()
    {
        fallSpeed = 1.0f - ((float)currentLevel * 0.1f);
    }

    public void PlayLineClearedSound()
    {
        audioSource.PlayOneShot(lineClearedSound);
    }

    public void UpdateUI()
    {
        hudScore.text = currentScore.ToString();
        hudLevel.text = currentLevel.ToString();
        hudLines.text = numLinesCleared.ToString();
    }

    public void UpdateScore()
    {
        if (numberOfRowsDeleted > 0)
        {
            currentScore += PointsForLinesCleared[numberOfRowsDeleted - 1] + (currentLevel * numberOfRowsDeleted * 10);
            numLinesCleared += numberOfRowsDeleted;
            numberOfRowsDeleted = 0;
            PlayLineClearedSound();
        }
    }

    public void UpdateHighScore()
    {
        if (currentScore > startingHighScore)
        {
            PlayerPrefs.SetInt("highscore3", startingHighScore2);
            PlayerPrefs.SetInt("highscore2", startingHighScore);
            PlayerPrefs.SetInt("highscore", currentScore);
        }
        else if (currentScore > startingHighScore2)
        {
            PlayerPrefs.SetInt("highscore3", startingHighScore2);
            PlayerPrefs.SetInt("highscore2", currentScore);
        }
        else if (currentScore > startingHighScore3)
        {
            PlayerPrefs.SetInt("highscore3", currentScore);
        }
    }

    bool CheckIsValidPosition(GameObject tetrimino)
    {
        foreach (Transform mino in tetrimino.transform)
        {
            Vector2 pos = Round(mino.position);

            if (!CheckIsInsideGrid(pos))
                return false;

            if (GetTransformAtGridPosition(pos) != null && GetTransformAtGridPosition(pos).parent != tetrimino.transform)
                return false;
        }
        return true;
    }

    public bool CheckIsAboveGrid(Tetrimino tetrimino)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            foreach (Transform mino in tetrimino.transform) 
            {
                Vector2 pos = Round(mino.position);
                
                if (pos.y > gridHeight - 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsRowFullAt(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        numberOfRowsDeleted++;
        return true;
    }

    public void DeleteMinoAt(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    public void MoveRowDown(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] != null)
            {
                grid[x, y - 1] = grid[x, y];
                grid[x, y] = null;
                grid[x, y - 1].position += new Vector3(0, -1, 0); 
            }
        }
    }

    public void MoveAllRowsDown(int y)
    {
        for (int i = y; i < gridHeight; i++)
        {
            MoveRowDown(i);
        }
    }

    public void DeleteRow()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (IsRowFullAt(y))
            {
                DeleteMinoAt(y);
                MoveAllRowsDown(y + 1);

                y--;
            }
        }
    }

    public void UpdateGrid(Tetrimino tetrimino)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    if (grid[x, y].parent == tetrimino.transform)
                    {
                        grid[x, y] = null;
                    }
                }
            }
        }

        foreach (Transform mino in tetrimino.transform)
        {
            Vector2 pos = Round(mino.position);
            
            if (pos.y < gridHeight)
            {
                grid[(int)pos.x, (int)pos.y] = mino;
            }
        }
    }

    public Transform GetTransformAtGridPosition(Vector2 pos) 
    {
        if (pos.y > gridHeight - 1)
        {
            return null;
        }
        else
        {
            return grid[(int)pos.x, (int)pos.y];
        }
    }

    public void SpawnNextTetrimino()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            nextTetrimino = (GameObject)Instantiate(Resources.Load(GetRandomTetrimino(), typeof(GameObject)), new Vector2(5.0f, 20.0f), Quaternion.identity);
            previewTetrimino = (GameObject)Instantiate(Resources.Load(GetRandomTetrimino(), typeof(GameObject)), previewTetriminoPosition, Quaternion.identity);
            previewTetrimino.GetComponent<Tetrimino>().enabled = false;
            nextTetrimino.tag = "CurrentActiveTetrimino";
            SpawnGhostTetrimino();
        }
        else
        {
            previewTetrimino.transform.localPosition = new Vector2(5.0f, 20.0f);
            nextTetrimino = previewTetrimino;
            nextTetrimino.GetComponent<Tetrimino>().enabled = true;
            nextTetrimino.tag = "CurrentActiveTetrimino";

            previewTetrimino = (GameObject)Instantiate(Resources.Load(GetRandomTetrimino(), typeof(GameObject)), previewTetriminoPosition, Quaternion.identity);
            previewTetrimino.GetComponent<Tetrimino>().enabled = false;
            SpawnGhostTetrimino();
        }
        currentSwaps = 0;
    }

    public void SpawnGhostTetrimino()
    {
        if (GameObject.FindGameObjectWithTag("CurrentGhostTetrimino") != null)
            Destroy(GameObject.FindGameObjectWithTag("CurrentGhostTetrimino"));

        ghostTetrimino = (GameObject)Instantiate(nextTetrimino, nextTetrimino.transform.position, Quaternion.identity);
        Destroy(ghostTetrimino.GetComponent<Tetrimino>());
        ghostTetrimino.AddComponent<GhostTetrimino>();
    }

    public void SaveTetrimino(Transform t)
    {
        currentSwaps++;

        if (currentSwaps > maxSwaps)
            return;
        
        if (savedTetrimino != null)
        {
            // There's a tetrimino being held
            GameObject tempSavedTetrimino = GameObject.FindGameObjectWithTag("CurrentSavedTetrimino");
            tempSavedTetrimino.transform.localPosition = new Vector2(gridWidth / 2, gridHeight);

            if (!CheckIsValidPosition(tempSavedTetrimino))
            {
                tempSavedTetrimino.transform.localPosition = savedTetriminoPosition;
                return;
            }
            savedTetrimino = (GameObject)Instantiate(t.gameObject);
            savedTetrimino.GetComponent<Tetrimino>().enabled = false;
            savedTetrimino.transform.localPosition = savedTetriminoPosition;
            savedTetrimino.tag = "CurrentSavedTetrimino";

            nextTetrimino = (GameObject)Instantiate(tempSavedTetrimino);
            nextTetrimino.GetComponent<Tetrimino>().enabled = true;
            nextTetrimino.transform.localPosition = new Vector2(gridWidth / 2, gridHeight);
            nextTetrimino.tag = "CurrentActiveTetrimino";

            DestroyImmediate(t.gameObject);
            DestroyImmediate(tempSavedTetrimino);

            SpawnGhostTetrimino();
        }
        else
        {
            // There's no tetrimino being held
            savedTetrimino = (GameObject)Instantiate(GameObject.FindGameObjectWithTag("CurrentActiveTetrimino"));
            savedTetrimino.GetComponent<Tetrimino>().enabled = false;
            savedTetrimino.transform.localPosition = savedTetriminoPosition;
            savedTetrimino.tag = "CurrentSavedTetrimino";

            DestroyImmediate(GameObject.FindGameObjectWithTag("CurrentActiveTetrimino"));
            SpawnNextTetrimino();
        }
    }
    
    public bool CheckIsInsideGrid(Vector2 pos)
    {
        return ((int)pos.x >= 0 && (int)pos.x < gridWidth && (int)pos.y >= 0);
    }

    public Vector2 Round(Vector2 pos) 
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    string GetRandomTetrimino()
    {
        int randomTetrimino = Random.Range(1, 8);
        string randomTetriminoName = "TetriminoBlue";

        switch (randomTetrimino)
        {
            case 1: 
                randomTetriminoName = "TetriminoBlue";
                break;
            case 2:
                randomTetriminoName = "TetriminoGreen";
                break;
            case 3:
                randomTetriminoName = "TetriminoLightBlue";
                break;
            case 4:
                randomTetriminoName = "TetriminoOrange";
                break;
            case 5:
                randomTetriminoName = "TetriminoPurple";
                break;
            case 6:
                randomTetriminoName = "TetriminoRed";
                break;
            case 7:
                randomTetriminoName = "TetriminoYellow";
                break;
        }
        return randomTetriminoName;
    }

    public void GameOver()
    {
        UpdateHighScore();
        SceneManager.LoadScene("GameOver");
    }
}
