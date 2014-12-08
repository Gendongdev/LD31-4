using UnityEngine;
using System.Collections;
using Pathfinding;

public enum Tile
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
    Wall_Empty,
    Wall_Vert,
    Wall_Horiz,
    Wall_T_Vert_L,
    Wall_T_Vert_R,
    Wall_T_Horiz_D,
    Wall_T_Horiz_U,
    Wall_X,
    Wall_Point,
    Wall_L_L_U,
    Wall_L_R_U,
    Wall_L_L_D,
    Wall_L_R_D,
    Built_Wall_Empty,
    Built_Wall_Vert,
    Built_Wall_Horiz,
    Built_Wall_T_Vert_L,
    Built_Wall_T_Vert_R,
    Built_Wall_T_Horiz_D,
    Built_Wall_T_Horiz_U,
    Built_Wall_X,
    Built_Wall_Point,
    Built_Wall_L_L_U,
    Built_Wall_L_R_U,
    Built_Wall_L_L_D,
    Built_Wall_L_R_D,
    Mortar_Empty,
    Mortar_LU,
    Mortar_LM,
    Mortar_LD,
    Mortar_MU,
    Mortar_MM,
    Mortar_MD,
    Mortar_RU,
    Mortar_RM,
    Mortar_RD,
    Built_Mortar_Empty,
    Built_Mortar_LU,
    Built_Mortar_LM,
    Built_Mortar_LD,
    Built_Mortar_MU,
    Built_Mortar_MM,
    Built_Mortar_MD,
    Built_Mortar_RU,
    Built_Mortar_RM,
    Built_Mortar_RD,
    Max
}

public enum Building
{
    Mortar,
    Built_Mortar,
    Max
}

public class MapController : MonoBehaviour
{
    public int MapX;
    public int MapY;
    public Tile[,] TileArray;
    public GameObject[,] GameObjectArray;
    public Transform MapTransform;
    public Transform BuildingsTransform;
    public GameObject[] TileSprites = new GameObject[(int)Tile.Max];

