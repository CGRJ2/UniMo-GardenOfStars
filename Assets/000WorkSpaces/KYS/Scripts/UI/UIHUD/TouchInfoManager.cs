using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
#if CINEMACHINE
using Cinemachine;
#endif

namespace KYS
{
    /// <summary>
    /// 레이어별 HUD 타입 정의
    /// </summary>
    public enum HUDType
    {
        Building,       // 건물 정보
        Worker,         // 작업자 정보
        Resource,       // 자원 정보
        Facility,       // 시설 정보
        NPC,           // NPC 정보
        Default         // 기본 정보
    }

    /// <summary>
    /// HUD 데이터 구조
    /// </summary>
    [System.Serializable]
    public class HUDData
    {
        public string title;
        public string description;
        public Sprite icon;
        public HUDType hudType;
        public Dictionary<string, object> customData;

        public HUDData(string title, string description, Sprite icon = null, HUDType hudType = HUDType.Default)
        {
            this.title = title;
            this.description = description;
            this.icon = icon;
            this.hudType = hudType;
            this.customData = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// 터치/클릭 감지 및 정보 팝업 관리 매니저
    /// </summary>
    public class TouchInfoManager : MonoBehaviour
    {
        #region Fields and Properties
        [Header("Touch Detection Settings")]
        [SerializeField] private bool enableTouchDetection = true;
        [SerializeField] private bool enableClickDetection = true;
        [SerializeField] private LayerMask touchableLayerMask = -1; // 모든 레이어
        [SerializeField] private float maxTouchDistance = 100f; // 터치 감지 최대 거리
        
        [Header("Layer-Specific HUD Settings")]
        [SerializeField] private LayerMask buildingLayerMask = 1 << 8; // Building 레이어
        [SerializeField] private LayerMask workerLayerMask = 1 << 6;   // Worker 레이어 (6번으로 변경)
        [SerializeField] private LayerMask resourceLayerMask = 1 << 10; // Resource 레이어
        [SerializeField] private LayerMask facilityLayerMask = 1 << 11; // Facility 레이어
        [SerializeField] private LayerMask npcLayerMask = 1 << 12;      // NPC 레이어
        
        [Header("HUD Prefab References")]
        [SerializeField] private string buildingHUDKey = "BuildingInfoHUD";
        [SerializeField] private string workerHUDKey = "KYS/WorkerInfoHUD";
        [SerializeField] private string resourceHUDKey = "ResourceInfoHUD";
        [SerializeField] private string facilityHUDKey = "FacilityInfoHUD";
        [SerializeField] private string npcHUDKey = "NPCInfoHUD";
        [SerializeField] private string defaultHUDKey = "KYS/TouchInfoHUD";
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showTouchGizmos = false;
        
        private Camera mainCamera;
        #if CINEMACHINE
        private CinemachineVirtualCamera activeVirtualCamera;
        #endif
        private TouchInfoHUD currentHUD;
        private List<TouchInfoHUD> activeHUDs = new List<TouchInfoHUD>();
        
        // 레이어별 HUD 매핑
        private Dictionary<LayerMask, HUDType> layerToHUDType = new Dictionary<LayerMask, HUDType>();
        
        // 테스트용 데이터
        private Dictionary<Vector2, string> testData = new Dictionary<Vector2, string>();
        #endregion

        #region Unity Lifecycle
        private void OnDisable()
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[TouchInfoManager] TouchInfoManager가 비활성화됨: {gameObject.name}");
                Debug.LogWarning($"[TouchInfoManager] 부모 오브젝트 상태: {(transform.parent != null ? transform.parent.gameObject.activeInHierarchy : "No Parent")}");
                Debug.LogWarning($"[TouchInfoManager] 현재 시간: {System.DateTime.Now}");
                
                // Addressables 인스턴스화 과정인지 확인
                var stackTrace = System.Environment.StackTrace;
                bool isAddressablesInstantiation = stackTrace.Contains("Instantiate") || stackTrace.Contains("Addressables");
                
                if (isAddressablesInstantiation)
                {
                    Debug.LogWarning("[TouchInfoManager] Addressables 인스턴스화 과정에서 비활성화됨 - 정상적인 동작");
                    return; // Addressables 인스턴스화 과정에서는 추가 처리하지 않음
                }
                
                Debug.LogWarning($"[TouchInfoManager] 스택 트레이스: {stackTrace}");
            }
        }

        private void OnEnable()
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] TouchInfoManager가 활성화됨: {gameObject.name}");
                
                // Addressables 인스턴스화 과정인지 확인
                var stackTrace = System.Environment.StackTrace;
                bool isAddressablesInstantiation = stackTrace.Contains("Instantiate") || stackTrace.Contains("Addressables");
                
