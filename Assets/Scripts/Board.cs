using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Tile[,] _mAllTiles;
    private GamePiece[,] _mAllGamePieces;

    private Tile _mClickedTile;
    private Tile _mTargetTile;
    
    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
        _mAllTiles = new Tile[width, height];
        _mAllGamePieces = new GamePiece[width, height];
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
                
                _mAllTiles[i, j] = tile.GetComponent<Tile>();
                
                tile.transform.parent = transform;
                
                _mAllTiles[i,j].Init(i,j,this);
                
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
            _mAllGamePieces[x, y] = gamePiece;    
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
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    private IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = _mAllGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = _mAllGamePieces[targetTile.xIndex, targetTile.yIndex];

        if (targetPiece != null && clickedPiece != null)
        {
            clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

            yield return new WaitForSeconds(swapTime);

            List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

            if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
            {
                Debug.Log("No Matches");
                clickedPiece.Move(clickedTile.xIndex, clickedPiece.yIndex,swapTime);
                targetPiece.Move(targetTile.xIndex, targetTile.yIndex,swapTime);
            }
            else
            {
                //destroying the matching pieces
                yield return new WaitForSeconds(swapTime);
                ClearPieceAt(clickedPieceMatches);
                ClearPieceAt(targetPieceMatches);
                //HighLightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                //HighLightMatchesAt(targetTile.xIndex, targetTile.yIndex);
            }
        }
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

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;
        
        if (IsWithinBounds(startX,startY))
        {
            startPiece = _mAllGamePieces[startX, startY];
        } 

        if ( startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }
        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + Mathf.Clamp((int)searchDirection.x,-1,1 )* i;
            nextY = startY + Mathf.Clamp((int)searchDirection.y,-1,1 )* i;

            if (!IsWithinBounds(nextX,nextY))
            {
                //search done, no more search required
                break;
            }

            GamePiece nextPiece = _mAllGamePieces[nextX, nextY];
            if (nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }

        if (matches.Count >= minLength)
        {
            return matches;
        }
        return null;
    }
    private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);

        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }

        if (vertMatches == null)
        {
            vertMatches = new List<GamePiece>();
        }

        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }
    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }
        
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }
        
        /*foreach (GamePiece piece in downwardMatches)
        {
            //getting all the pieces in one List
            if (!upwardMatches.Contains(piece))
            {
                upwardMatches.Add(piece);
            }
        }
        return (upwardMatches.Count >= minLength) ? upwardMatches : null;*/
        //union is a great way to combine list without doubling/duplicating object.
        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        
        return (combinedMatches.Count >= minLength) ? combinedMatches:null;
    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> rightwardMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftwardMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);
        
        if (rightwardMatches == null)
        {
            rightwardMatches = new List<GamePiece>();
        }
        if (leftwardMatches == null)
        {
            leftwardMatches = new List<GamePiece>();
        }
        //union is a great way to combine list without doubling/duplicating object.
        var combinedMatches = rightwardMatches.Union(leftwardMatches).ToList();
        
        return (combinedMatches.Count >= minLength) ? combinedMatches:null;
    }

    void HighLightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = _mAllTiles[x, y].GetComponent<SpriteRenderer>();
        //changing opacity to '0'
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }
    void HighLightTileOn(int x, int y,Color color)
    {
        SpriteRenderer spriteRenderer = _mAllTiles[x,y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }
    private void HighLightMatchesAt(int x, int y)
    {
        HighLightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighLightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    void HighLightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighLightMatchesAt(i, j);
            }
        }
    }

    void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = _mAllGamePieces[x, y];
        if (pieceToClear != null)
        {
            _mAllGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        HighLightTileOff(x,y);
    }

    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i,j);
            }
        }
    }

    void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            ClearPieceAt(piece.xIndex,piece.yIndex);
        }
    }
}
    
    
