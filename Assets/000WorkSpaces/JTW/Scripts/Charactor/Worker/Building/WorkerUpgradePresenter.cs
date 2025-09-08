using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WorkerUpgradePresenter : KYS.BaseUI
{
    [SerializeField] private GameObject _workerPanelPrefab;

    private List<WorkerPanel> _workerPanelList = new();

    private GameObject _upgradePanel;
    private GameObject _lockPanel;

    private bool _isLocked;

    protected override void Awake()
    {
        base.Awake();
        _upgradePanel = GetUI("WorkerUpgradePanel");
        _lockPanel = GetUI("LockPanel");

        foreach (string key in Manager.data.Worker.Values.Keys.ToList())
        {
            WorkerPanel workerPanel = Instantiate(_workerPanelPrefab, _upgradePanel.transform).GetComponent<WorkerPanel>();

            workerPanel.Init(key, this);

            _workerPanelList.Add(workerPanel);
        }

        SetInfo();
    }

    public void SetInfo()
    {
        LockPanel(false);

        _isLocked = false;

        foreach (WorkerPanel panel in _workerPanelList)
        {
            if (_isLocked)
            {
                panel.SetInfo(WorkerPanelStates.Locked);
            }
            else if(panel.Worker == null)
            {
                panel.SetInfo(WorkerPanelStates.Purchase);
                _isLocked = true;
            }
            else
            {
                panel.SetInfo(WorkerPanelStates.Upgrade);
            }
        }
    }

    public void LockPanel(bool value)
    {
        _lockPanel.SetActive(value);
    }
}
