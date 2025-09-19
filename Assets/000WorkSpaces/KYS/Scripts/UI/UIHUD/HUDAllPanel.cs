using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KYS
{
    /// <summary>
    /// HUD 레이어 - 재화 표시, 기본 UI 패널 및 버튼들 (View만 사용)
    /// </summary>
    public class HUDAllPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string moneyTextName = "RunMoneyBottonText";
        [SerializeField] private string moneyButtonName = "MoneyButton";
        [SerializeField] private string gemButtonName = "GemButton";
        [SerializeField] private string levelTextName = "LevelText";
        [SerializeField] private string settingButtonName = "SettingButton";
        [SerializeField] private string propertyButtonName = "PropertyButton";
        [SerializeField] private string questProgressTextName = "QuestProgressText";
        [SerializeField] private string HRRooomButtonName = "HRRoomButton";
        [SerializeField] private string compossButtonName = "CompossButton";
        [SerializeField] private string StageTransitionPanelButtonName = "StageTransitionPanelButton";
        [SerializeField] private string StoryPanelButtonName = "StoryPanelButton";


        #region UI Element References (동적 참조)
        // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);
        private TextMeshProUGUI levelText => GetUI<TextMeshProUGUI>(levelTextName);
        private TextMeshProUGUI questProgressText => GetUI<TextMeshProUGUI>(questProgressTextName);

        private GameObject propertyButton => GetUI(propertyButtonName);
        
        private GameObject HRRoomButton => GetUI(HRRooomButtonName);
        private GameObject StageTransitionPanelButton => GetUI(StageTransitionPanelButtonName);
        private GameObject StoryPanelButton => GetUI(StoryPanelButtonName);
        private GameObject SettingButton => GetUI(settingButtonName);
        private GameObject moneyButton => GetUI(moneyButtonName);
        private GameObject gemButton => GetUI(gemButtonName);
        private GameObject compossButton => GetUI(compossButtonName);
        #endregion

        private bool isInitialized = false;

        protected override void Awake()
        {
            base.Awake();

            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.HUD;
            }

            // UIManager의 초기 표시 설정에 따라 활성화/비활성화 처리
            // UIManager에서 InitializeHUDElements()에서 처리됨
        }

        private void OnEnable()
        {
            // UI 요소들이 활성화된 후 완전 초기화
            StartCoroutine(CompleteInitializationAfterDelay());
        }

        private System.Collections.IEnumerator CompleteInitializationAfterDelay()
        {
            // 한 프레임 대기하여 UI 요소들이 완전히 활성화된 후 초기화
            yield return new WaitForEndOfFrame();
            
            if (!isInitialized)
            {
                // 여기서 완전한 초기화 수행
                CompleteInitialization();
            }
        }

        private void CompleteInitialization()
        {
            // BaseUI의 Initialize 대신 여기서 모든 초기화 수행
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

            isInitialized = true;
            Debug.Log("[HUDAllPanel] HUD 완전 초기화 완료");
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

            // OnEnable에서 CompleteInitialization이 호출되므로 여기서는 기본 초기화만
            // 실제 초기화는 CompleteInitialization()에서 수행
            Debug.Log("[HUDAllPanel] Initialize 호출됨 (CompleteInitialization에서 실제 초기화 수행)");
        }

        public override void Cleanup()
        {
            // ObservableProperty 구독 해제
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);

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
            Debug.Log($"[HUDAllPanel] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                Debug.Log($"[HUDAllPanel] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var settingEventHandler = GetEventWithSFX(settingButtonName, "SFX_ButtonClick");
            if (settingEventHandler != null)
            {
                settingEventHandler.Click += (data) => OnSettingButtonClicked();
            }

            var PropertyEventHandler = GetEventWithSFX(propertyButtonName, "SFX_ButtonClick");
            if (PropertyEventHandler != null)
            {
                PropertyEventHandler.Click += (data) => OnPropertyButtonClicked();
            }

            var HRRooomEventHandler = GetEventWithSFX(HRRooomButtonName, "SFX_ButtonClick");
            if (HRRooomEventHandler != null)
            {
                HRRooomEventHandler.Click += (data) => OnHRRoomButtonClicked();
            }

            var StageTransitionEventHandler = GetEventWithSFX(StageTransitionPanelButtonName, "SFX_ButtonClick");
            if (StageTransitionEventHandler != null)
            {
                StageTransitionEventHandler.Click += (data) => OnStageTransitionPanelButtonClicked();
            }

            var StoryPanelEventHandler = GetEventWithSFX(StoryPanelButtonName, "SFX_ButtonClick");
            if (StoryPanelEventHandler != null)
            {
                StoryPanelEventHandler.Click += OnStoryPanelButtonClicked;
            }


            // CompossButton 설정 - 누르고 있을 때 기능 (커스텀 효과음)
            var compossEventHandler = GetEvent(compossButtonName);
            if (compossEventHandler != null)
            {
                compossEventHandler.TouchStart += (data) => 
                {
                    // 컴퍼스 버튼 전용 효과음 재생
                    //PlayClickSound("SFX_Compass_Start");
                    OnCompossButtonPressed();
                };
                compossEventHandler.TouchEnd += (data) => 
                {
                    // 컴퍼스 버튼 해제 효과음 재생
                    //PlayClickSound("SFX_Compass_End");
                    OnCompossButtonReleased();
                };
                compossEventHandler.LongPress += (data) => 
                {
                    // 컴퍼스 롱프레스 효과음 재생
                    //PlayClickSound("SFX_Compass_LongPress");
                    OnCompossButtonLongPressed();
                };
            }

            isButtonsSetup = true; // 설정 완료 플래그
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

        private void OnStageTransitionPanelButtonClicked()
        {


            if (UIManager.Instance == null)
            {
                Debug.LogError("[HUDAllPanel] UIManager.Instance가 null입니다!");
                return;
            }

            // 이미 TitlePanel이 열려있는지 확인
            var existingPanels = UIManager.Instance.GetUIsByLayer(UILayerType.Panel);
            foreach (var panel in existingPanels)
            {
                if (panel is StageTransitionPanel)
                {

                    return;
                }
            }

            // 인벤토리 관련 로직 추가

            UIManager.Instance.ShowPanelAsync<StageTransitionPanel>((panel) =>
            {
                if (panel != null)
                {


                }
                else
                {
                    Debug.LogError("[HUDAllPanel] TitlePanel 열기 실패");
                }
            });
        }

        private void OnStoryPanelButtonClicked(PointerEventData data)
        {

            if (UIManager.Instance == null)
            {
                Debug.LogError("[HUDAllPanel] UIManager.Instance가 null입니다!");
                return;
            }
            // 이미 TitlePanel이 열려있는지 확인
            var existingPanels = UIManager.Instance.GetUIsByLayer(UILayerType.Panel);
            foreach (var panel in existingPanels)
            {
                if (panel is StoryPanel)
                {

                    return;
                }
            }
            // 인벤토리 관련 로직 추가
            UIManager.Instance.ShowPanelAsync<StoryPanel>((panel) =>
            {
                if (panel != null)
                {

                }
                else
                {
                    Debug.LogError("[HUDAllPanel] TitlePanel 열기 실패");
                }
            });
        }



        #endregion

        #region CompossButton Event Handlers


        private float compossButtonPressStartTime = 0f;
        private Coroutine compossButtonHoldCoroutine;

        private void OnCompossButtonPressed()
        {
            //Debug.Log("[HUDAllPanel] CompossButton 눌림");

            compossButtonPressStartTime = Time.time;

            StartCompossButtonHoldEffect();


            // 버튼을 누르고 있을 때의 효과 시작

        }

        private void OnCompossButtonReleased()
        {
            //Debug.Log("[HUDAllPanel] CompossButton 해제됨");



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

        [ContextMenu("일반 모드로 전환 (모든 버튼 표시)")]
        public void SwitchToNormalMode()
        {
            if (propertyButton != null)
            {
                propertyButton.SetActive(true);
            }
            if (HRRoomButton != null)
            {
                HRRoomButton.SetActive(true);
            }
            if (SettingButton != null)
            {
                SettingButton.SetActive(true);
            }
            if (gemButton != null)
            {
                gemButton.SetActive(true);
            }
            if (moneyButton != null)
            {
                moneyButton.SetActive(true);
            }
            if (compossButton != null)
            {
                compossButton.SetActive(true);
            }

        }

        [ContextMenu("튜토리얼 모드로 전환 (일부 버튼 숨김)")]
        public void SwitchToTutorialMode()
        {
            if (propertyButton != null)
            {
                propertyButton.SetActive(false);
            }
            if (HRRoomButton != null)
            {
                HRRoomButton.SetActive(false);
            }
  
            if (SettingButton != null)
            {
                SettingButton.SetActive(false);
            }
            if (gemButton != null)
            {
                gemButton.SetActive(false);
            }
            if (moneyButton != null)
            {
                moneyButton.SetActive(false);
            }
            if(compossButton != null)
            {
                compossButton.SetActive(false);
            }
        }

        [ContextMenu("튜토리얼 진행후 (돈이랑 잼 활성화)")]
        public void SwitchToTutorialProgressMode()
        {


            if (propertyButton != null)
            {
                propertyButton.SetActive(false);
            }
            if (HRRoomButton != null)
            {
                HRRoomButton.SetActive(false);
            }
 
            if (SettingButton != null)
            {
                SettingButton.SetActive(false);
            }
            if (gemButton != null)
            {
                gemButton.SetActive(true);
            }
            if (moneyButton != null)
            {
                moneyButton.SetActive(true);
            }
            if (compossButton != null)
            {
                compossButton.SetActive(false);
            }

        }

        [ContextMenu("모든 버튼 숨기기")]
        public void HindAllHUD()
        {
            GameObject.SetActive(false);
        }

        [ContextMenu("모든 버튼 보이기")]
        public void ShowAllHUD()
        {
            GameObject.SetActive(true);
        }


        #region Debug Methods



        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[HUDAllPanel] UI 요소 정보:");
            Debug.Log($"  - moneyText: {moneyTextName} -> {(moneyText != null ? "찾음" : "없음")}");
            Debug.Log($"  - levelText: {levelTextName} -> {(levelText != null ? "찾음" : "없음")}");
            Debug.Log($"  - settingButton: {settingButtonName} -> {(GetUI<UnityEngine.UI.Button>(settingButtonName) != null ? "찾음" : "없음")}");
            Debug.Log($"  - PropertyButton: {propertyButtonName} -> {(GetUI<UnityEngine.UI.Button>(propertyButtonName) != null ? "찾음" : "없음")}");
            Debug.Log($"  - HRRoomButton: {HRRooomButtonName} -> {(GetUI<UnityEngine.UI.Button>(HRRooomButtonName) != null ? "찾음" : "없음")}");
            Debug.Log($"  - CompossButton: {compossButtonName} -> {(GetUI<UnityEngine.UI.Button>(compossButtonName) != null ? "찾음" : "없음")}");
            Debug.Log($"  - questProgressText: {questProgressTextName} -> {(questProgressText != null ? "찾음" : "없음")}");
        }

        #endregion
    }
}
