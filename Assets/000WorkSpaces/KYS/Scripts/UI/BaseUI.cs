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
    /// MVP ������ �����ϴ� �⺻ UI Ŭ����
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
        [SerializeField] protected bool hidePreviousUI = true; // ���� UI ���� ����
        [SerializeField] protected bool disablePreviousUI = false; // ���� UI ��Ȱ��ȭ ����
        
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
            // DoTween�� ���� ���� ��� ǥ��
            canvasGroup.alpha = 1f;
            rectTransform.localScale = originalScale;
            Debug.LogWarning("[BaseUI] DoTween�� ��ġ���� �ʾ� �ִϸ��̼��� ��Ȱ��ȭ�Ǿ����ϴ�.");
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
            // DoTween�� ���� ���� ��� ����
            canvasGroup.alpha = 0f;
            rectTransform.localScale = Vector3.zero;
            onComplete?.Invoke();
            Debug.LogWarning("[BaseUI] DoTween�� ��ġ���� �ʾ� �ִϸ��̼��� ��Ȱ��ȭ�Ǿ����ϴ�.");
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
        /// UI ���ӿ�����Ʈ �������� (ĳ�õ� ��� ��ȯ)
        /// </summary>
        public GameObject GetUI(in string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[BaseUI] UI �̸��� null�̰ų� ����ֽ��ϴ�.");
                return null;
            }

            if (goDict == null)
            {
                Debug.LogError("[BaseUI] goDict�� null�Դϴ�. Awake()�� ȣ��Ǿ����� Ȯ���ϼ���.");
                return null;
            }

            // ĳ�ÿ��� ���� �˻�
            if (goDict.TryGetValue(name, out GameObject gameObject) && gameObject != null)
            {
                return gameObject;
            }

            // ĳ�ÿ� ���ų� null�� ��� �ٽ� �˻�
            gameObject = GameObject.Find($"{name}");
            if (gameObject == null)
            {
                // ���� ������Ʈ���� �˻�
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
                Debug.LogError($"[BaseUI] UI ���ӿ�����Ʈ�� ã�� �� �����ϴ�: {name}");
                return null;
            }

            // ĳ�ÿ� �߰�/������Ʈ
            goDict[name] = gameObject;
            return gameObject;
        }

        /// <summary>
        /// UI ���ӿ�����Ʈ�� ��ųʸ��� �߰�
        /// </summary>
        public GameObject AddUIToDictionary(GameObject go)
        {
            if (go == null)
            {
                Debug.LogError("[BaseUI] �߰��� GameObject�� null�Դϴ�.");
                return null;
            }

            if (goDict == null)
            {
                Debug.LogError("[BaseUI] UI ��ųʸ��� null�Դϴ�. Awake()�� ȣ��Ǿ����� Ȯ���ϼ���.");
                return null;
            }

            if (!goDict.TryAdd(go.name, go))
            {
                Debug.LogWarning($"[BaseUI] �̹� UI�� ��ųʸ��� �ֽ��ϴ�: {go.name}");
                goDict[go.name] = go; // ���� ���� ������Ʈ
            }

            return go;
        }

        /// <summary>
        /// ��ųʸ����� UI ����
        /// </summary>
        public GameObject DeleteFromDictionary(in string name)
        {
            if (goDict == null)
            {
                Debug.LogError("[BaseUI] UI ��ųʸ��� null�Դϴ�.");
                return null;
            }

            if (goDict.Remove(name, out GameObject outObject))
            {
                Debug.Log($"[BaseUI] ��ųʸ����� ���ŵ�: {name}");
                return outObject;
            }

            Debug.LogWarning($"[BaseUI] ��ųʸ����� ã�� �� ����: {name}");
            return null;
        }

        /// <summary>
        /// ������Ʈ Ÿ������ UI ��� �������� (ĳ�õ� ��� ��ȯ)
        /// </summary>
        public T GetUI<T>(in string name) where T : Component
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[BaseUI] UI �̸��� null�̰ų� ����ֽ��ϴ�.");
                return null;
            }

            if (compDict == null)
            {
                Debug.LogError("[BaseUI] compDict�� null�Դϴ�. Awake()�� ȣ��Ǿ����� Ȯ���ϼ���.");
                return null;
            }

            string key = $"{name}_{typeof(T).Name}";
            
            // ĳ�ÿ��� ���� �˻�
            if (compDict.TryGetValue(key, out Component comp) && comp != null)
            {
                return comp as T;
            }

            // ĳ�ÿ� ���ų� null�� ��� �ٽ� �˻�
            GameObject go = GetUI(name);
            if (go == null)
            {
                return null;
            }

            comp = go.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"[BaseUI] {name}���� {typeof(T).Name} ������Ʈ�� ã�� �� �����ϴ�.");
                return null;
            }

            // ĳ�ÿ� �߰�
            compDict[key] = comp;
            return comp as T;
        }

        /// <summary>
        /// UI ��� ���� ���� Ȯ��
        /// </summary>
        public bool HasUI(in string name)
        {
            if (goDict == null) return false;
            return goDict.ContainsKey(name) && goDict[name] != null;
        }

        /// <summary>
        /// Ư�� Ÿ���� ������Ʈ ���� ���� Ȯ��
        /// </summary>
        public bool HasUI<T>(in string name) where T : Component
        {
            if (compDict == null) return false;
            string key = $"{name}_{typeof(T).Name}";
            return compDict.ContainsKey(key) && compDict[key] != null;
        }

        /// <summary>
        /// ��� ĳ�õ� UI ��� �̸� ��������
        /// </summary>
        public string[] GetAllUINames()
        {
            if (goDict == null) return new string[0];
            string[] names = new string[goDict.Count];
            goDict.Keys.CopyTo(names, 0);
            return names;
        }

        /// <summary>
        /// ĳ�� �ʱ�ȭ (UI ���� ���� �� ȣ��)
        /// </summary>
        public void RefreshCache()
        {
            Debug.Log("[BaseUI] UI ĳ�ø� ���ΰ�ħ�մϴ�.");
            InitializeComponentCache();
        }

        #endregion

        #region Event Handling

        public PointerHandler GetEvent(in string name)
        {
            GameObject go = GetUI(name);
            if (go == null)
            {
                Debug.LogError($"UI�� ã�� �� �����ϴ�: {name}");
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
        /// ���߾�� �ؽ�Ʈ ��������
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
        /// Ư�� ����� �ؽ�Ʈ ��������
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