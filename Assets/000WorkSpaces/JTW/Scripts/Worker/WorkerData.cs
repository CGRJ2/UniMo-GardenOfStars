using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class WorkerData : FirebaseData
{
    public int Rank;

    public float _moveSpeed;
    public float MoveSpeed { get {return  _moveSpeed * MoveSpeedLv.Value; } }
    public FirebaseProperty<long> MoveSpeedLv;

    public int MaxCapacity;
    public FirebaseProperty<long> MaxCapacityLv;

    public float ProductionSpeed;

    public float StunTime;
    public float StunChance;

    public WorkerData(string id, string parentPath = null) : base(id, parentPath)
    {
        Rank = 1;

        _moveSpeed = 5;
        MoveSpeedLv = new FirebaseProperty<long>("MoveSpeedLv", Path);

        MaxCapacity = 5;
        MaxCapacityLv = new FirebaseProperty<long>("MaxCapacityLv", Path);

        ProductionSpeed = 2;

        StunTime = 3;
        StunChance = 20;
    }
}

public class WorkerDataJson : IUsableId
{
    public string Id;
    public int MoveSpeedLv;
    public int MaxCapacityLv;

    public string GetId()
    {
        return Id;
    }
}

public class WorkerDataCsv : IUsableId
{
    private string _id;
    public float Rank;

    public float ProductionSpeed;

    public float StunTime;
    public float StunChance;

    public string GetId()
    {
        return _id;
    }
}

public class WorkerLvDataCsv : IUsableId
{
    private string _id;

    List<float> MoveSpeedByLv = new List<float>();

    List<float> MaxCapacityByLv = new List<float>();

    public string GetId()
    {
        return _id;
    }
}