    public GameObject[] Buildings = new GameObject[(int)Building.Max];

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
                    TileArray[x, y] = Tile.Built_Horiz;
                } else
                {
                    TileArray[x, y] = Tile.Empty;
                }
                
                UpdateTileObject(x, y);
            }
        }
    }
    
    public bool PlaceTrenchTile(Vector2 location)
    {
        // Debug.Log("Placing tile at " + location);
        int x = (int)location.x;
        int y = (int)location.y;
        
        int[] location_array = new int[2] {x, y};
        
        // check if tile is empty
        if (!TileHelper.IsEmpty(TileArray[x, y]))
        {
            Debug.Log("Tile is not empty.");
            return false;
        }
        
        if (x == 0 | x == MapX - 1 | y == 0 | y == MapY - 1)
        {
            Debug.Log("Can't place tile on border!");
            return false;
        }
        
        TileArray[x, y] = CheckNeighboursTrench(x, y);
        UpdateTileObject(x, y);
        
        RefreshTileTrench(x - 1, y);
        RefreshTileTrench(x + 1, y);
        RefreshTileTrench(x, y - 1);
        RefreshTileTrench(x, y + 1);
        
        queue.Jobs.Add(new Job(location_array, JobType.Dig_Trench, 10));
        
        // Debug.Log(queue.Jobs.Count);
        
        return true;
    }

    public bool PlaceMortar(Vector2 location)
    {
        int x = (int)location.x;
        int y = (int)location.y;
        
        int[] location_array = new int[2] {x, y};

        for (int x_iter = x-1; x_iter <= x+1; x_iter++)
        {
            for (int y_iter = y-1; y_iter <= y+1; y_iter++)
            {
                if (!TileHelper.IsEmpty(TileArray[x_iter, y_iter]))
                {
                    return false;
                }
            }
        }

        TileArray[x - 1, y - 1] = Tile.Mortar_LD;
        TileArray[x - 1, y] = Tile.Mortar_LM;
        TileArray[x - 1, y + 1] = Tile.Mortar_LU;
        TileArray[x, y - 1] = Tile.Mortar_MD;
        TileArray[x, y] = Tile.Mortar_MM;
        TileArray[x, y + 1] = Tile.Mortar_MU;
        TileArray[x + 1, y - 1] = Tile.Mortar_RD;
        TileArray[x + 1, y] = Tile.Mortar_RM;
        TileArray[x + 1, y + 1] = Tile.Mortar_RU;

        for (int x_iter = x-1; x_iter == x+1; x_iter++)
        {
            for (int y_iter = y-1; y_iter == y+1; y_iter++)
            {
                UpdateTileObject(x, y);
            }
        }

        RefreshTileTrench(x, y - 1);

        queue.Jobs.Add(new Job(location_array, JobType.Build_Mortar, 30));

        GameObject new_mortar = (GameObject)Instantiate(Buildings[(int)Building.Mortar], new Vector2(x, y), Quaternion.identity);
        Transform new_transform = new_mortar.GetComponent<Transform>();
        new_transform.parent = BuildingsTransform;
        return true;
    }

    public bool PlaceWallTile(Vector2 location)
    {
        // Debug.Log("Placing tile at " + location);
        int x = (int)location.x;
        int y = (int)location.y;
        
        int[] location_array = new int[2] {x, y};
        
        // check if tile is empty
        if (!TileHelper.IsEmpty(TileArray[x, y]))
        {
            Debug.Log("Tile is not empty.");
            return false;
        }
        
        if (x == 0 | x == MapX - 1 | y == 0 | y == MapY - 1)
        {
            Debug.Log("Can't place tile on border!");
            return false;
        }
        
        TileArray[x, y] = CheckNeighboursWall(x, y);
        UpdateTileObject(x, y);
        
        RefreshTileWall(x - 1, y);
        RefreshTileWall(x + 1, y);
        RefreshTileWall(x, y - 1);
        RefreshTileWall(x, y + 1);
        
        queue.Jobs.Add(new Job(location_array, JobType.Build_Wall, 20));
        
        // Debug.Log(queue.Jobs.Count);
        
        return true;
    }
    
    public bool RemoveTile(Vector2 location)
    {
        int x = (int)location.x;
        int y = (int)location.y;
        
        // check if tile is empty
        if (TileHelper.IsEmpty(TileArray[x, y]))
        {
            Debug.Log("Tile is empty.");
            return false;
        }
        
        if (TileHelper.IsBuiltWall(TileArray[x, y]) | TileHelper.IsBuiltTrench(TileArray[x, y]))
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
        
        TileArray[x, y] = Tile.Empty;
        UpdateTileObject(x, y);
        
        RefreshTileTrench(x - 1, y);
        RefreshTileTrench(x + 1, y);
        RefreshTileTrench(x, y - 1);
        RefreshTileTrench(x, y + 1);

        RefreshTileWall(x - 1, y);
        RefreshTileWall(x + 1, y);
        RefreshTileWall(x, y - 1);
        RefreshTileWall(x, y + 1);
        
        return true;
    }
    
    public void BuildTrench(int x, int y)
    {
        if (TileArray[x, y] < Tile.Built_Empty)
        {
            TileArray[x, y] = (Tile)((int)TileArray[x, y] + (int)Tile.Built_Empty);
            UpdateTileObject(x, y);
        }
    }

    public void BuildWall(int x, int y)
    {
        if (TileArray[x, y] < Tile.Built_Wall_Empty)
        {
            TileArray[x, y] = (Tile)((int)TileArray[x, y] + (int)Tile.Built_Empty);
            UpdateTileObject(x, y);
        }
    }

    public void BuildMortar(int x, int y)
    {
        for (int x_iter = x-1; x_iter <= x+1; x_iter++)
        {
            for (int y_iter = y-1; y_iter <= y+1; y_iter++)
            {
                if (!TileHelper.IsEmpty(TileArray[x_iter, y_iter]))
                {
                    if (TileHelper.IsMortar(TileArray[x_iter, y_iter]) 
                        & !TileHelper.IsBuiltMortar(TileArray[x_iter, y_iter]))
                    {
                        TileArray[x_iter, y_iter] = (Tile)((int)TileArray[x_iter, y_iter] + 10); // magic number. bad boy!
                        UpdateTileObject(x_iter, y_iter);
                    }
                }
            }
        }

        foreach (Transform building in BuildingsTransform.GetComponentsInChildren<Transform>())
        {
            if (building.position.x == x & building.position.y == y)
            {
                Destroy(building.gameObject);
            }
        }

        GameObject new_mortar = (GameObject)Instantiate(Buildings[(int)Building.Built_Mortar], new Vector2(x, y), Quaternion.identity);
        Transform new_transform = new_mortar.GetComponent<Transform>();
        new_transform.parent = BuildingsTransform;
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
                    if (TileHelper.IsBuiltTrench(TileArray[x, y]))
                    {
                        TileArray[x, y] = Tile.Empty;
                        UpdateTileObject(x, y);
                    } else if (TileHelper.IsBuiltWall(TileArray[x, y]))
                    {
                        GameObjectArray[x, y].GetComponent<Wall>().HitPoints -= 5;
                    }
                } catch (System.Exception e)
                {
                    Debug.Log(TileArray + ", " + x + ", " + y + ", " + TileArray[x, y]);
                    throw e;
                }
            }
        }
        
        // update remaining tiles in 2-tile radius
        for (int x = hit_x - 2; x <= hit_x + 2; x++)
        {
            for (int y = hit_y - 2; y <= hit_y + 2; y++)
            {
                RefreshTileTrench(x, y);
                RefreshTileWall(x, y);
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
                        if (unit_script.inTrench)
                        {
                            unit_script.MoralePoints -= 2;
                            unit_script.HitPoints -= 2;
                        } else
                        {
                            unit_script.MoralePoints = 0;
                            unit_script.HitPoints -= 5;
                        }

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

    public void WallDestroy(int x, int y)
    {
        TileArray[x, y] = Tile.Empty;
        UpdateTileObject(x, y);

        RefreshTileWall(x - 1, y);
        RefreshTileWall(x + 1, y);
        RefreshTileWall(x, y - 1);
        RefreshTileWall(x, y + 1);

    }
    private void UpdateTileObject(int x, int y)
    {
        int hit_points = int.MinValue;

        if (TileHelper.IsBuiltWall(TileArray[x, y]))
        {
            Wall old_wall = GameObjectArray[x, y].GetComponent<Wall>();
            if (old_wall != null)
            {
                hit_points = old_wall.HitPoints;
            }
        }

        Destroy(GameObjectArray[x, y]);
        GameObject newObject = (GameObject)Instantiate(TileSprites[(int)TileArray[x, y]], new Vector3(x, y, 1f), 
                                                       Quaternion.identity);
        
        Transform newTransform = newObject.GetComponent<Transform>();
        newTransform.parent = MapTransform;
        GameObjectArray[x, y] = newObject;

        // preserve hit points
        if (TileHelper.IsBuiltWall(TileArray[x, y]))
        {
            if (hit_points > int.MinValue)
            {
                newObject.GetComponent<Wall>().HitPoints = hit_points;
            }
        }

        // pathfinder business

        GraphUpdateObject guo = new GraphUpdateObject(new Bounds(new Vector3(x, y), new Vector3(0.99f, 0.99f, 1)));

        // process penalties

        if (TileHelper.IsBuiltTrench(TileArray[x, y]))
        {
            guo.addPenalty = 1;
        } else if (TileHelper.IsBuiltWall(TileArray[x, y]))
        {
            guo.modifyWalkability = true;
            guo.setWalkability = false;
        } else if (TileHelper.IsUnpassableBuilding(TileArray[x, y]))
        {
            guo.modifyWalkability = true;
            guo.setWalkability = false;
        } else if (TileHelper.IsWalkableBuilding(TileArray[x, y]))
        {
            guo.addPenalty = 1;
        } else
        {
            guo.addPenalty = 30000;
        }
        
        AstarPath.active.UpdateGraphs(guo);

    }

    private void RefreshTileTrench(int x, int y)
    {
        if (TileHelper.IsTrench(TileArray[x, y]))
        {
            Tile oldTile = TileArray[x, y];
            if (y == 0)
            {
                TileArray[x, y] = CheckNeighboursTrenchBottom(x, y);
            } else
            {
                TileArray[x, y] = CheckNeighboursTrench(x, y);
            }
            
            // if the old tile was a built one, this one needs to be too
            if (oldTile > Tile.Built_Empty)
                TileArray[x, y] = (Tile)((int)TileArray[x, y] + (int)Tile.Built_Empty);
            
            if (TileArray[x, y] != oldTile)
            {
                UpdateTileObject(x, y);
            }
        }
    }

    private void RefreshTileWall(int x, int y)
    {
        if (TileHelper.IsWall(TileArray[x, y]))
        {
            Tile oldTile = TileArray[x, y];
            {
                TileArray[x, y] = CheckNeighboursWall(x, y);
            }
            
            // if the old tile was a built one, this one needs to be too
            if (oldTile > Tile.Built_Wall_Empty)
                TileArray[x, y] = (Tile)((int)TileArray[x, y] + (int)Tile.Built_Empty);
            
            if (TileArray[x, y] != oldTile)
            {
                UpdateTileObject(x, y);
            }
        }
    }
    
    private Tile CheckNeighboursTrench(int x, int y)
    {
        if (!TileHelper.IsTrench(TileArray[x - 1, y]) & !TileHelper.IsTrench(TileArray[x + 1, y])
            & (TileHelper.IsTrench(TileArray[x, y - 1]) | TileHelper.IsTrench(TileArray[x, y + 1])))
        {
            return Tile.Vert;
        }
        // horiz
        else if ((TileHelper.IsTrench(TileArray[x - 1, y]) | TileHelper.IsTrench(TileArray[x + 1, y])) 
            & !TileHelper.IsTrench(TileArray[x, y - 1]) & !TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.Horiz;
        }
        // T_Vert_L
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & !TileHelper.IsTrench(TileArray[x + 1, y]) 
            & TileHelper.IsTrench(TileArray[x, y - 1]) & TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.T_Vert_L;
        }
        // T_Vert_R
        else if (!TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x + 1, y]) 
            & TileHelper.IsTrench(TileArray[x, y - 1]) & TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.T_Vert_R;
        }
        // T_Horiz_D
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x + 1, y]) 
            & TileHelper.IsTrench(TileArray[x, y - 1]) & !TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.T_Horiz_D;
        }
        // T_Horiz_U
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x + 1, y]) 
            & !TileHelper.IsTrench(TileArray[x, y - 1]) & TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.T_Horiz_U;
        }
        // X
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x + 1, y]) 
            & TileHelper.IsTrench(TileArray[x, y - 1]) & TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.X;
        } 
        // L_L_U
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.L_L_U;
        }
        // L_R_U
        else if (TileHelper.IsTrench(TileArray[x + 1, y]) & TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.L_R_U;
        }
        // L_L_D
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x, y - 1]))
        {
            return Tile.L_L_D;
        }
        // L_R_D
        else if (TileHelper.IsTrench(TileArray[x + 1, y]) & TileHelper.IsTrench(TileArray[x, y - 1]))
        {
            return Tile.L_R_D;
        }
        // Point
        else
        {
            return Tile.Point;
        }
    }
    
    private Tile CheckNeighboursTrenchBottom(int x, int y)
    {
        if (TileHelper.IsTrench(TileArray[x, y + 1]))
        {
            return Tile.T_Horiz_U;
        } else
        {
            return Tile.Horiz;
        }
    }

    private Tile CheckNeighboursWall(int x, int y)
    {
        if (!TileHelper.IsWall(TileArray[x - 1, y]) & !TileHelper.IsWall(TileArray[x + 1, y])
            & (TileHelper.IsWall(TileArray[x, y - 1]) | TileHelper.IsWall(TileArray[x, y + 1])))
        {
            return Tile.Wall_Vert;
        }
        // horiz
        else if ((TileHelper.IsWall(TileArray[x - 1, y]) | TileHelper.IsWall(TileArray[x + 1, y])) 
            & !TileHelper.IsWall(TileArray[x, y - 1]) & !TileHelper.IsWall(TileArray[x, y + 1]))
        {
            return Tile.Wall_Horiz;
        }
        // T_Vert_L
        else if (TileHelper.IsWall(TileArray[x - 1, y]) & !TileHelper.IsWall(TileArray[x + 1, y]) 
            & TileHelper.IsWall(TileArray[x, y - 1]) & TileHelper.IsWall(TileArray[x, y + 1]))
        {
            return Tile.Wall_T_Vert_L;
        }
        // T_Vert_R
        else if (!TileHelper.IsWall(TileArray[x - 1, y]) & TileHelper.IsWall(TileArray[x + 1, y]) 
            & TileHelper.IsWall(TileArray[x, y - 1]) & TileHelper.IsWall(TileArray[x, y + 1]))
        {
            return Tile.Wall_T_Vert_R;
        }
        // T_Horiz_D
        else if (TileHelper.IsWall(TileArray[x - 1, y]) & TileHelper.IsWall(TileArray[x + 1, y]) 
            & TileHelper.IsWall(TileArray[x, y - 1]) & !TileHelper.IsWall(TileArray[x, y + 1]))
        {
            return Tile.Wall_T_Horiz_D;
        }
        // T_Horiz_U
        else if (TileHelper.IsWall(TileArray[x - 1, y]) & TileHelper.IsWall(TileArray[x + 1, y]) 
            & !TileHelper.IsWall(TileArray[x, y - 1]) & TileHelper.IsWall(TileArray[x, y + 1]))
        {
            return Tile.Wall_T_Horiz_U;
        }
        // X
        else if (TileHelper.IsWall(TileArray[x - 1, y]) & TileHelper.IsWall(TileArray[x + 1, y]) 
            & TileHelper.IsWall(TileArray[x, y - 1]) & TileHelper.IsWall(TileArray[x, y + 1]))
        {
            return Tile.Wall_X;
        } 
        // L_L_U
        else if (TileHelper.IsWall(TileArray[x - 1, y]) & TileHelper.IsWall(TileArray[x, y + 1]))
        {
            return Tile.Wall_L_L_U;
        }
        // L_R_U
        else if (TileHelper.IsWall(TileArray[x + 1, y]) & TileHelper.IsWall(TileArray[x, y + 1]))
        {
            return Tile.Wall_L_R_U;
        }
        // L_L_D
        else if (TileHelper.IsWall(TileArray[x - 1, y]) & TileHelper.IsWall(TileArray[x, y - 1]))
        {
            return Tile.Wall_L_L_D;
        }
        // L_R_D
        else if (TileHelper.IsWall(TileArray[x + 1, y]) & TileHelper.IsWall(TileArray[x, y - 1]))
        {
            return Tile.Wall_L_R_D;
        }
        // Point
        else
        {
            return Tile.Wall_Point;
        }
    }
}

