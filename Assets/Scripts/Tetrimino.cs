using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrimino : MonoBehaviour
{
    float fall = 0;
    private float fallSpeed;

    public bool allowRotation = true;
    public bool limitRotation = false;

    public AudioClip landSound;

    private AudioSource audioSource;

    public int individualScore = 100;

    private float individualScoreTime;

    private float continuousVerticalSpeed = 0.05f; // Speed of tetrimino when down button held down
    private float continuousHorizontalSpeed = 0.1f; // Speed of tetrimino when left/right button held down
    private float buttonDownWaitMax = 0.2f; // How long to wait before tetrimino recognizes the button is held down
    private float verticalTimer = 0;
    private float horizontalTimer = 0;
    private float buttonDownWaitTimerHorizontal = 0;
    private float buttonDownWaitTimerVertical = 0;

    private bool moveImmediateHorizontal = false;
    private bool moveImmediateVertical = false;

    void Start() 
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!Game.isPaused)
        {
            CheckUserInput();
            UpdateIndividualScore();
            UpdateFallSpeed();
        }
    }

    void UpdateFallSpeed()
    {
        fallSpeed = Game.fallSpeed;
    }

    void UpdateIndividualScore()
    {
        if (individualScoreTime < 1)
        {
            individualScoreTime += Time.deltaTime;
        }
        else
        {
            individualScoreTime = 0;
            individualScore = Mathf.Max(individualScore - 10, 0);
        }
    }

    void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            moveImmediateHorizontal = false;
            horizontalTimer = 0;
            buttonDownWaitTimerHorizontal = 0;
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            moveImmediateVertical = false;
            verticalTimer = 0;
            buttonDownWaitTimerVertical = 0;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate();
        }

        if (Input.GetKey(KeyCode.DownArrow) || Time.time - fall >= fallSpeed)
        {
            MoveDown();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            HardDrop();
        }
    }

    void MoveLeft()
    {
        if (moveImmediateHorizontal)
        {
            if (buttonDownWaitTimerHorizontal < buttonDownWaitMax)
            {
                buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }

            if (horizontalTimer < continuousHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return;
            }
        }
        else
        {
            moveImmediateHorizontal = true;
        }

        horizontalTimer = 0;

        transform.position += new Vector3(-1, 0, 0);

        if (!CheckIsValidPosition())
        {
            transform.position += new Vector3(1, 0, 0);
        }
        else
        {
            FindObjectOfType<Game>().UpdateGrid(this);
        }
    }

    void MoveRight()
    {
        if (moveImmediateHorizontal)
        {
            if (buttonDownWaitTimerHorizontal < buttonDownWaitMax)
            {
                buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }

            if (horizontalTimer < continuousHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return;
            }
        }
        else
        {
            moveImmediateHorizontal = true;
        }

        horizontalTimer = 0;

        transform.position += new Vector3(1, 0, 0);

        if (!CheckIsValidPosition())
        {
            transform.position += new Vector3(-1, 0, 0);
        }
        else
        {
            FindObjectOfType<Game>().UpdateGrid(this);
        }
    }
    
    void MoveDown()
    {
        if (moveImmediateVertical)
        {
            if (buttonDownWaitTimerVertical < buttonDownWaitMax)
            {
                buttonDownWaitTimerVertical += Time.deltaTime;
                return;
            }

            if (verticalTimer < continuousVerticalSpeed)
            {
                verticalTimer += Time.deltaTime;
                return;
            }
        }
        else
        {
            moveImmediateVertical = true;
        }

        verticalTimer = 0;

        transform.position += new Vector3(0, -1, 0);

        fall = Time.time;

        if (!CheckIsValidPosition())
        {
            transform.position += new Vector3(0, 1, 0);
            FindObjectOfType<Game>().DeleteRow();

            // Check if there's a mino above grid
            if (FindObjectOfType<Game>().CheckIsAboveGrid(this))
            {
                FindObjectOfType<Game>().GameOver();
            }
            // Spawn next piece
            PlayLandAudio();
            FindObjectOfType<Game>().SpawnNextTetrimino();

            Game.currentScore += individualScore;
            enabled = false;

            tag = "Untagged";
        }
        else
        {
            FindObjectOfType<Game>().UpdateGrid(this);
        }
    }

    void Rotate()
    {
        if (allowRotation)
        {
            if (limitRotation)
            {
                if (transform.rotation.eulerAngles.z >= 90)
                {
                    transform.Rotate(0, 0, -90);
                }
                else 
                {
                    transform.Rotate(0, 0, 90);
                }
            }
            else
            {
                transform.Rotate(0, 0, 90);
            }
            if (!CheckIsValidPosition())
            {
                if (transform.rotation.eulerAngles.z >= 90)
                {
                    transform.Rotate(0, 0, -90);
                }
                else
                {
                    transform.Rotate(0, 0, 90);
                }
            }
            else
            {
                FindObjectOfType<Game>().UpdateGrid(this);
            }
        }
    }

    void HardDrop()
    {
        while (CheckIsValidPosition())
        {
            transform.position += new Vector3(0, -1, 0);
        }

        if (!CheckIsValidPosition())
        {
            transform.position += new Vector3(0, 1, 0);
            FindObjectOfType<Game>().UpdateGrid(this);

            FindObjectOfType<Game>().DeleteRow();

            // Check if there's a mino above grid
            if (FindObjectOfType<Game>().CheckIsAboveGrid(this))
            {
                FindObjectOfType<Game>().GameOver();
            }
            // Spawn next piece
            PlayLandAudio();
            FindObjectOfType<Game>().SpawnNextTetrimino();

            Game.currentScore += individualScore;
            enabled = false;

            tag = "Untagged";
        }
    }

    void PlayLandAudio()
    {
        audioSource.PlayOneShot(landSound);
    }

    bool CheckIsValidPosition()
    {
        foreach (Transform mino in transform)
        {
            Vector2 pos = FindObjectOfType<Game>().Round(mino.position);

            if (FindObjectOfType<Game>().CheckIsInsideGrid(pos) == false)
            {
                return false;
            }

            if (FindObjectOfType<Game>().GetTransformAtGridPosition(pos) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(pos).parent != transform)
            {
                return false;
            }
        }
        return true;
    }
}
