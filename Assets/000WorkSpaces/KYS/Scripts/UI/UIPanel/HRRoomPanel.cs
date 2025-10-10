using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class HRRoomPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string hrRoomTextName = "HRRoomText";
        [SerializeField] private string runGemButtonTextName = "RunGemButtonText";
        [SerializeField] private string runMoneyButtonTextName = "RunMoneyButtonText";
        [SerializeField] private string runDetailMoneyTextName = "RunDetailMoneyText";
        [SerializeField] private string runDetailGemTextName = "RunDetailGemText";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string assetToggleName = "AssetToggle";
        [SerializeField] private string assetDetailName = "AssetDetail";
        [SerializeField] private string assetToggleBackgroundName = "AssetToggleBackground"; // AssetToggle의 배경 오브젝트
        [SerializeField] private string assetToggleCheckmarkName = "AssetToggleCheckmark"; // AssetToggle의 체크마크 오브젝트

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI hrRoomText => GetUI<TextMeshProUGUI>(hrRoomTextName);
        private TextMeshProUGUI runGemButtonText => GetUI<TextMeshProUGUI>(runGemButtonTextName);
        private TextMeshProUGUI runMoneyButtonText => GetUI<TextMeshProUGUI>(runMoneyButtonTextName);
        private TextMeshProUGUI runDetailMoneyText => GetUI<TextMeshProUGUI>(runDetailMoneyTextName);
        private TextMeshProUGUI runDetailGemText => GetUI<TextMeshProUGUI>(runDetailGemTextName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private GameObject assetToggle => GetUI(assetToggleName);
        private GameObject assetDetail => GetUI(assetDetailName);
        private GameObject assetToggleBackground => GetUI(assetToggleBackgroundName);
        private GameObject assetToggleCheckmark => GetUI(assetToggleCheckmarkName);

        // 이벤트 핸들러 저장용
        private System.Action<UnityEngine.EventSystems.PointerEventData> closeButtonHandler;

        // AssetDetail 토글 상태 관리
        private bool isAssetDetailVisible = false;

        protected override void Awake()
        {
            base.Awake();
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);
            Manager.player.Data.Gem.Subscribe(OnGemChanged);
            Initialize();

            Manager.Audio.SfxPlay("DoorBell");

            if (TutorialManager.Instance != null)
            {
                BlockAllImages();
                TutorialManager.Instance.overlayPanel_HRPanelBtn.SetActive(true);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);
            Manager.player?.Data?.Gem.Unsubscribe(OnGemChanged);
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_hr_room_title",
                "ui_hire_button",
                "ui_fire_button",
                "ui_close_button",
                "ui_worker_count_label",
                "ui_salary_label"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            
            // UI 업데이트
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
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            Debug.Log($"[HRRoomPanel] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                Debug.Log($"[HRRoomPanel] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

            // CloseButton 이벤트 설정
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
            // HRRoomText 업데이트
            if (hrRoomText != null)
                hrRoomText.text = GetLocalizedText("ui_hr_room_title");
        }

        public void UpdateMoney(int amount)
        {
            //currentMoney = amount;
            if (runMoneyButtonText != null)
            {
                // BaseUI의 돈 포맷팅 사용 (소수점 없음)
                runMoneyButtonText.text = FormatMoney(amount, false);
            }
        }

        public void UpdateGem(int amount)
        {
            if (runGemButtonText != null)
            {
                // BaseUI의 돈 포맷팅 사용 (소수점 없음)
                runGemButtonText.text = FormatMoney(amount, false);
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
            Debug.LogWarning("[HRRoomPanel] 패널 닫기");
            Manager.ui.ClosePanel();
        }

        private void OnAssetToggleClicked()
        {
            Debug.Log("[HRRoomPanel] AssetToggle 클릭됨");
            Debug.Log($"[HRRoomPanel] assetDetail null 체크: {assetDetail == null}");
            Debug.Log($"[HRRoomPanel] assetDetailName: {assetDetailName}");
            Debug.Log($"[HRRoomPanel] 현재 isAssetDetailVisible: {isAssetDetailVisible}");
            
            // AssetDetail 토글
            ToggleAssetDetail();
        }

        /// <summary>
        /// AssetDetail 표시/숨김 토글
        /// </summary>
        private void ToggleAssetDetail()
        {
            isAssetDetailVisible = !isAssetDetailVisible;
            
            Debug.Log($"[HRRoomPanel] ToggleAssetDetail 호출됨 - 새로운 상태: {isAssetDetailVisible}");
            Debug.Log($"[HRRoomPanel] assetDetail GameObject: {(assetDetail != null ? assetDetail.name : "null")}");
            
            // AssetDetail 토글
            if (assetDetail != null)
            {
                assetDetail.SetActive(isAssetDetailVisible);
                Debug.Log($"[HRRoomPanel] AssetDetail.SetActive({isAssetDetailVisible}) 호출됨");
                Debug.Log($"[HRRoomPanel] AssetDetail 활성 상태: {assetDetail.activeInHierarchy}");
            }
            else
            {
                Debug.LogError($"[HRRoomPanel] AssetDetail을 찾을 수 없습니다! assetDetailName: {assetDetailName}");
            }
            
            // 토글 버튼의 시각적 상태 변경
            UpdateToggleVisualState();
            
            Debug.Log($"[HRRoomPanel] AssetDetail {(isAssetDetailVisible ? "표시" : "숨김")}");
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
                
                Debug.Log($"[HRRoomPanel] 체크마크 {(isAssetDetailVisible ? "표시" : "숨김")} - 오브젝트: {assetToggleCheckmark.name}, 활성상태: {assetToggleCheckmark.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[HRRoomPanel] assetToggleCheckmark를 찾을 수 없습니다.");
            }
            
            if (assetToggleBackground != null)
            {
                assetToggleBackground.SetActive(!isAssetDetailVisible);
                Debug.Log($"[HRRoomPanel] 배경 {(!isAssetDetailVisible ? "표시" : "숨김")} - 오브젝트: {assetToggleBackground.name}, 활성상태: {assetToggleBackground.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[HRRoomPanel] assetToggleBackground를 찾을 수 없습니다.");
            }
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[HRRoomPanel] HRRoomText: {hrRoomText != null}");
            Debug.Log($"[HRRoomPanel] RunGemButtonText: {runGemButtonText != null}");
            Debug.Log($"[HRRoomPanel] RunMoneyButtonText: {runMoneyButtonText != null}");
            Debug.Log($"[HRRoomPanel] CloseButton: {closeButton != null}");
            Debug.Log($"[HRRoomPanel] AssetToggle: {assetToggle != null}");
            Debug.Log($"[HRRoomPanel] AssetDetail: {assetDetail != null}");
            Debug.Log($"[HRRoomPanel] AssetToggleBackground: {assetToggleBackground != null}");
            Debug.Log($"[HRRoomPanel] AssetToggleCheckmark: {assetToggleCheckmark != null}");
        }
    }
}
