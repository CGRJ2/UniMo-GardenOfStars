using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] private GameObject _workerPrefab;

    private List<WorkerRuntimeData> _workerList;
    private List<WorkerRuntimeData> _availableWorkerList;

    private void SearchWorkStation()
    {
        // TODO : 건물 정보와 연관 지어서 일할 공간 찾는 로직 작성.
    }

    public void AssignWorker(InteractableBase workStation)
    {
        float minDistance = float.MaxValue;
        WorkerRuntimeData resultWorker = null;

        foreach(WorkerRuntimeData worker in _workerList)
        {
            float distance = Vector3.Distance(worker.transform.position, workStation.transform.position);

            if (minDistance > distance)
            {
                resultWorker = worker;
                minDistance = distance;
            }
        }

        if(resultWorker != null)
        {
            resultWorker.SetWorkstation(workStation);
        }
    }

    public void InstantiateWorker(WorkerData data)
    {
        WorkerRuntimeData worker = Instantiate(_workerPrefab).GetComponent<WorkerRuntimeData>();

        worker.SetWorkerManager(this);
        worker.SetWorkerData(data);

        _workerList.Add(worker);
        _availableWorkerList.Add(worker);

        worker.CurWorkstation.Subscribe(workstation =>
        {
            if(workstation == null)
            {
                _availableWorkerList.Add(worker);
            }
        });
    }
}
