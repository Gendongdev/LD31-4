using UnityEngine;
using System.Collections;

public enum Tiles
{
    Empty,
    Vert,
    Horiz,
    T_Vert_L,
    T_Vert_R,
    T_Horiz_D,
    T_Horiz_U,
    X,
    Point,
    L_L_U,
    L_R_U,
    L_L_D,
    L_R_D
}

public class MapController : MonoBehaviour
{
    public int MapX;
    public int MapY;
    public Tiles[,] TileArray;
    public GameObject[,] GameObjectArray;
    public Transform MapTransform;
    public GameObject[] TileSprites;

    // Use this for initialization
    void Start()
    {

    }
    
    // Update is called once per frame
    void Update()
    {
    
    }

    public bool PlaceTile(Vector2 location)
    {
        Debug.Log("Placing tile at " + location);
        int x = (int)location.x;
        int y = (int)location.y;

        // check if tile is empty
        if (TileArray[x, y] != Tiles.Empty)
        {
            Debug.Log("Tile is not empty.");
            return false;
        }

        if (x == 0 | x == MapX - 1 | y == 0 | y == MapY - 1)
        {
            Debug.Log("Can't place tile on border!");
            return false;
        }

        TileArray[x, y] = CheckNeighbours(x, y);
        UpdateTileObject(x, y);

        RefreshTile(x - 1, y);
        RefreshTile(x + 1, y);
        RefreshTile(x, y - 1);
        RefreshTile(x, y + 1);

        return true;
    }

    public bool RemoveTile(Vector2 location)
    {
        int x = (int)location.x;
        int y = (int)location.y;
        
        // check if tile is empty
        if (TileArray[x, y] == Tiles.Empty)
        {
            Debug.Log("Tile is empty.");
            return false;
        }

        if (x == 0 | x == MapX - 1 | y == 0 | y == MapY - 1)
        {
            Debug.Log("Can't place tile on border!");
            return false;
        }

        TileArray[x, y] = Tiles.Empty;
        Destroy(GameObjectArray[x, y]);
        GameObjectArray[x, y] = null;

        RefreshTile(x - 1, y);
        RefreshTile(x + 1, y);
        RefreshTile(x, y - 1);
        RefreshTile(x, y + 1);

        return true;
    }

    private void RefreshTile(int x, int y)
    {
        if (TileArray[x, y] != Tiles.Empty)
        {
            Tiles oldTile = TileArray[x, y];
            TileArray[x, y] = CheckNeighbours(x, y);
            if (TileArray[x, y] != oldTile)
            {
                UpdateTileObject(x, y);
            }
        }
    }

    private void UpdateTileObject(int x, int y)
    {
        Destroy(GameObjectArray[x, y]);
        GameObject newObject = (GameObject)Instantiate(TileSprites[(int)TileArray[x, y]], new Vector3(x, y), 
                                                       Quaternion.identity);

        Transform newTransform = newObject.GetComponent<Transform>();
        newTransform.parent = MapTransform;
        GameObjectArray[x, y] = newObject;
    }

    private Tiles CheckNeighbours(int x, int y)
    {
        if (TileArray[x - 1, y] == Tiles.Empty & TileArray[x + 1, y] == Tiles.Empty 
            & (TileArray[x, y - 1] != Tiles.Empty | TileArray[x, y + 1] != Tiles.Empty))
        {
            return Tiles.Vert;
        }
        // horiz
        else if ((TileArray[x - 1, y] != Tiles.Empty | TileArray[x + 1, y] != Tiles.Empty) 
            & TileArray[x, y - 1] == Tiles.Empty & TileArray[x, y + 1] == Tiles.Empty)
        {
            return Tiles.Horiz;
        }
        // T_Vert_L
        else if (TileArray[x - 1, y] != Tiles.Empty & TileArray[x + 1, y] == Tiles.Empty 
            & TileArray[x, y - 1] != Tiles.Empty & TileArray[x, y + 1] != Tiles.Empty)
        {
            return Tiles.T_Vert_L;
        }
        // T_Vert_R
        else if (TileArray[x - 1, y] == Tiles.Empty & TileArray[x + 1, y] != Tiles.Empty 
            & TileArray[x, y - 1] != Tiles.Empty & TileArray[x, y + 1] != Tiles.Empty)
        {
            return Tiles.T_Vert_R;
        }
        // T_Horiz_D
        else if (TileArray[x - 1, y] != Tiles.Empty & TileArray[x + 1, y] != Tiles.Empty 
            & TileArray[x, y - 1] != Tiles.Empty & TileArray[x, y + 1] == Tiles.Empty)
        {
            return Tiles.T_Horiz_D;
        }
        // T_Horiz_U
        else if (TileArray[x - 1, y] != Tiles.Empty & TileArray[x + 1, y] != Tiles.Empty 
            & TileArray[x, y - 1] == Tiles.Empty & TileArray[x, y + 1] != Tiles.Empty)
        {
            return Tiles.T_Horiz_U;
        }
        // X
        else if (TileArray[x - 1, y] != Tiles.Empty & TileArray[x + 1, y] != Tiles.Empty 
            & TileArray[x, y - 1] != Tiles.Empty & TileArray[x, y + 1] != Tiles.Empty)
        {
            return Tiles.X;
        } 
        // L_L_U
        else if (TileArray[x - 1, y] != Tiles.Empty & TileArray[x, y + 1] != Tiles.Empty)
        {
            return Tiles.L_L_U;
        }
        // L_R_U
        else if (TileArray[x + 1, y] != Tiles.Empty & TileArray[x, y + 1] != Tiles.Empty)
        {
            return Tiles.L_R_U;
        }
        // L_L_D
        else if (TileArray[x - 1, y] != Tiles.Empty & TileArray[x, y - 1] != Tiles.Empty)
        {
            return Tiles.L_L_D;
        }
        // L_R_D
        else if (TileArray[x + 1, y] != Tiles.Empty & TileArray[x, y - 1] != Tiles.Empty)
        {
            return Tiles.L_R_D;
        }
        // Point
        else
        {
            return Tiles.Point;
        }
    }
}