using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
        [Header("Touch Detection Settings")]
        [SerializeField] private bool enableTouchDetection = true;
        [SerializeField] private bool enableClickDetection = true;
        [SerializeField] private LayerMask touchableLayerMask = -1; // 모든 레이어
        [SerializeField] private float maxTouchDistance = 100f; // 터치 감지 최대 거리
        
        [Header("Layer-Specific HUD Settings")]
        [SerializeField] private LayerMask buildingLayerMask = 1 << 8; // Building 레이어
        [SerializeField] private LayerMask workerLayerMask = 1 << 9;   // Worker 레이어
        [SerializeField] private LayerMask resourceLayerMask = 1 << 10; // Resource 레이어
        [SerializeField] private LayerMask facilityLayerMask = 1 << 11; // Facility 레이어
        [SerializeField] private LayerMask npcLayerMask = 1 << 12;      // NPC 레이어
        
        [Header("HUD Prefab References")]
        [SerializeField] private string buildingHUDKey = "BuildingInfoHUD";
        [SerializeField] private string workerHUDKey = "WorkerInfoHUD";
        [SerializeField] private string resourceHUDKey = "ResourceInfoHUD";
        [SerializeField] private string facilityHUDKey = "FacilityInfoHUD";
        [SerializeField] private string npcHUDKey = "NPCInfoHUD";
        [SerializeField] private string defaultHUDKey = "TouchInfoHUD";
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showTouchGizmos = false;
        
        private Camera mainCamera;
        private TouchInfoHUD currentHUD;
        private List<TouchInfoHUD> activeHUDs = new List<TouchInfoHUD>();
        
        // 레이어별 HUD 매핑
        private Dictionary<LayerMask, HUDType> layerToHUDType = new Dictionary<LayerMask, HUDType>();
        
        // 테스트용 데이터
        private Dictionary<Vector2, string> testData = new Dictionary<Vector2, string>();
        
        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[TouchInfoManager] Main Camera를 찾을 수 없습니다.");
            }
            
            // 레이어별 HUD 타입 매핑 초기화
            InitializeLayerMapping();
            
            // 테스트용 데이터 초기화
            InitializeTestData();
        }
        
        /// <summary>
        /// 레이어별 HUD 타입 매핑 초기화
        /// </summary>
        private void InitializeLayerMapping()
        {
            layerToHUDType.Clear();
            //layerToHUDType.Add(buildingLayerMask, HUDType.Building);
            //layerToHUDType.Add(workerLayerMask, HUDType.Worker);
            //layerToHUDType.Add(resourceLayerMask, HUDType.Resource);
            //layerToHUDType.Add(facilityLayerMask, HUDType.Facility);
            //layerToHUDType.Add(npcLayerMask, HUDType.NPC);
            
            if (enableDebugLogs)
            {
                Debug.Log("[TouchInfoManager] 레이어별 HUD 매핑 초기화 완료");
            }
        }
        
        private void Start()
        {
            if (enableDebugLogs)
            {
                Debug.Log("[TouchInfoManager] 터치 정보 매니저 초기화 완료");
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
                HandleMouseInput();
            }
        }
        
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
            
            // UI 요소 클릭인지 확인 (TouchInfoHUD 제외)
            bool isUIElementClicked = IsPointerOverUI(screenPosition);
            
            if (isUIElementClicked)
            {
                bool hadExistingHUD = CloseExistingTouchInfoHUD();
                
                if (enableDebugLogs && hadExistingHUD)
                {
                    Debug.Log("[TouchInfoManager] UI 요소 클릭 감지 - HUD 닫기 완료");
                }
                return;
            }
            
            // 게임 오브젝트 클릭인 경우
            CloseExistingTouchInfoHUD();
            
            Vector3 worldPosition = ScreenToWorldPoint(screenPosition);
            GameObject targetObject = GetTargetObject(worldPosition);
            
            if (targetObject != null)
            {
                ShowInfoForObject(targetObject, screenPosition);
            }
            else
            {
                ShowTestHUD(screenPosition);
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
            
            // TouchInfoHUD와 HUDBackdropUI를 제외한 다른 UI 요소만 필터링
            List<RaycastResult> filteredResults = new List<RaycastResult>();
            foreach (var result in results)
            {
                // TouchInfoHUD와 HUDBackdropUI 관련 UI 요소는 제외
                if (!result.gameObject.name.Contains("TouchInfoHUD") && 
                    !result.gameObject.name.Contains("InfoHUD") &&
                    !result.gameObject.name.Contains("HUDBackdrop") &&
                    result.gameObject.GetComponentInParent<TouchInfoHUD>() == null &&
                    result.gameObject.GetComponent<HUDBackdropUI>() == null)
                {
                    filteredResults.Add(result);
                }
            }
            
            if (enableDebugLogs && filteredResults.Count > 0)
            {
                string uiElementNames = "";
                for (int i = 0; i < Mathf.Min(filteredResults.Count, 3); i++) // 최대 3개까지만 표시
                {
                    uiElementNames += filteredResults[i].gameObject.name;
                    if (i < Mathf.Min(filteredResults.Count, 3) - 1) uiElementNames += ", ";
                }
                //Debug.Log($"[TouchInfoManager] UI 요소 클릭 감지: {uiElementNames} (총 {filteredResults.Count}개 UI 요소)");
                
                // HUDAllPanel 관련 UI 요소인지 확인
                foreach (var result in filteredResults)
                {
                    if (result.gameObject.name.Contains("RImageButton1") || result.gameObject.name.Contains("HUDRightPanel"))
                    {
                        //Debug.Log($"[TouchInfoManager] HUDAllPanel 관련 UI 요소 감지: {result.gameObject.name}");
                        
                        // HUDAllPanel 컴포넌트 찾기
                        var hudAllPanel = result.gameObject.GetComponentInParent<HUDAllPanel>();
                        if (hudAllPanel != null)
                        {
                            //Debug.Log($"[TouchInfoManager] HUDAllPanel 컴포넌트 발견: {hudAllPanel.gameObject.name}");
                            //hudAllPanel.PrintUIElementInfo();
                        }
                        else
                        {
                            Debug.LogWarning($"[TouchInfoManager] HUDAllPanel 컴포넌트를 찾을 수 없음");
                        }
                    }
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
            
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
            return worldPosition;
        }
        
        /// <summary>
        /// 레이캐스트로 대상 오브젝트 감지
        /// </summary>
        private GameObject GetTargetObject(Vector3 worldPosition)
        {
            if (mainCamera == null)
                return null;
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, maxTouchDistance, touchableLayerMask))
            {
                // HUDAllPanel 관련 오브젝트는 무시
                if (hit.collider.gameObject.name.Contains("HUDAllPanel") || 
                    hit.collider.gameObject.name.Contains("HUD") ||
                    hit.collider.transform.IsChildOf(transform)) // TouchInfoManager의 자식 오브젝트들 무시
                {
                    if (enableDebugLogs)
                        //Debug.Log($"[TouchInfoManager] HUD 관련 오브젝트 무시: {hit.collider.gameObject.name}");
                    return null;
                }
                
                if (enableDebugLogs)
                    //Debug.Log($"[TouchInfoManager] 대상 감지: {hit.collider.gameObject.name}");
                
                return hit.collider.gameObject;
            }
            
            return null;
        }
        
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
            HUDData data = new HUDData(
                worker.name,
                $"작업자 정보\n위치: {worker.transform.position}\n상태: 근무 중",
                null,
                HUDType.Worker
            );
            
            // 작업자별 특정 데이터 추가
            data.customData["workerLevel"] = 3;
            data.customData["skill"] = "Production";
            data.customData["happiness"] = 0.9f;
            data.customData["salary"] = 1500;
            
            return data;
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
                // UIManager를 통해 레이어별 HUD 생성
                var hud = await UIManager.Instance.CreateHUDAsync<TouchInfoHUD>(hudKey);
                
                if (hud != null)
                {
                    // HUD 위치 설정
                    hud.SetHUDPosition(screenPosition);
                    
                    // HUD 데이터 설정
                    hud.SetInfo(hudData.title, hudData.description, hudData.icon);
                    
                    // 레이어별 추가 설정
                    SetupLayerSpecificHUD(hud, hudData);
                    
                    // 활성 HUD 목록에 추가
                    activeHUDs.Add(hud);
                    currentHUD = hud;
                    
                    if (enableDebugLogs)
                    {
                        Debug.Log($"[TouchInfoManager] {hudData.hudType} HUD 생성 완료: {hudKey}");
                    }
                }
                else
                {
                    // 특정 HUD를 찾을 수 없는 경우 기본 HUD 사용
                    Debug.LogWarning($"[TouchInfoManager] {hudKey}를 찾을 수 없어 기본 HUD 사용");
                    await CreateDefaultHUD(hudData, screenPosition);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TouchInfoManager] HUD 생성 실패: {e.Message}");
                await CreateDefaultHUD(hudData, screenPosition);
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
        /// 레이어별 HUD 추가 설정
        /// </summary>
        private void SetupLayerSpecificHUD(TouchInfoHUD hud, HUDData hudData)
        {
            switch (hudData.hudType)
            {
                case HUDType.Building:
                    SetupBuildingHUD(hud, hudData);
                    break;
                case HUDType.Worker:
                    SetupWorkerHUD(hud, hudData);
                    break;
                case HUDType.Resource:
                    SetupResourceHUD(hud, hudData);
                    break;
                case HUDType.Facility:
                    SetupFacilityHUD(hud, hudData);
                    break;
                case HUDType.NPC:
                    SetupNPCHUD(hud, hudData);
                    break;
            }
        }
        
        /// <summary>
        /// 건물 HUD 추가 설정
        /// </summary>
        private void SetupBuildingHUD(TouchInfoHUD hud, HUDData hudData)
        {
            // 건물별 특정 UI 요소 설정
            if (hudData.customData.ContainsKey("buildingType"))
            {
                string buildingType = hudData.customData["buildingType"].ToString();
                hud.SetActionButtonText($"건설 ({buildingType})");
                hud.SetActionButtonActive(true);
            }
        }
        
        /// <summary>
        /// 작업자 HUD 추가 설정
        /// </summary>
        private void SetupWorkerHUD(TouchInfoHUD hud, HUDData hudData)
        {
            // 작업자별 특정 UI 요소 설정
            if (hudData.customData.ContainsKey("workerLevel"))
            {
                int level = (int)hudData.customData["workerLevel"];
                hud.SetActionButtonText($"고용 (레벨 {level})");
                hud.SetActionButtonActive(true);
            }
        }
        
        /// <summary>
        /// 자원 HUD 추가 설정
        /// </summary>
        private void SetupResourceHUD(TouchInfoHUD hud, HUDData hudData)
        {
            // 자원별 특정 UI 요소 설정
            if (hudData.customData.ContainsKey("resourceType"))
            {
                string resourceType = hudData.customData["resourceType"].ToString();
                hud.SetActionButtonText($"채굴 ({resourceType})");
                hud.SetActionButtonActive(true);
            }
        }
        
        /// <summary>
        /// 시설 HUD 추가 설정
        /// </summary>
        private void SetupFacilityHUD(TouchInfoHUD hud, HUDData hudData)
        {
            // 시설별 특정 UI 요소 설정
            if (hudData.customData.ContainsKey("facilityType"))
            {
                string facilityType = hudData.customData["facilityType"].ToString();
                hud.SetActionButtonText($"이용 ({facilityType})");
                hud.SetActionButtonActive(true);
            }
        }
        
        /// <summary>
        /// NPC HUD 추가 설정
        /// </summary>
        private void SetupNPCHUD(TouchInfoHUD hud, HUDData hudData)
        {
            // NPC별 특정 UI 요소 설정
            if (hudData.customData.ContainsKey("npcType"))
            {
                string npcType = hudData.customData["npcType"].ToString();
                hud.SetActionButtonText($"대화 ({npcType})");
                hud.SetActionButtonActive(true);
            }
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
        
        private void OnDrawGizmos()
        {
            if (!showTouchGizmos)
                return;
            
            // 터치 감지 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxTouchDistance);
        }
    }
}
