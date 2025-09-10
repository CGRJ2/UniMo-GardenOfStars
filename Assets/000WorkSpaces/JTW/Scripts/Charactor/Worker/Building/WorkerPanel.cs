using KYS;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum WorkerPanelStates
{
    Upgrade, Purchase, Locked
}

public class WorkerPanel : KYS.BaseUI
{
    private WorkerUpgradePresenter _presenter;
    private WorkerPanelStates _state;

    // 패널들 (상태별 활성화/비활성화)
    private GameObject _upgradeButton;
    private GameObject _beforeHireScreen;
    private GameObject _lockScreen;

    // Run 접두사가 붙은 동적 데이터 UI 요소들
    private TextMeshProUGUI _runWorkerNameText;
    private TextMeshProUGUI _runLevelText;
    private TextMeshProUGUI _runWorkerCostText;
    private Image _runWorkerImage;

    // 버튼들
    private Button _upgradeBtn;
    private Button _buyBtn;
    private Button _lockBtn;

    public WorkerData Worker => Manager.firebase?.UserData?.CurStageData?.WorkerList?.Get(_workerKey);
    private string _workerKey;

    private int _employCost;

    protected override void Awake()
    {
        base.Awake();
        
        // 패널들 초기화
        _upgradeButton = GetUI("UpgradeButton");
        _beforeHireScreen = GetUI("BeforeHireScreen");
        _lockScreen = GetUI("LockScreen");

        // Run 접두사가 붙은 동적 데이터 UI 요소들 초기화
        _runWorkerNameText = GetUI<TextMeshProUGUI>("RunWorkerNameText");
        _runLevelText = GetUI<TextMeshProUGUI>("RunLevelText");
        _runWorkerCostText = GetUI<TextMeshProUGUI>("RunWorkerCostText");
        _runWorkerImage = GetUI<Image>("RunWorkerImage");

        // 버튼들 초기화
        _upgradeBtn = GetUI<Button>("UpgradeButton");
        _buyBtn = GetUI<Button>("BuyButton");
        _lockBtn = GetUI<Button>("LockButton"); // LockScreen 내의 LockButton

        // 디버그: 버튼 초기화 확인
        Debug.Log($"[WorkerPanel] _upgradeBtn: {_upgradeBtn != null}");
        Debug.Log($"[WorkerPanel] _buyBtn: {_buyBtn != null}");
        Debug.Log($"[WorkerPanel] _lockBtn: {_lockBtn != null}");

        // 버튼 이벤트 등록
        if (_upgradeBtn != null) _upgradeBtn.onClick.AddListener(OnUpgradeClick);
        if (_buyBtn != null) _buyBtn.onClick.AddListener(OnBuyClick);
        if (_lockBtn != null) _lockBtn.onClick.AddListener(OnLockClick);

        // Firebase 데이터가 초기화된 후에만 리스너 등록
        if (Manager.firebase?.UserData?.CurStageData?.WorkerList != null)
        {
            Manager.firebase.UserData.CurStageData.WorkerList.OnAdded.AddListener(OnWorkerAdded);
        }
    }

    public void Init(string key, WorkerUpgradePresenter presenter)
    {
        _presenter = presenter;
        _workerKey = key;
        
        // 워커 데이터 바인딩
        UpdateWorkerData();
        
        // UI 상태 설정 (기본 상태로 초기화)
        SetInfo(WorkerPanelStates.Purchase);
        
        // Firebase 리스너 등록 (Init 시점에서는 데이터가 준비되어 있을 것)
        if (Manager.firebase?.UserData?.CurStageData?.WorkerList != null)
        {
            Manager.firebase.UserData.CurStageData.WorkerList.OnAdded.AddListener(OnWorkerAdded);
        }
    }



