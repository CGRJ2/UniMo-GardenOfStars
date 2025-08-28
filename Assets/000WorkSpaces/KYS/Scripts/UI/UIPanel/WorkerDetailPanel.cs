using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class WorkerDetailPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string workerNameTextName = "WorkerNameText";
        [SerializeField] private string workerLevelTextName = "WorkerLevelText";
        [SerializeField] private string workerSkillTextName = "WorkerSkillText";
        [SerializeField] private string workerSalaryTextName = "WorkerSalaryText";
        [SerializeField] private string workerExpTextName = "WorkerExpText";
        [SerializeField] private string workerHappinessTextName = "WorkerHappinessText";
        [SerializeField] private string workerImageName = "WorkerImage";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string upgradeButtonName = "UpgradeButton";
        [SerializeField] private string fireButtonName = "FireButton";
        [SerializeField] private string assignButtonName = "AssignButton";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI workerNameText => GetUI<TextMeshProUGUI>(workerNameTextName);
        private TextMeshProUGUI workerLevelText => GetUI<TextMeshProUGUI>(workerLevelTextName);
        private TextMeshProUGUI workerSkillText => GetUI<TextMeshProUGUI>(workerSkillTextName);
        private TextMeshProUGUI workerSalaryText => GetUI<TextMeshProUGUI>(workerSalaryTextName);
        private TextMeshProUGUI workerExpText => GetUI<TextMeshProUGUI>(workerExpTextName);
        private TextMeshProUGUI workerHappinessText => GetUI<TextMeshProUGUI>(workerHappinessTextName);
        private Image workerImage => GetUI<Image>(workerImageName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private Button upgradeButton => GetUI<Button>(upgradeButtonName);
        private Button fireButton => GetUI<Button>(fireButtonName);
        private Button assignButton => GetUI<Button>(assignButtonName);

        [Header("Worker Detail Info")]
        [SerializeField] private string workerName = "";
        [SerializeField] private int workerLevel = 1;
        [SerializeField] private string workerSkill = "";
        [SerializeField] private int workerSalary = 100;
        [SerializeField] private int workerExp = 0;
        [SerializeField] private float workerHappiness = 0.8f;
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
                "ui_worker_detail_name_label",
                "ui_worker_detail_level_label",
                "ui_worker_detail_skill_label",
                "ui_worker_detail_salary_label",
                "ui_worker_detail_exp_label",
                "ui_worker_detail_happiness_label",
                "ui_close_button",
                "ui_upgrade_button",
                "ui_fire_button",
                "ui_assign_button"
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
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }

            var upgradeEventHandler = GetEventWithSFX(upgradeButtonName, "SFX_ButtonClick");
            if (upgradeEventHandler != null)
            {
                upgradeEventHandler.Click += (data) => OnUpgradeButtonClicked();
            }

            var fireEventHandler = GetEventWithSFX(fireButtonName, "SFX_ButtonClick");
            if (fireEventHandler != null)
            {
                fireEventHandler.Click += (data) => OnFireButtonClicked();
            }

            var assignEventHandler = GetEventWithSFX(assignButtonName, "SFX_ButtonClick");
            if (assignEventHandler != null)
            {
                assignEventHandler.Click += (data) => OnAssignButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (workerNameText != null)
                workerNameText.text = $"{GetLocalizedText("ui_worker_detail_name_label")}: {workerName}";

            if (workerLevelText != null)
                workerLevelText.text = $"{GetLocalizedText("ui_worker_detail_level_label")}: {workerLevel}";

            if (workerSkillText != null)
                workerSkillText.text = $"{GetLocalizedText("ui_worker_detail_skill_label")}: {workerSkill}";

            if (workerSalaryText != null)
                workerSalaryText.text = $"{GetLocalizedText("ui_worker_detail_salary_label")}: {workerSalary}";

            if (workerExpText != null)
                workerExpText.text = $"{GetLocalizedText("ui_worker_detail_exp_label")}: {workerExp}";

            if (workerHappinessText != null)
                workerHappinessText.text = $"{GetLocalizedText("ui_worker_detail_happiness_label")}: {workerHappiness * 100:F0}%";

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

        public void SetWorkerDetailData(string name, int level, string skill, int salary, int exp, float happiness, Sprite portrait = null)
        {
            workerName = name;
            workerLevel = level;
            workerSkill = skill;
            workerSalary = salary;
            workerExp = exp;
            workerHappiness = happiness;
            workerPortrait = portrait;
            UpdateUI();
        }

        public void SetAssignmentStatus(bool assigned)
        {
            isAssigned = assigned;
            UpdateUI();
        }

        public void UpdateWorkerExp(int exp)
        {
            workerExp = exp;
            if (workerExpText != null)
                workerExpText.text = $"{GetLocalizedText("ui_worker_detail_exp_label")}: {workerExp}";
        }

        public void UpdateWorkerHappiness(float happiness)
        {
            workerHappiness = Mathf.Clamp01(happiness);
            if (workerHappinessText != null)
                workerHappinessText.text = $"{GetLocalizedText("ui_worker_detail_happiness_label")}: {workerHappiness * 100:F0}%";
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[WorkerDetailPanel] 패널 닫기");
            Hide();
        }

        private void OnUpgradeButtonClicked()
        {
            workerLevel++;
            workerSalary = Mathf.RoundToInt(workerSalary * 1.2f);
            workerHappiness = Mathf.Min(1f, workerHappiness + 0.1f);
            UpdateUI();
            Debug.Log($"[WorkerDetailPanel] 직원 {workerName} 업그레이드 완료 (레벨 {workerLevel})");
        }

        private void OnFireButtonClicked()
        {
            Debug.Log($"[WorkerDetailPanel] 직원 {workerName} 해고");
            Hide();
        }

        private void OnAssignButtonClicked()
        {
            isAssigned = !isAssigned;
            UpdateUI();
            Debug.Log($"[WorkerDetailPanel] 직원 {workerName} {(isAssigned ? "배치" : "해제")} 완료");
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[WorkerDetailPanel] WorkerNameText: {workerNameText != null}");
            Debug.Log($"[WorkerDetailPanel] WorkerLevelText: {workerLevelText != null}");
            Debug.Log($"[WorkerDetailPanel] WorkerSkillText: {workerSkillText != null}");
            Debug.Log($"[WorkerDetailPanel] WorkerSalaryText: {workerSalaryText != null}");
            Debug.Log($"[WorkerDetailPanel] WorkerExpText: {workerExpText != null}");
            Debug.Log($"[WorkerDetailPanel] WorkerHappinessText: {workerHappinessText != null}");
            Debug.Log($"[WorkerDetailPanel] WorkerImage: {workerImage != null}");
            Debug.Log($"[WorkerDetailPanel] CloseButton: {closeButton != null}");
            Debug.Log($"[WorkerDetailPanel] UpgradeButton: {upgradeButton != null}");
            Debug.Log($"[WorkerDetailPanel] FireButton: {fireButton != null}");
            Debug.Log($"[WorkerDetailPanel] AssignButton: {assignButton != null}");
        }
    }
}
