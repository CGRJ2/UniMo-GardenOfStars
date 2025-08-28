using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class WorkerContent : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string workerNameTextName = "WorkerNameText";
        [SerializeField] private string workerLevelTextName = "WorkerLevelText";
        [SerializeField] private string workerSkillTextName = "WorkerSkillText";
        [SerializeField] private string workerSalaryTextName = "WorkerSalaryText";
        [SerializeField] private string workerImageName = "WorkerImage";
        [SerializeField] private string assignButtonName = "AssignButton";
        [SerializeField] private string fireButtonName = "FireButton";
        [SerializeField] private string upgradeButtonName = "UpgradeButton";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI workerNameText => GetUI<TextMeshProUGUI>(workerNameTextName);
        private TextMeshProUGUI workerLevelText => GetUI<TextMeshProUGUI>(workerLevelTextName);
        private TextMeshProUGUI workerSkillText => GetUI<TextMeshProUGUI>(workerSkillTextName);
        private TextMeshProUGUI workerSalaryText => GetUI<TextMeshProUGUI>(workerSalaryTextName);
        private Image workerImage => GetUI<Image>(workerImageName);
        private Button assignButton => GetUI<Button>(assignButtonName);
        private Button fireButton => GetUI<Button>(fireButtonName);
        private Button upgradeButton => GetUI<Button>(upgradeButtonName);

        [Header("Worker Info")]
        [SerializeField] private string workerName = "";
        [SerializeField] private int workerLevel = 1;
        [SerializeField] private string workerSkill = "";
        [SerializeField] private int workerSalary = 100;
        [SerializeField] private Sprite workerPortrait;
        [SerializeField] private bool isAssigned = false;

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_worker_name_label",
                "ui_worker_level_label",
                "ui_worker_skill_label",
                "ui_worker_salary_label",
                "ui_assign_button",
                "ui_fire_button",
                "ui_upgrade_button"
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
            var assignEventHandler = GetEventWithSFX(assignButtonName, "SFX_ButtonClick");
            if (assignEventHandler != null)
            {
                assignEventHandler.Click += (data) => OnAssignButtonClicked();
            }

            var fireEventHandler = GetEventWithSFX(fireButtonName, "SFX_ButtonClick");
            if (fireEventHandler != null)
            {
                fireEventHandler.Click += (data) => OnFireButtonClicked();
            }

            var upgradeEventHandler = GetEventWithSFX(upgradeButtonName, "SFX_ButtonClick");
            if (upgradeEventHandler != null)
            {
                upgradeEventHandler.Click += (data) => OnUpgradeButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (workerNameText != null)
                workerNameText.text = $"{GetLocalizedText("ui_worker_name_label")}: {workerName}";

            if (workerLevelText != null)
                workerLevelText.text = $"{GetLocalizedText("ui_worker_level_label")}: {workerLevel}";

            if (workerSkillText != null)
                workerSkillText.text = $"{GetLocalizedText("ui_worker_skill_label")}: {workerSkill}";

            if (workerSalaryText != null)
                workerSalaryText.text = $"{GetLocalizedText("ui_worker_salary_label")}: {workerSalary}";

            if (workerImage != null && workerPortrait != null)
                workerImage.sprite = workerPortrait;

            // 버튼 상태 업데이트
            if (assignButton != null)
            {
                assignButton.interactable = !isAssigned;
                assignButton.GetComponentInChildren<TextMeshProUGUI>().text = 
                    isAssigned ? GetLocalizedText("ui_unassign_button") : GetLocalizedText("ui_assign_button");
            }
        }

        public void SetWorkerData(string name, int level, string skill, int salary, Sprite portrait = null)
        {
            workerName = name;
            workerLevel = level;
            workerSkill = skill;
            workerSalary = salary;
            workerPortrait = portrait;
            UpdateUI();
        }

        public void SetAssignmentStatus(bool assigned)
        {
            isAssigned = assigned;
            UpdateUI();
        }

        private void OnAssignButtonClicked()
        {
            isAssigned = !isAssigned;
            UpdateUI();
            Debug.Log($"[WorkerContent] 직원 {workerName} {(isAssigned ? "배치" : "해제")} 완료");
        }

        private void OnFireButtonClicked()
        {
            Debug.Log($"[WorkerContent] 직원 {workerName} 해고");
            Hide();
        }

        private void OnUpgradeButtonClicked()
        {
            workerLevel++;
            workerSalary = Mathf.RoundToInt(workerSalary * 1.2f);
            UpdateUI();
            Debug.Log($"[WorkerContent] 직원 {workerName} 업그레이드 완료 (레벨 {workerLevel})");
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[WorkerContent] WorkerNameText: {workerNameText != null}");
            Debug.Log($"[WorkerContent] WorkerLevelText: {workerLevelText != null}");
            Debug.Log($"[WorkerContent] WorkerSkillText: {workerSkillText != null}");
            Debug.Log($"[WorkerContent] WorkerSalaryText: {workerSalaryText != null}");
            Debug.Log($"[WorkerContent] WorkerImage: {workerImage != null}");
            Debug.Log($"[WorkerContent] AssignButton: {assignButton != null}");
            Debug.Log($"[WorkerContent] FireButton: {fireButton != null}");
            Debug.Log($"[WorkerContent] UpgradeButton: {upgradeButton != null}");
        }
    }
}
