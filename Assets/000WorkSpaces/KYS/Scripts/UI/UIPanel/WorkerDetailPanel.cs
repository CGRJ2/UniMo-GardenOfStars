using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class WorkerDetailPanel : BaseUI
    {
        private string _closeButtonName = "CloseButton";
        private string _speedUpgradeButtonName = "SpeedUpgradeButton";
        private string _capacityUpgradeButtonName = "CapacityUpgradeButton";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private Image _workerImage => GetUI<Image>("WorkerImage");
        private Button _closeButton => GetUI<Button>(_closeButtonName);
        private Button _speedUpgradeButton => GetUI<Button>(_speedUpgradeButtonName);
        private Button _capacityUpgradeButton => GetUI<Button>(_capacityUpgradeButtonName);
        private TextMeshProUGUI _workerNameText => GetUI<TextMeshProUGUI>("WorkerNameText");
        private TextMeshProUGUI _workerRankText => GetUI<TextMeshProUGUI>("WorkerRankText");
        private TextMeshProUGUI _effectRankText => GetUI<TextMeshProUGUI>("RunStatusEffectRankText");
        private TextMeshProUGUI _productSpeedRankText => GetUI<TextMeshProUGUI>("RunProductSpeedRankText");
        private TextMeshProUGUI _curSpeedText => GetUI<TextMeshProUGUI>("RunCurSpeedText");
        private TextMeshProUGUI _curCapacityText => GetUI<TextMeshProUGUI>("RunCurCapacityText");
        private TextMeshProUGUI _upgradeSpeedText => GetUI<TextMeshProUGUI>("RunUpgradeSpeedText");
        private TextMeshProUGUI _upgradeCapacityText => GetUI<TextMeshProUGUI>("RunUpgradeCapacityText");
        private TextMeshProUGUI _upgradeSpeedCostText => GetUI<TextMeshProUGUI>("UpgradeSpeedCostText");
        private TextMeshProUGUI _upgradeCapacityCostText => GetUI<TextMeshProUGUI>("UpgradeCapacityCostText");

        private WorkerData _worker;

        private int _upgradeSpeedCost;
        private int _upgradeCapacityCost;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        public void SetInfo(WorkerData worker)
        {
            _worker = worker;

            _workerRankText.text = ((CharacterRanks)_worker.Rank).ToString();
            Debug.LogWarning(_workerRankText.text);
            _effectRankText.text = _worker.StunRank;
            _productSpeedRankText.text = _worker.ProductionSpeedRank;
            

            UpdateSpeedInfo();
            UpdateCapacityInfo();

            _worker.MoveSpeedLv.Subscribe(UpdateSpeedInfo);
            _worker.MaxCapacityLv.Subscribe(UpdateCapacityInfo);
        }

        private void UpdateSpeedInfo(int value = 0)
        {
            _curSpeedText.text = _worker.MoveSpeed.ToString();

            if (_worker.IsMoveSpeedMaxLv)
            {
                _upgradeSpeedText.text = "Max";
                _upgradeSpeedCostText.text = "Max";
            }
            else
            {
                _upgradeSpeedText.text = Manager.data.CharacterLv
                    .Values[(_worker.MoveSpeedLv.Value + 1).ToString()].Speed.ToString();

                _upgradeSpeedCost = Manager.data.WorkerUpgradeCost.Values[_worker.MoveSpeedLv.Value.ToString()].Speed;

                _upgradeSpeedCostText.text = _upgradeSpeedCost.ToString();
            }

            _speedUpgradeButton.interactable = true;
        }

        private void UpdateCapacityInfo(int value = 0)
        {
            _curCapacityText.text = _worker.MaxCapacity.ToString();

            if (_worker.IsMaxCapacityMaxLv)
            {
                _upgradeCapacityText.text = "Max";
                _upgradeCapacityCostText.text = "Max";
            }
            else
            {
                _upgradeCapacityText.text = Manager.data.CharacterLv
                    .Values[(_worker.MaxCapacityLv.Value + 1).ToString()].Capacity.ToString();

                _upgradeCapacityCost = Manager.data.WorkerUpgradeCost.Values[_worker.MaxCapacityLv.Value.ToString()].Capacity;

                _upgradeCapacityCostText.text = _upgradeCapacityCost.ToString();
            }

            _capacityUpgradeButton.interactable = true;
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var eventHandler = GetEventWithSFX(_closeButtonName, "SFX_ButtonClick");
            if (eventHandler != null)
            {
                eventHandler.Click += (data) => OnCloseButtonClicked();
            }

            eventHandler = GetEventWithSFX(_speedUpgradeButtonName, "SFX_ButtonClick");
            if (eventHandler != null)
            {
                eventHandler.Click += (data) => OnUpgradeSpeedButtonClicked();
            }

            eventHandler = GetEventWithSFX(_capacityUpgradeButtonName, "SFX_ButtonClick");
            if (eventHandler != null)
            {
                eventHandler.Click += (data) => OnUpgradeCapacityButtonClicked();
            }
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[WorkerDetailPanel] 패널 닫기");
            Manager.ui.ClosePopup();
        }

        private void OnUpgradeSpeedButtonClicked()
        {
            if (Manager.player.Data.Money.Value < _upgradeSpeedCost || _worker.IsMoveSpeedMaxLv) return;

            _speedUpgradeButton.interactable = false;

            Manager.player.Data.Money.Value -= _upgradeSpeedCost;
            _worker.MoveSpeedLv.Value++;
        }

        private void OnUpgradeCapacityButtonClicked()
        {
            if (Manager.player.Data.Money.Value < _upgradeCapacityCost || _worker.IsMaxCapacityMaxLv) return;

            _capacityUpgradeButton.interactable = false;

            Manager.player.Data.Money.Value -= _upgradeCapacityCost;
            _worker.MaxCapacityLv.Value++;
        }
    }

    public enum CharacterRanks
    {
        Normal = 1, Rare, Epic, Legendary
    }
}
