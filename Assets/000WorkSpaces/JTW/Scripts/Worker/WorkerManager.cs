using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [SerializeField] private GameObject _workerPrefab;

    private List<WorkerController> _workers;

    private void SearchWorkStation()
    {
        // TODO : �ǹ� ������ ���� ��� ���� ���� ã�� ���� �ۼ�.
    }

    private void AssignWorker(InteractableBase workStation)
    {
        float minDistance = float.MaxValue;
        WorkerController resultWorker = null;

        foreach(WorkerController worker in _workers)
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

    public void AddWorker(WorkerData data)
    {
        WorkerController worker = Instantiate(_workerPrefab).GetComponent<WorkerController>();

        worker.SetWorkerData(data);
        _workers.Add(worker);
    }
}
