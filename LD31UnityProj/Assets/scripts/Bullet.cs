using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{

    public float BulletVelocity = -20f;

    public float ExposedHitChance = 0.95f;
    public float CoveredHitChance = 0.10f;

    // Use this for initialization
    void Start()
    {
	
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
        if (other.tag == "Player")
        {
            Debug.Log("Hit a player! " + Time.time);
            BasicUnit unit_script = other.GetComponent<BasicUnit>();
            Debug.Log("is in trench? " + unit_script.inTrench);

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
                Debug.Log("hp: " + unit_script.HitPoints);
                Destroy(gameObject);
            } else
            {
                unit_script.MoralePoints -= 1;
                Debug.Log("morale: " + unit_script.MoralePoints);
            }

        }

    }
}