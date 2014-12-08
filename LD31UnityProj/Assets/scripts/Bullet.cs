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
        if (gameController.IsRunning)
        {
            Vector3 current_pos = transform.position;
            Vector3 new_pos = new Vector3(current_pos.x, current_pos.y + BulletVelocity * Time.deltaTime, current_pos.z);
            transform.position = new_pos;

            if (transform.position.y < -0.5)
            {
                Destroy(gameObject);
            }

            if (transform.position.y > gameController.MapY + 0.5)
            {
                Debug.Log("Suppressing fire in column " + current_pos.x);
                gameController.SuppressFire((int)current_pos.x);
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Wall")
        {
            Wall wall_script = other.GetComponent<Wall>();
            wall_script.HitPoints -= 1;
            gameController.audio.clip = gameController.BulletHitSound;
            gameController.audio.Play();
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
                    if ((Vector2)(unit.position) == (Vector2)(building_script.transform.position))
                    {
                        BasicUnit unit_script = unit.GetComponent<BasicUnit>();
                        if (unit_script != null)
                            unit_script.MoralePoints -= 1;
                    }
                }
            }            
            gameController.audio.clip = gameController.BulletHitSound;
            gameController.audio.Play();
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
                unit_script.MoralePoints -= 1;
                gameController.audio.clip = gameController.BulletHitSound;
                gameController.audio.Play();
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