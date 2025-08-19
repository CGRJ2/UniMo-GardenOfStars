using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Added for Dictionary

namespace KYS.UI
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
            safeArea = Screen.safeArea;
            
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
            
            Debug.Log($"[SafeAreaManager] SafeArea 계산 완료: {safeArea}, Anchor: {anchorMin} ~ {anchorMax}");
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
                if (showDebugArea)
                {
                    Image image = panel.AddComponent<Image>();
                    image.color = debugColor;
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
            
            // 이미 SafeArea가 적용된 Canvas인지 확인
            if (safeAreaPanels.ContainsKey(canvas))
            {
                Debug.Log($"[SafeAreaManager] Canvas '{canvas.name}'에는 이미 SafeArea가 적용되어 있습니다.");
                return;
            }
            
            // SafeArea 패널 생성
            GameObject safeAreaPanel = Instantiate(safeAreaPanelPrefab, canvas.transform);
            safeAreaPanel.name = "SafeAreaPanel";
            safeAreaPanel.SetActive(true);
            
            // SafeArea 패널을 Canvas의 첫 번째 자식으로 이동
            safeAreaPanel.transform.SetAsFirstSibling();
            
            // 딕셔너리에 저장
            safeAreaPanels[canvas] = safeAreaPanel;
            
            Debug.Log($"[SafeAreaManager] Canvas '{canvas.name}'에 SafeArea 적용 완료");
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
        /// 자식 UI 요소들을 SafeArea에 맞게 조정
        /// </summary>
        private void ResizeChildrenToSafeArea()
        {
            RectTransform[] children = GetComponentsInChildren<RectTransform>();
            
            foreach (RectTransform child in children)
            {
                if (child == rectTransform) continue; // 자기 자신 제외
                
                // 자식 요소의 앵커를 SafeArea에 맞게 조정
                AdjustChildToSafeArea(child);
            }
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
