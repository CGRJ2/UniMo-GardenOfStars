using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KYS
{
    public enum PlayerUpgradeStats
    {
        Speed, Capacity, Nego
    }

    public class PlayerUpgradeContent : BaseUI
    {
        [SerializeField] private PlayerUpgradeStats _upgradeTarget;

        private string _upgradeButtonName = "UpgradeButton";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private Button _upgradeButton => GetUI<Button>(_upgradeButtonName);
        private TextMeshProUGUI _costText => GetUI<TextMeshProUGUI>("CostText");
        private TextMeshProUGUI _beforeText => GetUI<TextMeshProUGUI>("RunBeforeUpgradeNum");
        private TextMeshProUGUI _afterText => GetUI<TextMeshProUGUI>("RunAfterUpgradeNum");

        private FirebaseProperty<int> _targerLv;

        private int _cost;
        private float _curStat;
        private float _upgradeStat;

        private bool _isInPregress;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
            UpdateUI();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_upgrade_title",
                "ui_upgrade_description",
                "ui_upgrade_button",
                "ui_cost_label",
                "ui_level_label",
                "ui_effect_label"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var upgradeEventHandler = GetEventWithSFX(_upgradeButtonName, "SFX_ButtonClick");
            if (upgradeEventHandler != null)
            {
                upgradeEventHandler.Click += (data) => OnUpgradeButtonClicked();
            }
        }

        private void UpdateUI()
        {
            Dictionary<string, PlayerUpgradeCostDataCsv> upgradeCostdict = Manager.data.PlayerUpgradeCost.Values;
            Dictionary<string, CharacterLvDataCsv> characterLvDict = Manager.data.CharacterLv.Values;
            PlayerData playerData = Manager.player.Data;

            switch (_upgradeTarget)
            {
                case PlayerUpgradeStats.Speed:
                    _cost = upgradeCostdict[playerData.MoveSpeedLv.Value.ToString()].Speed;
                    _curStat = playerData.MoveSpeed;
                    if (playerData.IsMoveSpeedMaxLv)
                    {
                        _upgradeStat = -1;
                    }
                    else
                    {
                        _upgradeStat = characterLvDict[(playerData.MoveSpeedLv.Value + 3).ToString()].Speed;
                    }

                    _targerLv = playerData.MoveSpeedLv;
                    break;
                case PlayerUpgradeStats.Capacity:
                    _cost = upgradeCostdict[playerData.MaxCapacityLv.Value.ToString()].Capacity;
                    _curStat = playerData.MaxCapacity;
                    if (playerData.IsMaxCapacityMaxLv)
                    {
                        _upgradeStat = -1;
                    }
                    else
                    {
                        _upgradeStat = characterLvDict[(playerData.MaxCapacityLv.Value + 3).ToString()].Capacity;
                    }

                    _targerLv = playerData.MaxCapacityLv;
                    break;
                case PlayerUpgradeStats.Nego:
                    _cost = upgradeCostdict[playerData.NegoLv.Value.ToString()].Nego;
                    _curStat = playerData.Nego;
                    if (playerData.IsNegoMaxLv)
                    {
                        _upgradeStat = -1;
                    }
                    else
                    {
                        _upgradeStat = characterLvDict[(playerData.NegoLv.Value + 1).ToString()].Nego;
                    }

                    _targerLv = playerData.NegoLv;
                    break;
            }

            _beforeText.text = _curStat.ToString();
            if(_upgradeStat == -1)
            {
                _afterText.text = "Max";
                _costText.text = "Max";
            }
            else
            {
                _afterText.text = _upgradeStat.ToString();
                _costText.text = _cost.ToString();
            }

            _upgradeButton.interactable = true;
            _isInPregress = false;
        }

        private void OnUpgradeButtonClicked()
        {
            if (_isInPregress) return;

            if (Manager.player.Data.Money.Value < _cost || _upgradeStat == -1)
            {
                return;
            }

            Manager.Audio.SfxPlay("Money");

            _isInPregress = true;

            _upgradeButton.interactable = false;

            Manager.player.Data.Money.Value -= _cost; 
            _targerLv.Subscribe(EndUpgrade);
            _targerLv.Value++;
        }

        private void EndUpgrade(int value)
        {
            _targerLv.Unsubscribe(EndUpgrade);
            UpdateUI();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[PlayerUpgradeContent] UpgradeButton: {_upgradeButton != null}");
            Debug.Log($"[PlayerUpgradeContent] CostText: {_costText != null}");
        }
    }
}
