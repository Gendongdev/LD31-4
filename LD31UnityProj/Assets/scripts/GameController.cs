using UnityEngine;
using System.Collections;

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
    public float EnemyMachineGunRate = 0.2f;
    public int FirstEnemyMachineGunTime = 5;

    public float EnemyMortarRate = 0.5f;
    public int FirstEnemyMortarTime = 1;

    private MapController mapController;
    private float nextReinforcement;
    private float nextEnemyBullet;
    private float nextEnemyMortar;

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

        nextReinforcement = Time.time + ReinforcementsTime;
        nextEnemyBullet = Time.time + FirstEnemyMachineGunTime;
        nextEnemyMortar = Time.time + FirstEnemyMortarTime;
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

        if (Time.time >= nextEnemyBullet)
        {
            AddEnemyBullet();
            nextEnemyBullet = Time.time + EnemyMachineGunRate;
        }

        if (Time.time >= nextEnemyMortar)
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
            mapController.PlaceTrenchTile(p);
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
}