﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum JobTypes
{
    Empty,
    Dig_Trench,
    Max
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
    public JobTypes JobType = JobTypes.Empty;

    public Job(int[] location, JobTypes type)
    {
        Location = location;
        JobType = type;
    }
}