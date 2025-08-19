using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using KYS.UI;
using KYS.UI.MVP;
using KYS.UI.Localization;
#if DOTWEEN_AVAILABLE
using DG.Tweening;
#endif

namespace KYS
{
    /// <summary>
    /// MVP 패턴을 지원하는 기본 UI 클래스
    /// </summary>
    public class BaseUI : MonoBehaviour, IUIView
    {
        [Header("UI Layer Settings")]
        [SerializeField] protected UILayerType layerType = UILayerType.Panel;
        [SerializeField] protected UIPanelGroup panelGroup = UIPanelGroup.Other;
        
        [Header("UI Animation Settings")]
        [SerializeField] protected bool useAnimation = true;
        [SerializeField] protected float animationDuration = 0.3f;
#if DOTWEEN_AVAILABLE
        [SerializeField] protected Ease showEase = Ease.OutBack;
        [SerializeField] protected Ease hideEase = Ease.InBack;
#else
        [SerializeField] protected string showEase = "OutBack";
        [SerializeField] protected string hideEase = "InBack";
#endif
        
        [Header("UI Behavior Settings")]
        [SerializeField] protected bool canCloseWithESC = true;
        [SerializeField] protected bool canCloseWithBackdrop = true;
        [SerializeField] protected bool destroyOnClose = false;
        [SerializeField] protected bool hidePreviousUI = true; // 이전 UI 숨김 여부
        [SerializeField] protected bool disablePreviousUI = false; // 이전 UI 비활성화 여부
        
        [Header("Audio Settings")]
        [SerializeField] protected string defaultClickSound = "SFX_ButtonClick";
        [SerializeField] protected string defaultBackSound = "SFX_ButtonClickBack";
        [SerializeField] protected bool enableSFX = true;

        // MVP Components
        protected IUIPresenter presenter;
        protected IUIModel model;
        
        // UI Management
        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        protected Vector3 originalScale;
        protected Vector3 originalPosition;
        
        // Component Cache
        private Dictionary<string, GameObject> goDict;
        private Dictionary<string, Component> compDict;
        
        // Properties
        public UILayerType LayerType => layerType;
        public UIPanelGroup PanelGroup => panelGroup;
        public bool CanCloseWithESC => canCloseWithESC;
        public bool CanCloseWithBackdrop => canCloseWithBackdrop;
        public bool DestroyOnClose => destroyOnClose;
        public bool HidePreviousUI => hidePreviousUI;
        public bool DisablePreviousUI => disablePreviousUI;
        public bool IsActive => gameObject.activeInHierarchy;
        public GameObject GameObject => gameObject;

        protected virtual void Awake()
        {
            InitializeComponentCache();
            InitializeUIComponents();
            SetupMVP();
        }

        protected virtual void Start()
        {
            presenter?.Initialize();
            model?.Initialize();
        }

        protected virtual void OnDestroy()
        {
            presenter?.Cleanup();
            model?.Cleanup();
            Cleanup();
        }

        #region IUIView Implementation

        public virtual void Show()
        {
            if (IsActive) return;
            
            gameObject.SetActive(true);
            
            if (useAnimation)
            {
                PlayShowAnimation();
            }
            else
            {
                canvasGroup.alpha = 1f;
                rectTransform.localScale = originalScale;
            }
            
            OnShow();
        }

        public virtual void Hide()
        {
            if (!IsActive) return;
            
            if (useAnimation)
            {
                PlayHideAnimation(() => {
                    gameObject.SetActive(false);
                    OnHide();
                    
                    if (destroyOnClose)
                    {
                        Destroy(gameObject);
                    }
                });
            }
            else
            {
                gameObject.SetActive(false);
                OnHide();
                
                if (destroyOnClose)
                {
                    Destroy(gameObject);
                }
            }
        }

        public virtual void Initialize()
        {
            // Override in derived classes
        }

        public virtual void Cleanup()
        {
            // Override in derived classes
        }

        #endregion

        #region Animation Methods

        protected virtual void PlayShowAnimation()
        {
#if DOTWEEN_AVAILABLE
            // Reset to initial state
            canvasGroup.alpha = 0f;
            rectTransform.localScale = Vector3.zero;
            
            // Animate in
            canvasGroup.DOFade(1f, animationDuration).SetEase(showEase);
            rectTransform.DOScale(originalScale, animationDuration).SetEase(showEase);
#else
            // DoTween이 없을 때는 즉시 표시
            canvasGroup.alpha = 1f;
            rectTransform.localScale = originalScale;
            Debug.LogWarning("[BaseUI] DoTween이 설치되지 않아 애니메이션이 비활성화되었습니다.");
#endif
        }

