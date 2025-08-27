using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class StageTransitionPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string stageNameTextName = "StageNameText";
        [SerializeField] private string progressTextName = "ProgressText";
        [SerializeField] private string questTextName = "QuestText";
        [SerializeField] private string backButtonName = "BackButton";
        [SerializeField] private string menuButtonName = "MenuButton";
        [SerializeField] private string progressBarName = "ProgressBar";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI stageNameText => GetUI<TextMeshProUGUI>(stageNameTextName);
        private TextMeshProUGUI progressText => GetUI<TextMeshProUGUI>(progressTextName);
        private TextMeshProUGUI questText => GetUI<TextMeshProUGUI>(questTextName);
        private Button backButton => GetUI<Button>(backButtonName);
        private Button menuButton => GetUI<Button>(menuButtonName);
        private Slider progressBar => GetUI<Slider>(progressBarName);

        [Header("Stage Info")]
        [SerializeField] private string stageName = "";
        [SerializeField] private float stageProgress = 0f;
        [SerializeField] private string currentQuest = "";
        [SerializeField] private bool canGoBack = true;

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_stage_name_label",
                "ui_progress_label",
                "ui_quest_label",
                "ui_back_button",
                "ui_menu_button"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            UpdateUI();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var backEventHandler = GetEventWithSFX(backButtonName, "SFX_ButtonClick");
            if (backEventHandler != null)
            {
                backEventHandler.Click += (data) => OnBackButtonClicked();
            }

            var menuEventHandler = GetEventWithSFX(menuButtonName, "SFX_ButtonClick");
            if (menuEventHandler != null)
            {
                menuEventHandler.Click += (data) => OnMenuButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (stageNameText != null)
                stageNameText.text = $"{GetLocalizedText("ui_stage_name_label")}: {stageName}";

            if (progressText != null)
                progressText.text = $"{GetLocalizedText("ui_progress_label")}: {stageProgress * 100:F0}%";

            if (questText != null)
                questText.text = $"{GetLocalizedText("ui_quest_label")}: {currentQuest}";

            if (progressBar != null)
                progressBar.value = stageProgress;

            // 뒤로가기 버튼 활성화 상태
            if (backButton != null)
                backButton.interactable = canGoBack;
        }

        public void SetStageData(string name, float progress, string quest, bool canBack = true)
        {
            stageName = name;
            stageProgress = progress;
            currentQuest = quest;
            canGoBack = canBack;
            UpdateUI();
        }

        public void UpdateProgress(float progress)
        {
            stageProgress = Mathf.Clamp01(progress);
            if (progressText != null)
                progressText.text = $"{GetLocalizedText("ui_progress_label")}: {stageProgress * 100:F0}%";
            if (progressBar != null)
                progressBar.value = stageProgress;
        }

        public void UpdateQuest(string quest)
        {
            currentQuest = quest;
            if (questText != null)
                questText.text = $"{GetLocalizedText("ui_quest_label")}: {currentQuest}";
        }

        private void OnBackButtonClicked()
        {
            if (canGoBack)
            {
                Debug.Log("[StageTransitionTopPanel] 이전 스테이지로 이동");
                // 이전 스테이지 이동 로직
            }
        }

        private void OnMenuButtonClicked()
        {
            Debug.Log("[StageTransitionTopPanel] 메뉴 열기");
            // 메뉴 열기 로직
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[StageTransitionTopPanel] StageNameText: {stageNameText != null}");
            Debug.Log($"[StageTransitionTopPanel] ProgressText: {progressText != null}");
            Debug.Log($"[StageTransitionTopPanel] QuestText: {questText != null}");
            Debug.Log($"[StageTransitionTopPanel] BackButton: {backButton != null}");
            Debug.Log($"[StageTransitionTopPanel] MenuButton: {menuButton != null}");
            Debug.Log($"[StageTransitionTopPanel] ProgressBar: {progressBar != null}");
        }
    }
}
