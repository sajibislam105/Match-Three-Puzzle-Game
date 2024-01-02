using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    private int xIndex;
    private int yIndex;

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
}
