using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{

    public BuildingType Type;
    public int HitPoints;
    public MapController mapController;
    
    private int locationX;
    private int locationY;

    // Use this for initialization
    void Start()
    {
        mapController = GameObject.Find("map").GetComponent<MapController>();
        
        locationX = (int)transform.position.x;
        locationY = (int)transform.position.y;
    }
	
    // Update is called once per frame
    void Update()
    {
        if (HitPoints <= 0)
        {
            mapController.BuildingDestroy(locationX, locationY);
            Destroy(gameObject);
        }
    }
}
