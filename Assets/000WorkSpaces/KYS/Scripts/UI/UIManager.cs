using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

#if DOTWEEN
using DG.Tweening;
#endif

namespace KYS
{
    /// <summary>
    /// Addressable 기반 완전한 UI 관리자
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Addressable Canvas References")]
        [SerializeField] private AssetReferenceGameObject hudCanvasReference;
        [SerializeField] private AssetReferenceGameObject panelCanvasReference;
        [SerializeField] private AssetReferenceGameObject popupCanvasReference;
        [SerializeField] private AssetReferenceGameObject loadingCanvasReference;

        [Header("UI Management")]
        [SerializeField] public SafeAreaManager safeAreaManager;

        [Header("Backdrop Settings")]
        [SerializeField] private AssetReferenceGameObject backdropPrefabReference; // Backdrop Prefab Reference

        [Header("Addressable UI Settings")]
        //[SerializeField] private string uiPrefabLabel = "UI";
        //[SerializeField] private string hudPrefabLabel = "UI_HUD";
        //[SerializeField] private string panelPrefabLabel = "UI_Panel";
        //[SerializeField] private string popupPrefabLabel = "UI_Popup";

        // Canvas 참조들
        private Canvas hudCanvas;
        private Canvas panelCanvas;
        private Canvas popupCanvas;
        private Canvas loadingCanvas;

        // Layer별 UI 관리
        private Dictionary<UILayerType, List<BaseUI>> layerUIs = new Dictionary<UILayerType, List<BaseUI>>();

        // Stack 기반 UI 관리
        private Stack<BaseUI> panelStack = new Stack<BaseUI>();
        private Stack<BaseUI> popupStack = new Stack<BaseUI>();

        // Addressable 핸들 관리
        private Dictionary<string, AsyncOperationHandle> addressableHandles = new Dictionary<string, AsyncOperationHandle>();
        private Dictionary<string, GameObject> instantiatedUIs = new Dictionary<string, GameObject>();
        
        // 중복 생성 방지를 위한 플래그들
        private bool isCreatingPopup = false;
        private bool isCreatingPanel = false;
        private bool isCreatingUI = false;
        private bool isCreatingHUD = false;

        // UI 상태 관리
        public static int selectIndexUI { get; set; } = 0;
        public static bool canClosePopUp = true;
        bool canClose => (panelStack.Count > 0 || popupStack.Count > 0) &&
                        canClosePopUp && !IsCurrentUINonClosable();

        #region Properties

        public Canvas HUDCanvas => hudCanvas;
        public Canvas PanelCanvas => panelCanvas;
        public Canvas PopupCanvas => popupCanvas;
        public Canvas LoadingCanvas => loadingCanvas;
        public AssetReferenceGameObject BackdropPrefabReference => backdropPrefabReference;

        /// <summary>
        /// Canvas들이 모두 초기화되었는지 확인
        /// </summary>
        public bool AreCanvasesInitialized => hudCanvas != null && panelCanvas != null && popupCanvas != null && loadingCanvas != null;

        #endregion

        #region Initialization

        private async void Awake()
        {
            Init();
            await InitializeAddressableCanvases();
            InitializeLayerUIs();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ReleaseAllAddressables();
        }

        /// <summary>
        /// Addressable Canvas 초기화
        /// </summary>
        private async System.Threading.Tasks.Task InitializeAddressableCanvases()
        {
            try
            {
                ////Debug.Log("[UIManager] Addressable Canvas 초기화 시작");

                // HUD Canvas 로드
                if (hudCanvasReference != null && hudCanvasReference.RuntimeKeyIsValid())
                {
                    var hudHandle = hudCanvasReference.InstantiateAsync();
                    await hudHandle.Task;
                    hudCanvas = hudHandle.Result.GetComponent<Canvas>();
                    hudCanvas.sortingOrder = 0; // 가장 뒤에 렌더링
                    addressableHandles["HUDCanvas"] = hudHandle;
                    DontDestroyOnLoad(hudHandle.Result);

                    // HUD Canvas의 모든 UI 요소들을 숨김 상태로 초기화
                    HideAllHUDElements();
                }

                // Panel Canvas 로드
                if (panelCanvasReference != null && panelCanvasReference.RuntimeKeyIsValid())
                {
                    var panelHandle = panelCanvasReference.InstantiateAsync();
                    await panelHandle.Task;
                    panelCanvas = panelHandle.Result.GetComponent<Canvas>();
                    panelCanvas.sortingOrder = 10; // HUD 위에 렌더링
                    addressableHandles["PanelCanvas"] = panelHandle;
                    DontDestroyOnLoad(panelHandle.Result);
                }

                // Popup Canvas 로드
                if (popupCanvasReference != null && popupCanvasReference.RuntimeKeyIsValid())
                {
                    var popupHandle = popupCanvasReference.InstantiateAsync();
                    await popupHandle.Task;
                    popupCanvas = popupHandle.Result.GetComponent<Canvas>();
                    popupCanvas.sortingOrder = 20; // Panel 위에 렌더링
                    addressableHandles["PopupCanvas"] = popupHandle;
                    DontDestroyOnLoad(popupHandle.Result);
                }

                // Loading Canvas 로드
                if (loadingCanvasReference != null && loadingCanvasReference.RuntimeKeyIsValid())
                {
                    var loadingHandle = loadingCanvasReference.InstantiateAsync();
                    await loadingHandle.Task;
                    loadingCanvas = loadingHandle.Result.GetComponent<Canvas>();
                    loadingCanvas.sortingOrder = 30; // 가장 앞에 렌더링
                    addressableHandles["LoadingCanvas"] = loadingHandle;
                    DontDestroyOnLoad(loadingHandle.Result);
                }

                ApplySafeAreaToCanvases();

                // HUD 요소들 초기 생성 및 숨김
                await InitializeHUDElements();

                ////Debug.Log("[UIManager] Addressable Canvas 초기화 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] Canvas 초기화 중 오류: {e.Message}");
            }
        }

