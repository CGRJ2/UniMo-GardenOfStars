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

        public GameObject GetUI(in string name)
        {
            if (goDict == null)
            {
                Debug.Log("goDict가 null입니다");
                return null;
            }
            goDict.TryGetValue(name, out GameObject gameObject);
            if (gameObject == null)
            {
                gameObject = GameObject.Find($"{name}");
                if (gameObject == null)
                {
                    Debug.LogError($"해당 UI 게임오브젝트를 찾을 수 없습니다: {name}");
                }
                goDict.TryAdd(name, gameObject);
            }
            return gameObject;
        }

        public GameObject AddUIToDictionary(GameObject go)
        {
            if (goDict == null)
            {
                Debug.Log("UI 딕셔너리가 null입니다");
                return null;
            }
            if (!goDict.TryAdd(go.name, go))
            {
                Debug.Log($"이미 UI가 딕셔너리에 있습니다.:{go.name}");
            }

            return go;
        }

        public GameObject DeleteFromDictionary(in string name, in GameObject go)
        {
            goDict.Remove<string, GameObject>(name, out GameObject outObject);
            return outObject;
        }

        public T GetUI<T>(in string name) where T : Component
        {
            compDict.TryGetValue(name, out Component comp);
            if (comp != null)
            {
                return comp as T;
            }
            GameObject go = GetUI(name);
            if (go == null)
            {
                return null;
            }
            comp = go.GetComponent<T>();
            if (comp == null) return null;
            compDict.TryAdd($"{name}_{typeof(T).Name}", comp);
            return comp as T;
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
    }
}