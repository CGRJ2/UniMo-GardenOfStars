using Cinemachine;
using System.Collections;
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

        [Header("Compass Camera Settings")]
        [SerializeField] private float compassCameraMoveWaitTime = 1.0f; // 카메라 이동 대기 시간


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

        // 카메라 관련 변수들
        private CinemachineBrain cineBrain;
        private Coroutine compassCameraCoroutine;
        private bool isCompassCameraActive = false;
        private CompassMessagePopup currentCompassPopup = null; // 현재 열린 Compass 팝업 추적
        private bool isCompassButtonPressed = false; // 버튼 누름 상태 추적
        private float lastCompassButtonTime = 0f; // 마지막 버튼 누름 시간
        private const float COMPASS_BUTTON_COOLDOWN = 0.5f; // 버튼 쿨다운 시간

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

            // CinemachineBrain 초기화
            if (Camera.main != null)
            {
                cineBrain = Camera.main.GetComponent<CinemachineBrain>();
                if (cineBrain != null)
                {
                    Debug.Log($"[HUDAllPanel] CinemachineBrain 초기화 완료 - 카메라: {Camera.main.name}");
                }
                else
                {
                    Debug.LogWarning($"[HUDAllPanel] 메인 카메라({Camera.main.name})에 CinemachineBrain이 없습니다.");
                }
            }
            else
            {
                Debug.LogError("[HUDAllPanel] Camera.main이 null입니다.");
            }

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

            // Compass 카메라 코루틴 정리
            if (compassCameraCoroutine != null)
            {
                StopCoroutine(compassCameraCoroutine);
                compassCameraCoroutine = null;
            }

            // Compass 팝업 정리
            if (currentCompassPopup != null && currentCompassPopup.gameObject != null)
            {
                Manager.ui.ClosePopup();
            }
            currentCompassPopup = null;

            // Compass 버튼 상태 초기화
            isCompassButtonPressed = false;

            // 카메라 상태 초기화
            if (isCompassCameraActive)
            {
                Manager.camera.FocusPlayer();
                isCompassCameraActive = false;
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
                // BaseUI의 돈 포맷팅 사용 (소수점 없음)
                moneyText.text = FormatMoney(amount, false);
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

            // 쿨다운 시간 확인
            float currentTime = Time.time;
            if (currentTime - lastCompassButtonTime < COMPASS_BUTTON_COOLDOWN)
            {
                Debug.Log($"[HUDAllPanel] Compass 버튼 쿨다운 중입니다. 남은 시간: {COMPASS_BUTTON_COOLDOWN - (currentTime - lastCompassButtonTime):F2}초");
                return;
            }

            // 이미 버튼이 눌려있으면 중복 실행 방지
            if (isCompassButtonPressed)
            {
                Debug.Log("[HUDAllPanel] Compass 버튼이 이미 눌려있습니다.");
                return;
            }

            // 버튼 상태 설정
            isCompassButtonPressed = true;
            lastCompassButtonTime = currentTime;

            // 기존 팝업이 있으면 닫기 (안전하게 처리)
            if (currentCompassPopup != null)
            {
                Debug.Log("[HUDAllPanel] 기존 Compass 팝업을 닫습니다.");
                if (currentCompassPopup.gameObject != null)
                {
                    Manager.ui.ClosePopup();
                }
                currentCompassPopup = null;
            }

            // 기존 카메라 코루틴이 있으면 정리
            if (compassCameraCoroutine != null)
            {
                Debug.Log("[HUDAllPanel] 기존 카메라 코루틴을 정리합니다.");
                StopCoroutine(compassCameraCoroutine);
                compassCameraCoroutine = null;
            }

            // 카메라 상태 초기화
            if (isCompassCameraActive)
            {
                Debug.Log("[HUDAllPanel] 기존 카메라 상태를 초기화합니다.");
                isCompassCameraActive = false;
                Manager.camera.FocusPlayer();
            }

            // 이전 팝업이 완전히 정리되도록 잠시 대기
            StartCoroutine(StartCompassSequenceWithDelay());
        }

        /// <summary>
        /// 이전 팝업 정리 후 컴퍼스 시퀀스 시작
        /// </summary>
        private IEnumerator StartCompassSequenceWithDelay()
        {
            // 이전 팝업이 완전히 정리되도록 잠시 대기
            yield return new WaitForSeconds(0.1f);
            
            // 버튼이 여전히 눌려있는지 확인
            if (!isCompassButtonPressed)
            {
                Debug.Log("[HUDAllPanel] 대기 중 버튼이 해제되어 시퀀스를 시작하지 않습니다.");
                yield break;
            }
            
            // 카메라 전환 코루틴 시작
            compassCameraCoroutine = StartCoroutine(CompassCameraSequence());
        }

        private void StopCompossButtonHoldEffect()
        {
            // 버튼을 놓았을 때 실행될 효과
            Debug.Log("[HUDAllPanel] CompossButton 홀드 효과 종료");

            // 버튼 상태 초기화
            isCompassButtonPressed = false;

            // 카메라 전환이 진행 중이면 원위치로 복귀
            if (isCompassCameraActive)
            {
                // 플래그를 false로 설정하여 코루틴이 종료되도록 함
                isCompassCameraActive = false;

                // CompassMassagePopup 닫기
                if (currentCompassPopup != null && currentCompassPopup.gameObject != null)
                {
                    Debug.Log("[HUDAllPanel] CompassMassagePopup 닫기");
                    Manager.ui.ClosePopup();
                }
                currentCompassPopup = null;

                // 카메라를 플레이어 포커스로 복귀
            Manager.camera.FocusPlayer();
                Debug.Log("[HUDAllPanel] Compass 카메라 원위치 복귀");
            }
        }

        /// <summary>
        /// Compass 버튼을 눌렀을 때 실행되는 카메라 전환 시퀀스
        /// </summary>
        private IEnumerator CompassCameraSequence()
        {
            isCompassCameraActive = true;
            Debug.Log("[HUDAllPanel] Compass 카메라 전환 시퀀스 시작");

            // 1단계: NPC 포커스 카메라로 전환
            Manager.camera.FocusNPC();
            Debug.Log("[HUDAllPanel] NPC 포커스 카메라로 전환");

            // 2단계: 카메라 전환 완료 대기
            if (cineBrain != null)
            {
                Debug.Log("[HUDAllPanel] CinemachineBrain이 있음 - 카메라 전환 대기 시작");
                Debug.Log($"[HUDAllPanel] 현재 IsBlending 상태: {cineBrain.IsBlending}");
                
                // 카메라 전환이 시작될 때까지 대기
                yield return new WaitUntil(() => cineBrain.IsBlending);
                Debug.Log("[HUDAllPanel] 카메라 전환 시작됨");
                
                // 카메라 전환이 완료될 때까지 대기
                yield return new WaitUntil(() => !cineBrain.IsBlending);
                Debug.Log("[HUDAllPanel] NPC 포커스 카메라 전환 완료");
            }
            else
            {
                Debug.LogWarning("[HUDAllPanel] CinemachineBrain이 null - 대기 시간으로 대체");
                // CinemachineBrain이 없으면 대기 시간으로 대체
                yield return new WaitForSeconds(compassCameraMoveWaitTime);
                Debug.Log("[HUDAllPanel] CinemachineBrain이 없어 대기 시간으로 대체");
            }

            // 버튼이 여전히 눌려있는지 확인
            if (!isCompassButtonPressed)
            {
                Debug.Log("[HUDAllPanel] 버튼이 해제되어 팝업 표시를 건너뜁니다.");
                isCompassCameraActive = false;
                compassCameraCoroutine = null;
                yield break;
            }

            // 3단계: CompassMassagePopup 표시 (동기적으로 대기)
            yield return StartCoroutine(ShowCompassMessagePopupCoroutine());

            // 4단계: 버튼을 놓을 때까지 대기 (무한 대기)
            yield return new WaitUntil(() => !isCompassCameraActive);
            Debug.Log("[HUDAllPanel] Compass 버튼이 해제되어 시퀀스 종료");

            // 시퀀스 완료
            compassCameraCoroutine = null;
        }

        /// <summary>
        /// CompassMassagePopup 표시 (동기적 코루틴)
        /// </summary>
        private IEnumerator ShowCompassMessagePopupCoroutine()
        {
            // 버튼이 여전히 눌려있는지 다시 한번 확인
            if (!isCompassButtonPressed || !isCompassCameraActive)
            {
                Debug.Log("[HUDAllPanel] 팝업 생성 전 버튼 상태 확인 - 버튼이 해제되었거나 카메라가 비활성화됨");
                yield break;
            }

            var npc = Manager.firebase.UserData.CurStageData.Npc;
            string compassMessageNodePrefix = $"Compass_{npc.NpcID.Value}"; // Compass 메시지 노드 접두사
            
            // 동적으로 노드 개수 파악
            int compassMessageNodeCount = GetCompassNodeCount(compassMessageNodePrefix);
            
            // 랜덤 노드 ID 생성
            int randomIndex = Random.Range(1, compassMessageNodeCount + 1); // 1부터 compassMessageNodeCount까지
            string randomNodeId = $"{compassMessageNodePrefix}_{randomIndex}";

            Debug.Log($"[HUDAllPanel] CompassMassagePopup 표시 - 접두사: {compassMessageNodePrefix}, 노드 개수: {compassMessageNodeCount}, 랜덤 노드 ID: {randomNodeId}");

            // 팝업 생성 완료 플래그
            bool popupCreated = false;

            // CompassMessagePopup을 직접 열고 설정
            Manager.ui.ShowPopUpAsync<CompassMessagePopup>(popup =>
            {
                // 팝업이 null이거나 파괴되었는지 확인
                if (popup == null)
                {
                    Debug.LogWarning("[HUDAllPanel] 팝업이 null입니다.");
                    popupCreated = true; // 대기 종료를 위해 플래그 설정
                    return;
                }

                // 버튼 상태 재확인 (팝업 생성 시점에서)
                if (!isCompassButtonPressed || !isCompassCameraActive)
                {
                    Debug.Log("[HUDAllPanel] 팝업 생성 중 버튼 상태 확인 - 팝업 생성 취소");
                    if (popup != null && popup.gameObject != null)
                    {
                        Manager.ui.ClosePopup();
                    }
                    popupCreated = true; // 대기 종료를 위해 플래그 설정
                    return;
                }

                // 현재 팝업 추적
                currentCompassPopup = popup;
                
                // 팝업 위치 설정
                popup.SetCompassPosition(CompassMessagePopup.CompassPositionType.TopRight);
                
                // 노드 ID 설정 (이제 SetCompassNode에서 자동으로 대화 시작)
                popup.SetCompassNode(randomNodeId);
                
                Debug.Log($"[HUDAllPanel] CompassMessagePopup 설정 완료 - 랜덤 노드 ID: {randomNodeId}");
                
                // 팝업 생성 완료 플래그 설정
                popupCreated = true;
            });

            // 팝업이 완전히 생성될 때까지 대기 (최대 2초)
            float waitTime = 0f;
            while (!popupCreated && waitTime < 2f)
            {
                // 버튼 상태 재확인
                if (!isCompassButtonPressed || !isCompassCameraActive)
                {
                    Debug.Log("[HUDAllPanel] 팝업 대기 중 버튼 상태 확인 - 대기 중단");
                    yield break;
                }
                
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }

            if (popupCreated)
            {
                Debug.Log("[HUDAllPanel] CompassMessagePopup 생성 완료 - 다음 단계 진행");
            }
            else
            {
                Debug.LogWarning("[HUDAllPanel] CompassMessagePopup 생성 시간 초과");
            }
        }

        /// <summary>
        /// CompassMassagePopup 표시 (기존 메서드 - 호환성 유지)
        /// </summary>
        private void ShowCompassMessagePopup()
        {
            StartCoroutine(ShowCompassMessagePopupCoroutine());
        }

        /// <summary>
        /// Compass 노드 개수를 동적으로 파악
        /// </summary>
        private int GetCompassNodeCount(string nodePrefix)
        {
            try
            {
                // DataManager에서 Dialogue 데이터 가져오기
                if (Manager.data?.Dialogue?.Values == null)
                {
                    Debug.LogWarning($"[HUDAllPanel] Dialogue 데이터가 아직 로드되지 않았습니다. 접두사: {nodePrefix}");
                    return 1; // 기본값 반환
                }

                int count = 0;
                string searchPattern = nodePrefix + "_";
                
                // Dialogue CSV 데이터에서 해당 접두사로 시작하는 노드 개수 확인
                foreach (var dialogueData in Manager.data.Dialogue.Values.Values)
                {
                    if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.Id) && dialogueData.Id.StartsWith(searchPattern))
                    {
                        count++;
                        Debug.Log($"[HUDAllPanel] Compass 노드 발견: {dialogueData.Id}");
                    }
                }
                
                Debug.Log($"[HUDAllPanel] 접두사 '{nodePrefix}' 기준으로 {count}개의 Compass 노드를 찾았습니다.");
                
                // 노드가 없으면 기본값 1 반환
                return count > 0 ? count : 1;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HUDAllPanel] Compass 노드 개수 파악 중 오류 발생: {e.Message}");
                return 1; // 오류 시 기본값 반환
            }
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
            if (compossButton != null)
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
