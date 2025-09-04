using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class PlayerUpgradePanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string titleTextName = "TitleText";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string upgradeListName = "UpgradeList";
        [SerializeField] private string playerLevelTextName = "PlayerLevelText";
        [SerializeField] private string playerExpTextName = "PlayerExpText";
        [SerializeField] private string playerMoneyTextName = "PlayerMoneyText";
        [SerializeField] private string moneyTextName = "RunMoneyBottonText";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private Transform upgradeList => GetUI<Transform>(upgradeListName);
        private TextMeshProUGUI playerLevelText => GetUI<TextMeshProUGUI>(playerLevelTextName);
        private TextMeshProUGUI playerExpText => GetUI<TextMeshProUGUI>(playerExpTextName);
        private TextMeshProUGUI playerMoneyText => GetUI<TextMeshProUGUI>(playerMoneyTextName);
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);
        [Header("Player Info")]
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int playerExp = 0;
        [SerializeField] private int playerMoney = 1000;

        // 추가 변수 선언
        private int currentMoney = 0;

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_player_upgrade_title",
                "ui_close_button",
                "ui_player_level_label",
                "ui_player_exp_label",
                "ui_player_money_label"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            UpdateUI();

            // 초기 값 설정
            UpdateMoney(Manager.player.Data.Money.Value);

            // ObservableProperty 구독 - 실시간 돈 업데이트
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);
        }

        public override void Cleanup()
        {

            // ObservableProperty 구독 해제
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);


            base.Cleanup();
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = GetLocalizedText("ui_player_upgrade_title");

            if (playerLevelText != null)
                playerLevelText.text = $"{GetLocalizedText("ui_player_level_label")}: {playerLevel}";

            if (playerExpText != null)
                playerExpText.text = $"{GetLocalizedText("ui_player_exp_label")}: {playerExp}";


        }

        public void SetPlayerData(int level, int exp, int money)
        {
            playerLevel = level;
            playerExp = exp;
            playerMoney = money;
            UpdateUI();
        }


        public void UpdateMoney(int amount)
        {

            currentMoney = amount; // 현재 값 저장
            if (moneyText != null)
            {


                moneyText.text = $"{amount:N0}";
            }
        }

        /// <summary>
        /// ObservableProperty Money 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnMoneyChanged(int newMoneyValue)
        {
            UpdateMoney(newMoneyValue);
        }




        private void OnCloseButtonClicked()
        {
            Debug.Log("[PlayerUpgradePanel] 패널 닫기");
            Hide();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[PlayerUpgradePanel] TitleText: {titleText != null}");
            Debug.Log($"[PlayerUpgradePanel] CloseButton: {closeButton != null}");
            Debug.Log($"[PlayerUpgradePanel] UpgradeList: {upgradeList != null}");
            Debug.Log($"[PlayerUpgradePanel] PlayerLevelText: {playerLevelText != null}");
            Debug.Log($"[PlayerUpgradePanel] PlayerExpText: {playerExpText != null}");
            Debug.Log($"[PlayerUpgradePanel] PlayerMoneyText: {playerMoneyText != null}");
        }
    }
}
