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
    Max
}

public static class JobTime
{
    public const int DIG_TRENCH = 10;
    public const int BUILD_WALL = 15;
    public const int BUILD_MORTAR = 30;
    public const int FIRE_MORTAR = 15;
    public const int SENTRY = 10;
    public const int BUILD_GUN = 25;
    public const int FIRE_GUN = 5;

    public const float SENTRY_EXPIRE_TIME = 0.5f;
}

public class JobQueue : MonoBehaviour
{

    public List<Job> Jobs = new List<Job>();

//    // Use this for initialization
//    void Start()
//    {
//	
//    }
//	
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