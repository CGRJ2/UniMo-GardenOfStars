using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class PlayerUpgradePanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string moneyTextName = "RunMoneyButtonText";
        [SerializeField] private string gemTextName = "RunGemButtonText";
        [SerializeField] private string runDetailMoneyTextName = "RunDetailMoneyText";
        [SerializeField] private string runDetailGemTextName = "RunDetailGemText";
        [SerializeField] private string titleTextName = "PlayerUpgradeTitleText";
        [SerializeField] private string assetToggleName = "AssetToggle";
        [SerializeField] private string assetDetailName = "AssetDetail";
        [SerializeField] private string assetToggleBackgroundName = "AssetToggleBackground"; // AssetToggle의 배경 오브젝트
        [SerializeField] private string assetToggleCheckmarkName = "AssetToggleCheckmark"; // AssetToggle의 체크마크 오브젝트

        private Button closeButton => GetUI<Button>(closeButtonName);
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);
        private TextMeshProUGUI gemText => GetUI<TextMeshProUGUI>(gemTextName);
        private TextMeshProUGUI runDetailMoneyText => GetUI<TextMeshProUGUI>(runDetailMoneyTextName);
        private TextMeshProUGUI runDetailGemText => GetUI<TextMeshProUGUI>(runDetailGemTextName);
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);
        private GameObject assetToggle => GetUI(assetToggleName);
        private GameObject assetDetail => GetUI(assetDetailName);
        private GameObject assetToggleBackground => GetUI(assetToggleBackgroundName);
        private GameObject assetToggleCheckmark => GetUI(assetToggleCheckmarkName);
        private GameObject tutoOverlayPanel => GetUI("Panel_TutoOverlay");
        private Button tutoOverlayButton => GetUI<Button>("UpgradeButton_Tuto");

        


        // AssetDetail 토글 상태 관리
        private bool isAssetDetailVisible = false;

        protected override void Awake()
        {
            base.Awake();

            if(TutorialManager.Instance != null)
            {
                BlockAllImages(new() { "UpgradeButton" });
            }

            SetupButtons();
            UpdateUI();

            // 초기 값 설정
            UpdateMoney(Manager.player.Data.Money.Value);
            UpdateGem(Manager.player.Data.Gem.Value);
            UpdateRunDetailMoney(Manager.player.Data.Money.Value);
            UpdateRunDetailGem(Manager.player.Data.Gem.Value);

            // AssetDetail 초기 상태 설정
            if (assetDetail != null)
            {
                assetDetail.SetActive(isAssetDetailVisible);
            }
            
            // 토글 버튼의 초기 시각적 상태 설정
            UpdateToggleVisualState();

            // ObservableProperty 구독 - 실시간 돈 업데이트
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);
            Manager.player.Data.Gem.Subscribe(OnGemChanged);

            Manager.Audio.SfxPlay("DoorBell");

            if (TutorialManager.Instance != null)
            {
                BlockAllImages(new() { "UpgradeButton" });
                TutorialManager.Instance.overlayPanel_PlayerUpradeBtnInTalkPanel.SetActive(false);
                TutorialManager.Instance.overlayPanel_UpradeBtnInPlayerInfoPanel.SetActive(true);
            }
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);
            Manager.player?.Data?.Gem.Unsubscribe(OnGemChanged);
        }

        public override void Initialize()
        {
            base.Initialize();
            
            // 언어 변경 이벤트 구독
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }

        public override void Cleanup()
        {
            // 언어 변경 이벤트 구독 해제
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
            base.Cleanup();
        }

        private void SetupButtons()
        {
            Debug.Log($"[PlayerUpgradePanel] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                Debug.Log($"[PlayerUpgradePanel] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClickBack");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }

            // AssetToggle 설정 - AssetDetail 온오프 기능
            var assetToggleEventHandler = GetEventWithSFX(assetToggleName, "SFX_ButtonClick");
            if (assetToggleEventHandler != null)
            {
                assetToggleEventHandler.Click += (data) => OnAssetToggleClicked();
            }

            isButtonsSetup = true; // 설정 완료 플래그
        }

        private void UpdateUI()
        {
            // 제목 텍스트 업데이트
            if (titleText != null)
            {
                titleText.text = Manager.localization.GetText(titleTextName);
            }
        }

        public void UpdateMoney(int amount)
        {
            if (moneyText != null)
            {
                // BaseUI의 돈 포맷팅 사용 (소수점 없음)
                moneyText.text = FormatMoney(amount, true);
            }
        }

        public void UpdateGem(int amount)
        {
            if (gemText != null)
            {
                // BaseUI의 돈 포맷팅 사용 (소수점 없음)
                gemText.text = FormatMoney(amount, true);
            }
        }

        public void UpdateRunDetailMoney(int amount)
        {
            if (runDetailMoneyText != null)
            {
                // BaseUI의 콤마 포맷팅 사용 (100,000 형식)
                runDetailMoneyText.text = FormatMoneyWithCommas(amount);
            }
        }

        public void UpdateRunDetailGem(int amount)
        {
            if (runDetailGemText != null)
            {
                // BaseUI의 콤마 포맷팅 사용 (100,000 형식)
                runDetailGemText.text = FormatMoneyWithCommas(amount);
            }
        }

        /// <summary>
        /// ObservableProperty Money 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnMoneyChanged(int newMoneyValue)
        {
            UpdateMoney(newMoneyValue);
            UpdateRunDetailMoney(newMoneyValue);
        }

        /// <summary>
        /// ObservableProperty Gem 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnGemChanged(int newGemValue)
        {
            UpdateGem(newGemValue);
            UpdateRunDetailGem(newGemValue);
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[PlayerUpgradePanel] 패널 닫기");
            Manager.ui.ClosePanel();
        }

        private void OnAssetToggleClicked()
        {
            Debug.Log("[PlayerUpgradePanel] AssetToggle 클릭됨");
            Debug.Log($"[PlayerUpgradePanel] assetDetail null 체크: {assetDetail == null}");
            Debug.Log($"[PlayerUpgradePanel] assetDetailName: {assetDetailName}");
            Debug.Log($"[PlayerUpgradePanel] 현재 isAssetDetailVisible: {isAssetDetailVisible}");
            
            // AssetDetail 토글
            ToggleAssetDetail();
        }

        /// <summary>
        /// AssetDetail 표시/숨김 토글
        /// </summary>
        private void ToggleAssetDetail()
        {
            isAssetDetailVisible = !isAssetDetailVisible;
            
            Debug.Log($"[PlayerUpgradePanel] ToggleAssetDetail 호출됨 - 새로운 상태: {isAssetDetailVisible}");
            Debug.Log($"[PlayerUpgradePanel] assetDetail GameObject: {(assetDetail != null ? assetDetail.name : "null")}");
            
            // AssetDetail 토글
            if (assetDetail != null)
            {
                assetDetail.SetActive(isAssetDetailVisible);
                Debug.Log($"[PlayerUpgradePanel] AssetDetail.SetActive({isAssetDetailVisible}) 호출됨");
                Debug.Log($"[PlayerUpgradePanel] AssetDetail 활성 상태: {assetDetail.activeInHierarchy}");
            }
            else
            {
                Debug.LogError($"[PlayerUpgradePanel] AssetDetail을 찾을 수 없습니다! assetDetailName: {assetDetailName}");
            }
            
            // 토글 버튼의 시각적 상태 변경
            UpdateToggleVisualState();
            
            Debug.Log($"[PlayerUpgradePanel] AssetDetail {(isAssetDetailVisible ? "표시" : "숨김")}");
        }

        /// <summary>
        /// 토글 버튼의 시각적 상태 업데이트
        /// </summary>
        private void UpdateToggleVisualState()
        {
            // AssetDetail이 표시될 때: 체크마크 표시, 배경 숨김
            // AssetDetail이 숨겨질 때: 체크마크 숨김, 배경 표시
            if (assetToggleCheckmark != null)
            {
                assetToggleCheckmark.SetActive(isAssetDetailVisible);
                
                // 체크마크가 활성화되면 최상위로 이동
                if (isAssetDetailVisible)
                {
                    assetToggleCheckmark.transform.SetAsLastSibling();
                }
                
                Debug.Log($"[PlayerUpgradePanel] 체크마크 {(isAssetDetailVisible ? "표시" : "숨김")} - 오브젝트: {assetToggleCheckmark.name}, 활성상태: {assetToggleCheckmark.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[PlayerUpgradePanel] assetToggleCheckmark를 찾을 수 없습니다.");
            }
            
            if (assetToggleBackground != null)
            {
                assetToggleBackground.SetActive(!isAssetDetailVisible);
                Debug.Log($"[PlayerUpgradePanel] 배경 {(!isAssetDetailVisible ? "표시" : "숨김")} - 오브젝트: {assetToggleBackground.name}, 활성상태: {assetToggleBackground.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[PlayerUpgradePanel] assetToggleBackground를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 언어 변경 이벤트 핸들러
        /// </summary>
        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            UpdateUI();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[PlayerUpgradePanel] CloseButton: {closeButton != null}");
            Debug.Log($"[PlayerUpgradePanel] MoneyText: {moneyText != null}");
            Debug.Log($"[PlayerUpgradePanel] GemText: {gemText != null}");
            Debug.Log($"[PlayerUpgradePanel] TitleText: {titleText != null}");
            Debug.Log($"[PlayerUpgradePanel] AssetToggle: {assetToggle != null}");
            Debug.Log($"[PlayerUpgradePanel] AssetDetail: {assetDetail != null}");
            Debug.Log($"[PlayerUpgradePanel] AssetToggleBackground: {assetToggleBackground != null}");
            Debug.Log($"[PlayerUpgradePanel] AssetToggleCheckmark: {assetToggleCheckmark != null}");
        }
    }
}
