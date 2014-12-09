using UnityEngine;
using System.Collections;

public enum ControlMode
{
    Dig,
    Gun,
    Mortar,
    Wall,
    Sentry,
    Medic
}

public class GameController : MonoBehaviour
{

    public Camera LevelCamera;
    public GameObject SelectionBox;
    public int MapX = 34;
    public int MapY = 34;
    public GameObject SoldierPrefab;
    public GameObject EnemyBulletPrefab;
    public GameObject EnemyMortarPrefab;
    public GameObject PlayerBulletPrefab;
    public GameObject PlayerMortarPrefab;
    public Transform UnitTransform;
    public Transform ProjectileTransform;
    public float ReinforcementsTime = 20f;
    public float EnemyGunRate = 0.2f;
    public float EnemyMortarRate = 0.5f;
    public int FirstEnemyGunTime = 1;
    public int FirstEnemyMortarTime = 1;
    public float TimeBetweenEnemyMortarWave = 6;
    public float TimeBetweenEnemyGunWave = 5;
    public float DurationEnemyMortarWave = 6;
    public float DurationEnemyGunWave = 5;
    public int SuppressedFireChance = 80;
    public ControlMode Mode = ControlMode.Dig;
    public Transform[] Buttons;
    public UnityEngine.UI.Text StatusText;
    public Sprite OneTileSelectionBox;
    public Sprite ThreeTileSelectionBox;
    public float ScoreReduceTime = 5f;
    public int VictoryScore;
    public bool IsRunning = true;

    public bool HasWon = false;
    public bool HasLost = false;

    public AudioClip ReinforceSound;
    public AudioClip MortarHitSound;
    public AudioClip BulletHitSound;
    public AudioClip PlaceFailSound;

    public GameObject LosePanel;
    public GameObject WinPanel;
    public GameObject StartPanel;
    public UnityEngine.UI.Text ControlText;

    private MapController mapController;
    private float nextReinforcement;
    private float nextEnemyGun;
    private float nextEnemyMortar;
    private float nextEnemyGunStateChange;
    private float nextEnemyMortarStateChange;
    private bool enemyMortarFire;
    private bool enemyGunFire;
    private float[] fireSuppression;
    private JobQueue queue;
    private int score;
    private float nextScoreReduction;





    // Use this for initialization
    void Start()
    {
        mapController = GameObject.Find("map").GetComponent<MapController>();
        mapController.MapX = MapX;
        mapController.MapY = MapY;
        mapController.TileArray = new Tile[MapX, MapY];
        mapController.GameObjectArray = new GameObject[MapX, MapY];
        mapController.InitMap();

        queue = GameObject.Find("game_controller").GetComponent<JobQueue>();

        fireSuppression = new float[MapX];
        for (int i = 0; i < MapX; i++)
        {
            fireSuppression[i] = 0;
        }

        score = 0;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (IsRunning)
        {
            ProcessMouse();

            if (Time.time >= nextReinforcement)
            {
                AddSoldier();
                nextReinforcement = Time.time + ReinforcementsTime;
                audio.clip = ReinforceSound;
                audio.Play();
            }

            EnemyStateChanges();
            EnemyFire();

            if (Time.time >= nextScoreReduction)
            {
                if (score > 0)
                {
                    score -= 1;
                    Debug.Log("Score reduced. Score: " + score);
                }
                nextScoreReduction = Time.time + ScoreReduceTime;
            }

            ControlText.text = "Current Tool:\n" + Mode + "\n\n" + (VictoryScore - score) + " Troops\nto Victory";
            if (!HasWon & !HasLost)
            {
                HasWon = CheckWin();
                if (!HasWon)
                    HasLost = CheckLose();
            } else
            {
                IsRunning = false;
            }
        } else
        {
            if (HasLost)
            {
                LosePanel.SetActive(true);
            }
            if (HasWon)
            {
                WinPanel.SetActive(true);
            }
        }
    }