        /// <summary>
        /// HUD 요소들을 초기에 생성하고 SafeAreaPanel 아래에 배치한 후 숨김
        /// </summary>
        private async System.Threading.Tasks.Task InitializeHUDElements()
        {
            if (hudCanvas == null) return;

            try
            {
                //Debug.Log("[UIManager] HUD 요소들 초기화 시작");

                // HUD 요소들을 미리 생성할 Addressable 키들
                string[] hudKeys = {
                    "KYS/HUDAllPanel",
                    // 추가 HUD 키들을 여기에 추가
                };

                foreach (string key in hudKeys)
                {
                    try
                    {
                        // HUD 생성 (이미 SafeAreaPanel 자식으로 생성됨)
                        var hud = await CreateHUDAsync<BaseUI>(key);
                        if (hud != null)
                        {
                            // 생성 후 즉시 숨김
                            hud.gameObject.SetActive(false);
                            //Debug.Log($"[UIManager] HUD {hud.name} 생성 완료 및 숨김 (SafeAreaPanel 자식으로 생성됨)");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[UIManager] HUD {key} 생성 실패: {e.Message}");
                    }
                }

                ////Debug.Log("[UIManager] HUD 요소들 초기화 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] HUD 요소들 초기화 중 오류: {e.Message}");
            }
        }

        /// <summary>
        /// HUD를 위한 SafeArea 부모 Transform 반환
        /// </summary>
        private Transform GetSafeAreaParentForHUD()
        {
            if (hudCanvas == null) return null;

            // HUD Canvas에 SafeAreaPanel이 있는지 확인
            SafeAreaPanel safeAreaPanel = hudCanvas.GetComponentInChildren<SafeAreaPanel>();
            if (safeAreaPanel != null)
            {
                //Debug.Log($"[UIManager] HUD용 SafeAreaPanel 발견: {safeAreaPanel.name}");
                return safeAreaPanel.transform;
            }

            // SafeAreaPanel이 없으면 새로 생성
            if (safeAreaManager != null)
            {
                //Debug.Log($"[UIManager] HUD용 SafeAreaPanel 생성 시도: {hudCanvas.name}");
                safeAreaManager.ApplySafeAreaToCanvas(hudCanvas);

                // 다시 SafeAreaPanel 찾기
                safeAreaPanel = hudCanvas.GetComponentInChildren<SafeAreaPanel>();
                if (safeAreaPanel != null)
                {
                    //Debug.Log($"[UIManager] HUD용 SafeAreaPanel 생성 완료: {safeAreaPanel.name}");
                    return safeAreaPanel.transform;
                }
            }

            Debug.LogWarning($"[UIManager] HUD용 SafeAreaPanel 생성 실패, HUD Canvas를 직접 사용: {hudCanvas.name}");
            return hudCanvas.transform;
        }

        private void ApplySafeAreaToCanvases()
        {
            if (safeAreaManager != null)
            {
                if (hudCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(hudCanvas);
                if (panelCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(panelCanvas);
                if (popupCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(popupCanvas);
                // LoadingCanvas는 SafeArea 적용 제외
                // if (loadingCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(loadingCanvas);

                // LoadingCanvas에서 기존 SafeAreaPanel 제거
                safeAreaManager.RemoveSafeAreaFromLoadingCanvas();
            }
        }

        private void InitializeLayerUIs()
        {
            foreach (UILayerType layerType in System.Enum.GetValues(typeof(UILayerType)))
            {
                layerUIs[layerType] = new List<BaseUI>();
            }
        }

        void Init()
        {
            base.SingletonInit();
        }

        #endregion

        #region Addressable UI Loading

        /// <summary>
        /// 문자열 키로 UI 로드
        /// </summary>
        public async System.Threading.Tasks.Task<T> LoadUIAsync<T>(string addressableKey, Transform parent = null) where T : BaseUI
        {
            try
            {
                // 중복 생성 방지
                if (isCreatingUI)
                {
                    Debug.Log("[UIManager] 이미 UI 생성 중이므로 무시합니다.");
                    return null;
                }
                
                isCreatingUI = true;
                
                //Debug.Log($"[UIManager] UI 로드 시작: {addressableKey}");

                // 이미 로드된 UI가 있는지 확인
                if (instantiatedUIs.ContainsKey(addressableKey))
                {
                    GameObject existingUI = instantiatedUIs[addressableKey];
                    if (existingUI != null)
                    {
                        T existingComponent = existingUI.GetComponent<T>();
                        if (existingComponent != null)
                        {
                            //Debug.Log($"[UIManager] 기존 UI 반환: {addressableKey}");
                            isCreatingUI = false;
                            return existingComponent;
                        }
                    }
                }

                // Addressable에서 프리팹 로드
                var handle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[UIManager] UI 로드 실패: {addressableKey}");
                    Addressables.Release(handle);
                    return null;
                }

                // 부모 Transform 결정
                Transform targetParent = parent ?? GetCanvasForUI<T>();
                if (targetParent == null)
                {
                    Debug.LogError($"[UIManager] UI 부모를 찾을 수 없음: {addressableKey}");
                    Addressables.Release(handle);
                    return null;
                }

                // UI 인스턴스 생성
                var instanceHandle = Addressables.InstantiateAsync(handle.Result, targetParent);
                await instanceHandle.Task;

                if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[UIManager] UI 인스턴스 생성 실패: {addressableKey}");
                    Addressables.Release(handle);
                    return null;
                }

                GameObject uiInstance = instanceHandle.Result;
                T uiComponent = uiInstance.GetComponent<T>();

                if (uiComponent == null)
                {
                    Debug.LogError($"[UIManager] UI 컴포넌트를 찾을 수 없음: {addressableKey}");
                    Addressables.ReleaseInstance(uiInstance);
                    Addressables.Release(handle);
                    return null;
                }

                // 관리에 추가
                instantiatedUIs[addressableKey] = uiInstance;
                addressableHandles[addressableKey] = handle;
                RegisterUI(uiComponent);

                //Debug.Log($"[UIManager] UI 로드 완료: {addressableKey}");
                isCreatingUI = false;
                return uiComponent;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI 로드 중 오류: {addressableKey}, {e.Message}");
                isCreatingUI = false;
                return null;
            }
        }

        /// <summary>
        /// AssetReference로 UI 로드
        /// </summary>
        public async System.Threading.Tasks.Task<T> LoadUIAsync<T>(AssetReferenceGameObject assetReference, Transform parent = null) where T : BaseUI
        {
            try
            {
                // 중복 생성 방지
                if (isCreatingUI)
                {
                    Debug.Log("[UIManager] 이미 UI 생성 중이므로 무시합니다.");
                    return null;
                }
                
                isCreatingUI = true;
                
                if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                {
                    Debug.LogError("[UIManager] 유효하지 않은 AssetReference입니다.");
                    isCreatingUI = false;
                    return null;
                }

                string key = assetReference.RuntimeKey.ToString();
                //Debug.Log($"[UIManager] AssetReference로 UI 로드 시작: {key}");

                // 이미 로드된 UI가 있는지 확인
                if (instantiatedUIs.ContainsKey(key))
                {
                    GameObject existingUI = instantiatedUIs[key];
                    if (existingUI != null)
                    {
                        T existingComponent = existingUI.GetComponent<T>();
                        if (existingComponent != null)
                        {
                            //Debug.Log($"[UIManager] 기존 UI 반환: {key}");
                            isCreatingUI = false;
                            return existingComponent;
                        }
                    }
                }

                // 부모 Transform 결정
                Transform targetParent = parent ?? GetCanvasForUI<T>();
                if (targetParent == null)
                {
                    Debug.LogError($"[UIManager] UI 부모를 찾을 수 없음: {key}");
                    return null;
                }

                // UI 인스턴스 생성
                var instanceHandle = assetReference.InstantiateAsync(targetParent);
                await instanceHandle.Task;

                if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[UIManager] UI 인스턴스 생성 실패: {key}");
                    return null;
                }

                GameObject uiInstance = instanceHandle.Result;
                T uiComponent = uiInstance.GetComponent<T>();

                if (uiComponent == null)
                {
                    Debug.LogError($"[UIManager] UI 컴포넌트를 찾을 수 없음: {key}");
                    Addressables.ReleaseInstance(uiInstance);
                    return null;
                }

                // 관리에 추가
                instantiatedUIs[key] = uiInstance;
                RegisterUI(uiComponent);

                //Debug.Log($"[UIManager] UI 로드 완료: {key}");
                isCreatingUI = false;
                return uiComponent;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI 로드 중 오류: {e.Message}");
                isCreatingUI = false;
                return null;
            }
        }

        /// <summary>
        /// 라벨로 UI들 일괄 로드
        /// </summary>
        public async System.Threading.Tasks.Task<List<T>> LoadUIsByLabelAsync<T>(string label) where T : BaseUI
        {
            try
            {
                // 중복 생성 방지
                if (isCreatingUI)
                {
                    Debug.Log("[UIManager] 이미 UI 생성 중이므로 무시합니다.");
                    return new List<T>();
                }
                
                isCreatingUI = true;
                
                //Debug.Log($"[UIManager] 라벨로 UI 로드 시작: {label}");

                var handle = Addressables.LoadResourceLocationsAsync(label);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[UIManager] 라벨 로드 실패: {label}");
                    Addressables.Release(handle);
                    return new List<T>();
                }

                List<T> loadedUIs = new List<T>();

                foreach (var location in handle.Result)
                {
                    if (location.PrimaryKey.Contains("UI/"))
                    {
                        T ui = await LoadUIAsync<T>(location.PrimaryKey);
                        if (ui != null)
                        {
                            loadedUIs.Add(ui);
                        }
                    }
                }

                Addressables.Release(handle);
                //Debug.Log($"[UIManager] 라벨 로드 완료: {label}, {loadedUIs.Count}개");
                isCreatingUI = false;
                return loadedUIs;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] 라벨 로드 중 오류: {label}, {e.Message}");
                isCreatingUI = false;
                return new List<T>();
            }
        }

        /// <summary>
        /// UI 미리 로드
        /// </summary>
        public async System.Threading.Tasks.Task PreloadUIAsync<T>(string addressableKey) where T : BaseUI
        {
            try
            {
                // 중복 생성 방지
                if (isCreatingUI)
                {
                    Debug.Log("[UIManager] 이미 UI 생성 중이므로 무시합니다.");
                    return;
                }
                
                isCreatingUI = true;
                
                //Debug.Log($"[UIManager] UI 미리 로드 시작: {addressableKey}");

                var handle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    addressableHandles[addressableKey] = handle;
                    //Debug.Log($"[UIManager] UI 미리 로드 완료: {addressableKey}");
                }
                else
                {
                    Debug.LogError($"[UIManager] UI 미리 로드 실패: {addressableKey}");
                    Addressables.Release(handle);
                }
                
                isCreatingUI = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI 미리 로드 중 오류: {addressableKey}, {e.Message}");
                isCreatingUI = false;
            }
        }

        #endregion

        #region Addressable Resource Management

        /// <summary>
        /// 특정 UI 해제
        /// </summary>
        public void ReleaseUI(string addressableKey)
        {
            try
            {
                if (instantiatedUIs.ContainsKey(addressableKey))
                {
                    GameObject uiInstance = instantiatedUIs[addressableKey];
                    if (uiInstance != null)
                    {
                        BaseUI uiComponent = uiInstance.GetComponent<BaseUI>();
                        if (uiComponent != null)
                        {
                            UnregisterUI(uiComponent);
                        }
                        Addressables.ReleaseInstance(uiInstance);
                    }
                    instantiatedUIs.Remove(addressableKey);
                }

                if (addressableHandles.ContainsKey(addressableKey))
                {
                    Addressables.Release(addressableHandles[addressableKey]);
                    addressableHandles.Remove(addressableKey);
                }

                //Debug.Log($"[UIManager] UI 해제 완료: {addressableKey}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI 해제 중 오류: {addressableKey}, {e.Message}");
            }
        }

        /// <summary>
        /// 모든 Addressable 리소스 해제
        /// </summary>
        public void ReleaseAllAddressables()
        {
            try
            {
                //Debug.Log("[UIManager] 모든 Addressable 리소스 해제 시작");

                // 인스턴스 해제
                foreach (var kvp in instantiatedUIs)
                {
                    if (kvp.Value != null)
                    {
                        BaseUI uiComponent = kvp.Value.GetComponent<BaseUI>();
                        if (uiComponent != null)
                        {
                            UnregisterUI(uiComponent);
                        }
                        Addressables.ReleaseInstance(kvp.Value);
                    }
                }
                instantiatedUIs.Clear();

                // 핸들 해제
                foreach (var handle in addressableHandles.Values)
                {
                    Addressables.Release(handle);
                }
                addressableHandles.Clear();

                //Debug.Log("[UIManager] 모든 Addressable 리소스 해제 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] Addressable 리소스 해제 중 오류: {e.Message}");
            }
        }

        #endregion

        #region Canvas Management

        public Transform GetCanvasForUI<T>() where T : BaseUI
        {
            // T 타입의 기본 레이어 타입을 추정
            UILayerType layerType = UILayerType.Panel; // 기본값

            // 타입 이름으로 레이어 추정
            string typeName = typeof(T).Name.ToLower();
            if (typeName.Contains("hud"))
                layerType = UILayerType.HUD;
            else if (typeName.Contains("popup"))
                layerType = UILayerType.Popup;
            else if (typeName.Contains("loading"))
                layerType = UILayerType.Loading;

            //Debug.Log($"[UIManager] GetCanvasForUI: {typeof(T).Name} -> {layerType}");

            Transform targetTransform = null;

            switch (layerType)
            {
                case UILayerType.HUD:
                    targetTransform = hudCanvas?.transform;
                    break;
                case UILayerType.Panel:
                    targetTransform = panelCanvas?.transform;
                    break;
                case UILayerType.Popup:
                    targetTransform = popupCanvas?.transform;
                    break;
                case UILayerType.Loading:
                    targetTransform = loadingCanvas?.transform;
                    break;
                default:
                    targetTransform = panelCanvas?.transform;
                    break;
            }

            //Debug.Log($"[UIManager] 기본 Canvas: {targetTransform?.name}");

            // LoadingCanvas는 SafeAreaPanel 사용하지 않음
            if (layerType == UILayerType.Loading)
            {
                //Debug.Log($"[UIManager] Loading 타입이므로 SafeAreaPanel 사용하지 않음");
                return targetTransform;
            }

            // SafeAreaPanel이 있는지 확인하고 해당 패널의 자식으로 생성
            if (targetTransform != null && safeAreaManager != null)
            {
                Canvas targetCanvas = targetTransform.GetComponent<Canvas>();
                if (targetCanvas != null)
                {
                    //Debug.Log($"[UIManager] SafeAreaManager에서 SafeAreaPanel 찾기 시도: {targetCanvas.name}");
                    SafeAreaPanel safeAreaPanel = safeAreaManager.GetSafeAreaPanelForCanvas(targetCanvas);

                    if (safeAreaPanel != null)
                    {
                        //Debug.Log($"[UIManager] ✅ SafeAreaPanel을 부모로 사용: {safeAreaPanel.name} for {typeof(T).Name}");
                        //Debug.Log($"[UIManager] SafeAreaPanel 위치: {safeAreaPanel.transform.position}");
                        //Debug.Log($"[UIManager] SafeAreaPanel 자식 수: {safeAreaPanel.transform.childCount}");
                        return safeAreaPanel.transform;
                    }
                    else
                    {
                        Debug.LogWarning($"[UIManager] ❌ SafeAreaPanel을 찾을 수 없음: {targetCanvas.name}");

                        // SafeAreaPanel이 없으면 새로 생성 시도
                        //Debug.Log($"[UIManager] 🔧 SafeAreaPanel 새로 생성 시도: {targetCanvas.name}");
                        safeAreaManager.ApplySafeAreaToCanvas(targetCanvas);

                        // 다시 SafeAreaPanel 찾기
                        safeAreaPanel = safeAreaManager.GetSafeAreaPanelForCanvas(targetCanvas);
                        if (safeAreaPanel != null)
                        {
                            //Debug.Log($"[UIManager] ✅ 새로 생성된 SafeAreaPanel을 부모로 사용: {safeAreaPanel.name} for {typeof(T).Name}");
                            //Debug.Log($"[UIManager] SafeAreaPanel 위치: {safeAreaPanel.transform.position}");
                            //Debug.Log($"[UIManager] SafeAreaPanel 자식 수: {safeAreaPanel.transform.childCount}");
                            return safeAreaPanel.transform;
                        }
                        else
                        {
                            Debug.LogWarning($"[UIManager] SafeAreaPanel 생성 실패, Canvas를 직접 사용: {targetCanvas.name}");
                            return targetTransform;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[UIManager] Canvas 컴포넌트를 찾을 수 없음: {targetTransform.name}");
                }
            }
            else
            {
                if (targetTransform == null)
                {
                    Debug.LogWarning($"[UIManager] targetTransform이 null임");
                }
                if (safeAreaManager == null)
                {
                    Debug.LogWarning($"[UIManager] safeAreaManager가 null임");
                }
            }

            //Debug.Log($"[UIManager] Canvas를 부모로 사용: {targetTransform?.name} for {typeof(T).Name}");
            return targetTransform;
        }

        /// <summary>
        /// 레이어별 Canvas 가져오기
        /// </summary>
        public Canvas GetCanvasByLayer(UILayerType layerType)
        {
            switch (layerType)
            {
                case UILayerType.HUD:
                    return hudCanvas;
                case UILayerType.Panel:
                    return panelCanvas;
                case UILayerType.Popup:
                    return popupCanvas;
                case UILayerType.Loading:
                    return loadingCanvas;
                default:
                    return null;
            }
        }

        #endregion

        #region UI Registration

        /// <summary>
        /// UI 등록
        /// </summary>
        public void RegisterUI(BaseUI ui)
        {
            if (ui == null) return;

            UILayerType layerType = ui.LayerType;
            if (!layerUIs.ContainsKey(layerType))
            {
                layerUIs[layerType] = new List<BaseUI>();
            }

            if (!layerUIs[layerType].Contains(ui))
            {
                layerUIs[layerType].Add(ui);
                ////Debug.Log($"[UIManager] UI 등록: {ui.name} -> {layerType}");
            }
        }

        /// <summary>
        /// UI 등록 해제
        /// </summary>
        public void UnregisterUI(BaseUI ui)
        {
            if (ui == null) return;

            UILayerType layerType = ui.LayerType;
            if (layerUIs.ContainsKey(layerType))
            {
                layerUIs[layerType].Remove(ui);
                //Debug.Log($"[UIManager] UI 등록 해제: {ui.name} -> {layerType}");
            }
        }

        /// <summary>
        /// 레이어별 UI 목록 가져오기
        /// </summary>
        public List<BaseUI> GetUIsByLayer(UILayerType layerType)
        {
            if (layerUIs.ContainsKey(layerType))
            {
                return new List<BaseUI>(layerUIs[layerType]);
            }
            return new List<BaseUI>();
        }

        #endregion

        #region Panel Management

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void OpenPanel(BaseUI panel)
        {
            if (panel == null) return;
            
            // 동일한 타입의 패널이 이미 스택에 있는지 확인 (더 엄격한 체크)
            foreach (var existingPanel in panelStack)
            {
                if (existingPanel != null && existingPanel.GetType() == panel.GetType())
                {
                    Debug.LogWarning($"[UIManager] 동일한 타입의 패널이 이미 존재합니다: {panel.GetType().Name}");
                    // 기존 패널을 최상위로 이동
                    var tempStack = new Stack<BaseUI>();
                    while (panelStack.Count > 0)
                    {
                        var current = panelStack.Pop();
                        if (current != existingPanel)
                        {
                            tempStack.Push(current);
                        }
                    }
                    while (tempStack.Count > 0)
                    {
                        panelStack.Push(tempStack.Pop());
                    }
                    panelStack.Push(existingPanel);
                    return;
                }
            }

            // 이전 패널 처리
            if (panelStack.Count > 0)
            {
                BaseUI previousPanel = panelStack.Peek();
                if (previousPanel != null)
                {
                    if (panel.HidePreviousUI)
                    {
                        // 이전 UI를 완전히 숨김 (SetActive(false))
                        previousPanel.gameObject.SetActive(false);
                        //Debug.Log($"[UIManager] 이전 패널 숨김: {previousPanel.name}");
                    }
                    else if (panel.DisablePreviousUI)
                    {
                        // 이전 UI를 비활성화 (CanvasGroup.interactable = false)
                        var canvasGroup = previousPanel.GetComponent<CanvasGroup>();
                        if (canvasGroup != null)
                        {
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;
                            //Debug.Log($"[UIManager] 이전 패널 비활성화: {previousPanel.name}");
                        }
                    }
                    // 둘 다 false면 이전 UI는 활성화 상태 유지
                }
            }

            // 새 패널을 스택에 추가
            panelStack.Push(panel);
            RegisterUI(panel);
            panel.Show();

            Debug.Log($"[UIManager] 패널 열기 완료: {panel.name}");
            DebugStackStatus();
            
            // 생성 플래그는 ShowPanelAsyncCoroutine의 finally 블록에서 해제됨
        }

        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void ClosePanel()
        {
            if (panelStack.Count == 0) return;

            BaseUI currentPanel = panelStack.Pop();
            if (currentPanel != null)
            {
                // 스택 구조에서는 항상 파괴
                UnregisterUI(currentPanel);
                currentPanel.Hide();
                //Debug.Log($"[UIManager] 패널 닫기: {currentPanel.name}");
            }

            // 이전 패널 다시 표시
            if (panelStack.Count > 0)
            {
                BaseUI previousPanel = panelStack.Peek();
                if (previousPanel != null)
                {
                    // 이전 UI가 비활성화되어 있다면 다시 활성화
                    if (!previousPanel.gameObject.activeInHierarchy)
                    {
                        previousPanel.gameObject.SetActive(true);
                        //Debug.Log($"[UIManager] 이전 패널 다시 활성화: {previousPanel.name}");
                    }

                    // CanvasGroup이 비활성화되어 있다면 다시 활성화
                    var canvasGroup = previousPanel.GetComponent<CanvasGroup>();
                    if (canvasGroup != null && !canvasGroup.interactable)
                    {
                        canvasGroup.interactable = true;
                        canvasGroup.blocksRaycasts = true;
                        //Debug.Log($"[UIManager] 이전 패널 CanvasGroup 다시 활성화: {previousPanel.name}");
                    }

                    previousPanel.Show();
                }
            }

            DebugStackStatus();
        }

        #endregion

        #region Popup Management

        /// <summary>
        /// 팝업 열기
        /// </summary>
        public void OpenPopup(BaseUI popup)
        {
            if (popup == null) return;

            // 동일한 타입의 팝업이 이미 스택에 있는지 확인
            foreach (var existingPopup in popupStack)
            {
                if (existingPopup != null && existingPopup.GetType() == popup.GetType())
                {
                    Debug.LogWarning($"[UIManager] 동일한 타입의 팝업이 이미 존재합니다: {popup.GetType().Name}");
                    return;
                }
            }

            //Debug.Log($"[UIManager] Popup 열기: {popup.name}");

            // 이전 Popup 처리
            if (popupStack.Count > 0)
            {
                BaseUI previousPopup = popupStack.Peek();
                if (previousPopup != null && previousPopup != popup)
                {
                    if (popup.HidePreviousUI)
                    {
                        // 이전 UI를 완전히 숨김 (SetActive(false))
                        previousPopup.gameObject.SetActive(false);
                        //Debug.Log($"[UIManager] 이전 Popup 숨김: {previousPopup.name}");
                    }
                    else if (popup.DisablePreviousUI)
                    {
                        // 이전 UI를 비활성화 (CanvasGroup.interactable = false)
                        var canvasGroup = previousPopup.GetComponent<CanvasGroup>();
                        if (canvasGroup != null)
                        {
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;
                            //Debug.Log($"[UIManager] 이전 Popup 비활성화: {previousPopup.name}");
                        }
                    }
                    // 둘 다 false면 이전 UI는 활성화 상태 유지
                }
            }

            // 이전 UI들의 터치 차단
            DisablePreviousUITouch();

            // Popup 스택에 추가
            popupStack.Push(popup);

            // Popup 표시
            popup.Show();

            // UIManager에 등록
            RegisterUI(popup);

            //Debug.Log($"[UIManager] Popup 열기 완료: {popup.name}, 스택 크기: {popupStack.Count}");
        }

        /// <summary>
        /// 현재 열린 Popup 닫기
        /// </summary>
        public void ClosePopup()
        {
            if (popupStack.Count == 0)
            {
                Debug.LogWarning("[UIManager] 닫을 Popup이 없습니다.");
                return;
            }

            BaseUI currentPopup = popupStack.Pop();
            if (currentPopup != null)
            {
                //Debug.Log($"[UIManager] Popup 닫기: {currentPopup.name}");

                // Popup의 Backdrop 가져오기
                BackdropUI backdrop = currentPopup.GetOwnBackdrop();

                // Popup 숨기기
                currentPopup.Hide();

                // UIManager에서 등록 해제
                UnregisterUI(currentPopup);

                // Popup 파괴 (Backdrop도 함께 파괴됨 - Popup이 Backdrop의 자식이므로)
                if (backdrop != null)
                {
                    //Debug.Log($"[UIManager] Backdrop와 함께 Popup 파괴: {currentPopup.name}");
                    Destroy(backdrop.gameObject);
                }
                else
                {
                    //Debug.Log($"[UIManager] Popup 파괴: {currentPopup.name}");
                    Destroy(currentPopup.gameObject);
                }
            }

            // 이전 UI들의 터치 복원
            RestorePreviousUITouch();

            // 이전 Popup이 있다면 다시 활성화
            if (popupStack.Count > 0)
            {
                BaseUI previousPopup = popupStack.Peek();
                if (previousPopup != null)
                {
                    // 이전 UI가 비활성화되어 있다면 다시 활성화
                    if (!previousPopup.gameObject.activeInHierarchy)
                    {
                        previousPopup.gameObject.SetActive(true);
                        //Debug.Log($"[UIManager] 이전 Popup 다시 활성화: {previousPopup.name}");
                    }

                    // CanvasGroup이 비활성화되어 있다면 다시 활성화
                    var canvasGroup = previousPopup.GetComponent<CanvasGroup>();
                    if (canvasGroup != null && !canvasGroup.interactable)
                    {
                        canvasGroup.interactable = true;
                        canvasGroup.blocksRaycasts = true;
                        //Debug.Log($"[UIManager] 이전 Popup CanvasGroup 다시 활성화: {previousPopup.name}");
                    }
                }
            }

            //Debug.Log($"[UIManager] Popup 닫기 완료, 남은 스택 크기: {popupStack.Count}");
        }

        #endregion

        #region Loading Screen Methods

        /// <summary>
        /// LoadingScreen 표시
        /// </summary>
        public void ShowLoadingScreen(string message = "로딩 중...")
        {
            StartCoroutine(ShowLoadingScreenCoroutine(message));
        }

        private IEnumerator ShowLoadingScreenCoroutine(string message)
        {
            // 씬 전환 중 Addressable 시스템 안정성을 위한 대기
            yield return new WaitForSeconds(0.1f);

            // LoadingScreen 프리팹을 직접 로드
            string[] possibleKeys = {
                "KYS/LoadingScreen",
                "LoadingScreen",
                "UI/LoadingScreen",
                "KYS/UILoading/LoadingScreen"
            };

            GameObject loadingScreenPrefab = null;

            foreach (string key in possibleKeys)
            {
                AsyncOperationHandle<GameObject> handle = default;
                bool loadSuccess = false;

                try
                {
                    handle = Addressables.LoadAssetAsync<GameObject>(key);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[UIManager] LoadingScreen 프리팹 로드 실패 ({key}): {e.Message}");
                    continue;
                }

                yield return handle;

                try
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        loadingScreenPrefab = handle.Result;
                        //Debug.Log($"[UIManager] LoadingScreen 프리팹 로드 성공: {key}");
                        loadSuccess = true;
                        break;
                    }
                    else
                    {
                        Addressables.Release(handle);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[UIManager] LoadingScreen 프리팹 처리 실패 ({key}): {e.Message}");
                    if (!loadSuccess)
                    {
                        Addressables.Release(handle);
                    }
                }
            }

            if (loadingScreenPrefab != null)
            {
                // LoadingCanvas에 인스턴스 생성
                GameObject instance = Instantiate(loadingScreenPrefab, loadingCanvas.transform);
                LoadingScreen loadingScreen = instance.GetComponent<LoadingScreen>();

                if (loadingScreen != null)
                {
                    loadingScreen.Initialize();
                    if (!string.IsNullOrEmpty(message))
                    {
                        loadingScreen.SetCenterMessage(message);
                    }
                    //Debug.Log("[UIManager] LoadingScreen 표시 완료");
                }
                else
                {
                    Debug.LogError("[UIManager] LoadingScreen 컴포넌트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError("[UIManager] LoadingScreen 프리팹을 찾을 수 없습니다. Addressables 설정을 확인하세요.");

                // 대체 방법: Resources 폴더에서 로드 시도
                //Debug.Log("[UIManager] Resources 폴더에서 LoadingScreen 프리팹 로드 시도");
                GameObject fallbackPrefab = Resources.Load<GameObject>("LoadingScreen");
                if (fallbackPrefab != null)
                {
                    GameObject instance = Instantiate(fallbackPrefab, loadingCanvas.transform);
                    LoadingScreen loadingScreen = instance.GetComponent<LoadingScreen>();

                    if (loadingScreen != null)
                    {
                        loadingScreen.Initialize();
                        if (!string.IsNullOrEmpty(message))
                        {
                            loadingScreen.SetCenterMessage(message);
                        }
                        //Debug.Log("[UIManager] Resources에서 LoadingScreen 로드 성공");
                    }
                }
                else
                {
                    Debug.LogError("[UIManager] Resources 폴더에서도 LoadingScreen을 찾을 수 없습니다.");
                }
            }
        }

        /// <summary>
        /// LoadingScreen 숨기기 (Popup 스택과 무관하게 처리)
        /// </summary>
        public void HideLoadingScreen()
        {
            LoadingScreen.HideLoadingScreen();
        }

        /// <summary>
        /// 현재 활성화된 LoadingScreen 가져오기
        /// </summary>
        public LoadingScreen GetCurrentLoadingScreen()
        {
            LoadingScreen[] loadingScreens = FindObjectsOfType<LoadingScreen>();
            foreach (var loadingScreen in loadingScreens)
            {
                if (loadingScreen != null && loadingScreen.gameObject.activeInHierarchy)
                {
                    return loadingScreen;
                }
            }
            return null;
        }

        #endregion

        #region PopUp Methods (Backward Compatibility)

        /// <summary>
        /// 제네릭 팝업 UI 표시 (기존 호환성을 위한 래퍼)
        /// </summary>
        public T ShowPopUp<T>() where T : BaseUI
        {
            // 동기적 호출은 문제가 있으므로 비동기 버전을 사용하도록 안내
            Debug.LogWarning($"[UIManager] ShowPopUp<T>()는 비동기 작업이므로 ShowPopUpAsync<T>()를 사용하세요.");
            return null;
        }

        /// <summary>
        /// 제네릭 팝업 UI 표시 (비동기 버전)
        /// </summary>
        public void ShowPopUpAsync<T>(System.Action<T> onComplete = null) where T : BaseUI
        {
            string popupName = typeof(T).Name;
            
            // 중복 생성 방지
            if (isCreatingPopup)
            {
                Debug.Log($"[UIManager] 이미 팝업 생성 중이므로 {popupName} 무시합니다. (isCreatingPopup: {isCreatingPopup})");
                Debug.Log($"[UIManager] 호출 스택: {System.Environment.StackTrace}");
                onComplete?.Invoke(null);
                return;
            }
            
            Debug.Log($"[UIManager] {popupName} 팝업 생성 시작 (isCreatingPopup: {isCreatingPopup} -> true)");
            Debug.Log($"[UIManager] 호출 스택: {System.Environment.StackTrace}");
            isCreatingPopup = true;
            StartCoroutine(ShowPopUpAsyncCoroutine<T>(onComplete));
        }
        
        /// <summary>
        /// Panel 타입 UI를 비동기로 표시 (중복 생성 방지)
        /// </summary>
        public void ShowPanelAsync<T>(System.Action<T> onComplete = null) where T : BaseUI
        {
            string panelName = typeof(T).Name;
            
            // 중복 생성 방지
            if (isCreatingPanel)
            {
                Debug.Log($"[UIManager] 이미 패널 생성 중이므로 {panelName} 무시합니다. (isCreatingPanel: {isCreatingPanel})");
                Debug.Log($"[UIManager] 호출 스택: {System.Environment.StackTrace}");
                onComplete?.Invoke(null);
                return;
            }
            
            // 동일한 타입의 패널이 이미 스택에 있는지 확인
            foreach (var existingPanel in panelStack)
            {
                if (existingPanel != null && existingPanel.GetType() == typeof(T))
                {
                    Debug.LogWarning($"[UIManager] 동일한 타입의 패널이 이미 존재합니다: {panelName}");
                    onComplete?.Invoke(existingPanel as T);
                    return;
                }
            }
            
            Debug.Log($"[UIManager] {panelName} 패널 생성 시작 (isCreatingPanel: {isCreatingPanel} -> true)");
            Debug.Log($"[UIManager] 호출 스택: {System.Environment.StackTrace}");
            isCreatingPanel = true;
            StartCoroutine(ShowPanelAsyncCoroutine<T>(onComplete));
        }

        private System.Collections.IEnumerator ShowPopUpAsyncCoroutine<T>(System.Action<T> onComplete) where T : BaseUI
        {
            string prefabName = typeof(T).Name;
            ////Debug.Log($"[UIManager] Addressable에서 {prefabName} 팝업 로드 시작");

            AsyncOperationHandle<GameObject> handle = default;
            
            // Addressable에서 팝업 로드 시도 (KYS 그룹 사용)
            // 다양한 키 시도
            string[] possibleKeys = {
                $"KYS/{prefabName}",
                $"KYS/UIPanel/{prefabName}",
                $"KYS/UI/{prefabName}",
                $"KYS/Prefabs/UI/UIPanel/{prefabName}",
                prefabName,
                $"UI/{prefabName}",
                $"UI/Panel/{prefabName}"
            };

            // 가능한 키들을 순서대로 시도
            foreach (string addressableKey in possibleKeys)
            {
                ////Debug.Log($"[UIManager] 키 '{addressableKey}'로 로드 시도");

                handle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    ////Debug.Log($"[UIManager] 성공한 키: {addressableKey}");
                    break; // 성공하면 루프 종료
                }
                else
                {
                    //Debug.LogWarning($"[UIManager] 키 '{addressableKey}' 실패: {handle.OperationException?.Message}");
                    Addressables.Release(handle);
                }
            }

            // 모든 키 시도 후에도 실패한 경우
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[UIManager] {prefabName} 팝업 로드 실패 - 모든 키 시도 완료");
                onComplete?.Invoke(null);
                isCreatingPopup = false;
                yield break;
            }

            try
            {
                // 명시적으로 GameObject로 캐스팅하여 인스턴스 생성
                GameObject prefabAsset = handle.Result as GameObject;
                if (prefabAsset == null)
                {
                    Debug.LogError($"[UIManager] {prefabName} 프리팹을 GameObject로 캐스팅할 수 없습니다.");
                    onComplete?.Invoke(null);
                    yield break;
                }

                ////Debug.Log($"[UIManager] {prefabName} 프리팹 캐스팅 성공: {prefabAsset.name}");

                // UI 타입에 따라 적절한 Canvas 선택
                Transform targetCanvas = GetCanvasForUI<T>();
                if (targetCanvas == null)
                {
                    Debug.LogError($"[UIManager] {prefabName}에 대한 적절한 Canvas를 찾을 수 없습니다.");
                    onComplete?.Invoke(null);
                    yield break;
                }

                ////Debug.Log($"[UIManager] {prefabName}이 {targetCanvas.name}에 생성됩니다.");
                ////Debug.Log($"[UIManager] targetCanvas 타입: {targetCanvas.GetType().Name}");
                ////Debug.Log($"[UIManager] targetCanvas 부모: {targetCanvas.parent?.name}");
                ////Debug.Log($"[UIManager] targetCanvas가 SafeAreaPanel인가? {targetCanvas.GetComponent<SafeAreaPanel>() != null}");
                ////Debug.Log($"[UIManager] targetCanvas가 Canvas인가? {targetCanvas.GetComponent<Canvas>() != null}");

                // Unity의 기본 Instantiate 사용 (Addressables.InstantiateAsync 대신)
                ////Debug.Log($"[UIManager] {prefabName} 인스턴스 생성 시작... (Canvas: {targetCanvas.name})");
                GameObject uiInstance = Instantiate(prefabAsset, targetCanvas);

                ////Debug.Log($"[UIManager] {prefabName} 인스턴스 생성 완료: {uiInstance.name}");
                ////Debug.Log($"[UIManager] {prefabName} 부모: {uiInstance.transform.parent?.name}");
                ////Debug.Log($"[UIManager] {prefabName} 위치: {uiInstance.transform.position}");
                ////Debug.Log($"[UIManager] {prefabName} 부모가 SafeAreaPanel인가? {uiInstance.transform.parent?.GetComponent<SafeAreaPanel>() != null}");
                ////Debug.Log($"[UIManager] {prefabName} 부모가 Canvas인가? {uiInstance.transform.parent?.GetComponent<Canvas>() != null}");

                T uiComponent = uiInstance.GetComponent<T>();
                if (uiComponent == null)
                {
                    Debug.LogError($"[UIManager] {prefabName}에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
                    Destroy(uiInstance);
                    onComplete?.Invoke(null);
                    yield break;
                }

                // UI 타입에 따라 적절한 메서드 호출
                if (uiComponent.LayerType == UILayerType.Panel)
                {
                    OpenPanel(uiComponent);
                    ////Debug.Log($"[UIManager] {prefabName} 패널 표시 완료");
                }
                else
                {
                    OpenPopup(uiComponent);
                    ////Debug.Log($"[UIManager] {prefabName} 팝업 표시 완료");
                }

                onComplete?.Invoke(uiComponent);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] {prefabName} 팝업 생성 중 예외 발생: {e.Message}");
                onComplete?.Invoke(null);
            }
            finally
            {
                // 항상 플래그 해제 및 리소스 정리
                Debug.Log($"[UIManager] {prefabName} 팝업 생성 완료 - isCreatingPopup 플래그 해제");
                isCreatingPopup = false;
                
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }
        
        /// <summary>
        /// Panel 타입 UI를 비동기로 표시하는 코루틴
        /// </summary>
        private System.Collections.IEnumerator ShowPanelAsyncCoroutine<T>(System.Action<T> onComplete) where T : BaseUI
        {
            string prefabName = typeof(T).Name;
            Debug.Log($"[UIManager] Addressable에서 {prefabName} 패널 로드 시작");

            AsyncOperationHandle<GameObject> handle = default;
            
            // Addressable에서 패널 로드 시도 (KYS 그룹 사용)
            // 다양한 키 시도
            string[] possibleKeys = {
                $"KYS/{prefabName}",
                $"KYS/UIPanel/{prefabName}",
                $"KYS/UI/{prefabName}",
                $"KYS/Prefabs/UI/UIPanel/{prefabName}",
                prefabName,
                $"UI/{prefabName}",
                $"UI/Panel/{prefabName}"
            };

            // 가능한 키들을 순서대로 시도
            foreach (string addressableKey in possibleKeys)
            {
                Debug.Log($"[UIManager] 키 '{addressableKey}'로 패널 로드 시도");

                handle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"[UIManager] 성공한 키: {addressableKey}");
                    break; // 성공하면 루프 종료
                }
                else
                {
                    Debug.LogWarning($"[UIManager] 키 '{addressableKey}' 실패: {handle.OperationException?.Message}");
                    Addressables.Release(handle);
                }
            }

            // 모든 키 시도 후에도 실패한 경우
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[UIManager] {prefabName} 패널 로드 실패 - 모든 키 시도 완료");
                onComplete?.Invoke(null);
                isCreatingPanel = false;
                yield break;
            }

            try
            {
                // 명시적으로 GameObject로 캐스팅하여 인스턴스 생성
                GameObject prefabAsset = handle.Result as GameObject;
                if (prefabAsset == null)
                {
                    Debug.LogError($"[UIManager] {prefabName} 프리팹을 GameObject로 캐스팅할 수 없습니다.");
                    onComplete?.Invoke(null);
                    yield break;
                }

                Debug.Log($"[UIManager] {prefabName} 프리팹 캐스팅 성공: {prefabAsset.name}");

                // UI 타입에 따라 적절한 Canvas 선택
                Transform targetCanvas = GetCanvasForUI<T>();
                if (targetCanvas == null)
                {
                    Debug.LogError($"[UIManager] {prefabName}에 대한 적절한 Canvas를 찾을 수 없습니다.");
                    onComplete?.Invoke(null);
                    yield break;
                }

                Debug.Log($"[UIManager] {prefabName}이 {targetCanvas.name}에 생성됩니다.");

                // Unity의 기본 Instantiate 사용
                GameObject uiInstance = Instantiate(prefabAsset, targetCanvas);
                Debug.Log($"[UIManager] {prefabName} 인스턴스 생성 완료: {uiInstance.name}");

                T uiComponent = uiInstance.GetComponent<T>();
                if (uiComponent == null)
                {
                    Debug.LogError($"[UIManager] {prefabName}에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
                    Destroy(uiInstance);
                    onComplete?.Invoke(null);
                    yield break;
                }

                // Panel 타입이므로 OpenPanel 호출
                OpenPanel(uiComponent);
                
                // 성공적으로 완료된 경우에만 콜백 호출
                onComplete?.Invoke(uiComponent);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] {prefabName} 패널 생성 중 예외 발생: {e.Message}");
                onComplete?.Invoke(null);
            }
            finally
            {
                // 항상 플래그 해제 및 리소스 정리
                Debug.Log($"[UIManager] {prefabName} 패널 생성 완료 - isCreatingPanel 플래그 해제");
                isCreatingPanel = false;
                
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        /// <summary>
        /// 확인 팝업 표시 (기존 호환성)
        /// </summary>
        public CheckPopUp ShowConfirmPopUp(string message, string confirmText = "확인", string cancelText = "취소",
                                          System.Action confirmCallback = null, System.Action cancelCallback = null)
        {
            CheckPopUp result = null;
            ShowConfirmPopUpAsync(message, confirmText, cancelText, confirmCallback, cancelCallback, (popup) => result = popup);
            return result;
        }

        /// <summary>
        /// 간단한 확인 팝업 표시 (기존 호환성)
        /// </summary>
        public CheckPopUp ShowConfirmPopUp(string message, System.Action confirmCallback)
        {
            return ShowConfirmPopUp(message, "확인", "취소", confirmCallback, null);
        }

        /// <summary>
        /// 확인 팝업 표시 (비동기 버전)
        /// </summary>
        public void ShowConfirmPopUpAsync(string message, string confirmText = "확인", string cancelText = "취소",
                                         System.Action confirmCallback = null, System.Action cancelCallback = null,
                                         System.Action<CheckPopUp> onComplete = null)
        {
            ShowPopUpAsync<CheckPopUp>((popup) =>
            {
                if (popup != null)
                {
                    popup.SetMessage(message);
                    popup.SetConfirmText(confirmText);
                    popup.SetCancelText(cancelText);
                    popup.SetConfirmCallback(confirmCallback);
                    popup.SetCancelCallback(cancelCallback);
                }
                onComplete?.Invoke(popup);
            });
        }

        /// <summary>
        /// 간단한 확인 팝업 표시 (비동기 버전)
        /// </summary>
        public void ShowConfirmPopUpAsync(string message, System.Action confirmCallback, System.Action<CheckPopUp> onComplete = null)
        {
            ShowConfirmPopUpAsync(message, "확인", "취소", confirmCallback, null, onComplete);
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// ESC 키 처리
        /// </summary>
        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //Debug.Log("[UIManager] ESC 키 감지됨");

                if (canClose)
                {
                    if (popupStack.Count > 0)
                    {
                        //Debug.Log("[UIManager] 팝업 닫기 시도");
                        ClosePopup();
                    }
                    else if (panelStack.Count > 0)
                    {
                        //Debug.Log("[UIManager] 패널 닫기 시도");
                        ClosePanel();
                    }
                }
                else
                {
                    //Debug.Log("[UIManager] ESC 키 무시됨 - canClose 조건 불만족");
                }
            }
        }

        /// <summary>
        /// 현재 UI가 닫을 수 없는지 확인
        /// </summary>
        private bool IsCurrentUINonClosable()
        {
            if (popupStack.Count > 0)
            {
                BaseUI currentPopup = popupStack.Peek();
                return currentPopup != null && !currentPopup.CanCloseWithESC;
            }

            if (panelStack.Count > 0)
            {
                BaseUI currentPanel = panelStack.Peek();
                return currentPanel != null && !currentPanel.CanCloseWithESC;
            }

            return false;
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// 씬 로드 시 처리
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //Debug.Log($"[UIManager] 씬 로드됨: {scene.name}");

            // 씬 전환 시 모든 UI 정리
            CleanAllUI();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 모든 패널 닫기
        /// </summary>
        public void CloseAllPanels()
        {
            while (panelStack.Count > 0)
            {
                ClosePanel();
            }
        }

        /// <summary>
        /// 모든 팝업 닫기
        /// </summary>
        public void CloseAllPopups()
        {
            while (popupStack.Count > 0)
            {
                ClosePopup();
            }
        }

        /// <summary>
        /// 모든 UI 정리
        /// </summary>
        public void CleanAllUI()
        {
            //Debug.Log("[UIManager] 모든 UI 정리 시작");

            CloseAllPanels();
            CloseAllPopups();

            //Debug.Log("[UIManager] 모든 UI가 정리되었습니다.");
        }

        /// <summary>
        /// 스택 상태 디버그 출력
        /// </summary>
        public void DebugStackStatus()
        {
            ////Debug.Log($"[UIManager] === 스택 상태 ===");
            ////Debug.Log($"[UIManager] Panel 스택 크기: {panelStack.Count}");
            ////Debug.Log($"[UIManager] Popup 스택 크기: {popupStack.Count}");

            if (panelStack.Count > 0)
            {
                //Debug.Log($"[UIManager] 최상위 Panel: {panelStack.Peek().name}");
            }

            if (popupStack.Count > 0)
            {
                //Debug.Log($"[UIManager] 최상위 Popup: {popupStack.Peek().name}");
            }
        }

        /// <summary>
        /// 이전 UI들의 터치를 차단
        /// </summary>
        private void DisablePreviousUITouch()
        {
            // Panel은 그대로 두고 HUD만 터치 차단 (Panel은 뒤에서 보이도록)
            if (hudCanvas != null)
            {
                DisableCanvasTouch(hudCanvas);
            }
        }

        /// <summary>
        /// 이전 UI들의 터치를 복원
        /// </summary>
        private void RestorePreviousUITouch()
        {
            // HUD Canvas의 모든 UI 터치 복원
            if (hudCanvas != null)
            {
                EnableCanvasTouch(hudCanvas);
            }
        }

        /// <summary>
        /// 특정 UI의 터치를 차단
        /// </summary>
        private void DisableUITouch(BaseUI ui)
        {
            if (ui == null) return;

            CanvasGroup canvasGroup = ui.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = ui.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            //Debug.Log($"[UIManager] UI 터치 차단: {ui.name}");
        }

        /// <summary>
        /// 특정 UI의 터치를 복원
        /// </summary>
        private void EnableUITouch(BaseUI ui)
        {
            if (ui == null) return;

            CanvasGroup canvasGroup = ui.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                //Debug.Log($"[UIManager] UI 터치 복원: {ui.name}");
            }
        }

        /// <summary>
        /// Canvas의 모든 UI 터치를 차단
        /// </summary>
        private void DisableCanvasTouch(Canvas canvas)
        {
            if (canvas == null) return;

            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            //Debug.Log($"[UIManager] Canvas 터치 차단: {canvas.name}");
        }

        /// <summary>
        /// Canvas의 모든 UI 터치를 복원
        /// </summary>
        private void EnableCanvasTouch(Canvas canvas)
        {
            if (canvas == null) return;

            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                //Debug.Log($"[UIManager] Canvas 터치 복원: {canvas.name}");
            }
        }

        /// <summary>
        /// HUD 프리팹을 로드하고 생성
        /// </summary>
        public async System.Threading.Tasks.Task<T> CreateHUDAsync<T>(string addressableKey) where T : BaseUI
        {
            
            if (hudCanvas == null)
            {
                Debug.LogError("[UIManager] HUD Canvas가 초기화되지 않았습니다.");
                isCreatingHUD = false;
                return null;
            }

            try
            {
                // SafeAreaPanel을 부모로 사용
                Transform safeAreaParent = GetSafeAreaParentForHUD();
                if (safeAreaParent == null)
                {
                    Debug.LogError("[UIManager] HUD용 SafeAreaPanel을 찾을 수 없습니다.");
                    return null;
                }

                // HUD 프리팹을 SafeAreaPanel 자식으로 직접 생성
                var handle = Addressables.InstantiateAsync(addressableKey, safeAreaParent);
                await handle.Task;

                GameObject hudObject = handle.Result;
                T hudComponent = hudObject.GetComponent<T>();

                if (hudComponent == null)
                {
                    Debug.LogError($"[UIManager] HUD 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다: {addressableKey}");
                    Addressables.Release(handle);
                    return null;
                }

                // HUD 초기화
                hudComponent.Initialize();

                // Addressable 핸들 저장
                addressableHandles[addressableKey] = handle;
                instantiatedUIs[addressableKey] = hudObject;

                //Debug.Log($"[UIManager] HUD 생성 완료: {typeof(T).Name} from {addressableKey} (SafeAreaPanel 자식으로 생성)");
                return hudComponent;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] HUD 생성 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// HUD 프리팹을 로드하고 생성 (AssetReference 사용)
        /// </summary>
        public async System.Threading.Tasks.Task<T> CreateHUDAsync<T>(AssetReferenceGameObject assetReference) where T : BaseUI
        {
            
            if (hudCanvas == null)
            {
                Debug.LogError("[UIManager] HUD Canvas가 초기화되지 않았습니다.");
                isCreatingHUD = false;
                return null;
            }

            try
            {
                // SafeAreaPanel을 부모로 사용
                Transform safeAreaParent = GetSafeAreaParentForHUD();
                if (safeAreaParent == null)
                {
                    Debug.LogError("[UIManager] HUD용 SafeAreaPanel을 찾을 수 없습니다.");
                    return null;
                }

                // HUD 프리팹을 SafeAreaPanel 자식으로 직접 생성
                var handle = assetReference.InstantiateAsync(safeAreaParent);
                await handle.Task;

                GameObject hudObject = handle.Result;
                T hudComponent = hudObject.GetComponent<T>();

                if (hudComponent == null)
                {
                    Debug.LogError($"[UIManager] HUD 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.");
                    Addressables.Release(handle);
                    return null;
                }

                // HUD 초기화
                hudComponent.Initialize();

                // Addressable 핸들 저장
                string key = assetReference.RuntimeKey.ToString();
                addressableHandles[key] = handle;
                instantiatedUIs[key] = hudObject;

                //Debug.Log($"[UIManager] HUD 생성 완료: {typeof(T).Name} (SafeAreaPanel 자식으로 생성)");
                return hudComponent;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] HUD 생성 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 모든 InfoHUD 닫기
        /// </summary>
        /// <returns>닫은 HUD가 있었는지 여부</returns>
        public bool CloseAllInfoHUDs()
        {
            if (hudCanvas == null) return false;
            
            var existingHUDs = GetUIsByLayer(UILayerType.HUD);
            bool foundAndClosed = false;
            
            foreach (var ui in existingHUDs)
            {
                if (ui is TouchInfoHUD)
                {
                    if (ui.gameObject.activeInHierarchy)
                    {
                        Debug.Log("[UIManager] InfoHUD 닫기");
                        ui.Hide();
                        foundAndClosed = true;
                    }
                }
            }
            
            if (!foundAndClosed)
            {
                Debug.Log("[UIManager] 닫을 InfoHUD가 없음");
            }
            
            return foundAndClosed;
        }

        /// <summary>
        /// 모든 InfoHUD 완전 제거 (비활성화된 것도 포함)
        /// </summary>
        public bool DestroyAllInfoHUDs()
        {
            if (hudCanvas == null) return false;
            
            var existingHUDs = GetUIsByLayer(UILayerType.HUD);
            bool foundAndDestroyed = false;
            
            foreach (var ui in existingHUDs)
            {
                if (ui is TouchInfoHUD)
                {
                    Debug.Log("[UIManager] InfoHUD 완전 제거");
                    UnregisterUI(ui);
                    Destroy(ui.gameObject);
                    foundAndDestroyed = true;
                }
            }
            
            if (!foundAndDestroyed)
            {
                Debug.Log("[UIManager] 제거할 InfoHUD가 없음");
            }
            
            return foundAndDestroyed;
        }

        /// <summary>
        /// 단일 InfoHUD 표시 (중복 방지)
        /// </summary>
        public async System.Threading.Tasks.Task<T> ShowSingleInfoHUDAsync<T>(Vector2 screenPosition, string title, string description, Sprite icon, System.Action<T> onComplete = null) where T : BaseUI
        {
            // 이미 생성 중이면 무시
            if (isCreatingHUD)
            {
                Debug.Log("[UIManager] 이미 InfoHUD 생성 중이므로 무시합니다.");
                return null;
            }
            
            // 기존 InfoHUD 완전 제거 (비활성화된 것도 포함)
            DestroyAllInfoHUDs();
            
            Debug.Log("[UIManager] 새로운 InfoHUD 생성 시작");
            isCreatingHUD = true;
            
            try
            {
                // 다양한 Addressable 키 시도
                string[] possibleKeys = {
                    "KYS/TouchInfoHUD",
                    "KYS/UIHUD/TouchInfoHUD",
                    "KYS/Prefabs/UI/UIHUD/TouchInfoHUD",
                    "TouchInfoHUD",
                    "UI/TouchInfoHUD"
                };
                
                // 순차적으로 키들을 시도
                T result = await TryCreateInfoHUDWithKeys<T>(possibleKeys, 0, screenPosition, title, description, icon);
                
                if (result != null)
                {
                    // TouchInfoHUD 특정 메서드 호출
                    if (result is TouchInfoHUD touchInfoHUD)
                    {
                        touchInfoHUD.SetHUDPosition(screenPosition);
                        touchInfoHUD.SetInfo(title, description, icon);
                        touchInfoHUD.Show();
                    }
                    
                    onComplete?.Invoke(result);
                }
                
                return result;
            }
            finally
            {
                isCreatingHUD = false;
            }
        }
        
        /// <summary>
        /// 여러 키를 순차적으로 시도하여 InfoHUD 생성
        /// </summary>
        private async System.Threading.Tasks.Task<T> TryCreateInfoHUDWithKeys<T>(string[] keys, int index, Vector2 screenPosition, string title, string description, Sprite icon) where T : BaseUI
        {
            if (index >= keys.Length)
            {
                Debug.LogError("[UIManager] 모든 키로 InfoHUD 생성 실패");
                return null;
            }
            
            string currentKey = keys[index];
            Debug.Log($"[UIManager] 키 '{currentKey}'로 InfoHUD 생성 시도 ({index + 1}/{keys.Length})");
            
            try
            {
                T result = await CreateHUDAsync<T>(currentKey);
                if (result != null)
                {
                    Debug.Log($"[UIManager] InfoHUD 생성 성공: {currentKey}");
                    return result;
                }
                else
                {
                    Debug.LogWarning($"[UIManager] 키 '{currentKey}'로 생성 실패, 다음 키 시도");
                    // 다음 키로 재시도
                    return await TryCreateInfoHUDWithKeys<T>(keys, index + 1, screenPosition, title, description, icon);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] 키 '{currentKey}'로 InfoHUD 생성 중 오류: {e.Message}");
                // 다음 키로 재시도
                return await TryCreateInfoHUDWithKeys<T>(keys, index + 1, screenPosition, title, description, icon);
            }
        }

        /// <summary>
        /// HUD Canvas의 모든 UI 요소들을 숨김 상태로 초기화
        /// </summary>
        public void HideAllHUDElements()
        {
            if (hudCanvas == null) return;

            // HUD Canvas의 모든 BaseUI 컴포넌트들을 찾아서 숨김
            BaseUI[] hudUIs = hudCanvas.GetComponentsInChildren<BaseUI>(true);
            foreach (var hudUI in hudUIs)
            {
                if (hudUI != null && hudUI.LayerType == UILayerType.HUD)
                {
                    hudUI.gameObject.SetActive(false);
                    //Debug.Log($"[UIManager] HUD UI 숨김: {hudUI.name}");
                }
            }

            // 일반적인 UI 요소들도 숨김 (BaseUI를 상속받지 않은 경우)
            CanvasGroup[] canvasGroups = hudCanvas.GetComponentsInChildren<CanvasGroup>(true);
            foreach (var canvasGroup in canvasGroups)
            {
                if (canvasGroup != null && canvasGroup.gameObject != hudCanvas.gameObject)
                {
                    canvasGroup.gameObject.SetActive(false);
                    //Debug.Log($"[UIManager] HUD 요소 숨김: {canvasGroup.name}");
                }
            }

            ////Debug.Log($"[UIManager] HUD Canvas 초기화 완료 - 모든 HUD 요소가 숨김 상태로 설정됨");
        }

        /// <summary>
        /// HUD Canvas의 모든 UI 요소들을 표시
        /// </summary>
        public void ShowAllHUDElements()
        {
            if (hudCanvas == null) return;

            // HUD Canvas의 모든 BaseUI 컴포넌트들을 찾아서 표시
            BaseUI[] hudUIs = hudCanvas.GetComponentsInChildren<BaseUI>(true);
            foreach (var hudUI in hudUIs)
            {
                if (hudUI != null && hudUI.LayerType == UILayerType.HUD)
                {
                    hudUI.gameObject.SetActive(true);
                    //Debug.Log($"[UIManager] HUD UI 표시: {hudUI.name}");
                }
            }

            // 일반적인 UI 요소들도 표시 (BaseUI를 상속받지 않은 경우)
            CanvasGroup[] canvasGroups = hudCanvas.GetComponentsInChildren<CanvasGroup>(true);
            foreach (var canvasGroup in canvasGroups)
            {
                if (canvasGroup != null && canvasGroup.gameObject != hudCanvas.gameObject)
                {
                    canvasGroup.gameObject.SetActive(true);
                    //Debug.Log($"[UIManager] HUD 요소 표시: {canvasGroup.name}");
                }
            }

            //Debug.Log($"[UIManager] HUD Canvas 표시 완료 - 모든 HUD 요소가 표시됨");
        }

        /// <summary>
        /// 특정 HUD UI만 표시
        /// </summary>
        public void ShowHUDUI<T>() where T : BaseUI
        {
            if (hudCanvas == null) return;

            // SafeAreaPanel 아래에서 HUD UI 찾기
            Transform safeAreaParent = GetSafeAreaParentForHUD();
            T hudUI = null;

            if (safeAreaParent != null)
            {
                hudUI = safeAreaParent.GetComponentInChildren<T>(true);
            }

            // SafeAreaPanel에서 찾지 못한 경우 전체 HUD Canvas에서 검색
            if (hudUI == null)
            {
                hudUI = hudCanvas.GetComponentInChildren<T>(true);
            }

            if (hudUI != null)
            {
                hudUI.gameObject.SetActive(true);
                //Debug.Log($"[UIManager] HUD UI 표시: {hudUI.name}");
            }
            else
            {
                Debug.LogWarning($"[UIManager] HUD UI를 찾을 수 없음: {typeof(T).Name}");
            }
        }

        /// <summary>
        /// 특정 HUD UI만 숨김
        /// </summary>
        public void HideHUDUI<T>() where T : BaseUI
        {
            if (hudCanvas == null) return;

            // SafeAreaPanel 아래에서 HUD UI 찾기
            Transform safeAreaParent = GetSafeAreaParentForHUD();
            T hudUI = null;

            if (safeAreaParent != null)
            {
                hudUI = safeAreaParent.GetComponentInChildren<T>(true);
            }

            // SafeAreaPanel에서 찾지 못한 경우 전체 HUD Canvas에서 검색
            if (hudUI == null)
            {
                hudUI = hudCanvas.GetComponentInChildren<T>(true);
            }

            if (hudUI != null)
            {
                hudUI.gameObject.SetActive(false);
                //Debug.Log($"[UIManager] HUD UI 숨김: {hudUI.name}");
            }
            else
            {
                Debug.LogWarning($"[UIManager] HUD UI를 찾을 수 없음: {typeof(T).Name}");
            }
        }

        #endregion
    }
}

