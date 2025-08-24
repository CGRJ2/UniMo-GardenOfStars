using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KYS
{
    /// <summary>
    /// HUD UI 뒤의 터치를 차단하고 InfoHUD를 닫는 Backdrop 컴포넌트
    /// </summary>
    public class HUDBackdropUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("HUD Backdrop Settings")]
        [SerializeField] private bool enableBackdropClick = true;
        [SerializeField] private Color backdropColor = new Color(0, 0, 0, 0.1f); // 매우 투명한 검은색
        
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
            // Backdrop를 HUDCanvas의 최하단에 배치
            transform.SetAsFirstSibling();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!enableBackdropClick) return;
            
            // InfoHUD 영역 클릭 확인
            if (IsClickInInfoHUDArea(eventData.position))
            {
                //Debug.Log("[HUDBackdropUI] InfoHUD 영역 클릭으로 무시됨");
                return;
            }
            
            //Debug.Log("[HUDBackdropUI] HUD Backdrop 클릭됨 - InfoHUD 닫기");
            OnBackdropClicked?.Invoke();
        }
        
        /// <summary>
        /// 클릭 위치가 InfoHUD 영역인지 확인
        /// </summary>
        private bool IsClickInInfoHUDArea(Vector2 clickPosition)
        {
            // 자식 오브젝트들 중 TouchInfoHUD 컴포넌트가 있는지 확인
            TouchInfoHUD[] infoHUDs = GetComponentsInChildren<TouchInfoHUD>();
            foreach (var infoHUD in infoHUDs)
            {
                if (infoHUD == this) continue; // 자기 자신 제외
                
                RectTransform hudRect = infoHUD.GetComponent<RectTransform>();
                if (hudRect != null)
                {
                    // 스크린 좌표를 로컬 좌표로 변환
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        hudRect, clickPosition, null, out Vector2 localPoint))
                    {
                        // InfoHUD 영역 내부인지 확인
                        if (hudRect.rect.Contains(localPoint))
                        {
                            return true;
                        }
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
