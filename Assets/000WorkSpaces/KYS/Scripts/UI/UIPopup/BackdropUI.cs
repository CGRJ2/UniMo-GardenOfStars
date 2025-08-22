using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KYS
{
    /// <summary>
    /// Popup UI 뒤의 터치를 차단하는 Backdrop 컴포넌트
    /// </summary>
    public class BackdropUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Backdrop Settings")]
        [SerializeField] private bool enableBackdropClick = true;
        [SerializeField] private Color backdropColor = new Color(0, 0, 0, 0.5f);
        
        private Image backdropImage;
        
        public System.Action OnBackdropClicked;

        private void Awake()
        {
            backdropImage = GetComponent<Image>();
            if (backdropImage == null)
            {
                backdropImage = gameObject.AddComponent<Image>();
            }
            
            // Backdrop 이미지 설정
            backdropImage.color = backdropColor;
            backdropImage.raycastTarget = true;
            
        }

        private void Start()
        {
            // Backdrop를 PopupCanvas의 최하단에 배치
            transform.SetAsFirstSibling();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!enableBackdropClick) return;
            
            // Popup 영역 클릭 확인
            if (IsClickInPopupArea(eventData.position))
            {
                Debug.Log("[BackdropUI] Popup 영역 클릭으로 무시됨");
                return;
            }
            
            Debug.Log("[BackdropUI] Backdrop 클릭됨");
            OnBackdropClicked?.Invoke();
        }
        
        /// <summary>
        /// 클릭 위치가 Popup 영역인지 확인
        /// </summary>
        private bool IsClickInPopupArea(Vector2 clickPosition)
        {
            // 자식 오브젝트들 중 Popup 컴포넌트가 있는지 확인
            BaseUI[] popups = GetComponentsInChildren<BaseUI>();
            foreach (var popup in popups)
            {
                if (popup == this) continue; // 자기 자신 제외
                
                RectTransform popupRect = popup.GetComponent<RectTransform>();
                if (popupRect != null)
                {
                    // 스크린 좌표를 로컬 좌표로 변환
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        popupRect, clickPosition, null, out Vector2 localPoint);
                    
                    // Popup 영역 내부인지 확인
                    if (popupRect.rect.Contains(localPoint))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Backdrop 색상 설정
        /// </summary>
        public void SetBackdropColor(Color color)
        {
            if (backdropImage != null)
            {
                backdropColor = color;
                backdropImage.color = color;
            }
        }

        /// <summary>
        /// Backdrop 클릭 활성화/비활성화
        /// </summary>
        public void SetBackdropClickable(bool clickable)
        {
            enableBackdropClick = clickable;
            if (backdropImage != null)
            {
                backdropImage.raycastTarget = clickable;
            }
        }
    }
}
