using TMPro;
using UnityEngine;

namespace KYS
{
    /// <summary>
    /// HUD 레이어 - 재화 표시, 기본 UI 패널 및 버튼들 (View만 사용)
    /// </summary>
    public class HUDAllPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string moneyTextName = "RunMoneyBottonText";
        [SerializeField] private string levelTextName = "LevelText";
        [SerializeField] private string settingButtonName = "SettingButton";
        [SerializeField] private string PropertyButtonName = "PropertyButton";
        [SerializeField] private string questProgressTextName = "QuestProgressText";
        [SerializeField] private string HRRooomButtonName = "HRRoomButton";
        [SerializeField] private string compossButtonName = "CompossButton";

        #region UI Element References (동적 참조)
        // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);
        private TextMeshProUGUI levelText => GetUI<TextMeshProUGUI>(levelTextName);
        private TextMeshProUGUI questProgressText => GetUI<TextMeshProUGUI>(questProgressTextName);
        #endregion

        protected override void Awake()
        {
            base.Awake();

            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.HUD;
            }
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[] {
                // 숫자가 포함된 텍스트는 AutoLocalization에서 제외하고 수동으로 관리
                // "hud_money",      // UpdateMoney()에서 수동 관리
                // "hud_level",      // UpdateLevel()에서 수동 관리  
                // "hud_quest_progress", // UpdateQuestProgress()에서 수동 관리
                "hud_menu",
                "hud_inventory",
                "hud_composs_button"
            };
        }

        public override void Initialize()
        {
            base.Initialize();

            SetupButtons();
            SetupAutoLocalization();

            // 언어 변경 이벤트 구독
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }


            // 초기 값 설정
            UpdateMoney(Manager.player.Data.Money.Value);

            // ObservableProperty 구독 - 실시간 돈 업데이트
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);

            //UpdateLevel(1);
            //UpdateQuestProgress("진행 중");

            //Debug.Log("[HUDAllPanel] HUD 초기화 완료");
        }

        public override void Cleanup()
        {
            // ObservableProperty 구독 해제
            Manager.player.Data.Money.Unsubscribe(OnMoneyChanged);

            // 언어 변경 이벤트 구독 해제
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }

            base.Cleanup();
            //Debug.Log("[HUDAllPanel] HUD 정리 완료");
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var settingEventHandler = GetEventWithSFX(settingButtonName, "SFX_ButtonClick");
            if (settingEventHandler != null)
            {
                settingEventHandler.Click += (data) => OnSettingButtonClicked();
            }

            var PropertyEventHandler = GetEventWithSFX(PropertyButtonName, "SFX_ButtonClick");
            if (PropertyEventHandler != null)
            {
                PropertyEventHandler.Click += (data) => OnPropertyButtonClicked();
            }

            var HRRooomEventHandler = GetEventWithSFX(HRRooomButtonName, "SFX_ButtonClick");
            if (HRRooomEventHandler != null)
            {
                HRRooomEventHandler.Click += (data) => OnHRRoomButtonClicked();
            }

            // CompossButton 설정 - 누르고 있을 때 기능
            var compossEventHandler = GetEventWithSFX(compossButtonName, "SFX_ButtonClick");
            if (compossEventHandler != null)
            {
                compossEventHandler.TouchStart += (data) => OnCompossButtonPressed();
                compossEventHandler.TouchEnd += (data) => OnCompossButtonReleased();
                compossEventHandler.LongPress += (data) => OnCompossButtonLongPressed();
            }
        }

        #region UI Update Methods

        public void UpdateMoney(int amount)
        {

            currentMoney = amount; // 현재 값 저장
            if (moneyText != null)
            {


                moneyText.text = $"{amount:N0}";
            }
        }

        public void UpdateLevel(int level)
        {
            currentLevel = level; // 현재 값 저장
            if (levelText != null)
            {
                // 번역된 텍스트에 동적 값 삽입
                string localizedText = GetLocalizedText("hud_level");
                if (string.IsNullOrEmpty(localizedText) || localizedText == "hud_level")
                {
                    localizedText = "레벨"; // 번역이 없으면 기본값 사용
                }
                levelText.text = $"{localizedText}: {level}";
            }
        }

        public void UpdateQuestProgress(string progress)
        {
            currentQuestProgress = progress; // 현재 값 저장
            if (questProgressText != null)
            {
                // 번역된 텍스트에 동적 값 삽입
                string localizedText = GetLocalizedText("hud_quest_progress");
                if (string.IsNullOrEmpty(localizedText) || localizedText == "hud_quest_progress")
                {
                    localizedText = "퀘스트 진행"; // 번역이 없으면 기본값 사용
                }
                questProgressText.text = $"{localizedText}: {progress}";
            }
        }

        #endregion

        #region Language Change Handler

        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            // 언어 변경 시 현재 값으로 텍스트 다시 업데이트
            if (moneyText != null)
            {
                UpdateMoney(GetCurrentMoneyValue());
            }
            if (levelText != null)
            {
                UpdateLevel(GetCurrentLevelValue());
            }
            if (questProgressText != null)
            {
                UpdateQuestProgress(GetCurrentQuestProgress());
            }
        }

        /// <summary>
        /// ObservableProperty Money 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnMoneyChanged(int newMoneyValue)
        {
            UpdateMoney(newMoneyValue);
        }

        // 현재 값들을 저장할 변수들
        private int currentMoney = 1000;
        private int currentLevel = 1;
        private string currentQuestProgress = "진행 중";

        private int GetCurrentMoneyValue() => currentMoney;
        private int GetCurrentLevelValue() => currentLevel;
        private string GetCurrentQuestProgress() => currentQuestProgress;

        #endregion

        #region Event Handlers

        private void OnSettingButtonClicked()
        {
            Debug.Log("[HUDAllPanel] SettingPopUp 버튼 클릭");

            if (UIManager.Instance != null)
            {
                // 메뉴 팝업 열기
                UIManager.Instance.ShowPopUpAsync<SettingPopUp>((popup) =>
                {
                    if (popup != null)
                    {
                        Debug.Log("[HUDAllPanel] SettingPopUp 성공적으로 열림");
                    }
                    else
                    {
                        Debug.LogError("[HUDAllPanel] SettingPopUp 열기 실패");
                    }
                });
            }
            else
            {
                Debug.LogError("[HUDAllPanel] UIManager 인스턴스가 null입니다!");
            }
        }

        private void OnPropertyButtonClicked()
        {
            //Debug.Log("[HUDAllPanel] 인벤토리 버튼 클릭");

            if (UIManager.Instance == null)
            {
                Debug.LogError("[HUDAllPanel] UIManager.Instance가 null입니다!");
                return;
            }

            // 이미 TitlePanel이 열려있는지 확인
            var existingPanels = UIManager.Instance.GetUIsByLayer(UILayerType.Panel);
            foreach (var panel in existingPanels)
            {
                if (panel is PropertyPanel)
                {
                    //Debug.Log("[HUDAllPanel] 이미 TitlePanel이 열려있습니다. 중복 호출 무시");
                    return;
                }
            }


            UIManager.Instance.ShowPanelAsync<PropertyPanel>((panel) =>
            {
                if (panel != null)
                {
                    //Debug.Log("[HUDAllPanel] TitlePanel 성공적으로 열림");

                }
                else
                {
                    Debug.LogError("[HUDAllPanel] TitlePanel 열기 실패");
                }
            });
        }


        private void OnHRRoomButtonClicked()
        {
            //Debug.Log("[HUDAllPanel] 인벤토리 버튼 클릭");

            if (UIManager.Instance == null)
            {
                Debug.LogError("[HUDAllPanel] UIManager.Instance가 null입니다!");
                return;
            }

            // 이미 TitlePanel이 열려있는지 확인
            var existingPanels = UIManager.Instance.GetUIsByLayer(UILayerType.Panel);
            foreach (var panel in existingPanels)
            {
                if (panel is HRRoomPanel)
                {
                    //Debug.Log("[HUDAllPanel] 이미 TitlePanel이 열려있습니다. 중복 호출 무시");
                    return;
                }
            }

            // 인벤토리 관련 로직 추가

            UIManager.Instance.ShowPanelAsync<HRRoomPanel>((panel) =>
            {
                if (panel != null)
                {
                    //Debug.Log("[HUDAllPanel] TitlePanel 성공적으로 열림");

                }
                else
                {
                    Debug.LogError("[HUDAllPanel] TitlePanel 열기 실패");
                }
            });
        }




        #endregion

        #region CompossButton Event Handlers

        private bool isCompossButtonPressed = false;
        private float compossButtonPressStartTime = 0f;
        private Coroutine compossButtonHoldCoroutine;

        private void OnCompossButtonPressed()
        {
            Debug.Log("[HUDAllPanel] CompossButton 눌림");
            isCompossButtonPressed = true;
            compossButtonPressStartTime = Time.time;
            
            // 버튼을 누르고 있을 때의 효과 시작
            StartCompossButtonHoldEffect();
        }

        private void OnCompossButtonReleased()
        {
            Debug.Log("[HUDAllPanel] CompossButton 해제됨");
            isCompossButtonPressed = false;
            
            // 버튼을 놓았을 때의 효과 정리
            StopCompossButtonHoldEffect();
        }

        private void OnCompossButtonLongPressed()
        {
            Debug.Log("[HUDAllPanel] CompossButton 길게 눌림 (롱프레스)");
            // 롱프레스 시 추가 기능 (필요시 구현)
        }

        private void StartCompossButtonHoldEffect()
        {
            // 버튼을 누르고 있을 때 실행될 효과
            Debug.Log("[HUDAllPanel] CompossButton 홀드 효과 시작");

            // 여기에 카메라 이동 로직이 들어갈 예정
            // 예: 특정 NPC로 가상카메라 우선순위 이동
            // MoveCameraToTargetNPC();

            Manager.camera.FocusNPC();
        }

        private void StopCompossButtonHoldEffect()
        {
            // 버튼을 놓았을 때 실행될 효과
            Debug.Log("[HUDAllPanel] CompossButton 홀드 효과 종료");

            // 여기에 카메라 원위치 로직이 들어갈 예정
            // 예: 원래 카메라 우선순위로 복원
            // RestoreOriginalCameraPriority();
            Manager.camera.FocusPlayer();
        }

        // 카메라 이동 기능 (나중에 구현 예정)
        private void MoveCameraToTargetNPC()
        {
            // TODO: 특정 NPC로 가상카메라 우선순위 이동 로직
            Debug.Log("[HUDAllPanel] 카메라를 특정 NPC로 이동 (구현 예정)");
        }

        private void RestoreOriginalCameraPriority()
        {
            // TODO: 원래 카메라 우선순위로 복원 로직
            Debug.Log("[HUDAllPanel] 카메라 우선순위 복원 (구현 예정)");
        }

        #endregion

        #region Debug Methods



        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[HUDAllPanel] UI 요소 정보:");
            Debug.Log($"  - moneyText: {moneyTextName} -> {(moneyText != null ? "찾음" : "없음")}");
            Debug.Log($"  - levelText: {levelTextName} -> {(levelText != null ? "찾음" : "없음")}");
            Debug.Log($"  - settingButton: {settingButtonName} -> {(GetUI<UnityEngine.UI.Button>(settingButtonName) != null ? "찾음" : "없음")}");
            Debug.Log($"  - PropertyButton: {PropertyButtonName} -> {(GetUI<UnityEngine.UI.Button>(PropertyButtonName) != null ? "찾음" : "없음")}");
            Debug.Log($"  - HRRoomButton: {HRRooomButtonName} -> {(GetUI<UnityEngine.UI.Button>(HRRooomButtonName) != null ? "찾음" : "없음")}");
            Debug.Log($"  - CompossButton: {compossButtonName} -> {(GetUI<UnityEngine.UI.Button>(compossButtonName) != null ? "찾음" : "없음")}");
            Debug.Log($"  - questProgressText: {questProgressTextName} -> {(questProgressText != null ? "찾음" : "없음")}");
        }

        #endregion
    }
}
