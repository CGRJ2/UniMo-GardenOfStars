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
    private TextMeshProUGUI _runRankText;
    private TextMeshProUGUI _runWorkerCostText;
    private Image _runWorkerImage;

    // 버튼들
    private Button _upgradeBtn;
    private Button _buyBtn;

    public WorkerData Worker => Manager.firebase?.UserData?.CurStageData?.WorkerList?.Get(_workerKey);
    private string _workerKey;

    private int _employCost;
    private int _employBMCost;

    private bool _isBMCost;

    protected override void Awake()
    {
        base.Awake();
        
        // 패널들 초기화
        _upgradeButton = GetUI("UpgradeButton");
        _beforeHireScreen = GetUI("BeforeHireScreen");
        _lockScreen = GetUI("LockScreen");

        // Run 접두사가 붙은 동적 데이터 UI 요소들 초기화
        _runWorkerNameText = GetUI<TextMeshProUGUI>("RunWorkerNameText");
        _runRankText = GetUI<TextMeshProUGUI>("RunRankText");
        _runWorkerCostText = GetUI<TextMeshProUGUI>("RunWorkerCostText");
        _runWorkerImage = GetUI<Image>("RunWorkerImage");

        // 버튼들 초기화
        _upgradeBtn = GetUI<Button>("UpgradeButton");
        _buyBtn = GetUI<Button>("BuyButton");

        // 디버그: 버튼 초기화 확인
        Debug.Log($"[WorkerPanel] _upgradeBtn: {_upgradeBtn != null}");
        Debug.Log($"[WorkerPanel] _buyBtn: {_buyBtn != null}");

        // 버튼 이벤트 등록
        if (_upgradeBtn != null) _upgradeBtn.onClick.AddListener(OnUpgradeClick);
        if (_buyBtn != null) _buyBtn.onClick.AddListener(OnBuyClick);
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

        WorkerDataCsv _workerData = Manager.data.Worker.Values[_workerKey];
        CharacterDataCsv _characterData = Manager.data.Character.Values[$"{_workerKey}_{Manager.firebase.UserData.CurStage.Value}"];

        // 워커 이름 설정
        if (_runWorkerNameText != null)
        {
            _runWorkerNameText.text = _characterData.Name_Kr;
        }

        //// 워커 이미지 설정
        if (_runWorkerImage != null && _characterData.Sprite != null)
        {
            _runWorkerImage.sprite = _characterData.Sprite;
        }

        // 워커 등급 설정
        var runWorkerRankText = GetUI<TextMeshProUGUI>("RunRankText");
        if (runWorkerRankText != null)
        {
            _runRankText.text = ((CharacterRanks)_workerData.Rank).ToString();
        }
        
        // 고용 비용 설정
        if (Manager.data?.WorkerEmployCost?.Values != null)
        {
            string key = $"{_workerKey}_{Manager.firebase.UserData.CurStage.Value}";

            _employCost = Manager.data.WorkerEmployCost.Values[key].Cost;
            _employBMCost = Manager.data.WorkerEmployCost.Values[key].BMCost;

            
            if (_runWorkerCostText != null)
            {
                if (_employCost == -1)
                {
                    _isBMCost = true;
                    _runWorkerCostText.text = _employBMCost.ToString();
                }
                else
                {
                    _runWorkerCostText.text = _employCost.ToString();
                }
            }
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
                break;
                
            case WorkerPanelStates.Purchase:
                if (_beforeHireScreen != null) _beforeHireScreen.SetActive(true);
                break;
                
            case WorkerPanelStates.Locked:
                if (_lockScreen != null) _lockScreen.SetActive(true);
                break;
        }
    }

    private void OnUpgradeClick()
    {
        ShowUpgradePopUp();
    }

    private void OnBuyClick()
    {
        if (_isBMCost)
        {
            if(Manager.player.Data.Gem.Value < _employBMCost)
            {
                Debug.LogWarning("유료 재화 잔액이 부족합니다.");
                return;
            }

            if (Manager.player.Data.Gem.IsInUpdate)
            {
                Debug.LogWarning("유료 재화 갱신 중입니다.");
                return;
            }

            Manager.player.Data.Gem.Subscribe(EmployWorker);
            Manager.player.Data.Gem.Value -= _employBMCost;
        }
        else
        {
            if (Manager.player.Data.Money.Value < _employCost)
            {
                Debug.LogWarning("인게임 머니 잔액이 부족합니다.");
                return;
            }

            Manager.player.Data.Money.Value -= _employCost;

            EmployWorker();
        }
    }

    private void EmployWorker(int value = 0)
    {
        Manager.firebase.UserData.CurStageData.WorkerList.Add(_workerKey);

        // 구매 후 상태를 Upgrade로 변경
        SetInfo(WorkerPanelStates.Upgrade);

        // Presenter에 데이터 변경 알림
        _presenter?.OnWorkerDataChanged();

        if (_isBMCost)
        {
            Manager.player.Data.Gem.Unsubscribe(EmployWorker);
        }
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
