using UnityEngine;
using System.Collections;
using Pathfinding;

public class BasicUnit : MonoBehaviour
{

    const int MAX_MORALE = 10;
    const int MORALE_CHANCE = 2;
    const int MAX_HIT_POINTS = 10;

    public Sprite[] HitPointSpriteArray;
    public Sprite[] MoralePointSpriteArray;

    public SpriteRenderer HitBar;
    public SpriteRenderer MoraleBar;

    public int HitPoints = 10;
    public int MoralePoints = 10;
    public float CheckTime = 0.25f;
    public float MoveSpeed = 10f;
    public float MoraleRecoverTime = 0.5f;

    public Job MyJob;
    public Path Path;
    public bool inTrench = true;
    public bool isFleeing = false;



    private JobQueue queue;
    private MapController mapController;
    private float nextCheckTime;
    private float nextMoraleTime;
    private Seeker seeker;
    private int currentWayPoint = 0;

    // Use this for initialization
    void Start()
    {
        queue = GameObject.Find("game_controller").GetComponent<JobQueue>();
        mapController = GameObject.Find("map").GetComponent<MapController>();
        nextCheckTime = Time.time;
        seeker = GetComponent<Seeker>();

    }
    
    // Update is called once per frame
    void Update()
    {
        if (HitPoints <= 0)
        {
            Die();
        }
        if (MoralePoints <= 0 & !isFleeing)
        {
            isFleeing = true;
            seeker.StartPath(transform.position, new Vector2(Random.Range(0, mapController.MapX), 0), OnPathComplete);
            nextMoraleTime = Time.time + MoraleRecoverTime;

            // return job to queue.
            if (MyJob != null)
            {
                queue.Jobs.Add(MyJob);
                MyJob = null;
            }
        }

        if (!isFleeing)
        {
            if (MyJob == null)
            {
                GetJob();
            } else
            {
                Move();
                DoJob();
            }
        } else
        {
            Flee();
        }

        Vector2 center_pos = (Vector2)transform.position + new Vector2(0.5f, 0.5f);
        if (mapController.TileArray[Mathf.FloorToInt(center_pos.x), Mathf.FloorToInt(center_pos.y)] 
            <= Tile.Built_Empty)
        {
            inTrench = false;
        } else
        {
            inTrench = true;
        }

        int hit_sprite_index = HitPoints;
        int morale_sprite_index = MoralePoints;

        if (hit_sprite_index < 0)
            hit_sprite_index = 0;
        if (morale_sprite_index < 0)
            morale_sprite_index = 0;

        HitBar.sprite = HitPointSpriteArray[hit_sprite_index];
        MoraleBar.sprite = MoralePointSpriteArray[morale_sprite_index];

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
            {
                Debug.Log("Morale too low. Needed " + action_chance + ", had " + MoralePoints);
                return;
            }

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
                seeker.StartPath(transform.position, new Vector2(MyJob.Location[0], MyJob.Location[1]), OnPathComplete);
            }
        }
    }

    private void Move()
    {
        if (Path == null)
        {
            return;
        }
        if (currentWayPoint >= this.Path.vectorPath.Count)
        {
            return;
        }

        Vector2 current_pos = transform.position;
        Vector2 dest_pos = this.Path.vectorPath[currentWayPoint];

        Vector2 direction_vector = (dest_pos - current_pos);
        
        float distance = direction_vector.magnitude;
        if (distance > MoveSpeed * Time.deltaTime)
        {
            float x_move = direction_vector.normalized.x * MoveSpeed * Time.deltaTime;
            float y_move = direction_vector.normalized.y * MoveSpeed * Time.deltaTime;
            Vector2 movement_vector = new Vector2(x_move, y_move);
            transform.position = current_pos + (movement_vector);
        } else
        {
            transform.position = dest_pos;
            currentWayPoint++;
        }
        
        // Debug.DrawLine(current_pos, dest_pos);
    }

    private void DoJob()
    {
        if (MyJob == null)
        {
            Debug.Log("Got into DoJob without a job.");
            return;
        }

        Vector2 dest_pos = new Vector2(MyJob.Location[0], MyJob.Location[1]);
        Debug.DrawLine(dest_pos, new Vector3(dest_pos.x, dest_pos.y + MyJob.TimeLeft / 10f), Color.green);

        if (((Vector2)transform.position == dest_pos) & (Time.time > nextCheckTime))
        {
            nextCheckTime = Time.time + CheckTime;
            MyJob.TimeLeft -= 1;

            if (MyJob.TimeLeft <= 0)
            {
                mapController.BuildTrench(MyJob.Location[0], MyJob.Location[1]);
                // Debug.Log("Job's done!");
                MyJob = null;
            }
        }
    }

    private void Die()
    {
        // Debug.Log("I died!");

        if (MyJob != null)
            queue.Jobs.Add(MyJob);

        Destroy(gameObject);
    }

    private void Flee()
    {
        Move();

        if (Time.time > nextMoraleTime)
        {
            MoralePoints += 1;
            if (MoralePoints == MAX_MORALE)
            {
                isFleeing = false;
            }

            nextMoraleTime = Time.time + MoraleRecoverTime;
        }
    }

    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            Path = p;
            currentWayPoint = 0;
        }
    }
}
