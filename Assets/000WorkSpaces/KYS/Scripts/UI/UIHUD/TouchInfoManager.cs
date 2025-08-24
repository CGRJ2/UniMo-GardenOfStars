using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace KYS
{
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
        
        [Header("Popup Settings")]
        //[SerializeField] private float popupCloseDelay = 0.1f; // 팝업 닫기 지연 시간
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showTouchGizmos = false;
        
        private Camera mainCamera;
        private TouchInfoHUD currentHUD;
        private List<TouchInfoHUD> activeHUDs = new List<TouchInfoHUD>();
        
        // 테스트용 데이터
        private Dictionary<Vector2, string> testData = new Dictionary<Vector2, string>();
        
        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[TouchInfoManager] Main Camera를 찾을 수 없습니다.");
            }
            
            // 테스트용 데이터 초기화
            InitializeTestData();
        }
        
        private void Start()
        {
            if (enableDebugLogs)
            {
                //Debug.Log("[TouchInfoManager] 터치 정보 매니저 초기화 완료");
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
                // TouchInfoHUD 자체를 클릭한 경우 아무것도 하지 않음 (HUD 유지)
                return;
            }
            
            // UI 요소 클릭인지 확인 (TouchInfoHUD 제외)
            bool isUIElementClicked = IsPointerOverUI(screenPosition);
            
            // UI 요소 클릭인 경우 - HUD만 닫고 새로운 HUD 생성하지 않음
            if (isUIElementClicked)
            {
                // 기존 TouchInfoHUD가 있는지 확인하고 닫기
                bool hadExistingHUD = CloseExistingTouchInfoHUD();
                
                if (enableDebugLogs)
                {
                    if (hadExistingHUD)
                    {
                        Debug.Log("[TouchInfoManager] UI 요소 클릭 감지 - HUD 닫기 완료, UI 기능 실행 허용");
                    }
                    else
                    {
                        Debug.Log("[TouchInfoManager] UI 요소 클릭 감지 - HUD 없음, UI 기능 실행 허용");
                    }
                }
                // UI 요소의 클릭 이벤트가 정상적으로 처리되도록 return
                return;
            }
            
            // 게임 오브젝트 클릭인 경우 - 기존 HUD 닫고 새로운 HUD 생성
            // 기존 TouchInfoHUD가 있는지 확인하고 닫기
            CloseExistingTouchInfoHUD();
            
            // 월드 좌표로 변환
            Vector3 worldPosition = ScreenToWorldPoint(screenPosition);
            
            // 레이캐스트로 대상 감지
            GameObject targetObject = GetTargetObject(worldPosition);
            
            if (targetObject != null)
            {
                ShowInfoForObject(targetObject, screenPosition);
            }
            else
            {
                // 대상이 없으면 테스트용 HUD 표시
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
                            hudAllPanel.CheckHUDAllPanelStatus();
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
        /// 오브젝트에 대한 정보 HUD 표시
        /// </summary>
        private void ShowInfoForObject(GameObject targetObject, Vector2 screenPosition)
        {
            // 오브젝트 정보 가져오기
            string title = GetObjectTitle(targetObject);
            string description = GetObjectDescription(targetObject);
            Sprite icon = GetObjectIcon(targetObject);
            
            // HUD 생성 (기존 HUD는 이미 ProcessTouch에서 닫혔음)
            TouchInfoHUD.ShowInfoHUD(screenPosition, title, description, icon);

            if (enableDebugLogs)
            {
                //Debug.Log($"[TouchInfoManager] 오브젝트 정보 HUD 표시: {title}");
            }
        }
        
        /// <summary>
        /// 테스트용 HUD 표시
        /// </summary>
        private void ShowTestHUD(Vector2 screenPosition)
        {
            // 테스트용 정보 생성
            string title = "빈 공간";
            string description = $"터치 위치: ({screenPosition.x:F0}, {screenPosition.y:F0})\n이곳에 실제 건물이나 생산시설이 배치됩니다.";
            
            // HUD 생성 (기존 HUD는 이미 ProcessTouch에서 닫혔음)
            TouchInfoHUD.ShowInfoHUD(screenPosition, title, description);

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
