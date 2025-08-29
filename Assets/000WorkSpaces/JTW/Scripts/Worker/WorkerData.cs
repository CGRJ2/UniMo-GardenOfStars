using System.Collections.Generic;
using UnityEngine;

public class WorkerData : FirebaseData
{
    public int Rank => Manager.data.Worker.Values[Id].Rank;

    public float _moveSpeed;
    public float MoveSpeed { get { return _moveSpeed * MoveSpeedLv.Value; } }
    public FirebaseProperty<long> MoveSpeedLv;

    public int MaxCapacity;
    public FirebaseProperty<long> MaxCapacityLv;

    public float ProductionSpeed => Manager.data.Worker.Values[Id].ProductionSpeed;

    public float StunTime => Manager.data.Worker.Values[Id].StunTime;
    public float StunChance => Manager.data.Worker.Values[Id].StunChance;

    public WorkerData(string id, string parentPath = null) : base(id, parentPath)
    {
        _moveSpeed = 5;
        MoveSpeedLv = new FirebaseProperty<long>("MoveSpeedLv", Path);

        MaxCapacity = 5;
        MaxCapacityLv = new FirebaseProperty<long>("MaxCapacityLv", Path);
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
    public string Id;
    public int Rank;

    public float ProductionSpeed;

    public float StunTime;
    public float StunChance;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isWorkerAdressable;

    // 구글 스프레드 시트 다운로드 주소
    private const string _workerDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _workerAdress = "WorkerCsv";

    public DataTableParser<WorkerDataCsv> Worker;
    private async void WorkerRoutine()
    {
        string dataCsv;

        if (_isWorkerAdressable)
        {
            dataCsv = await GetDataString(_isWorkerAdressable, _workerAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isWorkerAdressable, _workerDataTableURL);
        }

        Worker = new DataTableParser<WorkerDataCsv>((words, dict) =>
        {
            WorkerDataCsv worker = new WorkerDataCsv();

            worker.Id = words[dict["CharacterFixedID"]];
            int.TryParse(words[dict["CharacterRank"]], out worker.Rank);
            float.TryParse(words[dict["PDSpeed"]], out worker.ProductionSpeed);
            float.TryParse(words[dict["EffectTimeOverride"]], out worker.StunTime);
            float.TryParse(words[dict["EffectChance"]], out worker.StunChance);

            return worker;
        });

        Worker.Load(dataCsv);
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
