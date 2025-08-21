using UnityEngine;
using UnityEngine.UI;


namespace KYS
{
    /// <summary>
    /// SafeArea 사용 예시
    /// </summary>
    public class SafeAreaExamples : MonoBehaviour
    {
        [Header("SafeArea 테스트 UI")]
        [SerializeField] private Button testButton;
        [SerializeField] private Text infoText;
        [SerializeField] private Image debugImage;
        
        [Header("SafeArea 설정")]
        [SerializeField] private bool enableSafeAreaDebug = true;
        [SerializeField] private Color safeAreaDebugColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color unsafeAreaDebugColor = new Color(1, 0, 0, 0.3f);
        
        private SafeAreaManager safeAreaManager;
        
        private void Start()
        {
            safeAreaManager = FindObjectOfType<SafeAreaManager>();
            
            if (safeAreaManager != null)
            {
                SetupUI();
                UpdateSafeAreaInfo();
            }
            else
            {
                Debug.LogError("[SafeAreaExamples] SafeAreaManager를 찾을 수 없습니다.");
            }
        }
        
        /// <summary>
        /// UI 설정
        /// </summary>
        private void SetupUI()
        {
            if (testButton != null)
            {
                testButton.onClick.AddListener(OnTestButtonClick);
            }
            
            if (enableSafeAreaDebug)
            {
                CreateDebugUI();
            }
        }
        
        /// <summary>
        /// 디버그 UI 생성
        /// </summary>
        private void CreateDebugUI()
        {
            // SafeArea 디버그 이미지 생성
            if (debugImage == null)
            {
                GameObject debugObj = new GameObject("SafeAreaDebug");
                debugObj.transform.SetParent(transform);
                
                debugImage = debugObj.AddComponent<Image>();
                debugImage.color = safeAreaDebugColor;
                
                RectTransform rectTransform = debugImage.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                // SafeArea 정보에 맞게 조정
                var (anchorMin, anchorMax) = safeAreaManager.GetSafeAreaAnchors();
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
                
                // 최하위로 이동
                debugImage.transform.SetAsFirstSibling();
            }
        }
        
        /// <summary>
        /// SafeArea 정보 업데이트
        /// </summary>
        private void UpdateSafeAreaInfo()
        {
            if (infoText != null && safeAreaManager != null)
            {
                Rect safeArea = safeAreaManager.GetSafeArea();
                var (anchorMin, anchorMax) = safeAreaManager.GetSafeAreaAnchors();
                
                string info = $"=== SafeArea 정보 ===\n";
                info += $"화면 크기: {Screen.width} x {Screen.height}\n";
                info += $"SafeArea: {safeArea}\n";
                info += $"Anchor Min: {anchorMin}\n";
                info += $"Anchor Max: {anchorMax}\n";
                info += $"기기: {SystemInfo.deviceModel}\n";
                info += $"OS: {SystemInfo.operatingSystem}";
                
                infoText.text = info;
            }
        }
        
        /// <summary>
        /// 테스트 버튼 클릭 이벤트
        /// </summary>
        private void OnTestButtonClick()
        {
            Debug.Log("[SafeAreaExamples] SafeArea 테스트 버튼 클릭");
            
            // SafeArea 정보 출력
            safeAreaManager.PrintSafeAreaInfo();
            
            // UI 정보 업데이트
            UpdateSafeAreaInfo();
            
            // 디버그 UI 토글
            if (debugImage != null)
            {
                debugImage.gameObject.SetActive(!debugImage.gameObject.activeSelf);
            }
        }
        
        /// <summary>
        /// SafeArea 적용 테스트
        /// </summary>
        [ContextMenu("Test SafeArea Application")]
        public void TestSafeAreaApplication()
        {
            if (safeAreaManager != null)
            {
                // 모든 Canvas에 SafeArea 적용
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    safeAreaManager.ApplySafeAreaToCanvas(canvas);
                }
                
                Debug.Log($"[SafeAreaExamples] {canvases.Length}개의 Canvas에 SafeArea 적용 테스트 완료");
            }
        }
        
