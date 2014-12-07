using UnityEngine;
using System.Collections;
using Pathfinding;

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
    L_R_D,
    Built_Empty,
    Built_Vert,
    Built_Horiz,
    Built_T_Vert_L,
    Built_T_Vert_R,
    Built_T_Horiz_D,
    Built_T_Horiz_U,
    Built_X,
    Built_Point,
    Built_L_L_U,
    Built_L_R_U,
    Built_L_L_D,
    Built_L_R_D,
    Max
}

public class MapController : MonoBehaviour
{
    public int MapX;
    public int MapY;
    public Tiles[,] TileArray;
    public GameObject[,] GameObjectArray;
    public Transform MapTransform;
    public GameObject[] TileSprites = new GameObject[(int)Tiles.Max];

    private JobQueue queue;
    private GameController gameController;

    // Use this for initialization
    void Start()
    {
        gameController = GameObject.Find("game_controller").GetComponent<GameController>();
        queue = GameObject.Find("game_controller").GetComponent<JobQueue>();                
    }
    
    // Update is called once per frame
    void Update()
    {
    
    }

    public void InitMap()
    {
        for (int x = 0; x < MapX; x++)
        {
            for (int y = 0; y < MapY; y++)
            {
                if (y == 0)
                {
                    TileArray[x, y] = Tiles.Built_Horiz;
                } else
                {
                    TileArray[x, y] = Tiles.Empty;
                }

                UpdateTileObject(x, y);
            }
        }
    }

    public bool PlaceTile(Vector2 location)
    {
        // Debug.Log("Placing tile at " + location);
        int x = (int)location.x;
        int y = (int)location.y;

        int[] location_array = new int[2] {x, y};

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

        queue.Jobs.Add(new Job(location_array, JobTypes.Dig_Trench, 10));

        // Debug.Log(queue.Jobs.Count);

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

        if (TileArray[x, y] >= Tiles.Built_Empty)
        {
            Debug.Log("Tile is already built!");
            return false;
        }

        if (x == 0 | x == MapX - 1 | y == 0 | y == MapY - 1)
        {
            Debug.Log("Can't place tile on border!");
            return false;
        }

        bool has_removed = false;

        foreach (Job this_job in queue.Jobs)
        {
            if (this_job.Location[0] == x & this_job.Location[1] == y)
            {
                queue.Jobs.Remove(this_job);
                has_removed = true;
            }

            if (has_removed)
                break;
        }

        if (!has_removed)
        {
            Debug.Log("Job is already in progress!");
            return false;
        }

        TileArray[x, y] = Tiles.Empty;
        UpdateTileObject(x, y);

        RefreshTile(x - 1, y);
        RefreshTile(x + 1, y);
        RefreshTile(x, y - 1);
        RefreshTile(x, y + 1);

        return true;
    }

    public void BuildTrench(int x, int y)
    {
        if (TileArray[x, y] < Tiles.Built_Empty)
        {
            TileArray[x, y] = (Tiles)((int)TileArray[x, y] + (int)Tiles.Built_Empty);
            UpdateTileObject(x, y);
        }
    }

    public void MortarHit(int hit_x, int hit_y)
    {
        // destroy build trenches in 1-tile radius
        for (int x = hit_x - 1; x <= (hit_x + 1); x++)
        {
            for (int y = hit_y - 1; y <= (hit_y + 1); y++)
            {
                try
                {
                    if (TileArray[x, y] >= Tiles.Built_Empty)
                    {
                        TileArray[x, y] = Tiles.Empty;
                        UpdateTileObject(x, y);
                    }
                } catch (System.Exception e)
                {
                    Debug.Log(TileArray + ", " + x + ", " + y);
                    throw e;
                }
            }
        }

        // update remaining tiles in 2-tile radius
        for (int x = hit_x - 2; x <= hit_x + 2; x++)
        {
            for (int y = hit_y - 2; y <= hit_y + 2; y++)
            {
                RefreshTile(x, y);
            }
        }

        // find any units in 2-tile radius and damage them
        Transform[] unit_list = gameController.UnitTransform.GetComponentsInChildren<Transform>();

        foreach (Transform unit in unit_list)
        {
            if (gameController.UnitTransform.GetInstanceID() != unit.GetInstanceID())
            {
                float distance_to_hit = Mathf.Abs(((Vector2)unit.position - new Vector2(hit_x, hit_y)).magnitude);

                if (distance_to_hit <= 2)
                {
                    BasicUnit unit_script = unit.GetComponent<BasicUnit>();
                    if (unit_script != null)
                    {
                        unit_script.MoralePoints = 0;
                        unit_script.HitPoints -= 5;
                        if (unit_script.MyJob != null)
                        {
                            unit_script.MyJob.TimeLeft += 10;
                            if (unit_script.MyJob.TimeLeft > unit_script.MyJob.BuildTime)
                            {
                                unit_script.MyJob.TimeLeft = unit_script.MyJob.BuildTime;
                            }
                        }
                    }
                }
            }
        }

    }

    private void RefreshTile(int x, int y)
    {
        if (TileArray[x, y] != Tiles.Empty)
        {
            Tiles oldTile = TileArray[x, y];
            if (y == 0)
            {
                TileArray[x, y] = CheckNeighboursBottom(x, y);
            } else
            {
                TileArray[x, y] = CheckNeighbours(x, y);
            }

            // if the old tile was a built one, this one needs to be too
            if (oldTile > Tiles.Built_Empty)
                TileArray[x, y] = (Tiles)((int)TileArray[x, y] + (int)Tiles.Built_Empty);

            if (TileArray[x, y] != oldTile)
            {
                UpdateTileObject(x, y);
            }
        }
    }

    private void UpdateTileObject(int x, int y)
    {
        Destroy(GameObjectArray[x, y]);
        GameObject newObject = (GameObject)Instantiate(TileSprites[(int)TileArray[x, y]], new Vector3(x, y, 1f), 
                                                       Quaternion.identity);

        Transform newTransform = newObject.GetComponent<Transform>();
        newTransform.parent = MapTransform;
        GameObjectArray[x, y] = newObject;

        GraphUpdateObject guo = new GraphUpdateObject(new Bounds(new Vector3(x, y), new Vector3(0.99f, 0.99f, 1)));

        if (TileArray[x, y] > Tiles.Built_Empty)
        {
            guo.addPenalty = 1;
        } else
        {
            guo.addPenalty = 30000;
        }

        AstarPath.active.UpdateGraphs(guo);


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

    private Tiles CheckNeighboursBottom(int x, int y)
    {
        if (TileArray[x, y + 1] != Tiles.Empty)
        {
            return Tiles.T_Horiz_U;
        } else
        {
            return Tiles.Horiz;
        }
    }
}