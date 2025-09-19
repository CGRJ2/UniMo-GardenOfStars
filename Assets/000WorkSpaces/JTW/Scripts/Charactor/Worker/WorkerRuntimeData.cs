using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerRuntimeData : CharaterRuntimeData
{
    private WorkerData _data;

    #region WorkerDatas

    public string Id => _data.Id;
    public int Rank => _data.Rank;

    public float MoveSpeed => _data.MoveSpeed;
    public long MoveSpeedLv => _data.MoveSpeedLv.Value;

    public int MaxCapacity => _data.MaxCapacity;
    public long MaxCapacityLv => _data.MaxCapacityLv.Value;

    public float ProductionSpeed => _data.ProductionSpeed;

    public float StunTime => _data.StunTime;
    public float StunChance => _data.StunChance;

    #endregion

    private WorkerManager _workerManager;
    public WorkerManager WorkerManager => _workerManager;
    private WorkerController _workerController;
    public WorkerController WorkerController => _workerController;

    public ObservableProperty<IWorkStation> CurWorkstation = new ObservableProperty<IWorkStation>();
    public int NavMeshPriority;
    public bool IsHarvest;

    public ObservableProperty<bool> IsStun = new();
    public ObservableProperty<bool> IsAwake = new();

    public ObservableProperty<bool> IsPlayerTriggered = new();

    private void Awake()
    {
        _workerController = GetComponent<WorkerController>();
    }

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

    public override int GetMaxCapacity()
    {
        return MaxCapacity;
    }

    public override float GetProductionSpeed()
    {
        return ProductionSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerTriggered.Value = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerTriggered.Value = false;
        }
    }
}
