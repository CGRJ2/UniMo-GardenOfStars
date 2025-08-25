using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerSpawnTest : MonoBehaviour
{
    [SerializeField] private WorkerManager _workerManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("space");
            _workerManager.InstantiateWorker(new WorkerData());
        }
    }
}
