using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerRuntimeData : CharaterRuntimeData
{
    private WorkerData _data;

    #region WorkerDatas

    public int Id => _data.Id;
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

    private InteractableBase _curWorkstation;
    public InteractableBase CurWorkstation { get { return _curWorkstation; } set { _curWorkstation = value; } }

    public void SetWorkerManager(WorkerManager manager)
    {
        _workerManager = manager;
    }

    public void SetWorkerData(WorkerData data)
    {
        _data = data;
    }

    public void SetWorkstation(InteractableBase workstation)
    {
        _curWorkstation = workstation;
    }
}
