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
    Max
}

public static class JobTimes
{
    public const int DIG_TRENCH = 10;
    public const int BUILD_WALL = 15;
    public const int BUILD_MORTAR = 30;
    public const int FIRE_MORTAR = 10;
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
//    // Update is called once per frame
//    void Update()
//    {
//	
//    }
}

public class Job
{
    public int[] Location = new int[2];
    public JobType Type = JobType.Empty;
    public int BuildTime;
    public int TimeLeft;

    public Job(int[] location, JobType type, int buildtime)
    {
        Location = location;
        Type = type;
        BuildTime = buildtime;
        TimeLeft = buildtime;
    }
}