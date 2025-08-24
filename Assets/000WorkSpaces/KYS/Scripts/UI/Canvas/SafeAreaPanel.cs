using UnityEngine;
using UnityEngine.UI;

namespace KYS
{
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
                
                //Debug.Log($"[SafeAreaPanel] Anchors 업데이트: {anchorMin} ~ {anchorMax}");
                
                // 앵커 업데이트 후 자식 요소들도 다시 조정
                if (autoResizeChildren)
                {
                    ResizeChildrenToSafeArea();
                }
            }
        }
        
        /// <summary>
        /// 자식 UI 요소들을 SafeArea에 맞게 조정
        /// </summary>
        public void ResizeChildrenToSafeArea()
        {
            // 실제 자식 요소들만 가져오기 (자기 자신 제외)
            int childCount = transform.childCount;
            int adjustedCount = 0;
            
            for (int i = 0; i < childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);
                RectTransform childRectTransform = childTransform.GetComponent<RectTransform>();
                
                if (childRectTransform != null)
                {
                    // SafeAreaPanel 자체는 제외
                    if (childRectTransform.GetComponent<SafeAreaPanel>() != null) continue;
                    
                    // 자식 요소의 앵커를 SafeArea에 맞게 조정
                    AdjustChildToSafeArea(childRectTransform);
                    adjustedCount++;
                }
            }
            
            //Debug.Log($"[SafeAreaPanel] {adjustedCount}개의 자식 요소를 SafeArea에 맞게 조정 완료");
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
        
        /// <summary>
        /// 자식 요소 수 반환
        /// </summary>
        public int GetChildCount()
        {
            return transform.childCount;
        }
        
        /// <summary>
        /// 특정 인덱스의 자식 요소 반환
        /// </summary>
        public Transform GetChild(int index)
        {
            if (index >= 0 && index < transform.childCount)
            {
                return transform.GetChild(index);
            }
            return null;
        }
        
        /// <summary>
        /// 모든 자식 요소 반환
        /// </summary>
        public Transform[] GetAllChildren()
        {
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }
            return children;
        }
        
        /// <summary>
        /// 자식 요소 추가
        /// </summary>
        public void AddChild(Transform child)
        {
            if (child != null)
            {
                child.SetParent(transform, false);
            }
        }
        
        /// <summary>
        /// 자식 요소 제거
        /// </summary>
        public void RemoveChild(Transform child)
        {
            if (child != null && child.parent == transform)
            {
                child.SetParent(null);
            }
        }
        
        /// <summary>
        /// 모든 자식 요소 제거
        /// </summary>
        public void ClearChildren()
        {
            while (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                child.SetParent(null);
            }
        }
    }
}