        protected virtual void PlayHideAnimation(System.Action onComplete = null)
        {
#if DOTWEEN_AVAILABLE
            // Animate out
            canvasGroup.DOFade(0f, animationDuration).SetEase(hideEase);
            rectTransform.DOScale(Vector3.zero, animationDuration).SetEase(hideEase)
                .OnComplete(() => onComplete?.Invoke());
#else
            // DoTween이 없을 때는 즉시 숨김
            canvasGroup.alpha = 0f;
            rectTransform.localScale = Vector3.zero;
            onComplete?.Invoke();
            Debug.LogWarning("[BaseUI] DoTween이 설치되지 않아 애니메이션이 비활성화되었습니다.");
#endif
        }

        #endregion

        #region Lifecycle Methods

        protected virtual void OnShow()
        {
            // Override in derived classes
        }

        protected virtual void OnHide()
        {
            // Override in derived classes
        }

        #endregion

        #region MVP Setup

        protected virtual void SetupMVP()
        {
            // Override in derived classes to setup MVP components
        }

        public void SetPresenter(IUIPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void SetModel(IUIModel model)
        {
            this.model = model;
        }

        #endregion

        #region Component Management

        private void InitializeComponentCache()
        {
            RectTransform[] transforms = GetComponentsInChildren<RectTransform>(true);
            goDict = new Dictionary<string, GameObject>(transforms.Length << 2);
            foreach (Transform t in transforms)
            {
                goDict.TryAdd(t.gameObject.name, t.gameObject);
            }

            Component[] components = GetComponentsInChildren<Component>(true);
            compDict = new Dictionary<string, Component>(components.Length << 2);
            foreach (Component comp in components)
            {
                compDict.TryAdd($"{comp.gameObject.name}_{comp.GetType().Name}", comp);
            }
        }

        private void InitializeUIComponents()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
            originalPosition = rectTransform.localPosition;
        }

        #endregion

        #region Component Access Methods

        /// <summary>
        /// UI 게임오브젝트 가져오기 (캐시된 결과 반환)
        /// </summary>
        public GameObject GetUI(in string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[BaseUI] UI 이름이 null이거나 비어있습니다.");
                return null;
            }

            if (goDict == null)
            {
                Debug.LogError("[BaseUI] goDict가 null입니다. Awake()가 호출되었는지 확인하세요.");
                return null;
            }

            // 캐시에서 먼저 검색
            if (goDict.TryGetValue(name, out GameObject gameObject) && gameObject != null)
            {
                return gameObject;
            }

