using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerData
{
    public string Id;
    public int Rank;

    public float MoveSpeed;
    public float MoveSpeedLv;

    public int MaxCapacity;
    public int MaxCapacityLv;

    public float ProductionSpeed;

    public float StunTime;
    public float StunChance;

    public WorkerData()
    {
        MoveSpeed = 5;
        MoveSpeedLv = 1;

        MaxCapacity = 5;
        MaxCapacityLv = 1;

        ProductionSpeed = 2;
    }
}