    private void UpdateWorkerData()
    {
        // 워커 기본 데이터 가져오기
        if (Manager.data?.Worker?.Values == null || !Manager.data.Worker.Values.ContainsKey(_workerKey))
        {
            Debug.LogWarning($"[WorkerPanel] 워커 데이터를 찾을 수 없습니다. 키: {_workerKey}");
            return;
        }
        
        var workerData = Manager.data.Worker.Values[_workerKey];
        
        // 워커 이름 설정
        //if (_runWorkerNameText != null) 
        //{
        //    _runWorkerNameText.text = workerData.Name;
        //}
        //
        //// 워커 이미지 설정
        //if (_runWorkerImage != null && workerData.Image != null) 
        //{
        //    _runWorkerImage.sprite = workerData.Image;
        //}
        
        // 워커 등급 설정 (Run 접두사가 붙은 등급 텍스트가 있다면)
        var runWorkerRankText = GetUI<TextMeshProUGUI>("RunLevelText");
        if (runWorkerRankText != null)
        {
            runWorkerRankText.text = ((CharacterRanks)workerData.Rank).ToString();
        }
        
        // 고용 비용 설정
        if (Manager.data?.WorkerEmployCost?.Values != null && Manager.data.WorkerEmployCost.Values.ContainsKey(_workerKey))
        {
            _employCost = Manager.data.WorkerEmployCost.Values[_workerKey].Cost;
            if (_runWorkerCostText != null) 
            {
                _runWorkerCostText.text = _employCost.ToString();
            }
        }
        
        // 고용된 워커의 경우 레벨 표시
        if (Worker != null && _runLevelText != null)
        {
            //_runLevelText.text = $"Lv.{Worker.Level}";
        }
        else if (_runLevelText != null)
        {
            // 고용되지 않은 워커는 기본 레벨 표시
            _runLevelText.text = "Lv.1";
        }
    }

    public void SetInfo(WorkerPanelStates state)
    {
        _state = state;
        
        // 모든 패널 비활성화
        if (_upgradeButton != null) _upgradeButton.SetActive(false);
        if (_beforeHireScreen != null) _beforeHireScreen.SetActive(false);
        if (_lockScreen != null) _lockScreen.SetActive(false);

        // 상태에 따른 패널 활성화 및 데이터 업데이트
        switch (state)
        {
            case WorkerPanelStates.Upgrade:
                if (_upgradeButton != null) _upgradeButton.SetActive(true);
                // 고용된 워커의 현재 레벨 표시
                if (Worker != null && _runLevelText != null)
                {
                    //_runLevelText.text = $"Lv.{Worker.Level}";
                }
                break;
                
            case WorkerPanelStates.Purchase:
                if (_beforeHireScreen != null) _beforeHireScreen.SetActive(true);
                // 고용 비용 업데이트
                if (_runWorkerCostText != null) _runWorkerCostText.text = _employCost.ToString();
                // 구매 가능한 워커의 기본 레벨 표시
                if (_runLevelText != null) _runLevelText.text = "Lv.1";
                break;
                
            case WorkerPanelStates.Locked:
                if (_lockScreen != null) _lockScreen.SetActive(true);
                // 잠금된 워커의 기본 레벨 표시
                if (_runLevelText != null) _runLevelText.text = "Lv.1";
                break;
        }
    }


    private void OnUpgradeClick()
    {
        ShowUpgradePopUp();
    }

    private void OnBuyClick()
    {

        Manager.player.Data.Money.Value += 500;

        if (Manager.player.Data.Money.Value < _employCost)
        {
            return;
        }
       
        Manager.player.Data.Money.Value -= _employCost;
        
        // Firebase에 워커 추가 (안전한 접근)
        if (Manager.firebase?.UserData?.CurStageData?.WorkerList != null)
        {
            Manager.firebase.UserData.CurStageData.WorkerList.Add(_workerKey);
        }
        
        // 구매 후 상태를 Upgrade로 변경
        SetInfo(WorkerPanelStates.Upgrade);
        
        // Presenter에 데이터 변경 알림
        _presenter?.OnWorkerDataChanged();
    }

    private void OnLockClick()
    {
        // 잠금 상태에서의 클릭 처리 (예: 건물 해제 등)
        Debug.Log($"[WorkerPanel] Locked worker clicked: {_workerKey}");
    }

    private async Task ShowUpgradePopUp()
    {
        GameObject obj = await Manager.ui.ShowPopUpAsync<WorkerDetailPanel>();

        WorkerDetailPanel panel = obj.GetComponent<WorkerDetailPanel>();

        panel.SetInfo(Worker);

        _presenter.SetInfo();
    }

    private void OnWorkerAdded(WorkerData worker)
    {
        if (worker.Id != _workerKey) return;

        // 워커 데이터 업데이트
        UpdateWorkerData();
        
        // 상태를 Upgrade로 변경
        SetInfo(WorkerPanelStates.Upgrade);
        
        // 프레젠터에 워커 데이터 변경 알림
        _presenter.OnWorkerDataChanged();
    }
}