    void EnemyStateChanges()
    {
        if (Time.time > nextEnemyGunStateChange)
        {
            if (enemyGunFire)
            {
                nextEnemyGunStateChange = Time.time + TimeBetweenEnemyGunWave;
                enemyGunFire = false;
                TimeBetweenEnemyGunWave = TimeBetweenEnemyGunWave * Random.Range(0.9f, 1.0f);
            } else
            {
                nextEnemyGunStateChange = Time.time + DurationEnemyGunWave;
                enemyGunFire = true;
                nextEnemyGun = Time.time + EnemyGunRate;
                DurationEnemyGunWave = DurationEnemyGunWave * Random.Range(1.0f, 1.1f);
            }
        }
        if (Time.time > nextEnemyMortarStateChange)
        {
            if (enemyMortarFire)
            {
                nextEnemyMortarStateChange = Time.time + TimeBetweenEnemyMortarWave;
                enemyMortarFire = false;
                TimeBetweenEnemyMortarWave = TimeBetweenEnemyMortarWave * Random.Range(0.9f, 1.0f);
            } else
            {
                nextEnemyMortarStateChange = Time.time + DurationEnemyMortarWave;
                enemyMortarFire = true;
                nextEnemyMortar = Time.time + EnemyMortarRate;
                DurationEnemyMortarWave = DurationEnemyMortarWave * Random.Range(1.0f, 1.1f);
            }
        }

        if (enemyGunFire & enemyMortarFire)
        {
            StatusText.text = "Under heavy fire!";
        } else if (enemyGunFire)
        {
            StatusText.text = "Incoming machine gun fire.";
        } else if (enemyMortarFire)
        {
            StatusText.text = "Incoming mortar shells.";
        } else
        {
            StatusText.text = "All clear!";
        }
    }

    void EnemyFire()
    {
        if (enemyGunFire & Time.time > nextEnemyGun)
        {
            AddEnemyBullet();
            nextEnemyGun = Time.time + EnemyGunRate;
        }
        if (enemyMortarFire & Time.time > nextEnemyMortar)
        {
            AddEnemyMortar();
            nextEnemyMortar = Time.time + EnemyMortarRate;
        }
    }

    void ProcessMouse()
    {
        bool place_success = true;

        Vector2 p = LevelCamera.ScreenToWorldPoint(Input.mousePosition);
        p.x = Mathf.Floor(p.x);
        p.y = Mathf.Floor(p.y);

        // control valid positions of selection box based on job mode
        if (((Mode == ControlMode.Dig | Mode == ControlMode.Wall) 
            & (p.x < 1 | p.x >= MapX - 1 | p.y < 1 | p.y >= MapY - 1))
            | ((Mode == ControlMode.Mortar | Mode == ControlMode.Medic | Mode == ControlMode.Gun) & (p.x < 2 | p.x >= MapX - 2 | p.y < 2 | p.y >= MapY - 2))
            | ((Mode == ControlMode.Sentry) & (p.x < 0 | p.x >= MapX | p.y < 0 | p.y >= MapY)))
        {
            SelectionBox.SetActive(false);
        } else
        {
            SelectionBox.SetActive(true);
            SelectionBox.transform.position = p;
        }
       
        // process inputs that can be dragged
        if (Input.GetMouseButton(0) & SelectionBox.activeSelf)
        {
            switch (Mode)
            {
                case ControlMode.Dig:
                    mapController.PlaceTrenchTile(p);
                    break;
                case ControlMode.Wall:
                    mapController.PlaceWallTile(p);
                    break;
            }
        }

        // process inputs that are just clicks
        if (Input.GetMouseButtonDown(0) & SelectionBox.activeSelf)
        {
            switch (Mode)
            {
                case ControlMode.Sentry:
                    place_success = mapController.PlaceSentry(p);
                    break;
                case ControlMode.Mortar:
                    place_success = mapController.PlaceMortar(p);
                    break;
                case ControlMode.Medic:
                    place_success = mapController.PlaceMedic(p);
                    break;
                case ControlMode.Gun:
                    place_success = mapController.PlaceGun(p);
                    break;
            }
        }

        if (!place_success)
        {
            audio.clip = PlaceFailSound;
            audio.Play();
        }
//        if (Input.GetMouseButton(1) & SelectionBox.activeSelf)
//        {
//            mapController.RemoveTile(p);
//        }
    }

    void AddSoldier()
    {
        int soldier_x = Random.Range(0, MapX);
        GameObject new_soldier = (GameObject)Instantiate(SoldierPrefab, new Vector2(soldier_x, 0), Quaternion.identity);
        new_soldier.GetComponent<Transform>().SetParent(UnitTransform);
    }

    void AddEnemyBullet()
    {
        int bullet_x = Random.Range(0, MapX);

        int needed = 0;

        if (fireSuppression[bullet_x] > Time.time)
        {
            needed = SuppressedFireChance;
        }

        int to_fire = Random.Range(0, 100);

        if (to_fire >= needed)
        {
            GameObject new_bullet = (GameObject)Instantiate(EnemyBulletPrefab, new Vector3(bullet_x, MapY, -2f), Quaternion.identity);
            new_bullet.GetComponent<Transform>().SetParent(ProjectileTransform);
        } else
        {
            Debug.Log("Didn't fire bullet due to fire suppression.");
        }

    }

