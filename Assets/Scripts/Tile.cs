using UnityEngine;
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    private Board _mBoard;

    public void Init(int x,int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        _mBoard = board;
    }

    private void OnMouseDown()
    {
        if (_mBoard != null)
        {
            _mBoard.ClickedTile(this);
        }
    }

    private void OnMouseEnter()
    {
        if (_mBoard != null)
        {
            _mBoard.DragToTile(this);
        }
    }

    private void OnMouseUp()
    {
        if (_mBoard != null)
        {
            _mBoard.ReleaseTile();
        }
    }
}
