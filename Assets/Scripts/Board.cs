using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int borderSize;
    [SerializeField] private float swapTime = 0.5f;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] gamePiecesPrefabs;
    
    private Camera _camera;
    private Tile[,] m_AllTiles;
    private GamePiece[,] m_AllGamePieces;

    private Tile _mClickedTile;
    private Tile _mTargetTile;
    
    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
        m_AllTiles = new Tile[width, height];
        m_AllGamePieces = new GamePiece[width, height];
        SetupTiles();
        SetupCamera();
        FillRandom();
    }

    private void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), quaternion.identity);
                
                tile.name = $"Tile ({i},{j})";
                
                m_AllTiles[i, j] = tile.GetComponent<Tile>();
                
                tile.transform.parent = transform;
                
                m_AllTiles[i,j].Init(i,j,this);
                
            }
        }  
    }

    private void SetupCamera()
    {
        _camera.transform.position = new Vector3((width-1)/2f, (height-1)/2f, -10f);
        float aspectRatio = Screen.width / (float)Screen.height;
        float verticalSize = height/2f + borderSize;
        float horizontalSize = (width/2f + borderSize ) / aspectRatio;

        _camera.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize; // ? Operator Used
        /*if (verticalSize > horizontalSize)
        {
            camera.orthographicSize = verticalSize;
        }
        else
        {
            camera.orthographicSize = horizontalSize;
        }*/
    }

    private GameObject GetRandomPiece()
    {
        int randomIdx = Random.Range(0, gamePiecesPrefabs.Length);
        if (gamePiecesPrefabs[randomIdx] == null)
        {
            Debug.LogWarning($"Board: {randomIdx} does not contain a valid Prefab!");
        }
        return gamePiecesPrefabs[randomIdx];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("Board: Invalid GamePiece!");
            return;
        }
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x,y))
        {
            m_AllGamePieces[x, y] = gamePiece;    
        }
        gamePiece.SetCoord(x, y);
    }

    private bool IsWithinBounds(int x, int y)
    {
        //Checking is valid and on the board
        return (x >= 0 && x < width && y >= 0 && y < height);
    }
    private void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomPiece(),Vector3.zero, Quaternion.identity);
                if (randomPiece != null)
                {
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(),i,j);
                    randomPiece.transform.parent = transform; //hierarchy cleaning
                }
            }
        } 
    }

    public void ClickedTile(Tile tile)
    {
        if (_mClickedTile == null)
        {
            _mClickedTile = tile;
            //Debug.Log($"Clicked Tile {_mClickedTile}");
        }
    }

    public  void DragToTile(Tile tile)
    {
        if (_mClickedTile != null && IsNextTo(tile,_mClickedTile))
        {
            _mTargetTile = tile;
        }
    }
    public  void ReleaseTile()
    {
        if (_mClickedTile != null && _mTargetTile != null)
        {
            SwitchTiles(_mClickedTile,_mTargetTile);
        }
        
        _mClickedTile = null;
        _mTargetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = m_AllGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = m_AllGamePieces[targetTile.xIndex, targetTile.yIndex];

        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
        targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
    }

    //checking if the tile is adjacent or not
    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex-end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        if (Mathf.Abs(start.yIndex-end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }
        return false;
    }
}
    
    
