using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum JobType
{
    Empty,
    Dig_Trench,
    Build_Wall,
    Build_Mortar,
    Fire_Mortar,
    Sentry,
    Build_Gun,
    Fire_Gun,
    Build_Medic,
    Do_Medic,
    Max
}

public static class JobTime
{
    public const int DIG_TRENCH = 10;
    public const int BUILD_WALL = 15;
    public const int BUILD_MORTAR = 35;
    public const int FIRE_MORTAR = 20;
    public const int SENTRY = 10;
    public const int BUILD_GUN = 25;
    public const int FIRE_GUN = 5;
    public const int BUILD_MEDIC = 25;
    public const int DO_MEDIC = 15;

    public const float SENTRY_EXPIRE_TIME = 0.5f;
}

public class JobQueue : MonoBehaviour
{

    public List<Job> Jobs = new List<Job>();

    private MapController mapController;

    // Use this for initialization
    void Start()
    {
        mapController = GameObject.Find("map").GetComponent<MapController>();

    }
	
    // Update is called once per frame
    void Update()
    {
        List<Job> jobs_to_remove = new List<Job>();
        foreach (Job this_job in Jobs)
        {
            if (this_job.Type == JobType.Sentry & Time.time >= this_job.ExpireTime)
            {
                Debug.Log("Removing expired sentry job from queue.");
                jobs_to_remove.Add(this_job);
            }

            if (this_job.Type == JobType.Fire_Mortar | this_job.Type == JobType.Do_Medic)
            {
                bool stillExists = false;
                
                foreach (Transform building in mapController.BuildingsTransform)
                {
                    if (building.GetInstanceID() != mapController.BuildingsTransform.GetInstanceID()
                        & building.tag == "Building")
                    if ((Vector2)building.position == new Vector2(this_job.Location[0], this_job.Location[1]))
                    {
                        stillExists = true;
                    }
                }
                
                if (!stillExists)
                {
                    Debug.Log("Removing job for destroyed building from queue.");
                    jobs_to_remove.Add(this_job);
                    return;
                }
            }
        }

        foreach (Job expired_job in jobs_to_remove)
        {
            Jobs.Remove(expired_job);
        }
    }
}

public class Job
{
    public int[] Location = new int[2];
    public JobType Type = JobType.Empty;
    public int BuildTime;
    public int TimeLeft;
    public float ExpireTime;

    public Job(int[] location, JobType type, int buildtime)
    {
        Location = location;
        Type = type;
        BuildTime = buildtime;
        TimeLeft = buildtime;

        if (Type == JobType.Sentry)
        {
            ExpireTime = Time.time + JobTime.SENTRY_EXPIRE_TIME;
        }
    }

}