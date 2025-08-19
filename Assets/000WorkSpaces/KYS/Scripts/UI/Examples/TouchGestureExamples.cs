using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace KYS
{
    /// <summary>
    /// 터치 제스처 사용 예제
    /// </summary>
    public class TouchGestureExamples : BaseUI
    {
        [Header("터치 제스처 예제 UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image gestureArea;
        [SerializeField] private Slider pinchSlider;
        [SerializeField] private Button resetButton;

        [Header("터치 설정")]
        [SerializeField] private bool enableHapticFeedback = true;
        //[SerializeField] private float longPressDuration = 1.0f;
        //[SerializeField] private float doubleTapWindow = 0.3f;

        private Vector2 lastSwipeDirection;
        private float currentPinchValue = 1f;
        private int tapCount = 0;

        protected override void Awake()
        {
            base.Awake();
            SetupTouchGestures();
        }

        private void SetupTouchGestures()
        {
            // 기본 터치 이벤트
            SetupBasicTouchEvents();
            
            // 고급 터치 이벤트
            SetupAdvancedTouchEvents();
            
            // 제스처 이벤트
            SetupGestureEvents();
            
            // 멀티터치 이벤트
            SetupMultiTouchEvents();
        }

        #region Basic Touch Events

        private void SetupBasicTouchEvents()
        {
            // 터치 시작/종료
            GetTouchStartEvent("GestureArea", OnTouchStart);
            GetTouchEndEvent("GestureArea", OnTouchEnd);
            GetTouchMoveEvent("GestureArea", OnTouchMove);
        }

        private void OnTouchStart(PointerEventData data)
        {
            UpdateStatus($"터치 시작: {data.position}");
            Debug.Log($"터치 시작: {data.position}");
        }

        private void OnTouchEnd(PointerEventData data)
        {
            UpdateStatus($"터치 종료: {data.position}");
            Debug.Log($"터치 종료: {data.position}");
        }

        private void OnTouchMove(Vector2 delta)
        {
            UpdateStatus($"터치 이동: {delta}");
        }

        #endregion

        #region Advanced Touch Events

        private void SetupAdvancedTouchEvents()
        {
            // 복합 터치 이벤트 (클릭 + 롱프레스 + 더블탭)
            GetAdvancedTouchEvent("GestureArea",
                onClick: OnClick,
                onLongPress: OnLongPress,
                onDoubleTap: OnDoubleTap
            );
        }

        private void OnClick(PointerEventData data)
        {
            tapCount++;
            UpdateStatus($"클릭 #{tapCount}: {data.position}");
            Debug.Log($"클릭 #{tapCount}: {data.position}");
        }

        private void OnLongPress(PointerEventData data)
        {
            UpdateStatus($"롱프레스: {data.position}");
            Debug.Log($"롱프레스: {data.position}");
            
            // 롱프레스 시 시각적 피드백
            StartCoroutine(LongPressFeedback());
        }

        private void OnDoubleTap(PointerEventData data)
        {
            UpdateStatus($"더블탭: {data.position}");
            Debug.Log($"더블탭: {data.position}");
            
            // 더블탭 시 특별한 효과
            StartCoroutine(DoubleTapEffect());
        }

        private System.Collections.IEnumerator LongPressFeedback()
        {
            if (gestureArea != null)
            {
                Color originalColor = gestureArea.color;
                gestureArea.color = Color.red;
                
                yield return new WaitForSeconds(0.5f);
                
                gestureArea.color = originalColor;
            }
        }

        private System.Collections.IEnumerator DoubleTapEffect()
        {
            if (gestureArea != null)
            {
                Vector3 originalScale = gestureArea.transform.localScale;
                
                // 확대 효과
                gestureArea.transform.localScale = originalScale * 1.2f;
                
                yield return new WaitForSeconds(0.2f);
                
                // 원래 크기로 복원
                gestureArea.transform.localScale = originalScale;
            }
        }

        #endregion

        #region Gesture Events

        private void SetupGestureEvents()
        {
            // 방향별 스와이프
            GetDirectionalSwipeEvent("GestureArea",
                onSwipeUp: OnSwipeUp,
                onSwipeDown: OnSwipeDown,
                onSwipeLeft: OnSwipeLeft,
                onSwipeRight: OnSwipeRight
            );

            // 일반 스와이프
            GetSwipeEvent("GestureArea", OnSwipe);
        }

        private void OnSwipeUp()
        {
            lastSwipeDirection = Vector2.up;
            UpdateStatus("스와이프: 위쪽");
            Debug.Log("스와이프: 위쪽");
            
            // 위쪽 스와이프 효과
            StartCoroutine(SwipeEffect(Vector2.up));
        }

        private void OnSwipeDown()
        {
            lastSwipeDirection = Vector2.down;
            UpdateStatus("스와이프: 아래쪽");
            Debug.Log("스와이프: 아래쪽");
            
            // 아래쪽 스와이프 효과
            StartCoroutine(SwipeEffect(Vector2.down));
        }

        private void OnSwipeLeft()
        {
            lastSwipeDirection = Vector2.left;
            UpdateStatus("스와이프: 왼쪽");
            Debug.Log("스와이프: 왼쪽");
            
            // 왼쪽 스와이프 효과
            StartCoroutine(SwipeEffect(Vector2.left));
        }

        private void OnSwipeRight()
        {
            lastSwipeDirection = Vector2.right;
            UpdateStatus("스와이프: 오른쪽");
            Debug.Log("스와이프: 오른쪽");
            
            // 오른쪽 스와이프 효과
            StartCoroutine(SwipeEffect(Vector2.right));
        }

        private void OnSwipe(Vector2 direction)
        {
            UpdateStatus($"스와이프: {direction}");
            Debug.Log($"스와이프: {direction}");
        }

        private System.Collections.IEnumerator SwipeEffect(Vector2 direction)
        {
            if (gestureArea != null)
            {
                Vector3 originalPosition = gestureArea.transform.localPosition;
                Vector3 targetPosition = originalPosition + (Vector3)(direction * 20f);
                
                // 스와이프 방향으로 이동
                float duration = 0.2f;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    
                    gestureArea.transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, t);
                    yield return null;
                }
                
                // 원래 위치로 복원
                elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    
                    gestureArea.transform.localPosition = Vector3.Lerp(targetPosition, originalPosition, t);
                    yield return null;
                }
                
                gestureArea.transform.localPosition = originalPosition;
            }
        }

        #endregion

        #region Multi-Touch Events

        private void SetupMultiTouchEvents()
        {
            // 핀치 제스처
            GetPinchEvent("GestureArea", OnPinch);
        }

        private void OnPinch(float pinchDelta)
        {
            currentPinchValue += pinchDelta * 0.01f;
            currentPinchValue = Mathf.Clamp(currentPinchValue, 0.1f, 3f);
            
            UpdateStatus($"핀치: {currentPinchValue:F2}");
            Debug.Log($"핀치: {currentPinchValue:F2}");
            
            // 핀치 슬라이더 업데이트
            if (pinchSlider != null)
            {
                pinchSlider.value = currentPinchValue;
            }
            
            // 제스처 영역 스케일 변경
            if (gestureArea != null)
            {
                gestureArea.transform.localScale = Vector3.one * currentPinchValue;
            }
        }

        #endregion

        #region Touch Feedback

        private void SetupTouchFeedback()
        {
            // 터치 피드백이 포함된 이벤트
            GetTouchFeedbackEvent("GestureArea", OnTouchFeedback, enableHapticFeedback);
        }

        private void OnTouchFeedback(PointerEventData data)
        {
            UpdateStatus($"터치 피드백: {data.position}");
            Debug.Log($"터치 피드백: {data.position}");
        }

        #endregion

        #region Utility Methods

        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        public void ResetGestures()
        {
            tapCount = 0;
            currentPinchValue = 1f;
            lastSwipeDirection = Vector2.zero;
            
            if (gestureArea != null)
            {
                gestureArea.transform.localScale = Vector3.one;
                gestureArea.transform.localPosition = Vector3.zero;
                gestureArea.color = Color.white;
            }
            
            if (pinchSlider != null)
            {
                pinchSlider.value = 1f;
            }
            
            UpdateStatus("제스처 초기화됨");
        }

        #endregion

        #region Public Methods for Testing

        [ContextMenu("테스트 - 롱프레스")]
        public void TestLongPress()
        {
            OnLongPress(new PointerEventData(EventSystem.current));
        }

        [ContextMenu("테스트 - 더블탭")]
        public void TestDoubleTap()
        {
            OnDoubleTap(new PointerEventData(EventSystem.current));
        }

        [ContextMenu("테스트 - 스와이프 위")]
        public void TestSwipeUp()
        {
            OnSwipeUp();
        }

        [ContextMenu("테스트 - 핀치 확대")]
        public void TestPinchZoomIn()
        {
            OnPinch(0.5f);
        }

        [ContextMenu("테스트 - 핀치 축소")]
        public void TestPinchZoomOut()
        {
            OnPinch(-0.5f);
        }

        #endregion
    }

    /// <summary>
    /// 게임에서 사용할 수 있는 터치 제스처 예제
    /// </summary>
    public class GameTouchGestureExample : BaseUI
    {
        [Header("게임 제스처 예제")]
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private Slider zoomSlider;

        protected override void Awake()
        {
            base.Awake();
            SetupGameGestures();
        }

        private void SetupGameGestures()
        {
            // 인벤토리 버튼 - 롱프레스로 상세 정보
            GetAdvancedTouchEvent("InventoryButton",
                onClick: OnInventoryClick,
                onLongPress: OnInventoryLongPress,
                onDoubleTap: OnInventoryDoubleTap
            );

            // 스킬 버튼 - 스와이프로 스킬 변경
            GetDirectionalSwipeEvent("SkillButton",
                onSwipeLeft: OnSkillSwipeLeft,
                onSwipeRight: OnSkillSwipeRight
            );

            // 맵 버튼 - 핀치로 줌
            GetPinchEvent("MapButton", OnMapPinch);

            // 전체 화면 - 제스처
            GetAdvancedTouchEvent("GameArea",
                onClick: OnGameAreaClick,
                onLongPress: OnGameAreaLongPress,
                onDoubleTap: OnGameAreaDoubleTap
            );
        }

        #region Inventory Gestures

        private void OnInventoryClick(PointerEventData data)
        {
            Debug.Log("인벤토리 열기");
            // 인벤토리 UI 열기
        }

        private void OnInventoryLongPress(PointerEventData data)
        {
            Debug.Log("인벤토리 상세 정보");
            // 아이템 상세 정보 표시
        }

        private void OnInventoryDoubleTap(PointerEventData data)
        {
            Debug.Log("인벤토리 정렬");
            // 아이템 정렬
        }

        #endregion

        #region Skill Gestures

        private void OnSkillSwipeLeft()
        {
            Debug.Log("이전 스킬 선택");
            // 이전 스킬로 변경
        }

        private void OnSkillSwipeRight()
        {
            Debug.Log("다음 스킬 선택");
            // 다음 스킬로 변경
        }

        #endregion

        #region Map Gestures

        private void OnMapPinch(float pinchDelta)
        {
            float zoomValue = zoomSlider.value + pinchDelta * 0.1f;
            zoomValue = Mathf.Clamp(zoomValue, 0.1f, 3f);
            zoomSlider.value = zoomValue;
            
            Debug.Log($"맵 줌: {zoomValue:F2}");
            // 맵 줌 처리
        }

        #endregion

        #region Game Area Gestures

        private void OnGameAreaClick(PointerEventData data)
        {
            Debug.Log($"게임 영역 클릭: {data.position}");
            // 게임 영역 클릭 처리
        }

        private void OnGameAreaLongPress(PointerEventData data)
        {
            Debug.Log($"게임 영역 롱프레스: {data.position}");
            // 컨텍스트 메뉴 표시
        }

        private void OnGameAreaDoubleTap(PointerEventData data)
        {
            Debug.Log($"게임 영역 더블탭: {data.position}");
            // 빠른 액션 실행
        }

        #endregion
    }
}
