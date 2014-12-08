using UnityEngine;
using System.Collections;

public class PlayerMortar : MonoBehaviour
{

    public float MortarVelocity = 10f;
    public float HitY = 43f;

    private GameController gameController;
    // Use this for initialization
    void Start()
    {
        gameController = GameObject.Find("game_controller").GetComponent<GameController>();
    }
	
    // Update is called once per frame
    void Update()
    {
        if (gameController.IsRunning)
        {
            Vector3 current_pos = transform.position;
            Vector3 new_pos = new Vector3(current_pos.x, current_pos.y + MortarVelocity * Time.deltaTime, current_pos.z);
            transform.position = new_pos;

            if (new_pos.y >= HitY)
            {
                Debug.Log("Hit enemy line!");
                gameController.PlayerMortarHit();
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "EnemyMortar")
        {
            Debug.Log("Hit enemy projectile");
            Destroy(gameObject);
            Destroy(other.gameObject);
        }
    }
}
