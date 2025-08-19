using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerHandler : MonoBehaviour,
    IEventSystemHandler,
    IPointerClickHandler,
    IPointerUpHandler,
    IPointerDownHandler,
    IPointerMoveHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("터치 설정")]
    [SerializeField] private bool enableTouchFeedback = true;
    [SerializeField] private bool enableMultiTouch = true;
    [SerializeField] private bool enableGestureDetection = true;
    [SerializeField] private float longPressDuration = 1.0f;
    [SerializeField] private float doubleTapTimeWindow = 0.3f;
    [SerializeField] private float tapThreshold = 0.1f; // 터치 이동 허용 범위

    // 기본 이벤트들
    public event Action<PointerEventData> Click;
    public event Action<PointerEventData> Up;
    public event Action<PointerEventData> Down;
    public event Action<PointerEventData> Move;
    public event Action<PointerEventData> Enter;
    public event Action<PointerEventData> Exit;
    public event Action<PointerEventData> BeginDrag;
    public event Action<PointerEventData> Drag;
    public event Action<PointerEventData> EndDrag;

    // 터치 특화 이벤트들
    public event Action<PointerEventData> LongPress;
    public event Action<PointerEventData> DoubleTap;
    public event Action<PointerEventData> TouchStart;
    public event Action<PointerEventData> TouchEnd;
    public event Action<Vector2> Swipe;
    public event Action<float> Pinch;
    public event Action<Vector2> TouchMove;

    // 터치 상태
    private bool isPressed = false;
    private bool isLongPressed = false;
    private float pressStartTime = 0f;
    private Vector2 pressStartPosition;
    private Vector2 lastTapPosition;
    private float lastTapTime = 0f;
    private int tapCount = 0;
    private Coroutine longPressCoroutine;

    // 멀티터치 관련
    private int currentTouchCount = 0;
    private Vector2[] touchPositions = new Vector2[10];
    private float initialPinchDistance = 0f;

    // 제스처 관련
    private Vector2 swipeStartPosition;
    private bool isSwiping = false;
    private float swipeThreshold = 50f; // 스와이프 감지 임계값

    #region Unity Event Handlers

    public void OnPointerClick(PointerEventData eventData)
    {
        Click?.Invoke(eventData);
        
        if (enableGestureDetection)
        {
            HandleTap(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Up?.Invoke(eventData);
        
        if (enableTouchFeedback)
        {
            HandleTouchEnd(eventData);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Down?.Invoke(eventData);
        
        if (enableTouchFeedback)
        {
            HandleTouchStart(eventData);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        Move?.Invoke(eventData);
        
        if (enableTouchFeedback && isPressed)
        {
            HandleTouchMove(eventData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Enter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Exit?.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDrag?.Invoke(eventData);
        
        if (enableGestureDetection)
        {
            HandleDragStart(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Drag?.Invoke(eventData);
        
        if (enableGestureDetection)
        {
            HandleDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDrag?.Invoke(eventData);
        
        if (enableGestureDetection)
        {
            HandleDragEnd(eventData);
        }
    }

    #endregion

    #region Touch Handling

    private void HandleTouchStart(PointerEventData eventData)
    {
        isPressed = true;
        pressStartTime = Time.time;
        pressStartPosition = eventData.position;
        isLongPressed = false;

        // 롱프레스 코루틴 시작
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
        }
        longPressCoroutine = StartCoroutine(LongPressCoroutine(eventData));

        TouchStart?.Invoke(eventData);
    }

    private void HandleTouchEnd(PointerEventData eventData)
    {
        isPressed = false;
        isLongPressed = false;

        // 롱프레스 코루틴 중지
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
            longPressCoroutine = null;
        }

        TouchEnd?.Invoke(eventData);
    }

    private void HandleTouchMove(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - pressStartPosition;
        TouchMove?.Invoke(delta);
    }

    private void HandleTap(PointerEventData eventData)
    {
        float currentTime = Time.time;
        float distance = Vector2.Distance(eventData.position, lastTapPosition);

        // 더블탭 감지
        if (currentTime - lastTapTime < doubleTapTimeWindow && distance < tapThreshold)
        {
            tapCount++;
            if (tapCount >= 2)
            {
                DoubleTap?.Invoke(eventData);
                tapCount = 0;
            }
        }
        else
        {
            tapCount = 1;
        }

        lastTapTime = currentTime;
        lastTapPosition = eventData.position;
    }

    private IEnumerator LongPressCoroutine(PointerEventData eventData)
    {
        yield return new WaitForSeconds(longPressDuration);
        
        if (isPressed && !isLongPressed)
        {
            isLongPressed = true;
            LongPress?.Invoke(eventData);
        }
    }

    #endregion

    #region Gesture Handling

    private void HandleDragStart(PointerEventData eventData)
    {
        swipeStartPosition = eventData.position;
        isSwiping = true;
    }

    private void HandleDrag(PointerEventData eventData)
    {
        if (!isSwiping) return;

        Vector2 delta = eventData.position - swipeStartPosition;
        
        // 스와이프 감지
        if (delta.magnitude > swipeThreshold)
        {
            Swipe?.Invoke(delta.normalized);
            isSwiping = false;
        }
    }

    private void HandleDragEnd(PointerEventData eventData)
    {
        isSwiping = false;
    }

    #endregion

    #region Multi-Touch Support

    private void Update()
    {
        if (!enableMultiTouch) return;

        // 멀티터치 감지
        int touchCount = Input.touchCount;
        if (touchCount != currentTouchCount)
        {
            currentTouchCount = touchCount;
            HandleMultiTouchChange();
        }

        // 핀치 제스처 감지
        if (touchCount == 2)
        {
            HandlePinchGesture();
        }
    }

    private void HandleMultiTouchChange()
    {
        // 터치 개수 변경 시 처리
        for (int i = 0; i < currentTouchCount && i < touchPositions.Length; i++)
        {
            touchPositions[i] = Input.GetTouch(i).position;
        }
    }

    private void HandlePinchGesture()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
        }
        else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
        {
            float currentDistance = Vector2.Distance(touch0.position, touch1.position);
            float pinchDelta = currentDistance - initialPinchDistance;
            
            Pinch?.Invoke(pinchDelta);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 터치 피드백 활성화/비활성화
    /// </summary>
    public void SetTouchFeedback(bool enabled)
    {
        enableTouchFeedback = enabled;
    }

    /// <summary>
    /// 멀티터치 활성화/비활성화
    /// </summary>
    public void SetMultiTouch(bool enabled)
    {
        enableMultiTouch = enabled;
    }

    /// <summary>
    /// 제스처 감지 활성화/비활성화
    /// </summary>
    public void SetGestureDetection(bool enabled)
    {
        enableGestureDetection = enabled;
    }

    /// <summary>
    /// 롱프레스 지속시간 설정
    /// </summary>
    public void SetLongPressDuration(float duration)
    {
        longPressDuration = duration;
    }

    /// <summary>
    /// 더블탭 시간 윈도우 설정
    /// </summary>
    public void SetDoubleTapTimeWindow(float timeWindow)
    {
        doubleTapTimeWindow = timeWindow;
    }

    /// <summary>
    /// 현재 터치 상태 확인
    /// </summary>
    public bool IsPressed => isPressed;

    /// <summary>
    /// 롱프레스 상태 확인
    /// </summary>
    public bool IsLongPressed => isLongPressed;

    /// <summary>
    /// 현재 터치 개수
    /// </summary>
    public int TouchCount => currentTouchCount;

    /// <summary>
    /// 터치 시작 위치
    /// </summary>
    public Vector2 TouchStartPosition => pressStartPosition;

    /// <summary>
    /// 터치 지속시간
    /// </summary>
    public float TouchDuration => isPressed ? Time.time - pressStartTime : 0f;

    #endregion

    #region Utility Methods

    /// <summary>
    /// 특정 방향으로의 스와이프 감지
    /// </summary>
    public bool IsSwipeDirection(Vector2 swipeVector, Vector2 direction, float threshold = 0.7f)
    {
        return Vector2.Dot(swipeVector.normalized, direction.normalized) > threshold;
    }

    /// <summary>
    /// 터치가 특정 영역 내에 있는지 확인
    /// </summary>
    public bool IsTouchInArea(Vector2 touchPosition, Rect area)
    {
        return area.Contains(touchPosition);
    }

    /// <summary>
    /// 터치 이동 거리 계산
    /// </summary>
    public float GetTouchDistance(Vector2 currentPosition)
    {
        return Vector2.Distance(currentPosition, pressStartPosition);
    }

    #endregion
}