    void AddEnemyMortar()
    {
        int mortar_x = Random.Range(2, MapX - 2);

        int needed = 0;
        
        if (fireSuppression[mortar_x] > Time.time)
        {
            needed = SuppressedFireChance;
        }
        
        int to_fire = Random.Range(0, 100);
        
        if (to_fire >= needed)
        {
            GameObject new_mortar = (GameObject)Instantiate(EnemyMortarPrefab, new Vector3(mortar_x, MapY, -2f), Quaternion.identity);
            new_mortar.GetComponent<Transform>().SetParent(ProjectileTransform);
        } else
        {
            Debug.Log("Didn't fire mortar due to fire suppression.");
        }
    }

    public void SetMode(int mode)
    {
        Mode = (ControlMode)mode;

        switch (Mode)
        {
            case ControlMode.Dig:
                SelectionBox.GetComponent<SpriteRenderer>().sprite = OneTileSelectionBox;
                break;
            case ControlMode.Gun:
                SelectionBox.GetComponent<SpriteRenderer>().sprite = ThreeTileSelectionBox;
                break;
            case ControlMode.Wall:
                SelectionBox.GetComponent<SpriteRenderer>().sprite = OneTileSelectionBox;
                break;
            case ControlMode.Mortar:
                SelectionBox.GetComponent<SpriteRenderer>().sprite = ThreeTileSelectionBox;
                break;
            case ControlMode.Sentry:
                SelectionBox.GetComponent<SpriteRenderer>().sprite = OneTileSelectionBox;
                break;
            case ControlMode.Medic:
                SelectionBox.GetComponent<SpriteRenderer>().sprite = ThreeTileSelectionBox;
                break;
        }
    }
    
    public void PlayerMortarHit()
    {
        if (enemyMortarFire)
        {
            nextEnemyMortarStateChange -= 0.5f;
        } else
        {
            nextEnemyMortarStateChange += 0.5f;
        }

        if (enemyGunFire)
        {
            nextEnemyGunStateChange -= 0.5f;
        } else
        {
            nextEnemyGunStateChange += 0.5f;
        }
        audio.clip = MortarHitSound;
        audio.Play();
    }

    public void SuppressFire(int x)
    {
        for (int i = (x-2); i <= (x+2); i++)
        {
            if (i >= 0 & i < MapX)
            {
                fireSuppression[i] = Time.time + JobTime.MACHINE_GUN_SUPPRESS_TIME;
            }
        }
    }

    public void Charge()
    {
        if (IsRunning)
        {
            int charge_x = Random.Range(0, MapX);
            int[] location_array = new int[2] {charge_x, MapY};
            queue.Jobs.Add(new Job(location_array, JobType.Charge, JobTime.CHARGE));

            // Charging kicks off enemy gunfire

            enemyGunFire = true;
            enemyMortarFire = true;

            nextEnemyGunStateChange = Time.time + DurationEnemyGunWave;
            nextEnemyGun = Time.time + EnemyGunRate;
            nextEnemyMortarStateChange = Time.time + DurationEnemyMortarWave;
            nextEnemyMortar = Time.time + EnemyMortarRate;
        }
    }
    
    public void ScorePoint()
    {
        score += 1;
        nextScoreReduction = Time.time + ScoreReduceTime;
        Debug.Log("Score: " + score);
    }

    public bool CheckWin()
    {
        if (score >= VictoryScore)
        {
            Debug.Log("You win!!!");
            return true;
        }

        return false;
    }

    public bool CheckLose()
    {
        foreach (Transform unit in UnitTransform)
        {
            if (unit.tag == "Player")
                return false;
        }

        Debug.Log("You lose!!!");
        return true;
    }

    public void GameStart()
    {
        StartPanel.SetActive(false);

        nextReinforcement = Time.time + ReinforcementsTime;
        
        enemyMortarFire = false;
        enemyGunFire = false;
        
        nextEnemyGunStateChange = Time.time + FirstEnemyGunTime;
        nextEnemyMortarStateChange = Time.time + FirstEnemyMortarTime;

        AddSoldier();
        AddSoldier();
        AddSoldier();
        AddSoldier();
        AddSoldier();

        IsRunning = true;

    }

    public void Restart()
    {
        Application.LoadLevel(0);
    }

}