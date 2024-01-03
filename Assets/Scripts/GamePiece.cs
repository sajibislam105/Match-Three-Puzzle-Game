using System.Collections;
using UnityEngine;
public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    private Board _mBoard;

    private bool _mIsMoving;

    [SerializeField] private InterpType interpolation; //Set From Inspector

    private enum InterpType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    };

    public MatchValue matchValue;

    public enum MatchValue
    {
        Yellow,
        Red,
        Blue,
        Green,
        Cyan,
        Indigo,
        Magenta, 
        Teal
    }
    

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.D))
        {
            Move((int)transform.position.x + 1,(int)transform.position.y,0.5f);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Move((int)transform.position.x-1,(int)transform.position.y,0.5f);
        }*/
    }
    public void Init(Board board)
    {
        _mBoard = board;
    }
    
    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Move(int destinationX, int destinationY, float timeToMove)
    {
        if (!_mIsMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(destinationX, destinationY, 0), timeToMove));
        }
    }

    private IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination = false;
        float elapsedTime = 0f;
        _mIsMoving = true;
        
        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position,destination) < 0.01f)
            {
                reachedDestination = true;

                if (_mBoard != null)
                {
                    _mBoard.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                }
                break;
            }
            
            //tracking total running time
            elapsedTime += Time.deltaTime;
            
            //calculating lerp value
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

            switch (interpolation)
            {
                case InterpType.Linear:
                    break;
                case InterpType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;
                case InterpType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;
                case InterpType.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;
                case InterpType.SmootherStep:
                    t = t*t*t*(t*(t*6-15)+10);
                    break;
            }
            
            
            
            //moving the game piece
            transform.position = Vector3.Lerp(startPosition, destination, t);
            
            //wait until next frame
            yield return null;
        }

        _mIsMoving = false;
    }
}