                if (isAddressablesInstantiation)
                {
                    Debug.Log("[TouchInfoManager] Addressables 인스턴스화 과정에서 활성화됨 - 정상적인 동작");
                }
            }
        }
        private void Awake()
        {
            FindMainCamera();
            
            // 레이어별 HUD 타입 매핑 초기화
            InitializeLayerMapping();
            
            // 테스트용 데이터 초기화
            InitializeTestData();
        }

        /// <summary>
        /// 메인 카메라 찾기 (시네머신 환경 대응)
        /// </summary>
        private void FindMainCamera()
        {
            // 1단계: Camera.main 시도
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                // 2단계: 태그로 찾기
                GameObject cameraObj = GameObject.FindWithTag("MainCamera");
                if (cameraObj != null)
                {
                    mainCamera = cameraObj.GetComponent<Camera>();
                    Debug.Log("[TouchInfoManager] MainCamera 태그로 카메라 발견");
                }
            }
            
            #if CINEMACHINE
            if (mainCamera == null)
            {
                // 3단계: 시네머신 브레인이 있는 카메라 찾기
                CinemachineBrain brain = FindObjectOfType<CinemachineBrain>();
                if (brain != null)
                {
                    mainCamera = brain.GetComponent<Camera>();
                    Debug.Log($"[TouchInfoManager] 시네머신 브레인 카메라 발견: {mainCamera.name}");
                }
            }
            #endif
            
            if (mainCamera == null)
            {
                // 4단계: 모든 카메라 중 첫 번째 활성화된 카메라 찾기
                Camera[] allCameras = FindObjectsOfType<Camera>();
                foreach (var cam in allCameras)
                {
                    if (cam.enabled && cam.gameObject.activeInHierarchy)
                    {
                        mainCamera = cam;
                        Debug.Log($"[TouchInfoManager] 활성 카메라 발견: {cam.name}");
                        break;
                    }
                }
            }
            
            if (mainCamera != null)
            {
                //Debug.Log($"[TouchInfoManager] 메인 카메라 설정 완료: {mainCamera.name}");
                //Debug.Log($"[TouchInfoManager] 카메라 위치: {mainCamera.transform.position}");
                //Debug.Log($"[TouchInfoManager] 카메라 회전: {mainCamera.transform.eulerAngles}");
                
                #if CINEMACHINE
                // 시네머신 정보도 출력
                CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
                if (brain != null)
                {
                    Debug.Log($"[TouchInfoManager] 시네머신 브레인 발견, 활성 가상 카메라: {brain.ActiveVirtualCamera?.Name ?? "없음"}");
                }
                #endif
            }
            else
            {
                Debug.LogError("[TouchInfoManager] 어떤 방법으로도 카메라를 찾을 수 없습니다!");
            }
        }
        
        /// <summary>
        /// 레이어별 HUD 타입 매핑 초기화
        /// </summary>
        private void InitializeLayerMapping()
        {
            layerToHUDType.Clear();
            
            // 중복 키 방지를 위해 TryAdd 사용
            if (!layerToHUDType.ContainsKey(buildingLayerMask))
                layerToHUDType.Add(buildingLayerMask, HUDType.Building);
            if (!layerToHUDType.ContainsKey(workerLayerMask))
                layerToHUDType.Add(workerLayerMask, HUDType.Worker);
            if (!layerToHUDType.ContainsKey(resourceLayerMask))
                layerToHUDType.Add(resourceLayerMask, HUDType.Resource);
            if (!layerToHUDType.ContainsKey(facilityLayerMask))
                layerToHUDType.Add(facilityLayerMask, HUDType.Facility);
            if (!layerToHUDType.ContainsKey(npcLayerMask))
                layerToHUDType.Add(npcLayerMask, HUDType.NPC);
            
            if (enableDebugLogs)
            {
                Debug.Log("[TouchInfoManager] 레이어별 HUD 매핑 초기화 완료");
                Debug.Log($"[TouchInfoManager] Worker 레이어: {workerLayerMask.value}");
                Debug.Log($"[TouchInfoManager] Worker HUD 키: {workerHUDKey}");
            }
        }
        
        private void Start()
        {
            if (enableDebugLogs)
            {
                Debug.Log("[TouchInfoManager] 터치 정보 매니저 초기화 완료");
                Debug.Log("[TouchInfoManager] Worker 클릭 감지 준비 완료 - Worker 생성 후 클릭해보세요!");
            }
        }
        
        private void Update()
        {
            if (!enableTouchDetection && !enableClickDetection)
                return;
            
            // 모바일 터치 감지
            if (enableTouchDetection && Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            
            // PC 클릭 감지
            if (enableClickDetection && Input.GetMouseButtonDown(0))
            {
                if (enableDebugLogs)
                    Debug.Log("[TouchInfoManager] 마우스 클릭 감지!");
                HandleMouseInput();
            }
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// 터치 입력 처리
        /// </summary>
        private void HandleTouchInput()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                if (touch.phase == TouchPhase.Began)
                {
                    ProcessTouch(touch.position);
                }
            }
        }
        
        /// <summary>
        /// 마우스 입력 처리
        /// </summary>
        private void HandleMouseInput()
        {
            Vector2 mousePosition = Input.mousePosition;
            ProcessTouch(mousePosition);
        }
        
        /// <summary>
        /// 터치/클릭 처리
        /// </summary>
        private void ProcessTouch(Vector2 screenPosition)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] 터치/클릭 처리 시작: {screenPosition}");
            }
            
            // 먼저 TouchInfoHUD 자체가 클릭되었는지 확인
            bool isTouchInfoHUDClicked = IsTouchInfoHUDClicked(screenPosition);
            
            if (isTouchInfoHUDClicked)
            {
                if (enableDebugLogs)
                {
                    Debug.Log("[TouchInfoManager] TouchInfoHUD 자체 클릭 감지 - HUD 유지");
                }
                return;
            }
            
            if (enableDebugLogs)
            {
                Debug.Log("[TouchInfoManager] TouchInfoHUD 클릭 아님 - UI 요소 확인 중...");
            }
            
            // UI 요소 클릭인지 확인 (TouchInfoHUD 제외)
            bool isUIElementClicked = IsPointerOverUI(screenPosition);
            
            if (isUIElementClicked)
            {
                if (enableDebugLogs)
                {
                    Debug.Log("[TouchInfoManager] UI 요소 클릭 감지 - HUD 닫기 및 종료");
                }
                
                bool hadExistingHUD = CloseExistingTouchInfoHUD();
                
                if (enableDebugLogs && hadExistingHUD)
                {
                    Debug.Log("[TouchInfoManager] 기존 HUD 닫기 완료");
                }
                return;
            }
            
            if (enableDebugLogs)
            {
                Debug.Log("[TouchInfoManager] UI 요소 클릭 아님 - 게임 오브젝트 클릭으로 진행");
            }
            
            // 게임 오브젝트 클릭인 경우
            CloseExistingTouchInfoHUD();
            
            GameObject targetObject = GetTargetObject(screenPosition);
            
            if (targetObject != null)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[TouchInfoManager] ✅ 대상 오브젝트 발견: {targetObject.name}, 레이어: {targetObject.layer} ({LayerMask.LayerToName(targetObject.layer)})");
                    
                    // Worker인지 확인
                    var workerRuntime = targetObject.GetComponent<WorkerRuntimeData>();
                    if (workerRuntime != null)
                    {
                        Debug.Log($"[TouchInfoManager] 🎯 Worker 클릭 감지! ID: {workerRuntime.Id}, Rank: {workerRuntime.Rank}");
                    }
                    
                    // Collider 정보
                    var collider = targetObject.GetComponent<Collider>();
                    Debug.Log($"[TouchInfoManager] Collider: {(collider != null ? collider.GetType().Name : "없음")}");
                }
                
                ShowInfoForObject(targetObject, screenPosition);
            }
            else
            {
                if (enableDebugLogs)
                {
                    Debug.Log("[TouchInfoManager] ❌ 대상 오브젝트를 찾을 수 없음 - 레이캐스트 실패");
                }
            }
        }
        
        /// <summary>
        /// TouchInfoHUD 자체가 클릭되었는지 확인
        /// </summary>
        private bool IsTouchInfoHUDClicked(Vector2 screenPosition)
        {
            if (EventSystem.current == null)
            {
                return false;
            }
            
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPosition;
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            foreach (var result in results)
            {
                // TouchInfoHUD 관련 UI 요소인지 확인
                if (result.gameObject.name.Contains("TouchInfoHUD") || 
                    result.gameObject.name.Contains("InfoHUD") ||
                    result.gameObject.name.Contains("HUDBackdrop") ||
                    result.gameObject.GetComponentInParent<TouchInfoHUD>() != null ||
                    result.gameObject.GetComponent<HUDBackdropUI>() != null)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// UI 요소 위에 포인터가 있는지 확인 (TouchInfoHUD 제외)
        /// </summary>
        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (EventSystem.current == null)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("[TouchInfoManager] EventSystem.current가 null입니다");
                return false;
            }
            
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPosition;
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] UI 레이캐스트 결과: {results.Count}개 발견");
                foreach (var result in results)
                {
                    Debug.Log($"  - {result.gameObject.name} (Layer: {result.gameObject.layer}, {LayerMask.LayerToName(result.gameObject.layer)})");
                }
            }
            
            // 실제 UI Canvas에 속한 요소만 필터링
            List<RaycastResult> filteredResults = new List<RaycastResult>();
            foreach (var result in results)
            {
                // Layer 6(NPC)에 있는 오브젝트 특별 처리 - Worker UI 요소일 수 있음
                if (result.gameObject.layer == 6)
                {
                    if (enableDebugLogs)
                        Debug.Log($"[TouchInfoManager] NPC 레이어 오브젝트 발견: {result.gameObject.name}");
                    
                    // StateText 등 Worker UI 요소에서 부모의 WorkerRuntimeData 찾기
                    WorkerRuntimeData workerData = result.gameObject.GetComponentInParent<WorkerRuntimeData>();
                    if (workerData != null)
                    {
                        if (enableDebugLogs)
                            Debug.Log($"[TouchInfoManager] ✅ UI 요소 {result.gameObject.name}에서 부모 Worker 발견: {workerData.gameObject.name}");
                        
                        // 즉시 Worker HUD 표시
                        ShowInfoForObject(workerData.gameObject, screenPosition);
                        return true; // UI 클릭으로 처리하여 3D 레이캐스트 생략
                    }
                    else
                    {
                        if (enableDebugLogs)
                            Debug.Log($"[TouchInfoManager] NPC 레이어 오브젝트 제외 (WorkerRuntimeData 없음): {result.gameObject.name}");
                        continue;
                    }
                }
                
                // Canvas 컴포넌트를 가진 부모를 찾아서 실제 UI인지 확인
                Canvas parentCanvas = result.gameObject.GetComponentInParent<Canvas>();
                
                // TouchInfoHUD와 HUDBackdropUI 관련 UI 요소는 제외
                bool isTouchInfoHUD = !result.gameObject.name.Contains("TouchInfoHUD") && 
                    !result.gameObject.name.Contains("InfoHUD") &&
                    !result.gameObject.name.Contains("HUDBackdrop") &&
                    result.gameObject.GetComponentInParent<TouchInfoHUD>() == null &&
                    result.gameObject.GetComponent<HUDBackdropUI>() == null;
                
                // 실제 UI Canvas에 속하고 TouchInfoHUD가 아닌 경우만 포함
                if (parentCanvas != null && isTouchInfoHUD)
                {
                    filteredResults.Add(result);
                }
            }
            
            if (enableDebugLogs)
            {
                if (filteredResults.Count > 0)
                {
                    string uiElementNames = "";
                    for (int i = 0; i < Mathf.Min(filteredResults.Count, 5); i++) // 최대 5개까지 표시
                    {
                        uiElementNames += filteredResults[i].gameObject.name;
                        if (i < Mathf.Min(filteredResults.Count, 5) - 1) uiElementNames += ", ";
                    }
                    Debug.Log($"[TouchInfoManager] UI 요소 클릭 감지: {uiElementNames} (총 {filteredResults.Count}개 UI 요소)");
                }
                else
                {
                    Debug.Log("[TouchInfoManager] UI 요소 클릭 없음 - 게임 오브젝트 클릭으로 진행");
                }
            }
            
            return filteredResults.Count > 0;
        }
        
        /// <summary>
        /// 화면 좌표를 월드 좌표로 변환
        /// </summary>
        private Vector3 ScreenToWorldPoint(Vector2 screenPosition)
        {
            if (mainCamera == null)
                return Vector3.zero;
            
            // 카메라에서 일정 거리 떨어진 지점으로 월드 좌표 변환
            float distanceFromCamera = 10f; // 카메라로부터 10 유닛 떨어진 지점
            
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera));
            
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] 월드 좌표 변환 상세: screen=({screenPosition.x:F1}, {screenPosition.y:F1}), distance={distanceFromCamera}, world=({worldPosition.x:F2}, {worldPosition.y:F2}, {worldPosition.z:F2})");
            }
            
            return worldPosition;
        }
        
        /// <summary>
        /// 레이캐스트로 대상 오브젝트 감지
        /// </summary>
        private GameObject GetTargetObject(Vector2 screenPosition)
        {
            if (mainCamera == null)
                return null;
            
            // 화면 좌표를 직접 사용하여 레이 생성
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            
            RaycastHit hit;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] 레이캐스트 시도: 화면좌표=({screenPosition.x:F1}, {screenPosition.y:F1}), 거리: {maxTouchDistance}, 레이어마스크: {touchableLayerMask.value}");
                Debug.Log($"[TouchInfoManager] 레이 방향: {ray.direction}, 시작점: {ray.origin}");
            }
            
            // 모든 레이어에 대해 레이캐스트 시도 (디버깅용)
            RaycastHit[] allHits = Physics.RaycastAll(ray, maxTouchDistance);
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] 모든 히트 감지: {allHits.Length}개");
                if (allHits.Length > 0)
                {
                    for (int i = 0; i < allHits.Length; i++)
                    {
                        var hitInfo = allHits[i];
                        Debug.Log($"[TouchInfoManager] 히트 {i}: {hitInfo.collider.gameObject.name}, 레이어: {hitInfo.collider.gameObject.layer}, 거리: {hitInfo.distance:F2}");
                    }
                }
                else
                {
                    Debug.LogWarning("[TouchInfoManager] RaycastAll로도 아무것도 감지되지 않음!");
                    
                    // TestWorker가 실제로 존재하는지 확인
                    var testWorker = GameObject.Find("TestWorker");
                    if (testWorker != null)
                    {
                        Debug.Log($"[TouchInfoManager] TestWorker 존재 확인: {testWorker.name}, 활성화: {testWorker.activeInHierarchy}");
                        var collider = testWorker.GetComponent<Collider>();
                        Debug.Log($"[TouchInfoManager] TestWorker Collider: {(collider != null ? $"{collider.GetType().Name}, 활성화: {collider.enabled}" : "없음")}");
                        
                        if (collider != null)
                        {
                            Debug.Log($"[TouchInfoManager] Collider 영역: {collider.bounds.min} ~ {collider.bounds.max}");
                            Debug.Log($"[TouchInfoManager] Collider isTrigger: {collider.isTrigger}");
                        }
                        
                        // 물리 레이어 매트릭스 확인
                        int testWorkerLayer = testWorker.layer;
                        Debug.Log($"[TouchInfoManager] TestWorker 레이어: {testWorkerLayer} ({LayerMask.LayerToName(testWorkerLayer)})");
                        
                        // 레이와 TestWorker 간의 기하학적 관계 확인
                        Vector3 workerPos = testWorker.transform.position;
                        Vector3 toWorker = workerPos - ray.origin;
                        float projectionLength = Vector3.Dot(toWorker, ray.direction);
                        Vector3 projectionPoint = ray.origin + ray.direction * projectionLength;
                        float distanceToRay = Vector3.Distance(workerPos, projectionPoint);
                        
                        Debug.Log($"[TouchInfoManager] 레이-Worker 기하학적 분석:");
                        Debug.Log($"[TouchInfoManager]   Worker 위치: {workerPos}");
                        Debug.Log($"[TouchInfoManager]   레이 투영 길이: {projectionLength:F2}");
                        Debug.Log($"[TouchInfoManager]   레이와의 거리: {distanceToRay:F2}");
                        Debug.Log($"[TouchInfoManager]   레이가 Worker를 향하는지: {projectionLength > 0}");
                    }
                }
            }
            
            if (Physics.Raycast(ray, out hit, maxTouchDistance))
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[TouchInfoManager] 첫 번째 레이캐스트 히트: {hit.collider.gameObject.name}, 레이어: {hit.collider.gameObject.layer} ({LayerMask.LayerToName(hit.collider.gameObject.layer)})");
                    Debug.Log($"[TouchInfoManager] 히트 거리: {hit.distance}");
                }
                
                // touchableLayerMask와 비교
                int hitLayerMask = 1 << hit.collider.gameObject.layer;
                bool isTouchable = (touchableLayerMask.value & hitLayerMask) != 0;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[TouchInfoManager] 터치 가능 여부: {isTouchable} (레이어마스크: {hitLayerMask}, 터치가능마스크: {touchableLayerMask.value})");
                }
                
                if (!isTouchable)
                {
                    if (enableDebugLogs)
                        Debug.Log($"[TouchInfoManager] 터치 불가능한 레이어 무시: {hit.collider.gameObject.name}");
                    return null;
                }
                
                // HUDAllPanel 관련 오브젝트는 무시
                if (hit.collider.gameObject.name.Contains("HUDAllPanel") || 
                    hit.collider.gameObject.name.Contains("HUD") ||
                    hit.collider.transform.IsChildOf(transform)) // TouchInfoManager의 자식 오브젝트들 무시
                {
                    if (enableDebugLogs)
                        Debug.Log($"[TouchInfoManager] HUD 관련 오브젝트 무시: {hit.collider.gameObject.name}");
                    return null;
                }
                
                if (enableDebugLogs)
                    Debug.Log($"[TouchInfoManager] 대상 감지 성공: {hit.collider.gameObject.name}");
                
                return hit.collider.gameObject;
            }
            else
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[TouchInfoManager] 모든 레이어 레이캐스트 실패: 아무것도 히트하지 않음");
                    Debug.Log($"[TouchInfoManager] 카메라 위치: {mainCamera.transform.position}");
                    Debug.Log($"[TouchInfoManager] 레이 시작점: {ray.origin}");
                    Debug.Log($"[TouchInfoManager] 레이 방향: {ray.direction}");
                    Debug.Log($"[TouchInfoManager] 최대 거리: {maxTouchDistance}");
                    
                    // TestWorker 위치와 비교
                    var testWorker = GameObject.Find("TestWorker");
                    if (testWorker != null)
                    {
                        Vector3 workerPos = testWorker.transform.position;
                        float distanceToWorker = Vector3.Distance(ray.origin, workerPos);
                        Vector3 directionToWorker = (workerPos - ray.origin).normalized;
                        float dotProduct = Vector3.Dot(ray.direction, directionToWorker);
                        
                        Debug.Log($"[TouchInfoManager] TestWorker 위치: {workerPos}");
                        Debug.Log($"[TouchInfoManager] 카메라-Worker 거리: {distanceToWorker:F2}");
                        Debug.Log($"[TouchInfoManager] 레이-Worker 방향 일치도: {dotProduct:F3} (1에 가까울수록 정확)");
                        
                        // 레이가 Worker 근처를 지나가는지 확인
                        Vector3 closestPoint = ray.origin + Vector3.Project(workerPos - ray.origin, ray.direction);
                        float distanceToRay = Vector3.Distance(workerPos, closestPoint);
                        Debug.Log($"[TouchInfoManager] Worker와 레이의 최단거리: {distanceToRay:F2}");
                    }
                    else
                    {
                        Debug.LogWarning("[TouchInfoManager] TestWorker를 찾을 수 없음!");
                    }
                }
            }
            
            return null;
                }
        #endregion

        #region HUD Management
        /// <summary>
        /// 오브젝트에 대한 정보 HUD 표시 (레이어별 처리)
        /// </summary>
        private void ShowInfoForObject(GameObject targetObject, Vector2 screenPosition)
        {
            // 레이어별 HUD 데이터 생성
            HUDData hudData = CreateHUDDataForObject(targetObject);
            
            // 레이어별 HUD 생성
            CreateLayerSpecificHUD(hudData, screenPosition);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] {hudData.hudType} HUD 표시: {hudData.title}");
            }
        }
        
        /// <summary>
        /// 오브젝트에 대한 HUD 데이터 생성
        /// </summary>
        private HUDData CreateHUDDataForObject(GameObject targetObject)
        {
            int objectLayer = targetObject.layer;
            HUDType hudType = GetHUDTypeForLayer(objectLayer);
            
            switch (hudType)
            {
                case HUDType.Building:
                    return CreateBuildingHUDData(targetObject);
                case HUDType.Worker:
                    return CreateWorkerHUDData(targetObject);
                case HUDType.Resource:
                    return CreateResourceHUDData(targetObject);
                case HUDType.Facility:
                    return CreateFacilityHUDData(targetObject);
                case HUDType.NPC:
                    return CreateNPCHUDData(targetObject);
                default:
                    return CreateDefaultHUDData(targetObject);
            }
        }
        
        /// <summary>
        /// 레이어에 따른 HUD 타입 반환
        /// </summary>
        private HUDType GetHUDTypeForLayer(int layer)
        {
            int layerMask = 1 << layer;
            
            foreach (var mapping in layerToHUDType)
            {
                if ((mapping.Key.value & layerMask) != 0)
                {
                    return mapping.Value;
                }
            }
            
            return HUDType.Default;
        }
        
        /// <summary>
        /// 건물 HUD 데이터 생성
        /// </summary>
        private HUDData CreateBuildingHUDData(GameObject building)
        {
            HUDData data = new HUDData(
                building.name,
                $"건물 정보\n위치: {building.transform.position}\n상태: 정상",
                null,
                HUDType.Building
            );
            
            // 건물별 특정 데이터 추가
            data.customData["buildingType"] = "Production";
            data.customData["level"] = 1;
            data.customData["efficiency"] = 0.85f;
            
            return data;
        }
        
        /// <summary>
