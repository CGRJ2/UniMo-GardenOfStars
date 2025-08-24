using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KYS
{
    /// <summary>
    /// HUD 레이어 - 재화 표시, 기본 UI 패널 및 버튼들 (View만 사용)
    /// </summary>
    public class HUDAllPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string moneyTextName = "MoneyText";
        [SerializeField] private string levelTextName = "LevelText";
        [SerializeField] private string menuButtonName = "MenuButton";
        [SerializeField] private string inventoryButtonName = "InventoryButton";
        [SerializeField] private string questProgressTextName = "QuestProgressText";

        #region UI Element References (동적 참조)
        // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);
        private TextMeshProUGUI levelText => GetUI<TextMeshProUGUI>(levelTextName);
        private Button menuButton => GetUI<Button>(menuButtonName);
        private Button inventoryButton => GetUI<Button>(inventoryButtonName);
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
        
        protected override string[] GetAutoLocalizeKeys()
        {
            return new string[] {
                // 숫자가 포함된 텍스트는 AutoLocalization에서 제외하고 수동으로 관리
                // "hud_money",      // UpdateMoney()에서 수동 관리
                // "hud_level",      // UpdateLevel()에서 수동 관리  
                // "hud_quest_progress", // UpdateQuestProgress()에서 수동 관리
                "hud_menu",
                "hud_inventory"
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
            
            // 초기 데이터 설정
            UpdateMoney(1000);
            UpdateLevel(1);
            UpdateQuestProgress("진행 중");
            
            //Debug.Log("[HUDAllPanel] HUD 초기화 완료");
        }
        
        public override void Cleanup()
        {
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
            var menuEventHandler = GetEventWithSFX(menuButtonName, "SFX_ButtonClick");
            if (menuEventHandler != null)
            {
                menuEventHandler.Click += (data) => OnMenuButtonClicked();
            }
            
            var inventoryEventHandler = GetEventWithSFX(inventoryButtonName, "SFX_ButtonClick");
            if (inventoryEventHandler != null)
            {
                inventoryEventHandler.Click += (data) => OnInventoryButtonClicked();
            }
        }
        
        #region UI Update Methods
        
        public void UpdateMoney(int amount)
        {
            currentMoney = amount; // 현재 값 저장
            if (moneyText != null)
            {
                // 번역된 텍스트에 동적 값 삽입
                string localizedText = GetLocalizedText("hud_money");
                if (string.IsNullOrEmpty(localizedText) || localizedText == "hud_money")
                {
                    localizedText = "재화"; // 번역이 없으면 기본값 사용
                }
                moneyText.text = $"{localizedText}: {amount:N0}";
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
        
        // 현재 값들을 저장할 변수들
        private int currentMoney = 1000;
        private int currentLevel = 1;
        private string currentQuestProgress = "진행 중";
        
        private int GetCurrentMoneyValue() => currentMoney;
        private int GetCurrentLevelValue() => currentLevel;
        private string GetCurrentQuestProgress() => currentQuestProgress;
        
        #endregion

        #region Event Handlers

        private void OnMenuButtonClicked()
        {
            //Debug.Log("[HUDAllPanel] 메뉴 버튼 클릭");
            
            if (UIManager.Instance != null)
            {
                // 메뉴 팝업 열기
                UIManager.Instance.ShowPopUpAsync<MenuPopUp>((popup) => {
                    if (popup != null)
                    {
                        //Debug.Log("[HUDAllPanel] MenuPopUp 성공적으로 열림");
                    }
                    else
                    {
                        Debug.LogError("[HUDAllPanel] MenuPopUp 열기 실패");
                    }
                });
            }
            else
            {
                Debug.LogError("[HUDAllPanel] UIManager 인스턴스가 null입니다!");
            }
        }
        
        private void OnInventoryButtonClicked()
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
                if (panel is TitlePanel)
                {
                    //Debug.Log("[HUDAllPanel] 이미 TitlePanel이 열려있습니다. 중복 호출 무시");
                    return;
                }
            }
            
            // 인벤토리 관련 로직 추가
            UIManager.Instance.ShowPanelAsync<TitlePanel>((panel) => {
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

        #region Debug Methods
        
        [ContextMenu("HUDAllPanel 상태 확인")]
        public void CheckHUDAllPanelStatus()
        {
            //Debug.Log($"[HUDAllPanel] GameObject 이름: {gameObject.name}");
            //Debug.Log($"[HUDAllPanel] 활성화 상태: {gameObject.activeInHierarchy}");
            //Debug.Log($"[HUDAllPanel] menuButton: {menuButtonName} -> {(menuButton != null ? "찾음" : "없음")}");
            //Debug.Log($"[HUDAllPanel] inventoryButton: {inventoryButtonName} -> {(inventoryButton != null ? "찾음" : "없음")}");
            //Debug.Log($"[HUDAllPanel] moneyText: {moneyTextName} -> {(moneyText != null ? "찾음" : "없음")}");
            //Debug.Log($"[HUDAllPanel] levelText: {levelTextName} -> {(levelText != null ? "찾음" : "없음")}");
            //Debug.Log($"[HUDAllPanel] questProgressText: {questProgressTextName} -> {(questProgressText != null ? "찾음" : "없음")}");
        }
        
        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            //Debug.Log($"[HUDAllPanel] UI 요소 정보:");
            //Debug.Log($"  - moneyText: {moneyTextName} -> {(moneyText != null ? "찾음" : "없음")}");
            //Debug.Log($"  - levelText: {levelTextName} -> {(levelText != null ? "찾음" : "없음")}");
            //Debug.Log($"  - menuButton: {menuButtonName} -> {(menuButton != null ? "찾음" : "없음")}");
            //Debug.Log($"  - inventoryButton: {inventoryButtonName} -> {(inventoryButton != null ? "찾음" : "없음")}");
            //Debug.Log($"  - questProgressText: {questProgressTextName} -> {(questProgressText != null ? "찾음" : "없음")}");
        }
        
        #endregion
    }
}