public class TileHelper : MonoBehaviour
{
    public static bool IsTrench(Tile tile)
    {
        if (tile == Tile.Empty)
        {
            return false;
        } else if (tile > Tile.Built_L_R_D)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public static bool IsBuiltTrench(Tile tile)
    {
        if (tile < Tile.Built_Empty)
        {
            return false;
        } else if (tile >= Tile.Wall_Empty)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public static bool IsWall(Tile tile)
    {
        if (tile < Tile.Wall_Empty)
        {
            return false;
        } else if (tile > Tile.Built_Wall_L_R_D)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public static bool IsBuiltWall(Tile tile)
    {
        if (tile < Tile.Built_Wall_Empty)
        {
            return false;
        } else if (tile > Tile.Built_Wall_L_R_D)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public static bool IsMortar(Tile tile)
    {
        if (tile < Tile.Mortar_LU)
        {
            return false;
        } else if (tile > Tile.Built_Mortar_RD)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public static bool IsBuiltMortar(Tile tile)
    {
        if (tile < Tile.Built_Mortar_LU)
        {
            return false;
        } else if (tile > Tile.Built_Mortar_RD)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public static bool IsUnpassableBuilding(Tile tile)
    {
        if (tile < Tile.Mortar_Empty)
        {
            return false;
        } else if (Tile.Built_Mortar_LU <= tile & tile <= Tile.Built_Mortar_MU)
        {
            return true;
        } else if (Tile.Built_Mortar_RU <= tile & tile <= Tile.Built_Mortar_RD)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public static bool IsWalkableBuilding(Tile tile)
    {
        if (tile < Tile.Mortar_Empty)
        {
            return false;
        } else if (Tile.Built_Mortar_MM <= tile & tile <= Tile.Built_Mortar_MD)
        {
            return true;
        } else
        {
            return false;
        }
    }
    public static bool IsEmpty(Tile tile)
    {
        if (tile == Tile.Empty)
        {
            return true;
        } else
        {
            return false;
        }
    }
}