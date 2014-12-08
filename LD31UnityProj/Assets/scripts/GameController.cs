using UnityEngine;
using System.Collections;


public enum ControlMode
{
    Dig,
    Gun,
    Mortar,
    Wall,
    Cancel,
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

    public ControlMode Mode = ControlMode.Dig;
    public Transform[] Buttons;

    public UnityEngine.UI.Text StatusText;

    private MapController mapController;
    private float nextReinforcement;
    private float nextEnemyGun;
    private float nextEnemyMortar;

    private float nextEnemyGunStateChange;
    private float nextEnemyMortarStateChange;

    private bool enemyMortarFire;
    private bool enemyGunFire;

    // Use this for initialization
    void Start()
    {
        mapController = GameObject.Find("map").GetComponent<MapController>();
        mapController.MapX = MapX;
        mapController.MapY = MapY;
        mapController.TileArray = new Tile[MapX, MapY];
        mapController.GameObjectArray = new GameObject[MapX, MapY];
        mapController.InitMap();

        AddSoldier();
        AddSoldier();
        AddSoldier();

        nextReinforcement = Time.time + ReinforcementsTime;
        enemyMortarFire = false;
        enemyGunFire = false;

        nextEnemyGunStateChange = FirstEnemyGunTime;
        nextEnemyMortarStateChange = FirstEnemyMortarTime;
    }
    
    // Update is called once per frame
    void Update()
    {
        ProcessMouse();

        if (Time.time >= nextReinforcement)
        {
            AddSoldier();
            nextReinforcement = Time.time + ReinforcementsTime;
        }

        EnemyStateChanges();
        EnemyFire();

    }

    void EnemyStateChanges()
    {
        if (Time.time > nextEnemyGunStateChange)
        {
            if (enemyGunFire)
            {
                nextEnemyGunStateChange = Time.time + TimeBetweenEnemyGunWave;
                enemyGunFire = false;
            } else
            {
                nextEnemyGunStateChange = Time.time + DurationEnemyGunWave;
                enemyGunFire = true;
                nextEnemyGun = Time.time + EnemyGunRate;
            }
        }
        if (Time.time > nextEnemyMortarStateChange)
        {
            if (enemyMortarFire)
            {
                nextEnemyMortarStateChange = Time.time + TimeBetweenEnemyMortarWave;
                enemyMortarFire = false;
            } else
            {
                nextEnemyMortarStateChange = Time.time + DurationEnemyMortarWave;
                enemyMortarFire = true;
                nextEnemyMortar = Time.time + EnemyMortarRate;
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
        Vector2 p = LevelCamera.ScreenToWorldPoint(Input.mousePosition);
        p.x = Mathf.Floor(p.x);
        p.y = Mathf.Floor(p.y);
        if (p.x < 0 | p.x >= MapX | p.y < 0 | p.y >= MapY)
        {
            SelectionBox.SetActive(false);
        } else
        {
            SelectionBox.SetActive(true);
            SelectionBox.transform.position = p;
        }
       
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
        
        if (Input.GetMouseButton(1) & SelectionBox.activeSelf)
        {
            mapController.RemoveTile(p);
        }
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
        GameObject new_bullet = (GameObject)Instantiate(EnemyBulletPrefab, new Vector2(bullet_x, MapY), Quaternion.identity);
        new_bullet.GetComponent<Transform>().SetParent(ProjectileTransform);
    }

    void AddEnemyMortar()
    {
        int mortar_x = Random.Range(2, MapX - 2);
        GameObject new_mortar = (GameObject)Instantiate(EnemyMortarPrefab, new Vector2(mortar_x, MapY), Quaternion.identity);
        new_mortar.GetComponent<Transform>().SetParent(ProjectileTransform);
    }

    public void SetMode(int mode)
    {
        Mode = (ControlMode)mode;
        for (int i = 0; i < Buttons.Length; i++)
        {
            Debug.Log(i + ": " + Buttons[i].GetComponent<UnityEngine.UI.Button>().spriteState.ToString());

        }
    }
}