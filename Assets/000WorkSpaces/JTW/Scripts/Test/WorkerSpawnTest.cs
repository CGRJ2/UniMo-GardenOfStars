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

                Debug.Log("Space");

                WorkerData worker = Manager.firebase.UserData.WorkerList.Get("10101_F");

                if (worker == null)
                {
                    // 테스트를 위한 이벤트 구조.
                    // 실제에서는 UserData가 이미 초기화 되었을테니 OnEnable 같은데서 추가하면 된다.
                    Manager.firebase.UserData.WorkerList.OnAdded.AddListener(WorkerSpawnEvent);

                    WorkerDataJson data = new WorkerDataJson();

                    data.Id = $"10101_F";
                    data.MoveSpeedLv = 1;
                    data.MaxCapacityLv = 1;

                    Manager.firebase.UserData.WorkerList.Add(data);
                }
                else
                {
                    _workerManager.InstantiateWorker(worker);
                }
            }
            else
            {
                Debug.Log("일꾼 소환 실패 : 잔액 부족");
            }
        });
    }
    
    private void WorkerSpawnEvent(WorkerData worker)
    {
        _workerManager.InstantiateWorker(worker);
        Manager.firebase.UserData.WorkerList.OnAdded.RemoveListener(WorkerSpawnEvent);
    }
}
