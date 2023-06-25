using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostTetrimino : MonoBehaviour
{
    void Start()
    {
        tag = "CurrentGhostTetrimino";

        foreach (Transform mino in transform)
        {
            mino.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .2f);
        }
    }

    void Update()
    {
        FollowActiveTetrimino();
        MoveDown();
    }

    void FollowActiveTetrimino()
    {
        Transform CurrentActiveTetriminoTransform = GameObject.FindGameObjectWithTag("CurrentActiveTetrimino").transform;
        transform.position = CurrentActiveTetriminoTransform.position;
        transform.rotation = CurrentActiveTetriminoTransform.rotation;
    }

    void MoveDown()
    {
        while (CheckIsValidPosition())
        {
            transform.position += new Vector3(0, -1, 0);
        }

        if (!CheckIsValidPosition())
        {
            transform.position += new Vector3(0, 1, 0);
        }
    }

    bool CheckIsValidPosition()
    {
        foreach (Transform mino in transform) 
        {
            Vector2 pos = FindObjectOfType<Game>().Round(mino.position);

            if (!FindObjectOfType<Game>().CheckIsInsideGrid(pos))
                return false;
            
            if (FindObjectOfType<Game>().GetTransformAtGridPosition(pos) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(pos).parent.tag == "CurrentActiveTetrimino")
                return true;
            
            if (FindObjectOfType<Game>().GetTransformAtGridPosition(pos) != null && FindObjectOfType<Game>().GetTransformAtGridPosition(pos).parent != transform)
                return false;
        }
        return true;
    }
}
