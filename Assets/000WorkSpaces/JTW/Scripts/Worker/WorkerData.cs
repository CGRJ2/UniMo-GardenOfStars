using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class WorkerData
{
    public string Id;
    public int Rank;

    public ObservableProperty<bool> IsSpawned = new();

    public float MoveSpeed;
    public ObservableProperty<float> MoveSpeedLv = new();

    public int MaxCapacity;
    public ObservableProperty<int> MaxCapacityLv = new();

    public float ProductionSpeed;

    public float StunTime;
    public float StunChance;

    public WorkerData()
    {
        IsSpawned.Value = false;

        MoveSpeed = 5;
        MoveSpeedLv.Value = 1;

        MaxCapacity = 5;
        MaxCapacityLv.Value = 1;

        ProductionSpeed = 2;

        StunChance = 20f;
        StunTime = 3;
    }
}
