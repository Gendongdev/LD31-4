using UnityEngine;
using System.Collections;

public class BasicUnit : MonoBehaviour
{

    const int MAX_MORALE = 10;
    const int MORALE_CHANCE = 2;
    const int MAX_HIT_POINTS = 10;

    public int HitPoints = 10;
    public int MoralePoints = 10;
    public float CheckTime = 0.25f;
    public float MoveSpeed = 0.1f;

    public Job MyJob;

    private JobQueue queue;
    private MapController mapController;
    private float nextCheckTime;

    // Use this for initialization
    void Start()
    {
        queue = GameObject.Find("game_controller").GetComponent<JobQueue>();
        mapController = GameObject.Find("map").GetComponent<MapController>();
        nextCheckTime = Time.time;
    }
    
    // Update is called once per frame
    void Update()
    {

        if (MyJob == null)
        {
            GetJob();
        } else
        {
            MoveToJob();
            DoJob();
        }

    }

    private void GetJob()
    {
        if ((MyJob == null) & (queue.Jobs.Count > 0))
        {
            if (Time.time < nextCheckTime)
                return;
            
            nextCheckTime = Time.time + CheckTime;
            
            float action_chance = Random.Range(0, MAX_MORALE + MORALE_CHANCE);
            
            if (MoralePoints < action_chance)
                return;

            Vector2 current_pos = transform.position;
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
    }

    private void MoveToJob()
    {
        Vector2 current_pos = transform.position;
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
        }
        
        Debug.DrawLine(current_pos, dest_pos);
    }

    private void DoJob()
    {
        Vector2 dest_pos = new Vector2(MyJob.Location[0], MyJob.Location[1]);
        Debug.DrawLine(dest_pos, new Vector3(dest_pos.x, dest_pos.y + MyJob.TimeLeft / 10f), Color.green);

        if (((Vector2)transform.position == dest_pos) & (Time.time > nextCheckTime))
        {
            nextCheckTime = Time.time + CheckTime;
            MyJob.TimeLeft -= 1;

            if (MyJob.TimeLeft == 0)
            {
                mapController.BuildTrench(MyJob.Location[0], MyJob.Location[1]);
                Debug.Log("Job's done!");
                MyJob = null;
            }
        }
    }
}

