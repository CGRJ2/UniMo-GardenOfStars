using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WorkerUpgradePresenter : KYS.BaseUI
{
    [SerializeField] private GameObject _workerPanelPrefab;

    private List<WorkerPanel> _workerPanelList = new();

    // UI 패널들
    private GameObject _workerUpgradePanel; // WorkerUpgradePanel


    protected override void Awake()
    {
        base.Awake();
        
        // UI 패널들 초기화
        _workerUpgradePanel = GetUI("WorkerUpgradePanel");

        // 워커 패널들 생성
        CreateWorkerPanels();
        
        // 초기 상태 설정
        SetInfo();
    }


    private void CreateWorkerPanels()
    {
        // 기존 패널들 정리
        foreach (var panel in _workerPanelList)
        {
            if (panel != null) Destroy(panel.gameObject);
        }
        _workerPanelList.Clear();

        // 워커 패널 생성
        if (_workerPanelPrefab != null && _workerUpgradePanel != null && Manager.data?.Worker?.Values != null)
        {
            foreach (string key in Manager.data.Worker.Values.Keys.ToList())
            {
                if (!Manager.data.Character.Values.ContainsKey($"{key}_{Manager.firebase.UserData.CurStage.Value}")) continue;

                WorkerPanel workerPanel = Instantiate(_workerPanelPrefab, _workerUpgradePanel.transform).GetComponent<WorkerPanel>();
                
                if (workerPanel != null)
                {
                    // 워커 패널 초기화 (데이터 설정 포함)
                    workerPanel.Init(key, this);
                    _workerPanelList.Add(workerPanel);
                }
            }
        }
        
        Debug.Log($"[WorkerUpgradePresenter] {_workerPanelList.Count}개의 워커 패널 생성 완료");
    }

    public void SetInfo()
    {
        // 각 워커 패널의 상태 업데이트
        UpdateWorkerPanelStates();
    }

    private void UpdateWorkerPanelStates()
    {
        foreach (WorkerPanel panel in _workerPanelList)
        {
            if (panel.Worker != null)
            {
                // 이미 고용된 워커는 업그레이드 가능
                panel.SetInfo(WorkerPanelStates.Upgrade);
                continue;
            }

            int questOrder = Manager.data.WorkerEmployCost.Values[$"{panel.WorkerKey}_{Manager.firebase.UserData.CurStage.Value}"].QuestOrder;

            if (questOrder == 0 || Manager.firebase.UserData.CurStageData.Npc.QuestList.List[questOrder - 1].QuestState.Value == 3)
            {
                // 구매 가능한 워커
                panel.SetInfo(WorkerPanelStates.Purchase);
            }
            else
            {
                panel.SetInfo(WorkerPanelStates.Locked);
            }
        }
    }


    // 워커 데이터 변경 시 호출되는 메서드
    public void OnWorkerDataChanged()
    {
        // 워커 패널 상태 재계산
        SetInfo();
    }

    // 특정 워커의 상태만 업데이트
    public void UpdateSpecificWorker(string workerKey)
    {
        var panel = _workerPanelList.FirstOrDefault(p => p != null && p.Worker?.Id == workerKey);
        if (panel != null)
        {
            // 해당 워커의 상태만 재계산
            if (panel.Worker == null)
            {
                panel.SetInfo(WorkerPanelStates.Purchase);
            }
            else
            {
                panel.SetInfo(WorkerPanelStates.Upgrade);
            }
        }
    }

    // 워커 패널들 새로고침 (데이터 변경 시)
    public void RefreshWorkerPanels()
    {
        CreateWorkerPanels();
        SetInfo();
    }
}
