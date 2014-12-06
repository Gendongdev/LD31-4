using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{

    public Camera LevelCamera;
    public GameObject SelectionBox;
    public int MapX = 34;
    public int MapY = 34;
    public GameObject SoldierPrefab;
    public Transform SoldierTransform;
    public int ReinforcementsTime = 20;

    private MapController mapController;
    private float nextReinforcement;

    // Use this for initialization
    void Start()
    {
        mapController = GameObject.Find("map").GetComponent<MapController>();
        mapController.MapX = MapX;
        mapController.MapY = MapY;
        mapController.TileArray = new Tiles[MapX, MapY];
        mapController.GameObjectArray = new GameObject[MapX, MapY];
        mapController.InitMap();

        AddSoldier();

        nextReinforcement = Time.time + ReinforcementsTime;
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
            mapController.PlaceTile(p);
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
        new_soldier.GetComponent<Transform>().SetParent(SoldierTransform);
    }
}