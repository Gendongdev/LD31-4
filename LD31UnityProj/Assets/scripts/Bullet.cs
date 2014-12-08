using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{

    public float BulletVelocity = -20f;
    public float ExposedHitChance = 0.95f;
    public float CoveredHitChance = 0.10f;
    private GameController gameController;

    // Use this for initialization
    void Start()
    {
        gameController = GameObject.Find("game_controller").GetComponent<GameController>();
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector2 current_pos = transform.position;
        Vector2 new_pos = new Vector2(current_pos.x, current_pos.y + BulletVelocity * Time.deltaTime);
        transform.position = new_pos;

        if (transform.position.y < -0.5)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Wall")
        {
            Wall wall_script = other.GetComponent<Wall>();
            wall_script.HitPoints -= 1;
            Destroy(gameObject);
        } else if (other.tag == "Building")
        {
            Building building_script = other.GetComponent<Building>();
            building_script.HitPoints -= 1;

            Transform[] unit_list = gameController.UnitTransform.GetComponentsInChildren<Transform>();

            foreach (Transform unit in unit_list)
            {
                if (gameController.UnitTransform.GetInstanceID() != unit.GetInstanceID())
                {
                    if (unit.position == building_script.transform.position)
                    {
                        BasicUnit unit_script = unit.GetComponent<BasicUnit>();
                        unit_script.MoralePoints -= 1;
                    }
                }
            }
            Destroy(gameObject);
        } else if (other.tag == "Player")
        {
            // Debug.Log("Hit a player! " + Time.time);
            BasicUnit unit_script = other.GetComponent<BasicUnit>();
            // Debug.Log("is in trench? " + unit_script.inTrench);

            float hit_chance = Random.Range(0.0f, 1.0f);
            float to_hit;

            if (unit_script.inTrench)
            {
                to_hit = CoveredHitChance;
            } else
            {
                to_hit = ExposedHitChance;
            }

            if (hit_chance <= to_hit)
            {
                unit_script.HitPoints -= 1;
                // Debug.Log("hp: " + unit_script.HitPoints);
                Destroy(gameObject);
            } else
            {
                unit_script.MoralePoints -= 1;
                // Debug.Log("morale: " + unit_script.MoralePoints);
            }

        }

    }
}