using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class HRRoomPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string titleTextName = "TitleText";
        [SerializeField] private string workerListName = "WorkerList";
        [SerializeField] private string hireButtonName = "HireButton";
        [SerializeField] private string fireButtonName = "FireButton";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string workerCountTextName = "WorkerCountText";
        [SerializeField] private string salaryTextName = "SalaryText";
        [SerializeField] private string moneyTextName = "RunMoneyBottonText";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);
        private Transform workerList => GetUI<Transform>(workerListName);
        private Button hireButton => GetUI<Button>(hireButtonName);
        private Button fireButton => GetUI<Button>(fireButtonName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private TextMeshProUGUI workerCountText => GetUI<TextMeshProUGUI>(workerCountTextName);
        private TextMeshProUGUI salaryText => GetUI<TextMeshProUGUI>(salaryTextName);
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);

        [Header("HR Room Settings")]
        [SerializeField] private int currentWorkerCount = 0;
        [SerializeField] private int maxWorkerCount = 10;
        [SerializeField] private int totalSalary = 0;

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
            var hireEventHandler = GetEventWithSFX(hireButtonName, "SFX_ButtonClick");
            if (hireEventHandler != null)
            {
                hireEventHandler.Click += (data) => OnHireButtonClicked();
            }

            var fireEventHandler = GetEventWithSFX(fireButtonName, "SFX_ButtonClick");
            if (fireEventHandler != null)
            {
                fireEventHandler.Click += (data) => OnFireButtonClicked();
            }

            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = GetLocalizedText("ui_hr_room_title");

            if (workerCountText != null)
                workerCountText.text = $"{GetLocalizedText("ui_worker_count_label")}: {currentWorkerCount}/{maxWorkerCount}";

            if (salaryText != null)
                salaryText.text = $"{GetLocalizedText("ui_salary_label")}: {totalSalary}";

            // 버튼 활성화 상태 업데이트
            if (hireButton != null)
                hireButton.interactable = currentWorkerCount < maxWorkerCount;

            if (fireButton != null)
                fireButton.interactable = currentWorkerCount > 0;
        }

        public void SetWorkerData(int currentCount, int maxCount, int salary)
        {
            currentWorkerCount = currentCount;
            maxWorkerCount = maxCount;
            totalSalary = salary;
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

    

        private void OnHireButtonClicked()
        {
            if (currentWorkerCount < maxWorkerCount)
            {
                currentWorkerCount++;
                totalSalary += 100; // 기본 급여
                UpdateUI();
                Debug.Log($"[HRRoomPanel] 직원 고용 완료. 현재 직원 수: {currentWorkerCount}");
            }
        }

        private void OnFireButtonClicked()
        {
            if (currentWorkerCount > 0)
            {
                currentWorkerCount--;
                totalSalary = Mathf.Max(0, totalSalary - 100);
                UpdateUI();
                Debug.Log($"[HRRoomPanel] 직원 해고 완료. 현재 직원 수: {currentWorkerCount}");
            }
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[HRRoomPanel] 패널 닫기");
            Manager.ui.ClosePanel();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[HRRoomPanel] TitleText: {titleText != null}");
            Debug.Log($"[HRRoomPanel] WorkerList: {workerList != null}");
            Debug.Log($"[HRRoomPanel] HireButton: {hireButton != null}");
            Debug.Log($"[HRRoomPanel] FireButton: {fireButton != null}");
            Debug.Log($"[HRRoomPanel] CloseButton: {closeButton != null}");
            Debug.Log($"[HRRoomPanel] WorkerCountText: {workerCountText != null}");
            Debug.Log($"[HRRoomPanel] SalaryText: {salaryText != null}");
        }
    }
}
