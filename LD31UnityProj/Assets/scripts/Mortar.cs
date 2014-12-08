using UnityEngine;
using System.Collections;

public class Mortar : MonoBehaviour
{

    public float MortarVelocity = -5f;
    public int MapY = 34;
    public int TargetYPos;

    private MapController mapController;
    private GameController gameController;

    void Awake()
    {
        mapController = GameObject.Find("map").GetComponent<MapController>();
        gameController = GameObject.Find("game_controller").GetComponent<GameController>();
        TargetYPos = Mathf.FloorToInt(Mathf.Sqrt
                                      (Random.Range(4.0f, Mathf.Pow(MapY - 2, 2))));
        // Debug.Log(TargetYPos);

    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.IsRunning)
        {
            Vector3 current_pos = transform.position;
            Vector3 new_pos = new Vector3(current_pos.x, current_pos.y + MortarVelocity * Time.deltaTime, current_pos.z);
            transform.position = new_pos;

            if (transform.position.y <= TargetYPos)
            {
                mapController.MortarHit((int)current_pos.x, TargetYPos);
                Destroy(gameObject);
            }
        }
    }
}
