using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] private GameObject _workerPrefab;
    [SerializeField] private float _assignDelay = 3f;

    private List<WorkerRuntimeData> _workerList = new List<WorkerRuntimeData>();
    private List<WorkerRuntimeData> _availableWorkerList = new List<WorkerRuntimeData>();

    // Test용
    public WorkStatoinLists workStatinLists = new();


    private void Start()
    {
        StartCoroutine(AssignWorkerCoroutine());
    }

    // 반환값이 true면 worker를 availableWorker에서 제외하는 등의 로직 실행.
    public bool AssignWorker(WorkerRuntimeData worker)
    {
        if (worker.IsHarvest)
        {
            // 수확 과정에서 할게 없으면 수확 상태 종료
            worker.IsHarvest = SearchHarvestStation(worker);

            if (worker.IsHarvest) return true;
        }

        float minDistance = float.MaxValue;
        IWorkStation workstation = null;

        // 손에 무언가 들고 있다면 투입 영역에 할 거 있는지 탐색
        if (worker.IngrediantStack.Count > 0)
        {
            minDistance = float.MaxValue;
            workstation = null;

            foreach (InsertArea insert in workStatinLists.insertAreas)
            {
                if (!insert.GetWorkableState()) continue;

                if (insert.ownerInstance.originData.RequireProdID != worker.IngrediantStack.Peek().Data.ID) continue;

                float distance = Vector3.Distance(worker.transform.position, insert.transform.position);

                if (minDistance <= distance) continue;

                workstation = insert;
                minDistance = distance;
            }

            if(workstation != null)
            {
                worker.SetWorkstation(workstation);
                return true;
            }
        }

        minDistance = float.MaxValue;
        workstation = null;

        // 작업 영역에 일거리 있는지 탐색
        foreach (WorkArea work in workStatinLists.workAreas)
        {
            if (!work.GetWorkableState() || work.GetReserveState() || work.curWorker != null) continue;

            float distance = Vector3.Distance(worker.transform.position, work.transform.position);

            if (minDistance <= distance) continue;

            workstation = work;
            minDistance = distance;
        }

        if (workstation != null)
        {
            worker.SetWorkstation(workstation);
            return true;
        }

        // 수확할 게 있는지 확인하고 결과 저장.
        worker.IsHarvest = SearchHarvestStation(worker);
        if (worker.IsHarvest) return true;

        return false;
    }

    private bool SearchHarvestStation(WorkerRuntimeData worker)
    {
        if (worker.IngrediantStack.Count >= worker.MaxCapacity) return false;

        float minDistance = float.MaxValue;
        IWorkStation workstation = null;

        // 생산 결과 구역에 수확할게 있는지 확인
        foreach (ProdsArea prod in workStatinLists.prodsAreas)
        {
            if (!prod.GetWorkableState()) continue;

            if (!(worker.IngrediantStack.Count == 0
                || prod.ownerInstance.originData.ProductID == worker.IngrediantStack.Peek().Data.ID)) continue;

            float distance = Vector3.Distance(worker.transform.position, prod.transform.position);

            if (minDistance <= distance) continue;

            workstation = prod;
            minDistance = distance;
        }

        if (workstation != null)
        {
            worker.SetWorkstation(workstation);
            return true;
        }

        minDistance = float.MaxValue;

        // 수확 건물 중에서 수확할게 있는지 확인
        foreach (ProductGenerater gene in workStatinLists.productGeneraters)
        {
            if (!gene.GetWorkableState()) continue;

            if (!(worker.IngrediantStack.Count == 0 
                || gene._SpawnedProduct.Data.ID == worker.IngrediantStack.Peek().Data.ID)) continue;

            float distance = Vector3.Distance(worker.transform.position, gene.transform.position);

            if (minDistance <= distance) continue;

            workstation = gene;
            minDistance = distance;
        }

        if (workstation != null)
        {
            worker.SetWorkstation(workstation);
            return true;
        }

        return false;
    }

    public void InstantiateWorker(WorkerData data)
    {
        WorkerRuntimeData worker = Instantiate(_workerPrefab).GetComponent<WorkerRuntimeData>();

        worker.SetWorkerManager(this);
        worker.SetWorkerData(data);

        _workerList.Add(worker);
        if (!AssignWorker(worker))
        {
            _availableWorkerList.Add(worker);
        }

        worker.CurWorkstation.Subscribe(workstation =>
        {
            if (workstation == null)
            {
                if (!AssignWorker(worker))
                {
                    _availableWorkerList.Add(worker);
                }
            }
        });
    }

    private IEnumerator AssignWorkerCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(_assignDelay);

        while (true)
        {
            yield return delay;

            foreach(WorkerRuntimeData worker in _availableWorkerList.ToList())
            {
                if (AssignWorker(worker))
                {
                    _availableWorkerList.Remove(worker);
                }
            }
        }
    }
}
