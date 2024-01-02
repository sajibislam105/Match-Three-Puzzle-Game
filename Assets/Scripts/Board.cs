using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int borderSize;
    
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] gamePiecesPrefabs;
    
    private Camera _camera;
    private Tile[,] m_AllTiles;
    private GamePiece[,] m_AllGamePieces;
    
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

    private void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("Board: Invalid GamePiece!");
            return;
        }
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        gamePiece.SetCoord(x, y);
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
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(),i,j);
                }
            }
        } 
    }

}
    
    
