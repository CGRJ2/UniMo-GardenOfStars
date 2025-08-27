using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerSpawnTest : MonoBehaviour
{
    [SerializeField] private WorkerManager _workerManager;

    private List<WorkerData> _workerList = new List<WorkerData>();

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (_workerList.Count >= 10)
            {
                Debug.Log("일꾼은 최대 10까지만 소환 가능");
                return;
            }

            if (Manager.player.Data.Money.Value >= 500)
            {
                Manager.player.Data.Money.Value -= 500;

                WorkerData data = new WorkerData();

                _workerManager.InstantiateWorker(data);
                _workerList.Add(data);
            }
            else
            {
                Debug.Log("일꾼 소환 실패 : 잔액 부족");
            }
        });
    }


            
}
