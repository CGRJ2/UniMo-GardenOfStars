using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Added for Dictionary

namespace KYS
{
    /// <summary>
    /// SafeArea 관리를 위한 매니저
    /// </summary>
    public class SafeAreaManager : MonoBehaviour
    {
        [Header("SafeArea Settings")]
        [SerializeField] private bool enableSafeArea = true;
        [SerializeField] private Color debugColor = new Color(1, 0, 0, 0.3f);
        [SerializeField] private bool showDebugArea = false;
        
        [Header("Simulator Test Settings")]
        [SerializeField] private bool useTestSafeArea = false;
        [SerializeField] private Rect testSafeArea = new Rect(50, 100, 300, 600); // 테스트용 SafeArea
        [SerializeField] private bool showTestSafeArea = true;
        
        [Header("SafeArea Panel Prefab")]
        [SerializeField] private GameObject safeAreaPanelPrefab;
        
        // SafeArea 정보
        private Rect safeArea;
        private Vector2 anchorMin;
        private Vector2 anchorMax;
        
        // 생성된 SafeArea 패널들
        private Dictionary<Canvas, GameObject> safeAreaPanels = new Dictionary<Canvas, GameObject>();
        
        private void Awake()
        {
            if (enableSafeArea)
            {
                CalculateSafeArea();
                CreateSafeAreaPanelPrefab();
            }
        }
        
        private void Start()
        {
            if (enableSafeArea)
            {
                ApplySafeAreaToAllCanvases();
            }
        }
        
        /// <summary>
        /// SafeArea 계산
        /// </summary>
        private void CalculateSafeArea()
        {
            // 시뮬레이터 테스트용 SafeArea 사용
            if (useTestSafeArea)
            {
                safeArea = testSafeArea;
                Debug.Log($"[SafeAreaManager] 테스트 SafeArea 사용: {safeArea}");
            }
            else
            {
                safeArea = Screen.safeArea;
                //Debug.Log($"[SafeAreaManager] 실제 SafeArea 사용: {safeArea}");
            }
            
            // Canvas Scaler에 따른 정규화
            CanvasScaler scaler = FindObjectOfType<CanvasScaler>();
            if (scaler != null)
            {
                Vector2 screenSize = new Vector2(Screen.width, Screen.height);
                anchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
                anchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
            }
            else
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
            }
            
            //Debug.Log($"[SafeAreaManager] SafeArea 계산 완료: {safeArea}, Anchor: {anchorMin} ~ {anchorMax}");
        }
        
        /// <summary>
        /// SafeArea 패널 프리팹 생성 (런타임)
        /// </summary>
        private void CreateSafeAreaPanelPrefab()
        {
            if (safeAreaPanelPrefab == null)
            {
                // 동적으로 SafeArea 패널 생성
                GameObject panel = new GameObject("SafeAreaPanel");
                
                // RectTransform 설정
                RectTransform rectTransform = panel.AddComponent<RectTransform>();
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                // Image 컴포넌트 추가 (디버그용)
                if (showDebugArea || (useTestSafeArea && showTestSafeArea))
                {
                    Image image = panel.AddComponent<Image>();
                    image.color = debugColor;
                    
                    // 테스트 SafeArea인 경우 더 눈에 띄는 색상 사용
                    if (useTestSafeArea)
                    {
                        image.color = new Color(0, 1, 0, 0.5f); // 초록색으로 표시
                    }
                }
                
                // SafeArea 컴포넌트 추가
                SafeAreaPanel safeAreaComponent = panel.AddComponent<SafeAreaPanel>();
                
                safeAreaPanelPrefab = panel;
                DontDestroyOnLoad(panel);
                panel.SetActive(false); // 프리팹으로 사용하기 위해 비활성화
                
                Debug.Log("[SafeAreaManager] SafeArea 패널 프리팹 생성 완료");
            }
        }
        
        /// <summary>
        /// 모든 Canvas에 SafeArea 적용
        /// </summary>
        private void ApplySafeAreaToAllCanvases()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            
            foreach (Canvas canvas in canvases)
            {
                // LoadingCanvas는 SafeArea 적용 제외
                if (canvas.name.Contains("LoadingCanvas") || canvas.name.Contains("Loading"))
                {
                    Debug.Log($"[SafeAreaManager] LoadingCanvas '{canvas.name}'는 SafeArea 적용 제외");
                    continue;
                }
                
                ApplySafeAreaToCanvas(canvas);
            }
            