            // 캐시에 없거나 null인 경우 다시 검색
            gameObject = GameObject.Find($"{name}");
            if (gameObject == null)
            {
                // 하위 오브젝트에서 검색
                Transform[] allChildren = GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == name)
                    {
                        gameObject = child.gameObject;
                        break;
                    }
                }
            }

            if (gameObject == null)
            {
                Debug.LogError($"[BaseUI] UI 게임오브젝트를 찾을 수 없습니다: {name}");
                return null;
            }

            // 캐시에 추가/업데이트
            goDict[name] = gameObject;
            return gameObject;
        }

        /// <summary>
        /// UI 게임오브젝트를 딕셔너리에 추가
        /// </summary>
        public GameObject AddUIToDictionary(GameObject go)
        {
            if (go == null)
            {
                Debug.LogError("[BaseUI] 추가할 GameObject가 null입니다.");
                return null;
            }

            if (goDict == null)
            {
                Debug.LogError("[BaseUI] UI 딕셔너리가 null입니다. Awake()가 호출되었는지 확인하세요.");
                return null;
            }

            if (!goDict.TryAdd(go.name, go))
            {
                Debug.LogWarning($"[BaseUI] 이미 UI가 딕셔너리에 있습니다: {go.name}");
                goDict[go.name] = go; // 기존 참조 업데이트
            }

            return go;
        }

        /// <summary>
        /// 딕셔너리에서 UI 제거
        /// </summary>
        public GameObject DeleteFromDictionary(in string name)
        {
            if (goDict == null)
            {
                Debug.LogError("[BaseUI] UI 딕셔너리가 null입니다.");
                return null;
            }

            if (goDict.Remove(name, out GameObject outObject))
            {
                Debug.Log($"[BaseUI] 딕셔너리에서 제거됨: {name}");
                return outObject;
            }

            Debug.LogWarning($"[BaseUI] 딕셔너리에서 찾을 수 없음: {name}");
            return null;
        }

        /// <summary>
        /// 컴포넌트 타입으로 UI 요소 가져오기 (캐시된 결과 반환)
        /// </summary>
        public T GetUI<T>(in string name) where T : Component
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[BaseUI] UI 이름이 null이거나 비어있습니다.");
                return null;
            }

            if (compDict == null)
            {
                Debug.LogError("[BaseUI] compDict가 null입니다. Awake()가 호출되었는지 확인하세요.");
                return null;
            }

            string key = $"{name}_{typeof(T).Name}";
            
            // 캐시에서 먼저 검색
            if (compDict.TryGetValue(key, out Component comp) && comp != null)
            {
                return comp as T;
            }

            // 캐시에 없거나 null인 경우 다시 검색
            GameObject go = GetUI(name);
            if (go == null)
            {
                return null;
            }

            comp = go.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"[BaseUI] {name}에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
                return null;
            }

            // 캐시에 추가
            compDict[key] = comp;
            return comp as T;
        }

        /// <summary>
        /// UI 요소 존재 여부 확인
        /// </summary>
        public bool HasUI(in string name)
        {
            if (goDict == null) return false;
            return goDict.ContainsKey(name) && goDict[name] != null;
        }

        /// <summary>
        /// 특정 타입의 컴포넌트 존재 여부 확인
        /// </summary>
        public bool HasUI<T>(in string name) where T : Component
        {
            if (compDict == null) return false;
            string key = $"{name}_{typeof(T).Name}";
            return compDict.ContainsKey(key) && compDict[key] != null;
        }

        /// <summary>
        /// 모든 캐시된 UI 요소 이름 가져오기
        /// </summary>
        public string[] GetAllUINames()
        {
            if (goDict == null) return new string[0];
            string[] names = new string[goDict.Count];
            goDict.Keys.CopyTo(names, 0);
            return names;
        }

        /// <summary>
        /// 캐시 초기화 (UI 구조 변경 시 호출)
        /// </summary>
        public void RefreshCache()
        {
            Debug.Log("[BaseUI] UI 캐시를 새로고침합니다.");
            InitializeComponentCache();
        }

        #endregion

        #region Event Handling

        public PointerHandler GetEvent(in string name)
        {
            GameObject go = GetUI(name);
            if (go == null)
            {
                Debug.LogError($"UI를 찾을 수 없습니다: {name}");
                return null;
            }

            PointerHandler temp = go.GetComponent<PointerHandler>();
            if (temp == null)
            {
                temp = go.AddComponent<PointerHandler>();
            }

            return temp;
        }

        public PointerHandler GetSelfEvent()
        {
            PointerHandler temp = GetComponent<PointerHandler>();
            if (temp == null)
            {
                temp = gameObject.AddComponent<PointerHandler>();
            }

            return temp;
        }

        #endregion

        #region Localization Methods

        /// <summary>
        /// 다중언어 텍스트 가져오기
        /// </summary>
        protected string GetLocalizedText(string key)
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(key);
            }
            return key;
        }

        /// <summary>
        /// 특정 언어의 텍스트 가져오기
        /// </summary>
        protected string GetLocalizedText(string key, SystemLanguage language)
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.GetText(key, language);
            }
            return key;
        }

        #endregion

        #region Audio Methods

        protected void PlayClickSound(string soundName = null)
        {
            if (!enableSFX) return;

            string soundToPlay = soundName ?? defaultClickSound;
            //if (!string.IsNullOrEmpty(soundToPlay) && Manager.Audio != null)
            //{
            //    Manager.Audio.SfxPlay(soundToPlay);
            //}
        }

        protected void PlayBackSound(string soundName = null)
        {
            if (!enableSFX) return;

            string soundToPlay = soundName ?? defaultBackSound;
            //if (!string.IsNullOrEmpty(soundToPlay) && Manager.Audio != null)
            //{
            //    Manager.Audio.SfxPlay(soundToPlay);
            //}
        }

        public PointerHandler GetEventWithSFX(in string name, string clickSound = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                handler.Click += (data) => {
                    Button button = handler.GetComponent<Button>();
                    if (button != null && !button.interactable)
                    {
                        return;
                    }
                    PlayClickSound(clickSound);
                };
            }
            return handler;
        }

        public PointerHandler GetBackEvent(in string name, string backSound = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                handler.Click += (data) => PlayBackSound(backSound);
            }
            return handler;
        }

        #endregion

        #region Touch Gesture Methods

        /// <summary>
        /// 롱프레스 이벤트 핸들러 가져오기
        /// </summary>
        public PointerHandler GetLongPressEvent(string name, System.Action<PointerEventData> onLongPress = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null && onLongPress != null)
            {
                handler.LongPress += onLongPress;
            }
            return handler;
        }

        /// <summary>
        /// 더블탭 이벤트 핸들러 가져오기
        /// </summary>
        public PointerHandler GetDoubleTapEvent(string name, System.Action<PointerEventData> onDoubleTap = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null && onDoubleTap != null)
            {
                handler.DoubleTap += onDoubleTap;
            }
            return handler;
        }

        /// <summary>
        /// 스와이프 이벤트 핸들러 가져오기
        /// </summary>
        public PointerHandler GetSwipeEvent(string name, System.Action<Vector2> onSwipe = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null && onSwipe != null)
            {
                handler.Swipe += onSwipe;
            }
            return handler;
        }

        /// <summary>
        /// 핀치 이벤트 핸들러 가져오기
        /// </summary>
        public PointerHandler GetPinchEvent(string name, System.Action<float> onPinch = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null && onPinch != null)
            {
                handler.Pinch += onPinch;
            }
            return handler;
        }

        /// <summary>
        /// 터치 시작 이벤트 핸들러 가져오기
        /// </summary>
        public PointerHandler GetTouchStartEvent(string name, System.Action<PointerEventData> onTouchStart = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null && onTouchStart != null)
            {
                handler.TouchStart += onTouchStart;
            }
            return handler;
        }

        /// <summary>
        /// 터치 종료 이벤트 핸들러 가져오기
        /// </summary>
        public PointerHandler GetTouchEndEvent(string name, System.Action<PointerEventData> onTouchEnd = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null && onTouchEnd != null)
            {
                handler.TouchEnd += onTouchEnd;
            }
            return handler;
        }

        /// <summary>
        /// 터치 이동 이벤트 핸들러 가져오기
        /// </summary>
        public PointerHandler GetTouchMoveEvent(string name, System.Action<Vector2> onTouchMove = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null && onTouchMove != null)
            {
                handler.TouchMove += onTouchMove;
            }
            return handler;
        }

        /// <summary>
        /// 복합 터치 이벤트 설정 (클릭 + 롱프레스 + 더블탭)
        /// </summary>
        public PointerHandler GetAdvancedTouchEvent(string name, 
            System.Action<PointerEventData> onClick = null,
            System.Action<PointerEventData> onLongPress = null,
            System.Action<PointerEventData> onDoubleTap = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                if (onClick != null) handler.Click += onClick;
                if (onLongPress != null) handler.LongPress += onLongPress;
                if (onDoubleTap != null) handler.DoubleTap += onDoubleTap;
            }
            return handler;
        }

        /// <summary>
        /// 방향별 스와이프 이벤트 설정
        /// </summary>
        public PointerHandler GetDirectionalSwipeEvent(string name,
            System.Action onSwipeUp = null,
            System.Action onSwipeDown = null,
            System.Action onSwipeLeft = null,
            System.Action onSwipeRight = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                handler.Swipe += (swipeVector) =>
                {
                    if (handler.IsSwipeDirection(swipeVector, Vector2.up) && onSwipeUp != null)
                        onSwipeUp();
                    else if (handler.IsSwipeDirection(swipeVector, Vector2.down) && onSwipeDown != null)
                        onSwipeDown();
                    else if (handler.IsSwipeDirection(swipeVector, Vector2.left) && onSwipeLeft != null)
                        onSwipeLeft();
                    else if (handler.IsSwipeDirection(swipeVector, Vector2.right) && onSwipeRight != null)
                        onSwipeRight();
                };
            }
            return handler;
        }

        /// <summary>
        /// 터치 피드백이 포함된 이벤트 핸들러
        /// </summary>
        public PointerHandler GetTouchFeedbackEvent(string name, 
            System.Action<PointerEventData> onClick = null,
            bool enableHaptic = true)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                handler.Click += (data) =>
                {
                    // 터치 피드백 효과
                    if (enableHaptic)
                    {
                        // HapticFeedback.PlayLightImpact(); // 하드웨어 진동
                    }
                    
                    // 시각적 피드백
                    StartCoroutine(TouchFeedbackCoroutine(handler.gameObject));
                    
                    onClick?.Invoke(data);
                };
            }
            return handler;
        }

        /// <summary>
        /// 터치 피드백 코루틴
        /// </summary>
        private System.Collections.IEnumerator TouchFeedbackCoroutine(GameObject target)
        {
            if (target == null) yield break;

            // 원래 스케일 저장
            Vector3 originalScale = target.transform.localScale;
            
            // 터치 효과 (스케일 축소)
            target.transform.localScale = originalScale * 0.95f;
            
            yield return new WaitForSeconds(0.1f);
            
            // 원래 스케일로 복원
            target.transform.localScale = originalScale;
        }

        #endregion


    }
}
