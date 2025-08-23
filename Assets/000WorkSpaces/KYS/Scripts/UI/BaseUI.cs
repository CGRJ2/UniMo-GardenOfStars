using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if DOTWEEN
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
#if DOTWEEN
        [SerializeField] protected Ease showEase = Ease.OutBack;
        [SerializeField] protected Ease hideEase = Ease.InBack;
#else
        [SerializeField] protected string showEase = "OutBack";
        [SerializeField] protected string hideEase = "InBack";
#endif
        
        [Header("UI Behavior Settings")]
        [SerializeField] protected bool canCloseWithESC = false;
        [SerializeField] protected bool canCloseWithBackdrop = false;
        [SerializeField] protected bool hidePreviousUI = false; // 이전 UI 숨김 여부 (SetActive(false))
        [SerializeField] protected bool disablePreviousUI = false; // 이전 UI 비활성화 여부 (CanvasGroup.interactable = false)
        [SerializeField] protected bool createBackdropForPopup = false; // Popup일 때 Backdrop 자동 생성
        
        [Header("Backdrop Settings")]
        // Backdrop Prefab Reference는 UIManager에서 관리
        
        [Header("Audio Settings")]
        [SerializeField] protected bool enableSFX = true;
        [SerializeField] protected string defaultClickSound = "SFX_ButtonClick";
        [SerializeField] protected string defaultBackSound = "SFX_ButtonClickBack";
        [SerializeField] protected string defaultHoverSound = "SFX_ButtonHover";
        [SerializeField] protected string defaultErrorSound = "SFX_Error";
        [SerializeField] protected string defaultSuccessSound = "SFX_Success";
        [SerializeField] protected string defaultOpenSound = "SFX_UI_Open";
        [SerializeField] protected string defaultCloseSound = "SFX_UI_Close";

        // MVP Components
        protected IUIPresenter presenter;
        protected IUIModel model;
        
        // UI Management
        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        protected Vector3 originalScale;
        protected Vector3 originalPosition;
        
        // Backdrop Management
        protected BackdropUI ownBackdrop; // 각 Popup의 고유 Backdrop
        
        // Component Cache
        private Dictionary<string, GameObject> goDict;
        private Dictionary<string, Component> compDict;
        
        // Properties
        public UILayerType LayerType => layerType;
        public UIPanelGroup PanelGroup => panelGroup;
        public bool CanCloseWithESC => canCloseWithESC;
        public bool CanCloseWithBackdrop => canCloseWithBackdrop;
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
            // DOTween 애니메이션 정리
#if DOTWEEN
            if (canvasGroup != null)
            {
                canvasGroup.DOKill(); // 진행 중인 DOTween 애니메이션 중단
            }
            if (rectTransform != null)
            {
                rectTransform.DOKill(); // 진행 중인 DOTween 애니메이션 중단
            }