        /// <summary>
        /// SafeArea 제거 테스트
        /// </summary>
        [ContextMenu("Test SafeArea Removal")]
        public void TestSafeAreaRemoval()
        {
            if (safeAreaManager != null)
            {
                safeAreaManager.RemoveAllSafeAreas();
                Debug.Log("[SafeAreaExamples] 모든 SafeArea 제거 테스트 완료");
            }
        }
        
        /// <summary>
        /// UI 요소를 SafeArea에 맞게 조정하는 예시
        /// </summary>
        public void AdjustUIElementToSafeArea(RectTransform uiElement)
        {
            if (safeAreaManager != null && uiElement != null)
            {
                var (anchorMin, anchorMax) = safeAreaManager.GetSafeAreaAnchors();
                
                // UI 요소의 앵커를 SafeArea 내부로 제한
                Vector2 currentAnchorMin = uiElement.anchorMin;
                Vector2 currentAnchorMax = uiElement.anchorMax;
                
                uiElement.anchorMin = new Vector2(
                    Mathf.Clamp(currentAnchorMin.x, anchorMin.x, anchorMax.x),
                    Mathf.Clamp(currentAnchorMin.y, anchorMin.y, anchorMax.y)
                );
                
                uiElement.anchorMax = new Vector2(
                    Mathf.Clamp(currentAnchorMax.x, anchorMin.x, anchorMax.x),
                    Mathf.Clamp(currentAnchorMax.y, anchorMin.y, anchorMax.y)
                );
                
                Debug.Log($"[SafeAreaExamples] UI 요소 '{uiElement.name}'을 SafeArea에 맞게 조정 완료");
            }
        }
        
        /// <summary>
        /// 특정 위치가 SafeArea 내부인지 확인
        /// </summary>
        public bool IsPositionInSafeArea(Vector2 screenPosition)
        {
            if (safeAreaManager != null)
            {
                Rect safeArea = safeAreaManager.GetSafeArea();
                return safeArea.Contains(screenPosition);
            }
            return true; // SafeAreaManager가 없으면 기본적으로 true 반환
        }
        
        /// <summary>
        /// SafeArea 내부의 안전한 위치 계산
        /// </summary>
        public Vector2 GetSafePosition(Vector2 desiredPosition)
        {
            if (safeAreaManager != null)
            {
                Rect safeArea = safeAreaManager.GetSafeArea();
                
                return new Vector2(
                    Mathf.Clamp(desiredPosition.x, safeArea.xMin, safeArea.xMax),
                    Mathf.Clamp(desiredPosition.y, safeArea.yMin, safeArea.yMax)
                );
            }
            return desiredPosition;
        }
        
        private void OnGUI()
        {
            if (enableSafeAreaDebug && safeAreaManager != null)
            {
                // SafeArea 정보를 화면에 표시
                Rect safeArea = safeAreaManager.GetSafeArea();
                
                GUI.color = Color.green;
                GUI.Label(new Rect(10, 10, 300, 100), $"SafeArea: {safeArea}");
                GUI.Label(new Rect(10, 30, 300, 100), $"Screen: {Screen.width} x {Screen.height}");
                
                // SafeArea 영역을 시각적으로 표시
                GUI.color = new Color(0, 1, 0, 0.3f);
                GUI.Box(safeArea, "");
                
                // 안전하지 않은 영역 표시
                GUI.color = new Color(1, 0, 0, 0.3f);
                GUI.Box(new Rect(0, 0, Screen.width, safeArea.yMin), ""); // 상단
                GUI.Box(new Rect(0, safeArea.yMax, Screen.width, Screen.height - safeArea.yMax), ""); // 하단
                GUI.Box(new Rect(0, 0, safeArea.xMin, Screen.height), ""); // 좌측
                GUI.Box(new Rect(safeArea.xMax, 0, Screen.width - safeArea.xMax, Screen.height), ""); // 우측
            }
        }
    }
}
