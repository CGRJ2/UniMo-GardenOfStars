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
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string hrScrollViewName = "HR Scroll View";
        [SerializeField] private string hrViewportName = "HR Viewport";
        [SerializeField] private string workerUpgradePanelName = "WorkerUpgradePanel";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI hrRoomText => GetUI<TextMeshProUGUI>(hrRoomTextName);
        private TextMeshProUGUI runGemButtonText => GetUI<TextMeshProUGUI>(runGemButtonTextName);
        private TextMeshProUGUI runMoneyButtonText => GetUI<TextMeshProUGUI>(runMoneyButtonTextName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private GameObject hrScrollView => GetUI(hrScrollViewName);
        private GameObject hrViewport => GetUI(hrViewportName);
        private GameObject workerUpgradePanel => GetUI(workerUpgradePanelName);

        [Header("HR Room Settings")]
        [SerializeField] private int currentMoney = 0;

        // WorkerUpgradePresenter 참조
        private WorkerUpgradePresenter _workerUpgradePresenter;


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
            
            // WorkerUpgradePresenter 초기화
            InitializeWorkerUpgradePresenter();
            
            // UI 업데이트
            UpdateUI();

            // 초기 값 설정
            UpdateMoney(Manager.player.Data.Money.Value);

            // ObservableProperty 구독 - 실시간 업데이트
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);
        }

        private void InitializeWorkerUpgradePresenter()
        {
            // WorkerUpgradePanel에서 WorkerUpgradePresenter 컴포넌트 찾기
            if (workerUpgradePanel != null)
            {
                Debug.Log($"[HRRoomPanel] workerUpgradePanel GameObject 이름: {workerUpgradePanel.name}");
                Debug.Log($"[HRRoomPanel] workerUpgradePanel 활성화 상태: {workerUpgradePanel.activeInHierarchy}");
                
                // 모든 WorkerUpgradePresenter 컴포넌트 찾기
                WorkerUpgradePresenter[] allPresenters = FindObjectsOfType<WorkerUpgradePresenter>();
                Debug.Log($"[HRRoomPanel] 씬에서 찾은 WorkerUpgradePresenter 개수: {allPresenters.Length}");
                
                foreach (var presenter in allPresenters)
                {
                    Debug.Log($"[HRRoomPanel] WorkerUpgradePresenter 위치: {presenter.gameObject.name}");
                }
                
                _workerUpgradePresenter = workerUpgradePanel.GetComponent<WorkerUpgradePresenter>();
                
                if (_workerUpgradePresenter == null)
                {
                    Debug.LogError("[HRRoomPanel] WorkerUpgradePresenter 컴포넌트를 찾을 수 없습니다!");
                }
                else
                {
                    Debug.Log("[HRRoomPanel] WorkerUpgradePresenter 컴포넌트를 찾았습니다!");
                }
            }
            else
            {
                Debug.LogError("[HRRoomPanel] workerUpgradePanel이 null입니다!");
            }
        }

        public override void Cleanup()
        {
            // ObservableProperty 구독 해제
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);

            base.Cleanup();
        }

        private void SetupButtons()
        {
            // CloseButton 이벤트 설정
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }
        }

        private void UpdateUI()
        {
            // HRRoomText 업데이트
            if (hrRoomText != null)
                hrRoomText.text = GetLocalizedText("ui_hr_room_title");
        }

        public void UpdateMoney(int amount)
        {
            currentMoney = amount;
            if (runMoneyButtonText != null)
            {
                runMoneyButtonText.text = $"{amount:N0}";
            }
        }

        /// <summary>
        /// ObservableProperty Money 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnMoneyChanged(int newMoneyValue)
        {
            UpdateMoney(newMoneyValue);
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[HRRoomPanel] 패널 닫기");
            Manager.ui.ClosePanel();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[HRRoomPanel] HRRoomText: {hrRoomText != null}");
            Debug.Log($"[HRRoomPanel] RunGemButtonText: {runGemButtonText != null}");
            Debug.Log($"[HRRoomPanel] RunMoneyButtonText: {runMoneyButtonText != null}");
            Debug.Log($"[HRRoomPanel] CloseButton: {closeButton != null}");
            Debug.Log($"[HRRoomPanel] HR Scroll View: {hrScrollView != null}");
            Debug.Log($"[HRRoomPanel] HR Viewport: {hrViewport != null}");
            Debug.Log($"[HRRoomPanel] WorkerUpgradePanel: {workerUpgradePanel != null}");
            Debug.Log($"[HRRoomPanel] WorkerUpgradePresenter: {_workerUpgradePresenter != null}");
        }
    }
}