#endif
            
            // 언어 변경 이벤트 구독 해제
            try
            {
                if (Manager.localization != null && Manager.localization.gameObject != null)
                {
                    Manager.localization.OnLanguageChanged -= OnLanguageChanged;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BaseUI] OnDestroy에서 LocalizationManager 이벤트 구독 해제 중 오류: {e.Message}");
            }
            
            presenter?.Cleanup();
            model?.Cleanup();
            Cleanup();
        }

        #region IUIView Implementation

        public virtual void Show()
        {
            //Debug.Log($"[BaseUI] {gameObject.name} Show() 호출됨");
            
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            
            // Popup 타입이고 Backdrop 생성이 활성화된 경우 Backdrop 생성
            if (layerType == UILayerType.Popup && createBackdropForPopup)
            {
                CreateBackdrop();
            }
           
            
            // 항상 Initialize 호출 (이미 활성화되어 있어도)
            Initialize();
            
            // 애니메이션 재생
            PlayShowAnimation();
            
            // UI 열기 사운드 재생
            PlayOpenSound();
            
            // 이벤트 호출
            OnShow();
            
            //Debug.Log($"[BaseUI] {gameObject.name} Show() 완료");
        }

        public virtual void Hide()
        {
            if (!IsActive) return;
            
            // UI 닫기 사운드 재생
            PlayCloseSound();
            
            if (useAnimation)
            {
                PlayHideAnimation(() => {
                    // 애니메이션 완료 후 안전하게 정리
                    SafeDestroyUI();
                });
            }
            else
            {
                SafeDestroyUI();
            }
        }

        /// <summary>
        /// UI를 안전하게 파괴하는 메서드
        /// </summary>
        protected virtual void SafeDestroyUI()
        {
            // DOTween 애니메이션 정리
#if DOTWEEN
            if (canvasGroup != null)
            {
                canvasGroup.DOKill(); // 진행 중인 DOTween 애니메이션 중단
            }
            if (rectTransform != null)
            {
                rectTransform.DOKill(); // 진행 중인 DOTween 애니메이션 중단
            }
#endif
            
            gameObject.SetActive(false);
            OnHide();
            
            // 스택 구조에서는 항상 파괴
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UnregisterUI(this);
            }
            Destroy(gameObject);
        }

        public virtual void Initialize()
        {
            ////Debug.Log($"[BaseUI] {gameObject.name}의 Initialize() 메서드 실행");
            
            // 자동 로컬라이제이션 설정
            SetupAutoLocalization();
            
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
#if DOTWEEN
            //Debug.Log($"[BaseUI] {gameObject.name} Show 애니메이션 시작 - Duration: {animationDuration}, Ease: {showEase}, UseAnimation: {useAnimation}");
            
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
#if DOTWEEN
            //Debug.Log($"[BaseUI] {gameObject.name} Hide 애니메이션 시작 - Duration: {animationDuration}, Ease: {hideEase}, UseAnimation: {useAnimation}");
            
            // Animate out
            canvasGroup.DOFade(0f, animationDuration).SetEase(hideEase);
            rectTransform.DOScale(Vector3.zero, animationDuration).SetEase(hideEase)
                .OnComplete(() => {
                    // 애니메이션 완료 후 콜백 실행
                    onComplete?.Invoke();
                })
                .SetAutoKill(true); // 애니메이션 완료 후 자동 정리
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
            // 하위 클래스에서 구현
        }


        /// <summary>
        /// Popup용 Backdrop 생성 (PopupCanvas에 생성하고 Popup을 Backdrop의 자식으로 설정)
        /// </summary>
        protected virtual async void CreateBackdrop()
        {
            //Debug.Log($"[BaseUI] {gameObject.name}에서 Backdrop 생성 시작");
            
            // 이미 Backdrop가 있는지 확인
            if (ownBackdrop != null)
            {
                //Debug.Log($"[BaseUI] {gameObject.name}에 이미 Backdrop가 존재합니다.");
                return;
            }

            // PopupCanvas 가져오기
            Canvas popupCanvas = UIManager.Instance?.GetCanvasByLayer(UILayerType.Popup);
            if (popupCanvas == null)
            {
                Debug.LogError("[BaseUI] PopupCanvas를 찾을 수 없습니다.");
                return;
            }

            // UIManager에서 Backdrop Prefab Reference 가져오기
            AssetReferenceGameObject backdropPrefabRef = UIManager.Instance?.BackdropPrefabReference;
            if (backdropPrefabRef == null || !backdropPrefabRef.RuntimeKeyIsValid())
            {
                Debug.LogWarning("[BaseUI] Backdrop Prefab Reference가 설정되지 않았거나 유효하지 않습니다. 기본 방식으로 생성합니다.");
                CreateBackdropFallback(popupCanvas);
                return;
            }

            try
            {
                // Backdrop Prefab을 Addressables로 로드 및 인스턴스 생성 (PopupCanvas의 자식으로)
                AsyncOperationHandle<GameObject> handle = backdropPrefabRef.InstantiateAsync(popupCanvas.transform);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[BaseUI] Backdrop Prefab 로드 및 인스턴스 생성 실패: {handle.OperationException?.Message}");
                    CreateBackdropFallback(popupCanvas);
                    return;
                }

                GameObject backdropGO = handle.Result;
                backdropGO.name = "Backdrop"; // 일관된 이름 보장
                
                //Debug.Log($"[BaseUI] Backdrop Prefab 인스턴스 생성 완료: {backdropGO.name}, 부모: {backdropGO.transform.parent.name}");
                
                // RectTransform 설정 (전체 화면)
                RectTransform backdropRect = backdropGO.GetComponent<RectTransform>();
                if (backdropRect != null)
                {
                    backdropRect.anchorMin = Vector2.zero;
                    backdropRect.anchorMax = Vector2.one;
                    backdropRect.offsetMin = Vector2.zero;
                    backdropRect.offsetMax = Vector2.zero;
                    backdropRect.localScale = Vector3.one;
                    
                    //Debug.Log($"[BaseUI] Backdrop RectTransform 설정 완료 - Anchors: ({backdropRect.anchorMin}, {backdropRect.anchorMax})");
                }
                
                // BackdropUI 컴포넌트 확인 및 저장
                ownBackdrop = backdropGO.GetComponent<BackdropUI>();
                if (ownBackdrop == null)
                {
                    Debug.LogWarning("[BaseUI] Backdrop Prefab에 BackdropUI 컴포넌트가 없습니다. 추가합니다.");
                    ownBackdrop = backdropGO.AddComponent<BackdropUI>();
                }
                
                // Popup을 Backdrop의 자식으로 이동
                transform.SetParent(backdropGO.transform);
                
                // Backdrop를 PopupCanvas의 최상위로 이동 (하이어라키 순서 조정)
                backdropGO.transform.SetAsLastSibling();
                
                // Backdrop 클릭 이벤트 설정
                SetupBackdropClickEvent();
                
                //Debug.Log($"[BaseUI] {gameObject.name}을 Backdrop의 자식으로 이동 완료 - Backdrop 위치: {backdropGO.transform.GetSiblingIndex()}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BaseUI] Backdrop Prefab 생성 중 오류 발생: {e.Message}");
                CreateBackdropFallback(popupCanvas);
            }
        }

        /// <summary>
        /// Backdrop Prefab 로드 실패 시 사용하는 기본 생성 방식
        /// </summary>
        private void CreateBackdropFallback(Canvas popupCanvas)
        {
            //Debug.Log($"[BaseUI] {gameObject.name}에 기본 방식으로 Backdrop 생성");
            
            // Backdrop GameObject 생성 (PopupCanvas의 자식으로)
            GameObject backdropGO = new GameObject("Backdrop");
            backdropGO.transform.SetParent(popupCanvas.transform);
            
            //Debug.Log($"[BaseUI] Backdrop GameObject 생성: {backdropGO.name}, 부모: {backdropGO.transform.parent.name}");
            
            // RectTransform 설정 (전체 화면)
            RectTransform backdropRect = backdropGO.AddComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            backdropRect.localScale = Vector3.one;
            
            //Debug.Log($"[BaseUI] Backdrop RectTransform 설정 완료 - Anchors: ({backdropRect.anchorMin}, {backdropRect.anchorMax})");
            
            // BackdropUI 컴포넌트 추가 및 저장
            ownBackdrop = backdropGO.AddComponent<BackdropUI>();
            
            // Popup을 Backdrop의 자식으로 이동
            transform.SetParent(backdropGO.transform);
            
            // Backdrop를 PopupCanvas의 최상위로 이동 (하이어라키 순서 조정)
            backdropGO.transform.SetAsLastSibling();
            
            // Backdrop 클릭 이벤트 설정
            SetupBackdropClickEvent();
            
            //Debug.Log($"[BaseUI] {gameObject.name}을 Backdrop의 자식으로 이동 완료 - Backdrop 위치: {backdropGO.transform.GetSiblingIndex()}");
        }

        /// <summary>
        /// Backdrop 클릭 이벤트 설정
        /// </summary>
        protected virtual void SetupBackdropClickEvent()
        {
            if (ownBackdrop == null) return;
            
            // Backdrop 클릭 가능 여부 설정
            ownBackdrop.SetBackdropClickable(canCloseWithBackdrop);
            
            if (canCloseWithBackdrop)
            {
                ownBackdrop.OnBackdropClicked += () =>
                {
                    //Debug.Log($"[BaseUI] {gameObject.name}의 Backdrop 클릭으로 Popup 닫기");
                    UIManager.Instance?.ClosePopup();
                };
                
                //Debug.Log($"[BaseUI] {gameObject.name}의 Backdrop 클릭 이벤트 설정 완료");
            }
            else
            {
                //Debug.Log($"[BaseUI] {gameObject.name}은 Backdrop 클릭으로 닫을 수 없습니다.");
            }
        }

        /// <summary>
        /// Popup의 Backdrop 인스턴스 반환
        /// </summary>
        public BackdropUI GetOwnBackdrop()
        {
            return ownBackdrop;
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

            //Debug.Log($"[BaseUI] {gameObject.name}에서 '{name}' 검색 시작");

            // 캐시에서 먼저 검색
            if (goDict.TryGetValue(name, out GameObject foundGameObject) && foundGameObject != null)
            {
                //Debug.Log($"[BaseUI] 캐시에서 '{name}' 발견");
                return foundGameObject;
            }

            //Debug.Log($"[BaseUI] 캐시에서 '{name}'을 찾을 수 없음. 새로 검색합니다.");

            // 캐시에 없거나 null인 경우 다시 검색
            foundGameObject = GameObject.Find($"{name}");
            if (foundGameObject == null)
            {
                //Debug.Log($"[BaseUI] GameObject.Find로 '{name}'을 찾을 수 없음. 하위 오브젝트에서 검색합니다.");
                // 하위 오브젝트에서 검색
                Transform[] allChildren = GetComponentsInChildren<Transform>(true);
                //Debug.Log($"[BaseUI] 하위 오브젝트 수: {allChildren.Length}");
                foreach (Transform child in allChildren)
                {
                    //Debug.Log($"[BaseUI] 하위 오브젝트 확인: {child.name}");
                    if (child.name == name)
                    {
                        foundGameObject = child.gameObject;
                        //Debug.Log($"[BaseUI] '{name}' 발견: {foundGameObject.name}");
                        break;
                    }
                }
            }

            if (foundGameObject == null)
            {
                Debug.LogError($"[BaseUI] UI 게임오브젝트를 찾을 수 없습니다: {name}");
                return null;
            }

            // 캐시에 추가/업데이트
            goDict[name] = foundGameObject;
            //Debug.Log($"[BaseUI] '{name}'을 캐시에 추가했습니다.");
            return foundGameObject;
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
                //Debug.Log($"[BaseUI] 딕셔너리에서 제거됨: {name}");
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
            //Debug.Log("[BaseUI] UI 캐시를 새로고침합니다.");
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

        #region Localization Auto-System

        [Header("Auto Localization Settings")]
        [SerializeField] protected bool enableAutoLocalization = true;
        [SerializeField] protected string[] autoLocalizeKeys = new string[0];

        /// <summary>
        /// 자식 클래스에서 오버라이드하여 autoLocalizeKeys를 설정할 수 있습니다.
        /// </summary>
        protected virtual string[] GetAutoLocalizeKeys()
        {
            return autoLocalizeKeys;
        }

        private List<TextMeshProUGUI> autoLocalizedTexts = new List<TextMeshProUGUI>();
        private List<string> autoLocalizeKeyList = new List<string>();

                protected virtual void SetupAutoLocalization()
        {
            if (!enableAutoLocalization) return;

            ////Debug.Log($"[{GetType().Name}] 자동 로컬라이제이션 설정 시작");

            SetupAutoLocalizedTextComponents();
        }

        /// <summary>
        /// 새로운 AutoLocalizedText 컴포넌트를 사용한 자동 로컬라이제이션 설정
        /// </summary>
        private void SetupAutoLocalizedTextComponents()
        {
            ////Debug.Log($"[{GetType().Name}] AutoLocalizedText 컴포넌트 기반 자동 로컬라이제이션 설정");
            
            // 모든 TextMeshProUGUI 컴포넌트 찾기
            var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            
            foreach (var text in allTexts)
            {
                // AutoLocalizedText 컴포넌트가 없으면 추가
                if (text.GetComponent<AutoLocalizedText>() == null)
                {
                    var autoLocalizedText = text.gameObject.AddComponent<AutoLocalizedText>();
                    //Debug.Log($"[{GetType().Name}] {text.name}에 AutoLocalizedText 컴포넌트 추가");
                }
                else
                {
                    //Debug.Log($"[{GetType().Name}] {text.name}에 이미 AutoLocalizedText 컴포넌트가 존재");
                }
            }
        }



        protected virtual string GetLocalizationKeyForText(TextMeshProUGUI text)
        {
            // 1. 설정된 키 배열에서 찾기
            string[] keys = GetAutoLocalizeKeys();
            if (keys.Length > 0)
            {
                // UI 이름과 매칭되는 키 찾기
                for (int i = 0; i < keys.Length; i++)
                {
                    if (text.name.Contains(keys[i]) || 
                        keys[i].Contains(text.name))
                    {
                        return keys[i];
                    }
                }
            }

            // 2. LocalizationManager의 키 생성 함수 사용
            if (Manager.localization != null)
            {
                return Manager.localization.GenerateKeyFromUIName(text.name);
            }

            // 3. 기본 방식 (text 접미사만 제거)
            string baseKey = text.name.ToLower().Replace("text", "").Trim();
            return baseKey;
        }

        protected virtual void UpdateAutoLocalizedTexts()
        {
            if (!enableAutoLocalization || Manager.localization == null) return;

            for (int i = 0; i < autoLocalizedTexts.Count; i++)
            {
                if (i < autoLocalizeKeyList.Count && autoLocalizedTexts[i] != null)
                {
                    string localizedText = GetLocalizedText(autoLocalizeKeyList[i]);
                    autoLocalizedTexts[i].text = localizedText;
                    //Debug.Log($"[{GetType().Name}] {autoLocalizedTexts[i].name} 텍스트 업데이트: {localizedText}");
                }
            }
        }

        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            //Debug.Log($"[{GetType().Name}] 언어 변경 감지: {newLanguage}");
            UpdateAutoLocalizedTexts();
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

        protected void PlayHoverSound(string soundName = null)
        {
            if (!enableSFX) return;

            string soundToPlay = soundName ?? defaultHoverSound;
            //if (!string.IsNullOrEmpty(soundToPlay) && Manager.Audio != null)
            //{
            //    Manager.Audio.SfxPlay(soundToPlay);
            //}
        }

        protected void PlayErrorSound(string soundName = null)
        {
            if (!enableSFX) return;

            string soundToPlay = soundName ?? defaultErrorSound;
            //if (!string.IsNullOrEmpty(soundToPlay) && Manager.Audio != null)
            //{
            //    Manager.Audio.SfxPlay(soundToPlay);
            //}
        }

        protected void PlaySuccessSound(string soundName = null)
        {
            if (!enableSFX) return;

            string soundToPlay = soundName ?? defaultSuccessSound;
            //if (!string.IsNullOrEmpty(soundToPlay) && Manager.Audio != null)
            //{
            //    Manager.Audio.SfxPlay(soundToPlay);
            //}
        }

        protected void PlayOpenSound(string soundName = null)
        {
            if (!enableSFX) return;

            string soundToPlay = soundName ?? defaultOpenSound;
            //if (!string.IsNullOrEmpty(soundToPlay) && Manager.Audio != null)
            //{
            //    Manager.Audio.SfxPlay(soundToPlay);
            //}
        }

        protected void PlayCloseSound(string soundName = null)
        {
            if (!enableSFX) return;

            string soundToPlay = soundName ?? defaultCloseSound;
            //if (!string.IsNullOrEmpty(soundToPlay) && Manager.Audio != null)
            //{
            //    Manager.Audio.SfxPlay(soundToPlay);
            //}
        }

        /// <summary>
        /// 기본 클릭 사운드가 포함된 이벤트 핸들러
        /// </summary>
        public PointerHandler GetEventWithSFX(in string name)
        {
            return GetEventWithSFX(name, defaultClickSound);
        }

        /// <summary>
        /// 커스텀 클릭 사운드가 포함된 이벤트 핸들러
        /// </summary>
        public PointerHandler GetEventWithSFX(in string name, string clickSound)
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

        /// <summary>
        /// 기본 뒤로가기 사운드가 포함된 이벤트 핸들러
        /// </summary>
        public PointerHandler GetBackEvent(in string name)
        {
            return GetBackEvent(name, defaultBackSound);
        }

        /// <summary>
        /// 커스텀 뒤로가기 사운드가 포함된 이벤트 핸들러
        /// </summary>
        public PointerHandler GetBackEvent(in string name, string backSound)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                handler.Click += (data) => PlayBackSound(backSound);
            }
            return handler;
        }

        /// <summary>
        /// 호버 사운드가 포함된 이벤트 핸들러
        /// </summary>
        public PointerHandler GetHoverEvent(in string name, string hoverSound = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                handler.Enter += (data) => PlayHoverSound(hoverSound);
            }
            return handler;
        }

        /// <summary>
        /// 에러 사운드가 포함된 이벤트 핸들러
        /// </summary>
        public PointerHandler GetErrorEvent(in string name, string errorSound = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                handler.Click += (data) => PlayErrorSound(errorSound);
            }
            return handler;
        }

        /// <summary>
        /// 성공 사운드가 포함된 이벤트 핸들러
        /// </summary>
        public PointerHandler GetSuccessEvent(in string name, string successSound = null)
        {
            PointerHandler handler = GetEvent(name);
            if (handler != null)
            {
                handler.Click += (data) => PlaySuccessSound(successSound);
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