            Debug.Log($"[SafeAreaManager] {canvases.Length}개의 Canvas에 SafeArea 적용 완료");
        }
        
        /// <summary>
        /// 특정 Canvas에 SafeArea 적용
        /// </summary>
        public void ApplySafeAreaToCanvas(Canvas canvas)
        {
            if (!enableSafeArea || canvas == null) return;
            
            // LoadingCanvas는 SafeArea 적용 제외
            if (canvas.name.Contains("LoadingCanvas") || canvas.name.Contains("Loading"))
            {
                //Debug.Log($"[SafeAreaManager] LoadingCanvas '{canvas.name}'는 SafeArea 적용 제외");
                return;
            }
            
            // 이미 SafeArea가 적용된 Canvas인지 확인
            if (safeAreaPanels.ContainsKey(canvas))
            {
                //Debug.Log($"[SafeAreaManager] Canvas '{canvas.name}'에는 이미 SafeArea가 적용되어 있습니다.");
                return;
            }
            
            //Debug.Log($"[SafeAreaManager] 🔧 SafeArea 패널 생성 시작: {canvas.name}");
            //Debug.Log($"[SafeAreaManager] safeAreaPanelPrefab 존재 여부: {safeAreaPanelPrefab != null}");
            
            // SafeArea 패널 생성
            GameObject safeAreaPanel = Instantiate(safeAreaPanelPrefab, canvas.transform);
            safeAreaPanel.name = "SafeAreaPanel";
            safeAreaPanel.SetActive(true);
            
            //Debug.Log($"[SafeAreaManager] SafeArea 패널 생성됨: {safeAreaPanel.name}");
            //Debug.Log($"[SafeAreaManager] SafeArea 패널 부모: {safeAreaPanel.transform.parent?.name}");
            //Debug.Log($"[SafeAreaManager] SafeArea 패널 위치: {safeAreaPanel.transform.position}");
            
            // SafeArea 패널을 Canvas의 첫 번째 자식으로 이동
            safeAreaPanel.transform.SetAsFirstSibling();
            
            //Debug.Log($"[SafeAreaManager] SafeArea 패널을 첫 번째 자식으로 이동 완료");
            //Debug.Log($"[SafeAreaManager] Canvas 자식 수: {canvas.transform.childCount}");
            
            // 딕셔너리에 저장
            safeAreaPanels[canvas] = safeAreaPanel;
            
            //Debug.Log($"[SafeAreaManager] ✅ Canvas '{canvas.name}'에 SafeArea 적용 완료");
            //Debug.Log($"[SafeAreaManager] 현재 관리 중인 SafeArea 패널 수: {safeAreaPanels.Count}");
        }
        
        /// <summary>
        /// SafeArea 패널 제거
        /// </summary>
        public void RemoveSafeAreaFromCanvas(Canvas canvas)
        {
            if (safeAreaPanels.ContainsKey(canvas))
            {
                GameObject panel = safeAreaPanels[canvas];
                if (panel != null)
                {
                    DestroyImmediate(panel);
                }
                safeAreaPanels.Remove(canvas);
                
                Debug.Log($"[SafeAreaManager] Canvas '{canvas.name}'에서 SafeArea 제거 완료");
            }
        }
        
        /// <summary>
        /// LoadingCanvas에서 SafeAreaPanel 제거
        /// </summary>
        [ContextMenu("Remove SafeArea from LoadingCanvas")]
        public void RemoveSafeAreaFromLoadingCanvas()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name.Contains("LoadingCanvas") || canvas.name.Contains("Loading"))
                {
                    // SafeAreaPanel 찾기
                    Transform safeAreaPanel = canvas.transform.Find("SafeAreaPanel");
                    if (safeAreaPanel != null)
                    {
                        Debug.Log($"[SafeAreaManager] LoadingCanvas '{canvas.name}'에서 SafeAreaPanel 제거");
                        DestroyImmediate(safeAreaPanel.gameObject);
                        
                        // 딕셔너리에서도 제거
                        if (safeAreaPanels.ContainsKey(canvas))
                        {
                            safeAreaPanels.Remove(canvas);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 모든 SafeArea 패널 제거
        /// </summary>
        public void RemoveAllSafeAreas()
        {
            foreach (var kvp in safeAreaPanels)
            {
                if (kvp.Value != null)
                {
                    DestroyImmediate(kvp.Value);
                }
            }
            safeAreaPanels.Clear();
            
            Debug.Log("[SafeAreaManager] 모든 SafeArea 패널 제거 완료");
        }
        
        /// <summary>
        /// SafeArea 정보 반환
        /// </summary>
        public Rect GetSafeArea()
        {
            return safeArea;
        }
        
        /// <summary>
        /// SafeArea 앵커 정보 반환
        /// </summary>
        public (Vector2 min, Vector2 max) GetSafeAreaAnchors()
        {
            return (anchorMin, anchorMax);
        }
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("Print SafeArea Info")]
        public void PrintSafeAreaInfo()
        {
            Debug.Log($"[SafeAreaManager] === SafeArea 정보 ===");
            Debug.Log($"Screen Size: {Screen.width} x {Screen.height}");
            Debug.Log($"SafeArea: {safeArea}");
            Debug.Log($"Anchor Min: {anchorMin}");
            Debug.Log($"Anchor Max: {anchorMax}");
            Debug.Log($"적용된 Canvas 수: {safeAreaPanels.Count}");
            
            foreach (var kvp in safeAreaPanels)
            {
                Debug.Log($"  - {kvp.Key.name}: {kvp.Value.name}");
            }
        }
        
        /// <summary>
        /// 특정 Canvas의 SafeAreaPanel 가져오기
        /// </summary>
        public SafeAreaPanel GetSafeAreaPanelForCanvas(Canvas canvas)
        {
            Debug.Log($"[SafeAreaManager] GetSafeAreaPanelForCanvas 호출: {canvas?.name}");
            Debug.Log($"[SafeAreaManager] safeAreaPanels 딕셔너리 크기: {safeAreaPanels.Count}");
            
            if (safeAreaPanels.ContainsKey(canvas))
            {
                GameObject panelObject = safeAreaPanels[canvas];
                Debug.Log($"[SafeAreaManager] 딕셔너리에서 찾음: {panelObject?.name}");
                
                if (panelObject != null)
                {
                    SafeAreaPanel panel = panelObject.GetComponent<SafeAreaPanel>();
                    Debug.Log($"[SafeAreaManager] SafeAreaPanel 컴포넌트 존재: {panel != null}");
                    return panel;
                }
                else
                {
                    Debug.LogWarning($"[SafeAreaManager] 딕셔너리에 있는 패널이 null임");
                }
            }
            else
            {
                Debug.LogWarning($"[SafeAreaManager] 딕셔너리에 Canvas '{canvas?.name}'가 없음");
                
                // 딕셔너리 내용 출력
                foreach (var kvp in safeAreaPanels)
                {
                    Debug.Log($"[SafeAreaManager] 딕셔너리 내용: {kvp.Key?.name} -> {kvp.Value?.name}");
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 모든 SafeAreaPanel 가져오기
        /// </summary>
        public List<SafeAreaPanel> GetAllSafeAreaPanels()
        {
            List<SafeAreaPanel> panels = new List<SafeAreaPanel>();
            foreach (var kvp in safeAreaPanels)
            {
                if (kvp.Value != null)
                {
                    SafeAreaPanel panel = kvp.Value.GetComponent<SafeAreaPanel>();
                    if (panel != null)
                    {
                        panels.Add(panel);
                    }
                }
            }
            return panels;
        }
        
        /// <summary>
        /// 기존 SafeAreaPanel들의 Anchors 업데이트
        /// </summary>
        [ContextMenu("Update All SafeArea Panels")]
        public void UpdateAllSafeAreaPanels()
        {
            Debug.Log("[SafeAreaManager] 모든 SafeAreaPanel Anchors 업데이트 시작");
            
            foreach (var kvp in safeAreaPanels)
            {
                if (kvp.Value != null)
                {
                    SafeAreaPanel panel = kvp.Value.GetComponent<SafeAreaPanel>();
                    if (panel != null)
                    {
                        panel.UpdateSafeAreaAnchors(anchorMin, anchorMax);
                    }
                }
            }
            
            Debug.Log($"[SafeAreaManager] {safeAreaPanels.Count}개의 SafeAreaPanel Anchors 업데이트 완료");
        }
        
        /// <summary>
        /// 테스트용 SafeArea 설정
        /// </summary>
        [ContextMenu("Set Test SafeArea")]
        public void SetTestSafeArea()
        {
            useTestSafeArea = true;
            showTestSafeArea = true;
            
            // 기존 SafeArea 패널들 제거
            RemoveAllSafeAreas();
            
            // 새로운 SafeArea로 재계산
            CalculateSafeArea();
            CreateSafeAreaPanelPrefab();
            ApplySafeAreaToAllCanvases();
            
            // 기존 SafeAreaPanel들의 Anchors 업데이트
            UpdateAllSafeAreaPanels();
            
            Debug.Log("[SafeAreaManager] 테스트 SafeArea 설정 완료");
        }
        
        /// <summary>
        /// 실제 SafeArea로 복원
        /// </summary>
        [ContextMenu("Restore Real SafeArea")]
        public void RestoreRealSafeArea()
        {
            useTestSafeArea = false;
            showTestSafeArea = false;
            
            // 기존 SafeArea 패널들 제거
            RemoveAllSafeAreas();
            
            // 실제 SafeArea로 재계산
            CalculateSafeArea();
            CreateSafeAreaPanelPrefab();
            ApplySafeAreaToAllCanvases();
            
            // 기존 SafeAreaPanel들의 Anchors 업데이트
            UpdateAllSafeAreaPanels();
            
            Debug.Log("[SafeAreaManager] 실제 SafeArea로 복원 완료");
        }
        
        /// <summary>
        /// 다양한 테스트 SafeArea 설정
        /// </summary>
        [ContextMenu("Set iPhone Notch SafeArea")]
        public void SetIPhoneNotchSafeArea()
        {
            testSafeArea = new Rect(0, 47, 375, 812 - 47 - 34); // iPhone X 스타일
            SetTestSafeArea();
        }
        
        [ContextMenu("Set Android Notch SafeArea")]
        public void SetAndroidNotchSafeArea()
        {
            testSafeArea = new Rect(0, 24, 360, 640 - 24 - 24); // Android 스타일
            SetTestSafeArea();
        }
        
        /// <summary>
        /// SafeArea 상태 확인
        /// </summary>
        [ContextMenu("Check SafeArea Status")]
        public void CheckSafeAreaStatus()
        {
            Debug.Log($"[SafeAreaManager] === SafeArea 상태 확인 ===");
            Debug.Log($"Enable SafeArea: {enableSafeArea}");
            Debug.Log($"Use Test SafeArea: {useTestSafeArea}");
            Debug.Log($"Show Test SafeArea: {showTestSafeArea}");
            Debug.Log($"SafeArea: {safeArea}");
            Debug.Log($"적용된 Canvas 수: {safeAreaPanels.Count}");
            
            foreach (var kvp in safeAreaPanels)
            {
                Debug.Log($"  - {kvp.Key.name}: {kvp.Value.name}");
                SafeAreaPanel panel = kvp.Value.GetComponent<SafeAreaPanel>();
                if (panel != null)
                {
                    Debug.Log($"    자식 요소 수: {panel.transform.childCount}");
                }
            }
        }
        
        /// <summary>
        /// SafeAreaPanel 상태 상세 확인
        /// </summary>
        [ContextMenu("Check SafeAreaPanel Details")]
        public void CheckSafeAreaPanelDetails()
        {
            Debug.Log($"[SafeAreaManager] === SafeAreaPanel 상세 상태 ===");
            Debug.Log($"safeAreaPanelPrefab 존재: {safeAreaPanelPrefab != null}");
            Debug.Log($"safeAreaPanels 딕셔너리 크기: {safeAreaPanels.Count}");
            
            foreach (var kvp in safeAreaPanels)
            {
                Debug.Log($"Canvas: {kvp.Key.name}");
                Debug.Log($"  SafeAreaPanel GameObject: {kvp.Value?.name}");
                Debug.Log($"  SafeAreaPanel 활성화: {kvp.Value?.activeInHierarchy}");
                Debug.Log($"  SafeAreaPanel 부모: {kvp.Value?.transform.parent?.name}");
                Debug.Log($"  SafeAreaPanel 자식 수: {kvp.Value?.transform.childCount}");
                
                SafeAreaPanel panel = kvp.Value?.GetComponent<SafeAreaPanel>();
                if (panel != null)
                {
                    Debug.Log($"  SafeAreaPanel 컴포넌트: {panel.name}");
                    Debug.Log($"  SafeAreaPanel 자식 요소 수: {panel.transform.childCount}");
                    
                    // 자식 요소들 출력
                    for (int i = 0; i < panel.transform.childCount; i++)
                    {
                        Transform child = panel.transform.GetChild(i);
                        Debug.Log($"    자식 {i}: {child.name} ({child.GetType().Name})");
                    }
                }
                else
                {
                    Debug.LogWarning($"  SafeAreaPanel 컴포넌트가 없음!");
                }
            }
        }
        
        private void OnDestroy()
        {
            RemoveAllSafeAreas();
        }
    }
    
    /// <summary>
    /// SafeArea 패널 컴포넌트
    /// </summary>
    public class SafeAreaPanel : MonoBehaviour
    {
        [Header("SafeArea Settings")]
        [SerializeField] private bool autoResizeChildren = true;
        //[SerializeField] private bool maintainAspectRatio = true;
        
        private RectTransform rectTransform;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        private void Start()
        {
            if (autoResizeChildren)
            {
                ResizeChildrenToSafeArea();
            }
        }
        
        /// <summary>
        /// SafeArea Anchors 업데이트
        /// </summary>
        public void UpdateSafeAreaAnchors(Vector2 anchorMin, Vector2 anchorMax)
        {
            if (rectTransform != null)
            {
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                Debug.Log($"[SafeAreaPanel] Anchors 업데이트: {anchorMin} ~ {anchorMax}");
            }
        }
        
        /// <summary>
        /// 자식 UI 요소들을 SafeArea에 맞게 조정
        /// </summary>
        private void ResizeChildrenToSafeArea()
        {
            RectTransform[] children = GetComponentsInChildren<RectTransform>();
            
            foreach (RectTransform child in children)
            {
                if (child == rectTransform) continue; // 자기 자신 제외
                
                // SafeAreaPanel 자체는 제외
                if (child.GetComponent<SafeAreaPanel>() != null) continue;
                
                // 자식 요소의 앵커를 SafeArea에 맞게 조정
                AdjustChildToSafeArea(child);
            }
            
            Debug.Log($"[SafeAreaPanel] {children.Length - 1}개의 자식 요소를 SafeArea에 맞게 조정 완료");
        }
        
        /// <summary>
        /// 자식 요소를 SafeArea에 맞게 조정
        /// </summary>
        private void AdjustChildToSafeArea(RectTransform child)
        {
            // 현재 앵커 설정
            Vector2 currentAnchorMin = child.anchorMin;
            Vector2 currentAnchorMax = child.anchorMax;
            
            // SafeArea 매니저에서 앵커 정보 가져오기
            SafeAreaManager safeAreaManager = FindObjectOfType<SafeAreaManager>();
            if (safeAreaManager != null)
            {
                var (safeAnchorMin, safeAnchorMax) = safeAreaManager.GetSafeAreaAnchors();
                
                // 자식 요소의 앵커를 SafeArea 내부로 제한
                child.anchorMin = new Vector2(
                    Mathf.Clamp(currentAnchorMin.x, safeAnchorMin.x, safeAnchorMax.x),
                    Mathf.Clamp(currentAnchorMin.y, safeAnchorMin.y, safeAnchorMax.y)
                );
                
                child.anchorMax = new Vector2(
                    Mathf.Clamp(currentAnchorMax.x, safeAnchorMin.x, safeAnchorMax.x),
                    Mathf.Clamp(currentAnchorMax.y, safeAnchorMin.y, safeAnchorMax.y)
                );
            }
        }
        
        /// <summary>
        /// SafeArea 패널의 크기 정보 반환
        /// </summary>
        public Rect GetSafeAreaRect()
        {
            if (rectTransform != null)
            {
                return rectTransform.rect;
            }
            return Rect.zero;
        }
    }
}
