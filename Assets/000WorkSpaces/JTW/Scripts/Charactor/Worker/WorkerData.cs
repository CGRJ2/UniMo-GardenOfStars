using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkerData : FirebaseData
{
    private CharacterDataCsv CharacterCsv => Manager.data.Character.Values[Id];
    private WorkerDataCsv WorkerCsv => Manager.data.Worker.Values[CharacterCsv.StatusId];
    public string Name => CharacterCsv.Name;
    public int Rank => WorkerCsv.Rank;

    public Sprite Sprite => CharacterCsv.Sprite;
    public GameObject Avatar => CharacterCsv.Avatar;

    public float MoveSpeed => Manager.data.CharacterLv.Values[MoveSpeedLv.Value.ToString()].Speed;
    public FirebaseProperty<int> MoveSpeedLv;
    public int MoveSpeedMaxLv => WorkerCsv.SpeedMaxLv;
    public bool IsMoveSpeedMaxLv => MoveSpeedLv.Value >= MoveSpeedMaxLv;

    public int MaxCapacity => Manager.data.CharacterLv.Values[MaxCapacityLv.Value.ToString()].Capacity;
    public FirebaseProperty<int> MaxCapacityLv;
    public int MaxCapacityMaxLv => WorkerCsv.MaxCapacityMaxLv;
    public bool IsMaxCapacityMaxLv => MaxCapacityLv.Value >= MaxCapacityMaxLv;

    public float ProductionSpeed => WorkerCsv.ProductionSpeed;
    public string ProductionSpeedRank => WorkerCsv.ProductionSpeedRank;

    public float StunTime => WorkerCsv.StunTime;
    public float StunChance => WorkerCsv.StunChance;
    public string StunRank => WorkerCsv.StunRank;

    public FirebaseProperty<float> PositionX;
    public FirebaseProperty<float> PositionZ;


    public WorkerData(string id, string parentPath = null) : base(id, parentPath)
    {
        MoveSpeedLv = new FirebaseProperty<int>("MoveSpeedLv", Path, 1);
        InitList.Add(MoveSpeedLv);

        MaxCapacityLv = new FirebaseProperty<int>("MaxCapacityLv", Path, 1);
        InitList.Add(MaxCapacityLv);

        PositionX = new FirebaseProperty<float>("PositionX", Path);
        InitList.Add(PositionX);

        PositionZ = new FirebaseProperty<float>("PositionZ", Path);
        InitList.Add(PositionZ);
    }

    public void UpgradeMoveSpeed()
    {
        if (!IsMoveSpeedMaxLv)
        {
            MoveSpeedLv.Value += 1;
        }
    }

    public void UpgradeMaxCapacity()
    {
        if (!IsMaxCapacityMaxLv)
        {
            MaxCapacityLv.Value += 1;
        }
    }
}

public class WorkerDataCsv : IUsableId
{
    public string Id;
    public int Rank;

    public int SpeedMaxLv;
    public int MaxCapacityMaxLv;

    public float ProductionSpeed;
    public string ProductionSpeedRank;

    public float StunTime;
    public float StunChance;
    public string StunRank;

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

            int.TryParse(words[dict["MaxSpeedLV"]], out worker.SpeedMaxLv);
            int.TryParse(words[dict["MaxCapacityLV"]], out worker.MaxCapacityMaxLv);

            float.TryParse(words[dict["PDSpeed"]], out worker.ProductionSpeed);
            worker.ProductionSpeedRank = words[dict["PDSpeedTier"]];

            float.TryParse(words[dict["EffectTimeOverride"]], out worker.StunTime);
            float.TryParse(words[dict["EffectChance"]], out worker.StunChance);
            worker.StunRank = words[dict["EffectTier"]];

            return worker;
        });

        Worker.Load(dataCsv);
    }
}

public class CharacterLvDataCsv : IUsableId
{
    public string Id;

    public float Speed;
    public int Capacity;
    public float Nego;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    private bool _isCharacterLVAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _CharacterLvDataTableURL = "";


    // Addressable 에셋 주소
    private const string _CharacterLvAdress = "CharacterLvCsv";

    public DataTableParser<CharacterLvDataCsv> CharacterLv;
    private async void CharacterLvRoutine()
    {
        string dataCsv;

        if (_isCharacterLVAdressable)
        {
            dataCsv = await GetDataString(_isCharacterLVAdressable, _CharacterLvAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isCharacterLVAdressable, _CharacterLvDataTableURL);
        }

        CharacterLv = new DataTableParser<CharacterLvDataCsv>((words, dict) =>
        {
            CharacterLvDataCsv characterLv = new CharacterLvDataCsv();

            characterLv.Id = words[dict["LV"]];

            float.TryParse(words[dict["SpeedValue"]], out characterLv.Speed);
            int.TryParse(words[dict["CapacityValue"]], out characterLv.Capacity);
            float.TryParse(words[dict["NegoValue"]], out characterLv.Nego);

            return characterLv;
        });

        CharacterLv.Load(dataCsv);
    }
}