using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Tile : MonoBehaviour
{
    private Board m_board;
    [SerializeField] private int xIndex;
    [SerializeField] private int yIndex;

    public void Init(int x,int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
    }
}
