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
    Medic_Empty,
    Medic_LU,
    Medic_LM,
    Medic_LD,
    Medic_MU,
    Medic_MM,
    Medic_MD,
    Medic_RU,
    Medic_RM,
    Medic_RD,
    Built_Medic_Empty,
    Built_Medic_LU,
    Built_Medic_LM,
    Built_Medic_LD,
    Built_Medic_MU,
    Built_Medic_MM,
    Built_Medic_MD,
    Built_Medic_RU,
    Built_Medic_RM,
    Built_Medic_RD,
    Gun_Empty,
    Gun_LU,
    Gun_LM,
    Gun_LD,
    Gun_MU,
    Gun_MM,
    Gun_MD,
    Gun_RU,
    Gun_RM,
    Gun_RD,
    Built_Gun_Empty,
    Built_Gun_LU,
    Built_Gun_LM,
    Built_Gun_LD,
    Built_Gun_MU,
    Built_Gun_MM,
    Built_Gun_MD,
    Built_Gun_RU,
    Built_Gun_RM,
    Built_Gun_RD,
    Max
}

public enum BuildingType
{
    Mortar,
    Built_Mortar,
    Medic,
    Built_Medic,
    Gun,
    Built_Gun,
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
    public GameObject[] Buildings = new GameObject[(int)BuildingType.Max];
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

