using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerRuntimeData : CharaterRuntimeData
{
    private WorkerData _data = new();

    #region WorkerDatas

    public string Id => _data.Id;
    public int Rank => _data.Rank;

    public float MoveSpeed => _data.MoveSpeed;
    public float MoveSpeedLv => _data.MoveSpeedLv;

    public int MaxCapacity => _data.MaxCapacity;
    public int MaxCapacityLv => _data.MaxCapacityLv;

    public float ProductionSpeed => _data.ProductionSpeed;

    public float StunTime => _data.StunTime;
    public float StunChance => _data.StunChance;

    #endregion

    private WorkerManager _workerManager;
    public WorkerManager WorkerManager => _workerManager;

    public ObservableProperty<IWorkStation> CurWorkstation = new ObservableProperty<IWorkStation>();


    public bool IsHarvest;

    public void SetWorkerManager(WorkerManager manager)
    {
        _workerManager = manager;
    }

    public void SetWorkerData(WorkerData data)
    {
        _data = data;
    }

    public void SetWorkstation(IWorkStation workstation)
    {
        workstation.SetReserveState(true);
        CurWorkstation.Value = workstation;
    }
}
