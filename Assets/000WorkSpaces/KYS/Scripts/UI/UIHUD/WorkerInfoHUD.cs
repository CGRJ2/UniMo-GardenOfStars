using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KYS
{
    /// <summary>
    /// Worker 정보를 실시간으로 표시하는 HUD
    /// </summary>
    public class WorkerInfoHUD : TouchInfoHUD
    {
        [Header("Worker UI Element Names (다국어 지원을 위한 세분화)")]
        [Header("기본 정보 (런타임 데이터)")]
        [SerializeField] private string runWorkerTitleTextName = "RunWorkerTitleText";     // 런타임 - 일꾼별 제목
        [SerializeField] private string runWorkerDescriptionTextName = "RunWorkerDescriptionText"; // 런타임 - 일꾼별 설명
        [SerializeField] private string workerLevelTextName = "WorkerLevelText";           // 정적 - 설명용
        
        [Header("이동속도 관련")]
        [SerializeField] private string moveSpeedLabelName = "MoveSpeedLabel";             // "이동속도:" 라벨
        [SerializeField] private string runWorkerMoveSpeedNumName = "RunWorkerMoveSpeedNum"; // 실시간 숫자
        
        [Header("용량 관련")]
        [SerializeField] private string capacityLabelName = "CapacityLabel";               // "최대용량:" 라벨
        [SerializeField] private string runWorkerCapacityNumName = "RunWorkerCapacityNum"; // 실시간 숫자
        
        [Header("생산속도 관련")]
        [SerializeField] private string productionLabelName = "ProductionLabel";           // "생산속도:" 라벨
        [SerializeField] private string runWorkerProductionNumName = "RunWorkerProductionNum"; // 실시간 숫자
        
        [Header("기절 관련 (세분화)")]
        [SerializeField] private string stunTimeLabelName = "StunTimeLabel";               // "기절시간:" 라벨
        [SerializeField] private string runWorkerStunTimeNumName = "RunWorkerStunTimeNum"; // 실시간 숫자
        [SerializeField] private string stunTimeUnitTextName = "StunTimeUnitText";         // "s" 단위
        [SerializeField] private string stunChanceLabelName = "StunChanceLabel";           // "확률:" 라벨
        [SerializeField] private string runWorkerStunChanceNumName = "RunWorkerStunChanceNum"; // 실시간 숫자
        [SerializeField] private string stunChanceUnitTextName = "StunChanceUnitText";     // "%" 단위
        [SerializeField] private string workerImageName = "WorkerImage";
        [SerializeField] private string hireButtonName = "HireButton";
        [SerializeField] private string fireButtonName = "FireButton";
        [SerializeField] private string upgradeButtonName = "UpgradeButton";

        // UI 요소들 (다국어 지원을 위한 세분화)
        // 기본 정보 (런타임 데이터)
        private TextMeshProUGUI runWorkerTitleText => GetUI<TextMeshProUGUI>(runWorkerTitleTextName);
        private TextMeshProUGUI runWorkerDescriptionText => GetUI<TextMeshProUGUI>(runWorkerDescriptionTextName);
        private TextMeshProUGUI workerLevelText => GetUI<TextMeshProUGUI>(workerLevelTextName);
        
        // 이동속도 관련
        private TextMeshProUGUI moveSpeedLabel => GetUI<TextMeshProUGUI>(moveSpeedLabelName);
        private TextMeshProUGUI runWorkerMoveSpeedNum => GetUI<TextMeshProUGUI>(runWorkerMoveSpeedNumName);
        
        // 용량 관련
        private TextMeshProUGUI capacityLabel => GetUI<TextMeshProUGUI>(capacityLabelName);
        private TextMeshProUGUI runWorkerCapacityNum => GetUI<TextMeshProUGUI>(runWorkerCapacityNumName);
        
        // 생산속도 관련
        private TextMeshProUGUI productionLabel => GetUI<TextMeshProUGUI>(productionLabelName);
        private TextMeshProUGUI runWorkerProductionNum => GetUI<TextMeshProUGUI>(runWorkerProductionNumName);
        
        // 기절 관련 (세분화)
        private TextMeshProUGUI stunTimeLabel => GetUI<TextMeshProUGUI>(stunTimeLabelName);
        private TextMeshProUGUI runWorkerStunTimeNum => GetUI<TextMeshProUGUI>(runWorkerStunTimeNumName);
        private TextMeshProUGUI stunTimeUnitText => GetUI<TextMeshProUGUI>(stunTimeUnitTextName);
        private TextMeshProUGUI stunChanceLabel => GetUI<TextMeshProUGUI>(stunChanceLabelName);
        private TextMeshProUGUI runWorkerStunChanceNum => GetUI<TextMeshProUGUI>(runWorkerStunChanceNumName);
        private TextMeshProUGUI stunChanceUnitText => GetUI<TextMeshProUGUI>(stunChanceUnitTextName);
        
        // 기타 요소들
        private Image workerImage => GetUI<Image>(workerImageName);
        //private Button hireButton => GetUI<Button>(hireButtonName);
        //private Button fireButton => GetUI<Button>(fireButtonName);
        //private Button upgradeButton => GetUI<Button>(upgradeButtonName);

        // 실시간 데이터 연결
        private WorkerRuntimeData connectedWorkerData;
        private List<System.Action> unsubscribeActions = new List<System.Action>();

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "worker_info_title",
                "worker_level",
                "worker_move_speed",
                "worker_capacity",
                "worker_production",
                "worker_stun",
                "worker_hire",
                "worker_fire",
                "worker_upgrade"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupWorkerButtons();
        }

        public override void Cleanup()
        {
            DisconnectFromWorkerData();
            base.Cleanup();
        }

        /// <summary>
        /// Worker 데이터에 실시간 연결
        /// </summary>
        public void ConnectToWorkerData(WorkerRuntimeData workerData)
        {
            // 기존 연결 해제
            DisconnectFromWorkerData();

            connectedWorkerData = workerData;

            if (connectedWorkerData != null)
            {
                // ObservableProperty 이벤트 구독
                SubscribeToWorkerProperties();

                // 초기 UI 업데이트
                UpdateWorkerUI();

                Debug.Log($"[WorkerInfoHUD] Worker 데이터 연결 완료: {connectedWorkerData.Id}");
            }
        }

        /// <summary>
        /// Worker 속성들에 이벤트 구독
        /// </summary>
        private void SubscribeToWorkerProperties()
        {
            if (connectedWorkerData == null) return;

            try
            {
                // WorkerRuntimeData의 _data를 통해 FirebaseProperty에 접근
                // _data는 private이므로 public 메서드나 프로퍼티를 통해 접근해야 함

                // 실제로는 WorkerRuntimeData._data.MoveSpeedLv 같은 FirebaseProperty<long>에 구독
                // 하지만 _data가 private이므로 접근할 수 없음

                // 대안 1: WorkerRuntimeData에 public 이벤트 추가 요청
                // 대안 2: 현재는 주기적 업데이트 방식 사용

                Debug.Log("[WorkerInfoHUD] Worker 속성 구독 완료 (Firebase 직접 구독은 접근 권한 문제로 보류)");

                // 임시로 5초마다 UI 업데이트하는 방식 사용
                StartPeriodicUpdate();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WorkerInfoHUD] Worker 속성 구독 실패: {e.Message}");
            }
        }

        /// <summary>
        /// FirebaseProperty 이벤트 구독 (실제 구조에 맞게 구현 완료)
        /// </summary>
        private System.Action SubscribeToFirebaseProperty<T>(FirebaseProperty<T> firebaseProperty, System.Action<T> onChange)
        {
            // UnityAction으로 래핑
            UnityEngine.Events.UnityAction<T> unityAction = (value) => onChange(value);

            // Firebase 이벤트 구독
            firebaseProperty.Subscribe(unityAction);

            // 구독 해제 액션 반환
            return () => firebaseProperty.Unsubscribe(unityAction);
        }

        /// <summary>
        /// Worker UI 업데이트
        /// </summary>
        private void UpdateWorkerUI()
        {
            if (connectedWorkerData == null) return;

            try
            {
                 // 런타임 Title과 Description 업데이트 (일꾼마다 다름)
                 if (runWorkerTitleText != null)
                     runWorkerTitleText.text = $"{connectedWorkerData.Id}";

                 if (runWorkerDescriptionText != null)
                     runWorkerDescriptionText.text = $"{connectedWorkerData.Id}";

                 // 기본 정보
                 if (workerLevelText != null)
                     workerLevelText.text = $"{connectedWorkerData.Rank}";

                 // 이동속도 (세분화)
                 if (runWorkerMoveSpeedNum != null)
                     runWorkerMoveSpeedNum.text = $"{connectedWorkerData.MoveSpeed:F1}";

                 // 용량 (세분화)
                 if (runWorkerCapacityNum != null)
                     runWorkerCapacityNum.text = $"{connectedWorkerData.MaxCapacity}";

                 // 생산속도 (세분화)
                 if (runWorkerProductionNum != null)
                     runWorkerProductionNum.text = $"{connectedWorkerData.ProductionSpeed:F1}";

                 // 기절 관련 (완전 세분화)
                 if (runWorkerStunTimeNum != null)
                     runWorkerStunTimeNum.text = $"{connectedWorkerData.StunTime:F1}";

                 if (runWorkerStunChanceNum != null)
                     runWorkerStunChanceNum.text = $"{connectedWorkerData.StunChance:F1}";

                // 버튼 상태 업데이트
                UpdateButtonStates();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WorkerInfoHUD] UI 업데이트 실패: {e.Message}");
            }
        }


        /// <summary>
        /// 버튼 상태 업데이트
        /// </summary>
        private void UpdateButtonStates()
        {
            //if (hireButton != null)
            //    hireButton.interactable = true; // TODO: 고용 가능 여부 체크

            //if (fireButton != null)
            //    fireButton.interactable = true; // TODO: 해고 가능 여부 체크

            //if (upgradeButton != null)
            //    upgradeButton.interactable = true; // TODO: 업그레이드 가능 여부 체크
        }

        /// <summary>
        /// Worker 데이터 연결 해제
        /// </summary>
        private void DisconnectFromWorkerData()
        {
            // 주기적 업데이트 중지
            StopPeriodicUpdate();

            // 모든 구독 해제
            foreach (var unsub in unsubscribeActions)
            {
                unsub?.Invoke();
            }
            unsubscribeActions.Clear();

            connectedWorkerData = null;
            Debug.Log("[WorkerInfoHUD] Worker 데이터 연결 해제 완료");
        }

        /// <summary>
        /// 주기적 UI 업데이트 시작 (Firebase 직접 구독 대체 방안)
        /// </summary>
        private void StartPeriodicUpdate()
        {
            StopPeriodicUpdate();
            StartCoroutine(PeriodicUpdateCoroutine());
        }

        /// <summary>
        /// 주기적 UI 업데이트 중지
        /// </summary>
        private void StopPeriodicUpdate()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// 5초마다 UI 업데이트하는 코루틴
        /// </summary>
        private System.Collections.IEnumerator PeriodicUpdateCoroutine()
        {
            while (connectedWorkerData != null)
            {
                UpdateWorkerUI();
                yield return new WaitForSeconds(5f);
            }
        }

                 // 이벤트 핸들러들 (세분화된 요소 적용)
         private void OnMoveSpeedChanged(float newValue)
         {
             if (runWorkerMoveSpeedNum != null)
                 runWorkerMoveSpeedNum.text = $"{newValue:F1}";
         }

         private void OnMaxCapacityChanged(int newValue)
         {
             if (runWorkerCapacityNum != null)
                 runWorkerCapacityNum.text = $"{newValue}";
         }

         private void OnProductionSpeedChanged(float newValue)
         {
             if (runWorkerProductionNum != null)
                 runWorkerProductionNum.text = $"{newValue:F1}";
         }

         private void OnStunTimeChanged(float newValue)
         {
             if (runWorkerStunTimeNum != null)
                 runWorkerStunTimeNum.text = $"{newValue:F1}";
         }

         private void OnStunChanceChanged(float newValue)
         {
             if (runWorkerStunChanceNum != null)
                 runWorkerStunChanceNum.text = $"{newValue:F1}";
         }

        /// <summary>
        /// Worker 버튼 설정
        /// </summary>
        private void SetupWorkerButtons()
        {
            //if (hireButton != null)
            //    hireButton.onClick.AddListener(OnHireButtonClicked);

            //if (fireButton != null)
            //    fireButton.onClick.AddListener(OnFireButtonClicked);

            //if (upgradeButton != null)
            //    upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }

        // 버튼 클릭 이벤트
        private void OnHireButtonClicked()
        {
            Debug.Log("[WorkerInfoHUD] 고용 버튼 클릭");
            // TODO: Worker 고용 로직 구현
        }

        private void OnFireButtonClicked()
        {
            Debug.Log("[WorkerInfoHUD] 해고 버튼 클릭");
            // TODO: Worker 해고 로직 구현
        }

        private void OnUpgradeButtonClicked()
        {
            Debug.Log("[WorkerInfoHUD] 업그레이드 버튼 클릭");
            // TODO: Worker 업그레이드 로직 구현
        }

        /// <summary>
        /// 현재 연결된 Worker 데이터 반환
        /// </summary>
        public WorkerRuntimeData GetConnectedWorkerData()
        {
            return connectedWorkerData;
        }

        /// <summary>
        /// Worker 데이터가 연결되어 있는지 확인
        /// </summary>
        public bool IsConnectedToWorker()
        {
            return connectedWorkerData != null;
        }
    }
}