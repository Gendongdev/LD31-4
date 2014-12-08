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
    private GameController gameController;
    private float nextCheckTime;
    private float nextMoraleTime;
    private Seeker seeker;
    private int currentWayPoint = 0;

    private Vector2 destination;

    // Use this for initialization
    void Start()
    {
        gameController = GameObject.Find("game_controller").GetComponent<GameController>();
        queue = GameObject.Find("game_controller").GetComponent<JobQueue>();
        mapController = GameObject.Find("map").GetComponent<MapController>();
        nextCheckTime = Time.time;
        seeker = GetComponent<Seeker>();

    }
    
    // Update is called once per frame
    void Update()
    {
        if (gameController.IsRunning)
        {
            if (HitPoints <= 0)
            {
                Die();
            }
            if (MoralePoints <= 0 & !isFleeing)
            {
                MoralePoints = 0;
                isFleeing = true;
                destination = new Vector2(Random.Range(0, mapController.MapX), 0);
                seeker.StartPath(transform.position, destination, OnPathComplete);
                nextMoraleTime = Time.time + MoraleRecoverTime;

                // return job to queue.
                if (MyJob != null)
                {
                    // don't return charge jobs
                    if (MyJob.Type != JobType.Charge)
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
            inTrench = false;

            if (center_pos.y < gameController.MapY)
            {
                if (TileHelper.IsBuiltTrench(mapController.TileArray[Mathf.FloorToInt(center_pos.x), Mathf.FloorToInt(center_pos.y)]))
                {
                    inTrench = true;
                } 
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

                // charge jobs take precedence over all others
                if (this_job.Type == JobType.Charge)
                {
                    this_job_distance = -1f;
                }

                // make sure the tile is empty

                bool isOccupied = false; 
                Transform[] unit_list = gameController.UnitTransform.GetComponentsInChildren<Transform>();


                foreach (Transform unit in unit_list)
                {
                    if (gameController.UnitTransform.GetInstanceID() != unit.GetInstanceID() 
                        & transform.GetInstanceID() != unit.GetInstanceID()
                        & unit.tag == "Player")
                    {
                        if ((Vector2)unit.position == new Vector2(this_job.Location[0], this_job.Location[1]))
                        {
                            Debug.Log("Not taking job " + this_job.Type + " as tile is occupied by " + unit.name);
                            isOccupied = true;
                        }
                    }
                }

                if (this_job_distance < job_distance & !isOccupied)
                {
                    MyJob = this_job;
                    job_distance = this_job_distance;
                }
            }
            
            if (MyJob != null)
            {
                queue.Jobs.Remove(MyJob);
                destination = new Vector2(MyJob.Location[0], MyJob.Location[1]);
                seeker.StartPath(transform.position, destination, OnPathComplete);
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

        // for jobs in buildings, check that the building still exists
        
        if (MyJob.Type == JobType.Fire_Mortar | MyJob.Type == JobType.Do_Medic | MyJob.Type == JobType.Fire_Gun)
        {
            bool stillExists = false;
            
            foreach (Transform building in mapController.BuildingsTransform)
            {
                if (building.GetInstanceID() != mapController.BuildingsTransform.GetInstanceID()
                    & building.tag == "Building")
                if ((Vector2)building.position == new Vector2(MyJob.Location[0], MyJob.Location[1]))
                {
                    stillExists = true;
                }
            }
            
            if (!stillExists)
            {
                Debug.Log("My building is missing! Getting new job.");
                MyJob = null;
                return;
            }
        }

        Vector2 dest_pos = new Vector2(MyJob.Location[0], MyJob.Location[1]);
        Debug.DrawLine(dest_pos, new Vector3(dest_pos.x, dest_pos.y + MyJob.TimeLeft / 10f), Color.green);

        if (((Vector2)transform.position == dest_pos) & (Time.time > nextCheckTime))
        {
            nextCheckTime = Time.time + CheckTime;
            MyJob.TimeLeft -= 1;

            // process finished jobs
            if (MyJob.TimeLeft <= 0)
            {
                switch (MyJob.Type)
                {
                    case JobType.Dig_Trench:
                        mapController.BuildTrench(MyJob.Location[0], MyJob.Location[1]);
                        MyJob = null;
                        break;

                    case JobType.Build_Wall:
                        mapController.BuildWall(MyJob.Location[0], MyJob.Location[1]);
                        MyJob = null;
                        break;

                    case JobType.Build_Mortar:
                        mapController.BuildMortar(MyJob.Location[0], MyJob.Location[1]);
                        MyJob = null;
                        break;

                    case JobType.Fire_Mortar:
                        queue.Jobs.Add(new Job(MyJob.Location, JobType.Fire_Mortar, JobTime.FIRE_MORTAR));
                        mapController.FireMortar(MyJob.Location[0], MyJob.Location[1]);
                        MyJob = null;
                        break;

                    case JobType.Sentry:
                        MyJob = null;
                        break;

                    case JobType.Build_Medic:
                        mapController.BuildMedic(MyJob.Location[0], MyJob.Location[1]);
                        MyJob = null;
                        break;

                    case JobType.Do_Medic:
                        queue.Jobs.Add(new Job(MyJob.Location, JobType.Do_Medic, JobTime.DO_MEDIC));
                        mapController.DoMedic(MyJob.Location[0], MyJob.Location[1]);
                        MyJob = null;
                        break;

                    case JobType.Build_Gun:
                        mapController.BuildGun(MyJob.Location[0], MyJob.Location[1]);
                        MyJob = null;
                        break;

                    case JobType.Fire_Gun:
                        queue.Jobs.Add(new Job(MyJob.Location, JobType.Fire_Gun, JobTime.FIRE_GUN));
                        mapController.FireGun(MyJob.Location[0], MyJob.Location[1]);
                        MyJob = null;
                        break;

                    case JobType.Charge:
                        gameController.ScorePoint();
                        MyJob = null;
                        Destroy(gameObject);
                        break;
                }
            }
        }
    }

    private void Die()
    {
        // Debug.Log("I died!");

        if (MyJob != null)
        {
            // Charge jobs die with the soldier
            if (MyJob.Type != JobType.Charge)
                queue.Jobs.Add(MyJob);
            MyJob = null;
        }

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
            if ((Vector2)p.vectorPath[p.vectorPath.Count - 1] == destination)
            {
                Path = p;
                currentWayPoint = 0;
            } else
            {
                Debug.Log("path does not reach destination!");
                if (MyJob != null)
                {
                    Debug.Log("returning job to queue");
                    queue.Jobs.Add(MyJob);
                    MyJob = null;
                }
            }
        }
    }
}