/// 작업자 HUD 데이터 생성
/// </summary>
private HUDData CreateWorkerHUDData(GameObject worker)
{
    if (enableDebugLogs)
    {
        Debug.Log($"[TouchInfoManager] Worker HUD 데이터 생성 시작: {worker.name}");
    }
    
    // WorkerRuntimeData 컴포넌트 찾기
    var workerRuntime = worker.GetComponent<WorkerRuntimeData>();
    if (workerRuntime != null)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[TouchInfoManager] WorkerRuntimeData 컴포넌트 발견: ID={workerRuntime.Id}, Rank={workerRuntime.Rank}");
        }
        
        var hudData = new HUDData(
            $"작업자 - {workerRuntime.Id}",
            $"레벨: {workerRuntime.Rank}, 이동속도: {workerRuntime.MoveSpeed:F1}",
            null,
            HUDType.Worker
        );
        
        // customData에 실제 WorkerRuntimeData 저장
        hudData.customData = new Dictionary<string, object>
        {
            { "workerData", workerRuntime }
        };
        
        Debug.Log($"[TouchInfoManager] Worker HUD 데이터 생성 완료: {workerRuntime.Id}");
        return hudData;
    }
    
    if (enableDebugLogs)
    {
        Debug.LogWarning($"[TouchInfoManager] WorkerRuntimeData 컴포넌트를 찾을 수 없음: {worker.name}");
        Debug.LogWarning($"[TouchInfoManager] 사용 가능한 컴포넌트들:");
        var components = worker.GetComponents<Component>();
        foreach (var comp in components)
        {
            Debug.LogWarning($"  - {comp.GetType().Name}: {comp.name}");
        }
    }
    
    // WorkerRuntimeData가 없는 경우 기본 데이터 반환
    return CreateDefaultHUDData(worker);
}
        
        /// <summary>
        /// 자원 HUD 데이터 생성
        /// </summary>
        private HUDData CreateResourceHUDData(GameObject resource)
        {
            HUDData data = new HUDData(
                resource.name,
                $"자원 정보\n위치: {resource.transform.position}\n상태: 채굴 가능",
                null,
                HUDType.Resource
            );
            
            // 자원별 특정 데이터 추가
            data.customData["resourceType"] = "Iron";
            data.customData["quantity"] = 1000;
            data.customData["quality"] = 0.8f;
            
            return data;
        }
        
        /// <summary>
        /// 시설 HUD 데이터 생성
        /// </summary>
        private HUDData CreateFacilityHUDData(GameObject facility)
        {
            HUDData data = new HUDData(
                facility.name,
                $"시설 정보\n위치: {facility.transform.position}\n상태: 운영 중",
                null,
                HUDType.Facility
            );
            
            // 시설별 특정 데이터 추가
            data.customData["facilityType"] = "Storage";
            data.customData["capacity"] = 5000;
            data.customData["currentUsage"] = 3200;
            
            return data;
        }
        
        /// <summary>
        /// NPC HUD 데이터 생성
        /// </summary>
        private HUDData CreateNPCHUDData(GameObject npc)
        {
            HUDData data = new HUDData(
                npc.name,
                $"NPC 정보\n위치: {npc.transform.position}\n상태: 대화 가능",
                null,
                HUDType.NPC
            );
            
            // NPC별 특정 데이터 추가
            data.customData["npcType"] = "Merchant";
            data.customData["reputation"] = 75;
            data.customData["availableQuests"] = 3;
            
            return data;
        }
        
        /// <summary>
        /// 기본 HUD 데이터 생성
        /// </summary>
        private HUDData CreateDefaultHUDData(GameObject obj)
        {
            return new HUDData(
                obj.name,
                $"오브젝트 정보\n위치: {obj.transform.position}\n레이어: {LayerMask.LayerToName(obj.layer)}",
                null,
                HUDType.Default
            );
        }
        
        /// <summary>
        /// 레이어별 특정 HUD 생성
        /// </summary>
