using UnityEngine;
using System.Collections;

public class BasicUnit : MonoBehaviour
{

    const int MAX_MORALE = 10;
    const int MORALE_CHANCE = 2;
    const int MAX_HIT_POINTS = 10;

    public int HitPoints = 10;
    public int MoralePoints = 10;
    public float CheckTime = 0.5f;
    public float MoveSpeed = 0.2f;

    public Job MyJob;

    private JobQueue queue;
    private float nextCheckTime;

    // Use this for initialization
    void Start()
    {
        queue = GameObject.Find("game_controller").GetComponent<JobQueue>();
        nextCheckTime = Time.time;
    }
    
    // Update is called once per frame
    void Update()
    {

        Vector2 current_pos = transform.position;

        if ((MyJob == null) & (queue.Jobs.Count > 0))
        {
            if (Time.time < nextCheckTime)
                return;

            nextCheckTime = Time.time + CheckTime;

            float action_chance = Random.Range(0, MAX_MORALE + MORALE_CHANCE);

            if (MoralePoints < action_chance)
                return;

            float job_distance = float.PositiveInfinity;

            foreach (Job this_job in queue.Jobs)
            {
                Vector2 this_job_pos = new Vector2(this_job.Location[0], this_job.Location[1]);
                float this_job_distance = (this_job_pos - current_pos).magnitude;

                if (this_job_distance < job_distance)
                {
                    MyJob = this_job;
                    job_distance = this_job_distance;
                }
            }

            if (MyJob != null)
            {
                queue.Jobs.Remove(MyJob);
            }
        }

        if (MyJob != null)
        {
            Vector2 dest_pos = new Vector2(MyJob.Location[0], MyJob.Location[1]);

            Vector2 direction_vector = (dest_pos - current_pos);

            float distance = direction_vector.magnitude;
            if (distance > MoveSpeed)
            {
                float x_move = direction_vector.normalized.x * MoveSpeed;
                float y_move = direction_vector.normalized.y * MoveSpeed;
                Vector2 movement_vector = new Vector2(x_move, y_move);
                transform.position = current_pos + (movement_vector);
            } else
            {
                transform.position = dest_pos;
                MyJob = null;
                nextCheckTime = Time.time + CheckTime;
            }

            Debug.DrawLine(current_pos, dest_pos);
        }
    }
}

