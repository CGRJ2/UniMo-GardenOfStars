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
            
            Debug.Log("[BackdropUI] Backdrop 클릭됨");
            OnBackdropClicked?.Invoke();
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