private async void CreateLayerSpecificHUD(HUDData hudData, Vector2 screenPosition)
{
    string hudKey = GetHUDKeyForType(hudData.hudType);
    
    try
    {
        var hudInstance = await UIManager.Instance.CreateHUDAsync<TouchInfoHUD>(hudKey);
        
        if (hudInstance != null)
        {
            // HUD 위치 설정
            hudInstance.SetHUDPosition(screenPosition);
            
            // HUD 타입별로 실시간 데이터 연결
            SetupHUDRealtimeData(hudInstance, hudData);
            
            // HUD 표시
            hudInstance.Show();
            
            // 기존 HUD 닫기
            CloseCurrentHUD();
            currentHUD = hudInstance;
            activeHUDs.Add(hudInstance);
            
            Debug.Log($"[TouchInfoManager] {hudData.hudType} HUD 생성 완료: {hudKey}");
        }
        else
        {
            Debug.LogError($"[TouchInfoManager] {hudData.hudType} HUD 생성 실패: {hudKey}");
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[TouchInfoManager] HUD 생성 중 오류 발생: {e.Message}");
    }
}
        
        /// <summary>
        /// HUD 타입에 따른 키 반환
        /// </summary>
        private string GetHUDKeyForType(HUDType hudType)
        {
            switch (hudType)
            {
                case HUDType.Building: return buildingHUDKey;
                case HUDType.Worker: return workerHUDKey;
                case HUDType.Resource: return resourceHUDKey;
                case HUDType.Facility: return facilityHUDKey;
                case HUDType.NPC: return npcHUDKey;
                default: return defaultHUDKey;
            }
        }
/// <summary>
/// HUD 실시간 데이터 연결 설정
/// </summary>
private void SetupHUDRealtimeData(TouchInfoHUD hud, HUDData hudData)
{
    if (hud == null || hudData == null) return;

    try
    {
        switch (hudData.hudType)
        {
            case HUDType.Worker:
                SetupWorkerHUDRealtimeData(hud, hudData);
                break;
            case HUDType.Building:
                SetupBuildingHUDRealtimeData(hud, hudData);
                break;
            case HUDType.Resource:
                SetupResourceHUDRealtimeData(hud, hudData);
                break;
            case HUDType.Facility:
                SetupFacilityHUDRealtimeData(hud, hudData);
                break;
            case HUDType.NPC:
                SetupNPCHUDRealtimeData(hud, hudData);
                break;
            default:
                Debug.Log($"[TouchInfoManager] {hudData.hudType} 타입의 실시간 데이터 연결은 아직 구현되지 않음");
                break;
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[TouchInfoManager] HUD 실시간 데이터 연결 실패: {e.Message}");
    }
}
        
/// <summary>
/// Worker HUD 실시간 데이터 연결
/// </summary>
private void SetupWorkerHUDRealtimeData(TouchInfoHUD hud, HUDData hudData)
{
    if (hudData.customData != null && 
        hudData.customData.TryGetValue("workerData", out var workerObj) && 
        workerObj is WorkerRuntimeData workerData)
    {
        // WorkerInfoHUD로 캐스팅하여 실시간 데이터 연결
        if (hud is WorkerInfoHUD workerHUD)
        {
            workerHUD.ConnectToWorkerData(workerData);
            Debug.Log($"[TouchInfoManager] Worker HUD 실시간 데이터 연결 완료: {workerData.Id}");
        }
        else
        {
            Debug.LogWarning("[TouchInfoManager] Worker HUD가 WorkerInfoHUD 타입이 아님");
        }
    }
    else
    {
        Debug.LogWarning("[TouchInfoManager] Worker HUD 데이터에 workerData가 없음");
    }
}

/// <summary>
/// Building HUD 실시간 데이터 연결 (향후 구현)
/// </summary>
private void SetupBuildingHUDRealtimeData(TouchInfoHUD hud, HUDData hudData)
{
    // TODO: Building HUD 실시간 데이터 연결 구현
    Debug.Log("[TouchInfoManager] Building HUD 실시간 데이터 연결은 향후 구현 예정");
}

/// <summary>
/// Resource HUD 실시간 데이터 연결 (향후 구현)
/// </summary>
private void SetupResourceHUDRealtimeData(TouchInfoHUD hud, HUDData hudData)
{
    // TODO: Resource HUD 실시간 데이터 연결 구현
    Debug.Log("[TouchInfoManager] Resource HUD 실시간 데이터 연결은 향후 구현 예정");
}

/// <summary>
/// Facility HUD 실시간 데이터 연결 (향후 구현)
/// </summary>
private void SetupFacilityHUDRealtimeData(TouchInfoHUD hud, HUDData hudData)
{
    // TODO: Facility HUD 실시간 데이터 연결 구현
    Debug.Log("[TouchInfoManager] Facility HUD 실시간 데이터 연결은 향후 구현 예정");
}

/// <summary>
/// NPC HUD 실시간 데이터 연결 (향후 구현)
/// </summary>
private void SetupNPCHUDRealtimeData(TouchInfoHUD hud, HUDData hudData)
{
    // TODO: NPC HUD 실시간 데이터 연결 구현
    Debug.Log("[TouchInfoManager] NPC HUD 실시간 데이터 연결은 향후 구현 예정");
}
        
        /// <summary>
        /// 기본 HUD 생성 (fallback)
        /// </summary>
        private async System.Threading.Tasks.Task CreateDefaultHUD(HUDData hudData, Vector2 screenPosition)
        {
            await TouchInfoHUD.ShowInfoHUD(screenPosition, hudData.title, hudData.description, hudData.icon);
        }
        
        /// <summary>
        /// 테스트용 HUD 표시
        /// </summary>
        private void ShowTestHUD(Vector2 screenPosition)
        {
            // 테스트용 정보 생성
            string title = "빈 공간";
            string description = $"터치 위치: ({screenPosition.x:F0}, {screenPosition.y:F0})\n";
            
            // HUD 생성 (기존 HUD는 이미 ProcessTouch에서 닫혔음)
            _ = TouchInfoHUD.ShowInfoHUD(screenPosition, title, description);

            if (enableDebugLogs)
            {
                //Debug.Log($"[TouchInfoManager] 테스트 HUD 표시: {screenPosition}");
            }

        }
        
        /// <summary>
        /// 오브젝트 제목 가져오기
        /// </summary>
        private string GetObjectTitle(GameObject targetObject)
        {
            // 오브젝트 이름을 제목으로 사용
            return targetObject.name;
        }
        
        /// <summary>
        /// 오브젝트 설명 가져오기
        /// </summary>
        private string GetObjectDescription(GameObject targetObject)
        {
            // 오브젝트 이름과 기본 정보 표시
            return $"오브젝트 이름: {targetObject.name}\n" +
                   $"위치: ({targetObject.transform.position.x:F1}, {targetObject.transform.position.y:F1}, {targetObject.transform.position.z:F1})\n" +
                   $"향후 실제 건물/시설 정보가 여기에 표시됩니다.";
        }
        
        /// <summary>
        /// 오브젝트 아이콘 가져오기
        /// </summary>
        private Sprite GetObjectIcon(GameObject targetObject)
        {
            // 여기에 실제 오브젝트 아이콘 로직 추가
            return null;
        }
        
        /// <summary>
        /// 현재 HUD 닫기
        /// </summary>
        private void CloseCurrentHUD()
        {
            if (currentHUD != null)
            {
                currentHUD.Hide();
                currentHUD = null;
            }
        }
        
        /// <summary>
        /// 기존 TouchInfoHUD 닫기 (UIManager를 통해)
        /// </summary>
        /// <returns>닫은 HUD가 있었는지 여부</returns>
        private bool CloseExistingTouchInfoHUD()
        {
            if (UIManager.Instance != null)
            {
                return UIManager.Instance.DestroyAllInfoHUDs();
            }
            
            return false;
        }
        
        /// <summary>
        /// 테스트용 데이터 초기화
        /// </summary>
        private void InitializeTestData()
        {
            // 테스트용 데이터 추가
            testData.Clear();
            testData.Add(new Vector2(100, 100), "테스트 건물 1");
            testData.Add(new Vector2(200, 200), "테스트 건물 2");
            testData.Add(new Vector2(300, 300), "테스트 생산시설 1");
                }
        #endregion

        #region Public API
        /// <summary>
        /// 터치 감지 활성화/비활성화
        /// </summary>
        public void SetTouchDetection(bool enabled)
        {
            enableTouchDetection = enabled;

            if (enableDebugLogs)
            {
                //Debug.Log($"[TouchInfoManager] 터치 감지 {(enabled ? "활성화" : "비활성화")}");
            }

        }
        
        /// <summary>
        /// 클릭 감지 활성화/비활성화
        /// </summary>
        public void SetClickDetection(bool enabled)
        {
            enableClickDetection = enabled;

            if (enableDebugLogs)
            {
                //Debug.Log($"[ToucInfoManager] 클릭 감지 {(enabled ? "활성화" : "비활성화")}");
            }
        }

        /// <summary>
        /// 모든 HUD 완전 제거
        /// </summary>
        public void CloseAllHUDs()
        {
            // 현재 HUD 닫기
            CloseCurrentHUD();

            // UIManager를 통해 모든 TouchInfoHUD 완전 제거
            if (UIManager.Instance != null)
            {
                UIManager.Instance.DestroyAllInfoHUDs();
            }

            // 활성 HUD 목록에서 닫기
            foreach (var hud in activeHUDs)
            {
                if (hud != null)
                    hud.Hide();
            }

            activeHUDs.Clear();

            if (enableDebugLogs)
                { 
                Debug.Log("[TouchInfoManager] 모든 HUD 완전 제거 완료");
                }
        }
        
        #endregion

        #region Debug and Testing Methods
        /// <summary>
        /// 테스트용 HUD 강제 표시
        /// </summary>
        [ContextMenu("테스트 - HUD 표시")]
        public void TestShowHUD()
        {
            Vector2 testPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            ShowTestHUD(testPosition);
        }
        
        /// <summary>
        /// 테스트용 HUD 닫기
        /// </summary>
        [ContextMenu("테스트 - HUD 닫기")]
        public void TestCloseHUD()
        {
            CloseAllHUDs();
        }
        
        /// <summary>
        /// 레이어별 HUD 데이터 설정 (외부에서 호출 가능)
        /// </summary>
        public void SetLayerHUDData(int layer, HUDData hudData)
        {
            // 특정 레이어의 HUD 데이터를 동적으로 설정
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] 레이어 {layer} HUD 데이터 설정: {hudData.title}");
            }
        }
        
        /// <summary>
        /// 레이어별 HUD 키 설정 (외부에서 호출 가능)
        /// </summary>
        public void SetLayerHUDKey(HUDType hudType, string hudKey)
        {
            switch (hudType)
            {
                case HUDType.Building:
                    buildingHUDKey = hudKey;
                    break;
                case HUDType.Worker:
                    workerHUDKey = hudKey;
                    break;
                case HUDType.Resource:
                    resourceHUDKey = hudKey;
                    break;
                case HUDType.Facility:
                    facilityHUDKey = hudKey;
                    break;
                case HUDType.NPC:
                    npcHUDKey = hudKey;
                    break;
                default:
                    defaultHUDKey = hudKey;
                    break;
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] {hudType} HUD 키 설정: {hudKey}");
            }
        }
        
        /// <summary>
        /// 레이어별 HUD 테스트
        /// </summary>
        [ContextMenu("테스트 - 레이어별 HUD")]
        public void TestLayerSpecificHUD()
        {
            Vector2 testPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            
            // 각 레이어별 HUD 테스트
            var buildingData = new HUDData("테스트 건물", "건물 정보 테스트", null, HUDType.Building);
            var workerData = new HUDData("테스트 작업자", "작업자 정보 테스트", null, HUDType.Worker);
            var resourceData = new HUDData("테스트 자원", "자원 정보 테스트", null, HUDType.Resource);
            
            CreateLayerSpecificHUD(buildingData, testPosition);
        }
        
        /// <summary>
        /// Worker 레이어 감지 테스트
        /// </summary>
        [ContextMenu("테스트 - Worker 레이어 감지")]
        public void TestWorkerLayerDetection()
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] Worker 레이어 테스트 시작");
                Debug.Log($"[TouchInfoManager] Worker 레이어마스크: {workerLayerMask.value} (Layer 6)");
                Debug.Log($"[TouchInfoManager] Touchable 레이어마스크: {touchableLayerMask.value}");
                
                // 씬에서 Worker 레이어의 모든 오브젝트 찾기
                var allObjects = FindObjectsOfType<GameObject>();
                var workerObjects = new List<GameObject>();
                
                foreach (var obj in allObjects)
                {
                    if (obj.layer == 6) // Worker 레이어
                    {
                        workerObjects.Add(obj);
                    }
                }
                
                Debug.Log($"[TouchInfoManager] Worker 레이어 오브젝트 {workerObjects.Count}개 발견:");
                foreach (var worker in workerObjects)
                {
                    var workerRuntime = worker.GetComponent<WorkerRuntimeData>();
                    var collider = worker.GetComponent<Collider>();
                    var position = worker.transform.position;
                    var isUIElement = worker.GetComponent<RectTransform>() != null;
                    
                    Debug.Log($"  - {worker.name}: 레이어={worker.layer}, 위치={position}, Collider={collider != null}, WorkerRuntimeData={workerRuntime != null}, UI요소={isUIElement}");
                    
                    if (collider != null)
                    {
                        try
                        {
                            var bounds = collider.bounds;
                            Debug.Log($"    Collider 영역: {bounds.min} ~ {bounds.max}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"    Collider bounds 접근 실패: {e.Message}");
                        }
                    }
                    
                    // UI 요소가 Worker 레이어에 있으면 경고
                    if (isUIElement && !worker.name.StartsWith("TestWorker"))
                    {
                        Debug.LogWarning($"  ⚠️ UI 요소가 Worker 레이어에 잘못 설정됨: {worker.name} - Layer를 UI(5)로 변경하세요!");
                    }
                }
                
                // 카메라 정보 출력
                if (mainCamera != null)
                {
                    Debug.Log($"[TouchInfoManager] 카메라 위치: {mainCamera.transform.position}");
                    Debug.Log($"[TouchInfoManager] 카메라 방향: {mainCamera.transform.forward}");
                }
            }
        }

        [ContextMenu("테스트 - 임시 Worker 오브젝트 생성")]
        public void CreateTestWorkerObject()
        {
            // 임시 GameObject 생성
            GameObject testWorker = new GameObject("TestWorker");
            testWorker.layer = 6; // Worker 레이어
            
            // 3D 환경용 BoxCollider 추가
            testWorker.AddComponent<BoxCollider>();
            
            // 임시 WorkerRuntimeData 컴포넌트 추가
            var workerData = testWorker.AddComponent<WorkerRuntimeData>();
            
            // 카메라 앞 위치에 배치
            if (mainCamera != null)
            {
                Vector3 cameraForward = mainCamera.transform.forward;
                Vector3 targetPosition = mainCamera.transform.position + cameraForward * 5f;
                testWorker.transform.position = targetPosition;
                Debug.Log($"[TouchInfoManager] 카메라 정보 - 위치: {mainCamera.transform.position}, 방향: {cameraForward}");
                Debug.Log($"[TouchInfoManager] TestWorker 배치 위치: {targetPosition}");
            }
            else
            {
                Debug.LogWarning("[TouchInfoManager] 메인 카메라를 찾을 수 없음 - (0,0,5) 위치에 배치");
                testWorker.transform.position = new Vector3(0, 0, 5);
            }
            
            // 시각적 표시를 위한 큐브 추가
            GameObject visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualCube.transform.SetParent(testWorker.transform);
            visualCube.transform.localPosition = Vector3.zero;
            visualCube.GetComponent<Renderer>().material.color = Color.blue;
            
            Debug.Log($"[TouchInfoManager] 테스트 Worker 오브젝트 생성 완료: {testWorker.transform.position}");
            Debug.Log("[TouchInfoManager] 이제 파란색 큐브를 클릭해서 HUD 테스트를 해보세요!");
            
            // 생성된 오브젝트를 선택하여 Scene View에서 쉽게 찾을 수 있도록
            #if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = testWorker;
            #endif
        }

        [ContextMenu("테스트 - 모든 임시 Worker 삭제")]
        public void DestroyAllTestWorkers()
        {
            var testWorkers = GameObject.FindObjectsOfType<GameObject>()
                .Where(go => go.name.StartsWith("TestWorker"))
                .ToArray();
                
            foreach (var worker in testWorkers)
            {
                DestroyImmediate(worker);
            }
            
            Debug.Log($"[TouchInfoManager] {testWorkers.Length}개의 테스트 Worker 오브젝트 삭제 완료");
        }

        [ContextMenu("테스트 - 화면 중앙에 Worker 생성")]
        public void CreateTestWorkerAtScreenCenter()
        {
            // 먼저 기존 테스트 Worker 삭제
            DestroyAllTestWorkers();
            
            if (mainCamera == null)
            {
                Debug.LogError("[TouchInfoManager] 카메라를 찾을 수 없어서 TestWorker를 생성할 수 없습니다. '테스트 - 카메라 찾기'를 먼저 실행해보세요.");
                return;
            }
            
            // 임시 GameObject 생성
            GameObject testWorker = new GameObject("TestWorker");
            testWorker.layer = 6; // Worker 레이어
            
            // 3D 환경용 BoxCollider 추가
            testWorker.AddComponent<BoxCollider>();
            
            // 임시 WorkerRuntimeData 컴포넌트 추가
            var workerData = testWorker.AddComponent<WorkerRuntimeData>();
            
            Debug.Log($"[TouchInfoManager] 카메라 디버그 정보:");
            Debug.Log($"[TouchInfoManager]   카메라 위치: {mainCamera.transform.position}");
            Debug.Log($"[TouchInfoManager]   카메라 회전: {mainCamera.transform.rotation.eulerAngles}");
            Debug.Log($"[TouchInfoManager]   카메라 forward: {mainCamera.transform.forward}");
            
            // 카메라 앞쪽 방향으로 직접 배치 (카메라 transform 사용)
            float distance = 8f; // 8미터 앞에 배치
            Vector3 bestPosition = mainCamera.transform.position + mainCamera.transform.forward * distance;
            
            Debug.Log($"[TouchInfoManager] 카메라 forward 방향으로 {distance}m 앞에 배치: {bestPosition}");
            
            // 화면 좌표 미리 확인
            Vector3 previewScreenPos = mainCamera.WorldToScreenPoint(bestPosition);
            Debug.Log($"[TouchInfoManager] 예상 화면 좌표: ({previewScreenPos.x:F1}, {previewScreenPos.y:F1}, {previewScreenPos.z:F1})");
            
            // 카메라 앞쪽인지 확인 (Z > 0)
            if (previewScreenPos.z <= 0)
            {
                Debug.LogWarning("[TouchInfoManager] 여전히 카메라 뒤쪽! 다른 방법 시도...");
                
                // 화면 중앙에서 Ray 방식으로 시도
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Ray ray = mainCamera.ScreenPointToRay(screenCenter);
                bestPosition = ray.origin + ray.direction * distance;
                
                Debug.Log($"[TouchInfoManager] Ray 방식으로 재시도: {bestPosition}");
                Vector3 rayScreenPos = mainCamera.WorldToScreenPoint(bestPosition);
                Debug.Log($"[TouchInfoManager] Ray 방식 화면 좌표: ({rayScreenPos.x:F1}, {rayScreenPos.y:F1}, {rayScreenPos.z:F1})");
            }
            
            testWorker.transform.position = bestPosition;
            
            // 시각적 표시를 위한 큐브 추가 (매우 크게)
            GameObject visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualCube.transform.SetParent(testWorker.transform);
            visualCube.transform.localPosition = Vector3.zero;
            visualCube.transform.localScale = Vector3.one * 3f; // 3배 크기로 더 크게
            visualCube.GetComponent<Renderer>().material.color = Color.magenta; // 자홍색으로 눈에 잘 띄게
            
            // 화면 좌표 확인
            Vector3 finalScreenPos = mainCamera.WorldToScreenPoint(bestPosition);
            Debug.Log($"[TouchInfoManager] TestWorker 최종 위치: {bestPosition}");
            Debug.Log($"[TouchInfoManager] TestWorker 화면 좌표: ({finalScreenPos.x:F1}, {finalScreenPos.y:F1}, {finalScreenPos.z:F1})");
            Debug.Log($"[TouchInfoManager] 화면 크기: {Screen.width} x {Screen.height}");
            Debug.Log("[TouchInfoManager] 큰 자홍색 큐브 생성 완료 - 이제 클릭해보세요!");
            
            // 생성된 오브젝트를 선택하여 Scene View에서 쉽게 찾을 수 있도록
            #if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = testWorker;
            #endif
        }

        [ContextMenu("테스트 - 플레이어 근처에 Worker 생성")]
        public void CreateTestWorkerNearPlayer()
        {
            // 먼저 기존 테스트 Worker 삭제
            DestroyAllTestWorkers();
            
            // 플레이어 찾기 (태그나 이름으로)
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            
            Vector3 workerPosition;
            
            if (player != null)
            {
                // 플레이어 앞쪽 3미터 위치에 배치
                Vector3 playerForward = player.transform.forward;
                workerPosition = player.transform.position + playerForward * 3f + Vector3.up * 1f; // 1미터 위로
                Debug.Log($"[TouchInfoManager] 플레이어 발견, 앞쪽에 Worker 배치: {workerPosition}");
            }
            else
            {
                Debug.LogWarning("[TouchInfoManager] 플레이어를 찾을 수 없어서 원점 근처에 배치합니다.");
                workerPosition = new Vector3(0, 1, 3);
            }
            
            // 임시 GameObject 생성
            GameObject testWorker = new GameObject("TestWorker");
            testWorker.layer = 6; // Worker 레이어
            testWorker.transform.position = workerPosition;
            
            // 3D 환경용 BoxCollider 추가
            testWorker.AddComponent<BoxCollider>();
            
            // 임시 WorkerRuntimeData 컴포넌트 추가
            var workerData = testWorker.AddComponent<WorkerRuntimeData>();
            
            // 시각적 표시를 위한 큐브 추가 (노란색으로)
            GameObject visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualCube.transform.SetParent(testWorker.transform);
            visualCube.transform.localPosition = Vector3.zero;
            visualCube.transform.localScale = Vector3.one * 2f;
            visualCube.GetComponent<Renderer>().material.color = Color.yellow; // 노란색
            
            Debug.Log("[TouchInfoManager] 플레이어 근처에 노란색 TestWorker 생성 완료!");
            
            // 화면 좌표도 확인
            if (mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(workerPosition);
                Debug.Log($"[TouchInfoManager] TestWorker 화면 좌표: ({screenPos.x:F1}, {screenPos.y:F1}, {screenPos.z:F1})");
                
                bool isOnScreen = screenPos.x >= 0 && screenPos.x <= Screen.width && 
                                 screenPos.y >= 0 && screenPos.y <= Screen.height && 
                                 screenPos.z > 0;
                Debug.Log($"[TouchInfoManager] 화면에 보임: {isOnScreen}");
            }
            
            #if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = testWorker;
            #endif
        }

        [ContextMenu("테스트 - 실제 Worker 찾기")]
        public void FindRealWorkers()
        {
            Debug.Log("[TouchInfoManager] 씬에서 실제 Worker 찾기 시작");
            
            // WorkerRuntimeData가 있는 모든 오브젝트 찾기
            WorkerRuntimeData[] allWorkers = FindObjectsOfType<WorkerRuntimeData>();
            Debug.Log($"[TouchInfoManager] WorkerRuntimeData 컴포넌트가 있는 오브젝트: {allWorkers.Length}개");
            
            foreach (var worker in allWorkers)
            {
                GameObject workerObj = worker.gameObject;
                Debug.Log($"[TouchInfoManager] Worker 발견: {workerObj.name}");
                Debug.Log($"[TouchInfoManager]   위치: {workerObj.transform.position}");
                Debug.Log($"[TouchInfoManager]   레이어: {workerObj.layer} ({LayerMask.LayerToName(workerObj.layer)})");
                Debug.Log($"[TouchInfoManager]   활성화: {workerObj.activeInHierarchy}");
                
                // Collider 확인
                Collider collider = workerObj.GetComponent<Collider>();
                Debug.Log($"[TouchInfoManager]   Collider: {(collider != null ? collider.GetType().Name : "없음")}");
                
                if (collider != null)
                {
                    Debug.Log($"[TouchInfoManager]   Collider 활성화: {collider.enabled}");
                    Debug.Log($"[TouchInfoManager]   Collider 영역: {collider.bounds}");
                }
                
                // 화면에 보이는지 확인
                if (mainCamera != null)
                {
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(workerObj.transform.position);
                    bool isVisible = screenPos.z > 0 && 
                                   screenPos.x >= 0 && screenPos.x <= Screen.width &&
                                   screenPos.y >= 0 && screenPos.y <= Screen.height;
                    Debug.Log($"[TouchInfoManager]   화면 좌표: {screenPos}");
                    Debug.Log($"[TouchInfoManager]   화면에 보임: {isVisible}");
                }
                
                Debug.Log("----------------------------------------");
            }
            
            if (allWorkers.Length == 0)
            {
                Debug.LogWarning("[TouchInfoManager] 실제 Worker가 하나도 없습니다! 게임을 진행해서 Worker를 생성해보세요.");
            }
        }

        [ContextMenu("테스트 - Worker HUD 강제 표시")]
        public void ForceShowWorkerHUD()
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            // 테스트용 HUD 데이터 생성
            HUDData testHUDData = new HUDData(
                "테스트 Worker",
                "테스트용 Worker입니다\nID: TEST001\n레벨: 5",
                null,
                HUDType.Worker
            );
            
            CreateLayerSpecificHUD(testHUDData, screenCenter);
            Debug.Log("[TouchInfoManager] 테스트 Worker HUD 강제 표시");
        }

        [ContextMenu("테스트 - 카메라 찾기")]
        public void TestFindCamera()
        {
            Debug.Log("[TouchInfoManager] 카메라 찾기 테스트 시작");
            
            // 모든 카메라 출력
            Camera[] allCameras = FindObjectsOfType<Camera>();
            Debug.Log($"[TouchInfoManager] 씬에서 발견된 카메라 개수: {allCameras.Length}");
            
            for (int i = 0; i < allCameras.Length; i++)
            {
                var cam = allCameras[i];
                Debug.Log($"[TouchInfoManager] 카메라 {i}: {cam.name}, 활성화: {cam.enabled}, 게임오브젝트 활성화: {cam.gameObject.activeInHierarchy}");
                Debug.Log($"[TouchInfoManager]   위치: {cam.transform.position}, 태그: {cam.tag}");
                
                #if CINEMACHINE
                // 시네머신 브레인 확인
                CinemachineBrain brain = cam.GetComponent<CinemachineBrain>();
                if (brain != null)
                {
                    Debug.Log($"[TouchInfoManager]   시네머신 브레인 있음, 활성 가상 카메라: {brain.ActiveVirtualCamera?.Name ?? "없음"}");
                }
                #endif
            }
            
            #if CINEMACHINE
            // 시네머신 가상 카메라들도 찾기
            var virtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();
            Debug.Log($"[TouchInfoManager] 시네머신 가상 카메라 개수: {virtualCameras.Length}");
            foreach (var vcam in virtualCameras)
            {
                Debug.Log($"[TouchInfoManager] 가상 카메라: {vcam.name}, 우선순위: {vcam.Priority}, 활성화: {vcam.enabled}");
            }
            #endif
            
            // Camera.main 확인
            Debug.Log($"[TouchInfoManager] Camera.main: {(Camera.main != null ? Camera.main.name : "null")}");
            
            // 카메라 다시 찾기 시도
            FindMainCamera();
        }

        [ContextMenu("테스트 - 레이어 무시하고 레이캐스트")]
        public void TestRaycastIgnoreLayerMask()
        {
            var testWorker = GameObject.Find("TestWorker");
            if (testWorker == null)
            {
                Debug.LogError("[TouchInfoManager] TestWorker를 찾을 수 없음! 먼저 생성해주세요.");
                return;
            }

            if (mainCamera == null)
            {
                Debug.LogError("[TouchInfoManager] 메인 카메라를 찾을 수 없음!");
                return;
            }

            // TestWorker 위치를 화면 좌표로 변환
            Vector3 workerWorldPos = testWorker.transform.position;
            Vector3 workerScreenPos = mainCamera.WorldToScreenPoint(workerWorldPos);
            Vector2 testScreenPos = new Vector2(workerScreenPos.x, workerScreenPos.y);
            
            Ray ray = mainCamera.ScreenPointToRay(testScreenPos);
            
            Debug.Log($"[TouchInfoManager] 레이어 마스크 무시하고 레이캐스트 테스트");
            Debug.Log($"[TouchInfoManager] TestWorker 화면 좌표: {testScreenPos}");
            
            // 1. 모든 레이어에 대해 레이캐스트 (레이어 마스크 무시)
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxTouchDistance))
            {
                Debug.Log($"[TouchInfoManager] ✅ 레이어 무시 레이캐스트 성공! 히트: {hit.collider.gameObject.name}");
                Debug.Log($"[TouchInfoManager] 히트 레이어: {hit.collider.gameObject.layer}");
                Debug.Log($"[TouchInfoManager] 히트 거리: {hit.distance:F2}");
                
                if (hit.collider.gameObject == testWorker)
                {
                    Debug.Log($"[TouchInfoManager] 🎯 TestWorker 직접 히트!");
                    // 실제 HUD 표시 테스트
                    ShowInfoForObject(hit.collider.gameObject, testScreenPos);
                }
                else
                {
                    Debug.LogWarning($"[TouchInfoManager] TestWorker가 아닌 다른 오브젝트 히트: {hit.collider.gameObject.name}");
                }
            }
            else
            {
                Debug.LogError("[TouchInfoManager] ❌ 레이어 무시해도 레이캐스트 실패!");
            }
            
            // 2. 특정 레이어만 대상으로 레이캐스트
            int workerLayerMask = 1 << testWorker.layer;
            if (Physics.Raycast(ray, out hit, maxTouchDistance, workerLayerMask))
            {
                Debug.Log($"[TouchInfoManager] ✅ Worker 레이어 전용 레이캐스트 성공!");
            }
            else
            {
                Debug.LogWarning($"[TouchInfoManager] ⚠️ Worker 레이어({testWorker.layer}) 전용 레이캐스트 실패");
                Debug.Log($"[TouchInfoManager] Worker 레이어 마스크: {workerLayerMask}");
                Debug.Log($"[TouchInfoManager] 현재 touchableLayerMask: {touchableLayerMask.value}");
            }
        }

        [ContextMenu("테스트 - TestWorker 직접 레이캐스트")]
        public void TestDirectRaycastToWorker()
        {
            var testWorker = GameObject.Find("TestWorker");
            if (testWorker == null)
            {
                Debug.LogError("[TouchInfoManager] TestWorker를 찾을 수 없음! 먼저 생성해주세요.");
                return;
            }

            if (mainCamera == null)
            {
                Debug.LogError("[TouchInfoManager] 메인 카메라를 찾을 수 없음! '테스트 - 카메라 찾기'를 먼저 실행해보세요.");
                return;
            }

            // TestWorker 위치를 화면 좌표로 변환
            Vector3 workerWorldPos = testWorker.transform.position;
            Vector3 workerScreenPos = mainCamera.WorldToScreenPoint(workerWorldPos);
            
            Debug.Log($"[TouchInfoManager] TestWorker 월드 위치: {workerWorldPos}");
            Debug.Log($"[TouchInfoManager] TestWorker 화면 좌표: {workerScreenPos}");
            Debug.Log($"[TouchInfoManager] 화면 범위: 0~{Screen.width} x 0~{Screen.height}");
            
            // TestWorker가 화면에 보이는지 확인
            bool isOnScreen = workerScreenPos.x >= 0 && workerScreenPos.x <= Screen.width && 
                             workerScreenPos.y >= 0 && workerScreenPos.y <= Screen.height && 
                             workerScreenPos.z > 0;
            
            Debug.Log($"[TouchInfoManager] TestWorker 화면에 보임: {isOnScreen}");
            
            if (isOnScreen)
            {
                // TestWorker 위치로 직접 레이캐스트
                Vector2 testScreenPos = new Vector2(workerScreenPos.x, workerScreenPos.y);
                Debug.Log($"[TouchInfoManager] TestWorker 화면 좌표로 레이캐스트 테스트: ({testScreenPos.x:F1}, {testScreenPos.y:F1})");
                
                GameObject hitObject = GetTargetObject(testScreenPos);
                if (hitObject != null)
                {
                    Debug.Log($"[TouchInfoManager] ✅ 레이캐스트 성공! 히트 오브젝트: {hitObject.name}");
                    
                    // 실제로 HUD 표시 테스트
                    ShowInfoForObject(hitObject, testScreenPos);
                }
                else
                {
                    Debug.LogError("[TouchInfoManager] ❌ TestWorker 위치로 레이캐스트했는데도 감지 실패!");
                }
            }
            else
            {
                Debug.LogWarning("[TouchInfoManager] TestWorker가 화면 밖에 있음! 카메라를 조정하거나 새로운 위치에 생성하세요.");
            }
        }
        
        /// <summary>
        /// 특정 레이어의 오브젝트에 대한 HUD 표시 (외부에서 호출 가능)
        /// </summary>
        public void ShowHUDForLayerObject(GameObject targetObject, Vector2 screenPosition, HUDType hudType)
        {
            HUDData hudData = CreateHUDDataForObject(targetObject);
            hudData.hudType = hudType; // 강제로 HUD 타입 설정
            
            CreateLayerSpecificHUD(hudData, screenPosition);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] 강제 {hudType} HUD 표시: {hudData.title}");
            }
        }
        
        /// <summary>
        /// 레이어별 레이어마스크 설정
        /// </summary>
        public void SetLayerMask(HUDType hudType, LayerMask layerMask)
        {
            switch (hudType)
            {
                case HUDType.Building:
                    buildingLayerMask = layerMask;
                    break;
                case HUDType.Worker:
                    workerLayerMask = layerMask;
                    break;
                case HUDType.Resource:
                    resourceLayerMask = layerMask;
                    break;
                case HUDType.Facility:
                    facilityLayerMask = layerMask;
                    break;
                case HUDType.NPC:
                    npcLayerMask = layerMask;
                    break;
            }
            
            // 레이어 매핑 재초기화
            InitializeLayerMapping();
            
            if (enableDebugLogs)
            {
                Debug.Log($"[TouchInfoManager] {hudType} 레이어마스크 설정: {layerMask.value}");
            }
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmos()
        {
            if (!showTouchGizmos)
                return;
            
            // 터치 감지 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxTouchDistance);
        }
        #endregion
    }
}