        for (int x = 0; x < MapX; x++)
        {
            GraphUpdateObject guo = new GraphUpdateObject(new Bounds(new Vector3(x, MapY), new Vector3(0.99f, 0.99f, 1)));
            guo.addPenalty = 1000000;
            AstarPath.active.UpdateGraphs(guo);
        }
    }

    public bool PlaceSentry(Vector2 location)
    {
        int x = (int)location.x;
        int y = (int)location.y;

        int[] location_array = new int[2] {x, y};

        if (TileHelper.IsEmpty(TileArray[x, y]) | TileHelper.IsTrench(TileArray[x, y]))
        {
            queue.Jobs.Add(new Job(location_array, JobType.Sentry, JobTime.SENTRY));
            return true;
        }

        return false;
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
            return false;
        }
        
        if (x == 0 | x == MapX - 1 | y == 0 | y == MapY - 1)
        {
            return false;
        }
        
        TileArray[x, y] = CheckNeighboursTrench(x, y);
        UpdateTileObject(x, y);
        
        RefreshTileTrench(x - 1, y);
        RefreshTileTrench(x + 1, y);
        RefreshTileTrench(x, y - 1);
        RefreshTileTrench(x, y + 1);
        
        queue.Jobs.Add(new Job(location_array, JobType.Dig_Trench, JobTime.DIG_TRENCH));
        
        // Debug.Log(queue.Jobs.Count);
        
        return true;
    }

    public bool PlaceMortar(Vector2 location)
    {
        int x = (int)location.x;
        int y = (int)location.y;
        
        int[] location_array = new int[2] {x, y};

        if (!TileHelper.IsEmpty(TileArray[x, y - 2]) & !TileHelper.IsTrench(TileArray[x, y - 2]))
            return false;

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

        RefreshTileTrench(x, y - 2);

        queue.Jobs.Add(new Job(location_array, JobType.Build_Mortar, JobTime.BUILD_MORTAR));

        GameObject new_mortar = (GameObject)Instantiate(Buildings[(int)BuildingType.Mortar], new Vector3(x, y, 1f), Quaternion.identity);
        Transform new_transform = new_mortar.GetComponent<Transform>();
        new_transform.parent = BuildingsTransform;
        return true;
    }

    public bool PlaceMedic(Vector2 location)
    {
        int x = (int)location.x;
        int y = (int)location.y;
        
        int[] location_array = new int[2] {x, y};
        
        if (!TileHelper.IsEmpty(TileArray[x, y - 2]) & !TileHelper.IsTrench(TileArray[x, y - 2]))
            return false;
        
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
        
        TileArray[x - 1, y - 1] = Tile.Medic_LD;
        TileArray[x - 1, y] = Tile.Medic_LM;
        TileArray[x - 1, y + 1] = Tile.Medic_LU;
        TileArray[x, y - 1] = Tile.Medic_MD;
        TileArray[x, y] = Tile.Medic_MM;
        TileArray[x, y + 1] = Tile.Medic_MU;
        TileArray[x + 1, y - 1] = Tile.Medic_RD;
        TileArray[x + 1, y] = Tile.Medic_RM;
        TileArray[x + 1, y + 1] = Tile.Medic_RU;
        
        for (int x_iter = x-1; x_iter == x+1; x_iter++)
        {
            for (int y_iter = y-1; y_iter == y+1; y_iter++)
            {
                UpdateTileObject(x, y);
            }
        }
        
        RefreshTileTrench(x, y - 2);
        
        queue.Jobs.Add(new Job(location_array, JobType.Build_Medic, JobTime.BUILD_MEDIC));
        
        GameObject new_medic = (GameObject)Instantiate(Buildings[(int)BuildingType.Medic], new Vector3(x, y, 1f), Quaternion.identity);
        Transform new_transform = new_medic.GetComponent<Transform>();
        new_transform.parent = BuildingsTransform;
        return true;
    }

    public bool PlaceGun(Vector2 location)
    {
        int x = (int)location.x;
        int y = (int)location.y;
        
        int[] location_array = new int[2] {x, y};
        
        if (!TileHelper.IsEmpty(TileArray[x, y - 2]) & !TileHelper.IsTrench(TileArray[x, y - 2]))
            return false;
        
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
        
        TileArray[x - 1, y - 1] = Tile.Gun_LD;
        TileArray[x - 1, y] = Tile.Gun_LM;
        TileArray[x - 1, y + 1] = Tile.Gun_LU;
        TileArray[x, y - 1] = Tile.Gun_MD;
        TileArray[x, y] = Tile.Gun_MM;
        TileArray[x, y + 1] = Tile.Gun_MU;
        TileArray[x + 1, y - 1] = Tile.Gun_RD;
        TileArray[x + 1, y] = Tile.Gun_RM;
        TileArray[x + 1, y + 1] = Tile.Gun_RU;
        
        for (int x_iter = x-1; x_iter == x+1; x_iter++)
        {
            for (int y_iter = y-1; y_iter == y+1; y_iter++)
            {
                UpdateTileObject(x, y);
            }
        }
        
        RefreshTileTrench(x, y - 2);
        
        queue.Jobs.Add(new Job(location_array, JobType.Build_Gun, JobTime.BUILD_GUN));
        
        GameObject new_gun = (GameObject)Instantiate(Buildings[(int)BuildingType.Gun], new Vector3(x, y, 1f), Quaternion.identity);
        Transform new_transform = new_gun.GetComponent<Transform>();
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
            return false;
        }
        
        if (x == 0 | x == MapX - 1 | y == 0 | y == MapY - 1)
        {
            return false;
        }
        
        TileArray[x, y] = CheckNeighboursWall(x, y);
        UpdateTileObject(x, y);
        
        RefreshTileWall(x - 1, y);
        RefreshTileWall(x + 1, y);
        RefreshTileWall(x, y - 1);
        RefreshTileWall(x, y + 1);
        
        queue.Jobs.Add(new Job(location_array, JobType.Build_Wall, JobTime.BUILD_WALL));
        
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
            return false;
        }
        
        if (TileHelper.IsBuiltWall(TileArray[x, y]) | TileHelper.IsBuiltTrench(TileArray[x, y]))
        {
            return false;
        }
        
        if (x == 0 | x == MapX - 1 | y == 0 | y == MapY - 1)
        {
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

        GameObject new_mortar = (GameObject)Instantiate(Buildings[(int)BuildingType.Built_Mortar], new Vector3(x, y, 2f), Quaternion.identity);
        Transform new_transform = new_mortar.GetComponent<Transform>();
        new_transform.parent = BuildingsTransform;

        queue.Jobs.Add(new Job(new int[] {x,y}, JobType.Fire_Mortar, JobTime.FIRE_MORTAR));
        Debug.Log(queue.Jobs.Count);
    }

    public void BuildMedic(int x, int y)
    {
        for (int x_iter = x-1; x_iter <= x+1; x_iter++)
        {
            for (int y_iter = y-1; y_iter <= y+1; y_iter++)
            {
                if (!TileHelper.IsEmpty(TileArray[x_iter, y_iter]))
                {
                    if (TileHelper.IsMedic(TileArray[x_iter, y_iter]) 
                        & !TileHelper.IsBuiltMedic(TileArray[x_iter, y_iter]))
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
        
        GameObject new_medic = (GameObject)Instantiate(Buildings[(int)BuildingType.Built_Medic], new Vector3(x, y, 2f), Quaternion.identity);
        Transform new_transform = new_medic.GetComponent<Transform>();
        new_transform.parent = BuildingsTransform;
        
        queue.Jobs.Add(new Job(new int[] {x,y}, JobType.Do_Medic, JobTime.DO_MEDIC));
        Debug.Log(queue.Jobs.Count);
    }

    public void BuildGun(int x, int y)
    {
        for (int x_iter = x-1; x_iter <= x+1; x_iter++)
        {
            for (int y_iter = y-1; y_iter <= y+1; y_iter++)
            {
                if (!TileHelper.IsEmpty(TileArray[x_iter, y_iter]))
                {
                    if (TileHelper.IsGun(TileArray[x_iter, y_iter]) 
                        & !TileHelper.IsBuiltGun(TileArray[x_iter, y_iter]))
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
        
        GameObject new_gun = (GameObject)Instantiate(Buildings[(int)BuildingType.Built_Gun], new Vector3(x, y, 2f), Quaternion.identity);
        Transform new_transform = new_gun.GetComponent<Transform>();
        new_transform.parent = BuildingsTransform;
        
        queue.Jobs.Add(new Job(new int[] {x,y}, JobType.Fire_Gun, JobTime.FIRE_GUN));
        Debug.Log(queue.Jobs.Count);
    }

    public void FireMortar(int x, int y)
    {
        Debug.Log("Firing mortar at " + x + ", " + y);
        GameObject new_player_mortar = (GameObject)Instantiate(gameController.PlayerMortarPrefab, new Vector3(x, y + 1.5f, -2f), Quaternion.identity);
        new_player_mortar.GetComponent<Transform>().SetParent(gameController.ProjectileTransform);
    }
    public void FireGun(int x, int y)
    {
        Debug.Log("Firing gun at " + x + ", " + y);
        GameObject new_player_bullet = (GameObject)Instantiate(gameController.PlayerBulletPrefab, new Vector3(x, y + 1.8f, -2f), Quaternion.identity);
        new_player_bullet.GetComponent<Transform>().SetParent(gameController.ProjectileTransform);
    }
    
    public void DoMedic(int x, int y)
    {
        Transform[] unit_list = gameController.UnitTransform.GetComponentsInChildren<Transform>();

        int heal_count = 0;
        foreach (Transform unit in unit_list)
        {
            if (gameController.UnitTransform.GetInstanceID() != unit.GetInstanceID())
            {
                float distance_to_medic = Mathf.Abs(((Vector2)unit.position - new Vector2(x, y)).magnitude);
                
                if (distance_to_medic <= 8 & distance_to_medic > 0)
                {
                    BasicUnit unit_script = unit.GetComponent<BasicUnit>();

                    bool to_heal = false;
                    // heal units that aren't doing medic jobs
                    if (unit_script != null)
                    {
                        if (unit_script.MyJob == null)
                        {
                            to_heal = true;
                        } else if (unit_script.MyJob.Type != JobType.Do_Medic)
                        {
                            to_heal = true;
                        }

                        if (to_heal)
                        {
                            heal_count++;
                            if (unit_script.HitPoints < 10)
                                unit_script.HitPoints += 1;
                        }
                    }
                }
            }
        }

        Debug.Log("Healed " + heal_count + " units.");
    }

    public void MortarHit(int hit_x, int hit_y)
    {
        Transform[] building_list = BuildingsTransform.GetComponentsInChildren<Transform>();

        bool dealt_damage = false;

        // destroy trenches and walls in 1-tile radius
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
                    } else if (TileHelper.IsBuiltMortar(TileArray[x, y]))
                    {
                        int[] offset = TileHelper.GetOffsetOfBuilding(TileArray[x, y]);
                        int mortar_x = x + offset[0];
                        int mortar_y = y + offset[1];


                        foreach (Transform building in building_list)
                        {
                            if ((Vector2)building.position == new Vector2(mortar_x, mortar_y))
                            {
                                building.GetComponent<Building>().HitPoints -= 1;
                                dealt_damage = true;
                            }
                        }

                    } else if (TileHelper.IsBuiltMedic(TileArray[x, y]))
                    {
                        int[] offset = TileHelper.GetOffsetOfBuilding(TileArray[x, y]);
                        int medic_x = x + offset[0];
                        int medic_y = y + offset[1];
                    
                    
                        foreach (Transform building in building_list)
                        {
                            if ((Vector2)building.position == new Vector2(medic_x, medic_y))
                            {
                                building.GetComponent<Building>().HitPoints -= 1;
                                dealt_damage = true;
                            }
                        }
                    
                    } else if (TileHelper.IsBuiltGun(TileArray[x, y]))
                    {
                        int[] offset = TileHelper.GetOffsetOfBuilding(TileArray[x, y]);
                        int gun_x = x + offset[0];
                        int gun_y = y + offset[1];
                        
                        
                        foreach (Transform building in building_list)
                        {
                            if ((Vector2)building.position == new Vector2(gun_x, gun_y))
                            {
                                building.GetComponent<Building>().HitPoints -= 1;
                                dealt_damage = true;
                            }
                        }
                        
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
                            dealt_damage = true;
                        } else
                        {
                            unit_script.MoralePoints = 0;
                            unit_script.HitPoints -= 5;
                            dealt_damage = true;
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

        if (dealt_damage)
        {
            gameController.audio.clip = gameController.MortarHitSound;
            gameController.audio.Play();
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

    public void BuildingDestroy(int x, int y)
    {
        for (int x_iter = x -1; x_iter <= x + 1; x_iter++)
        {
            for (int y_iter = y-1; y_iter <= y +1; y_iter++)
            {
                TileArray[x_iter, y_iter] = Tile.Empty;
                UpdateTileObject(x_iter, y_iter);
            }
        }

        RefreshTileTrench(x, y - 2);
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
        GameObject newObject = (GameObject)Instantiate(TileSprites[(int)TileArray[x, y]], new Vector3(x, y, 3f), 
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
            & (TileHelper.IsTrench(TileArray[x, y - 1]) | TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1])))
        {
            return Tile.Vert;
        }
        // horiz
        else if ((TileHelper.IsTrench(TileArray[x - 1, y]) | TileHelper.IsTrench(TileArray[x + 1, y])) 
            & !TileHelper.IsTrench(TileArray[x, y - 1]) & !TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
        {
            return Tile.Horiz;
        }
        // T_Vert_L
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & !TileHelper.IsTrench(TileArray[x + 1, y]) 
            & TileHelper.IsTrench(TileArray[x, y - 1]) & TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
        {
            return Tile.T_Vert_L;
        }
        // T_Vert_R
        else if (!TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x + 1, y]) 
            & TileHelper.IsTrench(TileArray[x, y - 1]) & TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
        {
            return Tile.T_Vert_R;
        }
        // T_Horiz_D
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x + 1, y]) 
            & TileHelper.IsTrench(TileArray[x, y - 1]) & !TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
        {
            return Tile.T_Horiz_D;
        }
        // T_Horiz_U
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x + 1, y]) 
            & !TileHelper.IsTrench(TileArray[x, y - 1]) & TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
        {
            return Tile.T_Horiz_U;
        }
        // X
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrench(TileArray[x + 1, y]) 
            & TileHelper.IsTrench(TileArray[x, y - 1]) & TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
        {
            return Tile.X;
        } 
        // L_L_U
        else if (TileHelper.IsTrench(TileArray[x - 1, y]) & TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
        {
            return Tile.L_L_U;
        }
        // L_R_U
        else if (TileHelper.IsTrench(TileArray[x + 1, y]) & TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
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
        if (TileHelper.IsTrenchOrWalkableBuilding(TileArray[x, y + 1]))
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

    public static bool IsTrenchOrWalkableBuilding(Tile tile)
    {
        return (IsTrench(tile) | IsWalkableBuilding(tile) | IsQueuedWalkableBuilding(tile));
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

    public static bool IsMedic(Tile tile)
    {
        if (tile < Tile.Medic_LU)
        {
            return false;
        } else if (tile > Tile.Built_Medic_RD)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public static bool IsBuiltMedic(Tile tile)
    {
        if (tile < Tile.Built_Medic_LU)
        {
            return false;
        } else if (tile > Tile.Built_Medic_RD)
        {
            return false;
        } else
        {
            return true;
        }
    }

    public static bool IsGun(Tile tile)
    {
        if (tile < Tile.Gun_LU)
        {
            return false;
        } else if (tile > Tile.Built_Gun_RD)
        {
            return false;
        } else
        {
            return true;
        }
    }
    
    public static bool IsBuiltGun(Tile tile)
    {
        if (tile < Tile.Built_Gun_LU)
        {
            return false;
        } else if (tile > Tile.Built_Gun_RD)
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
        } else if ((Tile.Built_Mortar_LU <= tile & tile <= Tile.Built_Mortar_MU) 
            | (Tile.Built_Medic_LU <= tile & tile <= Tile.Built_Medic_MU)
            | (Tile.Built_Gun_LU <= tile & tile <= Tile.Built_Gun_MU))
        {
            return true;
        } else if ((Tile.Built_Mortar_RU <= tile & tile <= Tile.Built_Mortar_RD)
            | (Tile.Built_Medic_RU <= tile & tile <= Tile.Built_Medic_RD)
            | (Tile.Built_Gun_RU <= tile & tile <= Tile.Built_Gun_RD))
        {
            return true;
        } else
        {
            return false;
        }
    }

    public static bool IsQueuedWalkableBuilding(Tile tile)
    {
        if (tile < Tile.Mortar_Empty)
        {
            return false;
        } else if ((Tile.Mortar_MM <= tile & tile <= Tile.Mortar_MD)
            | (Tile.Medic_MM <= tile & tile <= Tile.Medic_MD)
            | (Tile.Gun_MM <= tile & tile <= Tile.Gun_MD))
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
        } else if ((Tile.Built_Mortar_MM <= tile & tile <= Tile.Built_Mortar_MD)
            | (Tile.Built_Medic_MM <= tile & tile <= Tile.Built_Medic_MD)
            | (Tile.Built_Gun_MM <= tile & tile <= Tile.Built_Gun_MD))
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

    public static int[] GetOffsetOfBuilding(Tile tile)
    {
        int[] offset = new int[2];
        switch (tile)
        {
            case Tile.Built_Mortar_LU:
                offset = new int[] {1,-1};
                break;
            case Tile.Built_Mortar_LM:
                offset = new int[] {1,0};
                break;
            case Tile.Built_Mortar_LD:
                offset = new int[] {1,1};
                break;
            case Tile.Built_Mortar_MU:
                offset = new int[] {0,-1};
                break;
            case Tile.Built_Mortar_MM:
                offset = new int[] {0,0};
                break;
            case Tile.Built_Mortar_MD:
                offset = new int[] {0,1};
                break;
            case Tile.Built_Mortar_RU:
                offset = new int[] {-1,-1};
                break;
            case Tile.Built_Mortar_RM:
                offset = new int[] {-1,0};
                break;
            case Tile.Built_Mortar_RD:
                offset = new int[] {-1,1};
                break;

            case Tile.Built_Medic_LU:
                offset = new int[] {1,-1};
                break;
            case Tile.Built_Medic_LM:
                offset = new int[] {1,0};
                break;
            case Tile.Built_Medic_LD:
                offset = new int[] {1,1};
                break;
            case Tile.Built_Medic_MU:
                offset = new int[] {0,-1};
                break;
            case Tile.Built_Medic_MM:
                offset = new int[] {0,0};
                break;
            case Tile.Built_Medic_MD:
                offset = new int[] {0,1};
                break;
            case Tile.Built_Medic_RU:
                offset = new int[] {-1,-1};
                break;
            case Tile.Built_Medic_RM:
                offset = new int[] {-1,0};
                break;
            case Tile.Built_Medic_RD:
                offset = new int[] {-1,1};
                break;

            case Tile.Built_Gun_LU:
                offset = new int[] {1,-1};
                break;
            case Tile.Built_Gun_LM:
                offset = new int[] {1,0};
                break;
            case Tile.Built_Gun_LD:
                offset = new int[] {1,1};
                break;
            case Tile.Built_Gun_MU:
                offset = new int[] {0,-1};
                break;
            case Tile.Built_Gun_MM:
                offset = new int[] {0,0};
                break;
            case Tile.Built_Gun_MD:
                offset = new int[] {0,1};
                break;
            case Tile.Built_Gun_RU:
                offset = new int[] {-1,-1};
                break;
            case Tile.Built_Gun_RM:
                offset = new int[] {-1,0};
                break;
            case Tile.Built_Gun_RD:
                offset = new int[] {-1,1};
                break;
        }
        
        return offset;
    }
}