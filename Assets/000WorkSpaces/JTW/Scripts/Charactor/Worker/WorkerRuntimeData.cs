using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerRuntimeData : CharaterRuntimeData
{
    [SerializeField] private GameObject _stunUI;
    public GameObject StunUI => _stunUI;
    [SerializeField] private Image _stunImage;
    public Image StunImage => _stunImage;

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
    private WorkerPresenter _workerPresenter;
    public WorkerPresenter WorkerPresenter => _workerPresenter;

    public ObservableProperty<IWorkStation> CurWorkstation = new ObservableProperty<IWorkStation>();
    public int NavMeshPriority;
    public bool IsHarvest;

    public ObservableProperty<bool> IsStun = new();
    public ObservableProperty<bool> IsAwake = new();

    public ObservableProperty<bool> IsPlayerTriggered = new();

    private WaitForSeconds _saveDelay = new WaitForSeconds(60f);

    private void Awake()
    {
        _workerController = GetComponent<WorkerController>();
        _workerPresenter = GetComponent<WorkerPresenter>();
    }

    private IEnumerator SavePositionCoroutine()
    {
        while (true)
        {
            _data.PositionX.Value = transform.position.x;
            _data.PositionZ.Value = transform.position.z;

            yield return _saveDelay;
        }
    }

    public void SetWorkerManager(WorkerManager manager)
    {
        _workerManager = manager;
    }

    public void SetWorkerData(WorkerData data)
    {
        _data = data;
        StartCoroutine(SavePositionCoroutine());

        if(data.Rank == 4)
        {
            data.MoveSpeedLv.Value = 6;
            data.MaxCapacityLv.Value = 6;
        }
        else if(data.Rank == 3 && data.MoveSpeedLv.Value == 1)
        {
            data.MoveSpeedLv.Value = 3;
            data.MaxCapacityLv.Value = 2;
        }
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
