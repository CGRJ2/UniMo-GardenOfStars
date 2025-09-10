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

    // 정보 관련
    private TextMeshProUGUI _workerNameText;
    private TextMeshProUGUI _workerRankText;
    private TextMeshProUGUI _workerPayText;
    private Image _workerImage;

    private Button _workerBtn;
    private TextMeshProUGUI _workerBtnText;

    private GameObject _lockPanel;

    public WorkerData Worker => Manager.firebase.UserData.CurStageData.WorkerList.Get(_workerKey);
    private string _workerKey;

    private int _employCost;

    protected override void Awake()
    {
        base.Awake();
        _workerNameText = GetUI<TextMeshProUGUI>("WorkerNameText");
        _workerRankText = GetUI<TextMeshProUGUI>("WorkerRankText");
        _workerPayText = GetUI<TextMeshProUGUI>("WorkerPayText");

        _workerImage = GetUI<Image>("WorkerImage");

        _workerBtn = GetUI<Button>("WorkerButton");
        _workerBtnText = GetUI<TextMeshProUGUI>("WorkerButtonText");
        _workerBtn.onClick.AddListener(OnClick);
        Manager.firebase.UserData.CurStageData.WorkerList.OnAdded.AddListener(OnWorkerAdded);

        _lockPanel = GetUI("LockPanel");
    }

    public void Init(string key, WorkerUpgradePresenter presenter)
    {
        _presenter = presenter;
        _workerKey = key;
    }

    public void SetInfo(WorkerPanelStates state)
    {
        switch (state)
        {
            case WorkerPanelStates.Upgrade:
                _workerBtnText.text = "업그레이드";
                _workerPayText.gameObject.SetActive(false);
                _lockPanel.SetActive(false);
                break;
            case WorkerPanelStates.Purchase:
                _workerBtnText.text = "고용";
                _workerPayText.gameObject.SetActive(true);
                _employCost = Manager.data.WorkerEmployCost.Values[_workerKey].Cost;
                _workerPayText.text = _employCost.ToString();
                _lockPanel.SetActive(false);
                break;
            case WorkerPanelStates.Locked:
                _workerBtnText.text = "잠금";
                _workerPayText.gameObject.SetActive(false);
                _lockPanel.SetActive(true);

                break;
        }
    }


    private void OnClick()
    {
        _presenter.LockPanel(true);

        if(Worker == null)
        {
            if(Manager.player.Data.Money.Value < _employCost)
            {
                _presenter.LockPanel(false);
                return;
            }

            Manager.player.Data.Money.Value -= _employCost;
            Manager.firebase.UserData.CurStageData.WorkerList.Add(_workerKey);
        }
        else
        {
            ShowUpgradePopUp();
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

        _presenter.SetInfo();
    }
}
