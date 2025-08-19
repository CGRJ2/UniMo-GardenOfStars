using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using KYS.UI;
using KYS.UI.MVP;
#if DOTWEEN_AVAILABLE
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
        
        // UI 상태 관리
        public static int selectIndexUI { get; set; } = 0;
        public static bool canClosePopUp = true;
        bool canClose => (panelStack.Count > 0 || popupStack.Count > 0) && 
                        !Util.escPressed && canClosePopUp && !IsCurrentUINonClosable();

        #region Properties

        public Canvas HUDCanvas => hudCanvas;
        public Canvas PanelCanvas => panelCanvas;
        public Canvas PopupCanvas => popupCanvas;
        public Canvas LoadingCanvas => loadingCanvas;

        #endregion

        #region Initialization

        private async void Awake()
        {
            SingletonInit();
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
                Debug.Log("[UIManager] Addressable Canvas 초기화 시작");
                
                // HUD Canvas 로드
                if (hudCanvasReference != null && hudCanvasReference.RuntimeKeyIsValid())
                {
                    var hudHandle = hudCanvasReference.InstantiateAsync();
                    await hudHandle.Task;
                    hudCanvas = hudHandle.Result.GetComponent<Canvas>();
                    addressableHandles["HUDCanvas"] = hudHandle;
                    DontDestroyOnLoad(hudHandle.Result);
                }
                
                // Panel Canvas 로드
                if (panelCanvasReference != null && panelCanvasReference.RuntimeKeyIsValid())
                {
                    var panelHandle = panelCanvasReference.InstantiateAsync();
                    await panelHandle.Task;
                    panelCanvas = panelHandle.Result.GetComponent<Canvas>();
                    addressableHandles["PanelCanvas"] = panelHandle;
                    DontDestroyOnLoad(panelHandle.Result);
                }
                
                // Popup Canvas 로드
                if (popupCanvasReference != null && popupCanvasReference.RuntimeKeyIsValid())
                {
                    var popupHandle = popupCanvasReference.InstantiateAsync();
                    await popupHandle.Task;
                    popupCanvas = popupHandle.Result.GetComponent<Canvas>();
                    addressableHandles["PopupCanvas"] = popupHandle;
                    DontDestroyOnLoad(popupHandle.Result);
                }
                
                // Loading Canvas 로드
                if (loadingCanvasReference != null && loadingCanvasReference.RuntimeKeyIsValid())
                {
                    var loadingHandle = loadingCanvasReference.InstantiateAsync();
                    await loadingHandle.Task;
                    loadingCanvas = loadingHandle.Result.GetComponent<Canvas>();
                    addressableHandles["LoadingCanvas"] = loadingHandle;
                    DontDestroyOnLoad(loadingHandle.Result);
                }
                
                ApplySafeAreaToCanvases();
                Debug.Log("[UIManager] Addressable Canvas 초기화 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] Canvas 초기화 중 오류: {e.Message}");
            }
        }

        private void ApplySafeAreaToCanvases()
        {
            var safeAreaManager = FindObjectOfType<SafeAreaManager>();
            if (safeAreaManager != null)
            {
                if (hudCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(hudCanvas);
                if (panelCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(panelCanvas);
                if (popupCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(popupCanvas);
                if (loadingCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(loadingCanvas);
            }
        }

        private void InitializeLayerUIs()
        {
            foreach (UILayerType layerType in System.Enum.GetValues(typeof(UILayerType)))
            {
                layerUIs[layerType] = new List<BaseUI>();
            }
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
                Debug.Log($"[UIManager] UI 로드 시작: {addressableKey}");
                
                // 이미 로드된 UI가 있는지 확인
                if (instantiatedUIs.ContainsKey(addressableKey))
                {
                    GameObject existingUI = instantiatedUIs[addressableKey];
                    if (existingUI != null)
                    {
                        T existingComponent = existingUI.GetComponent<T>();
                        if (existingComponent != null)
                        {
                            Debug.Log($"[UIManager] 기존 UI 반환: {addressableKey}");
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
                
                Debug.Log($"[UIManager] UI 로드 완료: {addressableKey}");
                return uiComponent;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI 로드 중 오류: {addressableKey}, {e.Message}");
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
                if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                {
                    Debug.LogError("[UIManager] 유효하지 않은 AssetReference입니다.");
                    return null;
                }
                
                string key = assetReference.RuntimeKey.ToString();
                Debug.Log($"[UIManager] AssetReference로 UI 로드 시작: {key}");
                
                // 이미 로드된 UI가 있는지 확인
                if (instantiatedUIs.ContainsKey(key))
                {
                    GameObject existingUI = instantiatedUIs[key];
                    if (existingUI != null)
                    {
                        T existingComponent = existingUI.GetComponent<T>();
                        if (existingComponent != null)
                        {
                            Debug.Log($"[UIManager] 기존 UI 반환: {key}");
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
                
                Debug.Log($"[UIManager] UI 로드 완료: {key}");
                return uiComponent;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI 로드 중 오류: {e.Message}");
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
                Debug.Log($"[UIManager] 라벨로 UI 로드 시작: {label}");
                
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
                Debug.Log($"[UIManager] 라벨 로드 완료: {label}, {loadedUIs.Count}개");
                return loadedUIs;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] 라벨 로드 중 오류: {label}, {e.Message}");
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
                Debug.Log($"[UIManager] UI 미리 로드 시작: {addressableKey}");
                
                var handle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
                await handle.Task;
                
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    addressableHandles[addressableKey] = handle;
                    Debug.Log($"[UIManager] UI 미리 로드 완료: {addressableKey}");
                }
                else
                {
                    Debug.LogError($"[UIManager] UI 미리 로드 실패: {addressableKey}");
                    Addressables.Release(handle);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI 미리 로드 중 오류: {addressableKey}, {e.Message}");
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
                
                Debug.Log($"[UIManager] UI 해제 완료: {addressableKey}");
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
                Debug.Log("[UIManager] 모든 Addressable 리소스 해제 시작");
                
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
                
                Debug.Log("[UIManager] 모든 Addressable 리소스 해제 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] Addressable 리소스 해제 중 오류: {e.Message}");
            }
        }

        #endregion

        #region Canvas Management

        private Transform GetCanvasForUI<T>() where T : BaseUI
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
            
            switch (layerType)
            {
                case UILayerType.HUD:
                    return hudCanvas?.transform;
                case UILayerType.Panel:
                    return panelCanvas?.transform;
                case UILayerType.Popup:
                    return popupCanvas?.transform;
                case UILayerType.Loading:
                    return loadingCanvas?.transform;
                default:
                    return panelCanvas?.transform;
            }
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
                Debug.Log($"[UIManager] UI 등록: {ui.name} -> {layerType}");
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
                Debug.Log($"[UIManager] UI 등록 해제: {ui.name} -> {layerType}");
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

            // 이전 패널 처리 (숨김)
            if (panelStack.Count > 0)
            {
                BaseUI previousPanel = panelStack.Peek();
                if (previousPanel != null && panel.HidePreviousUI)
                {
                    previousPanel.Hide();
                }
            }

            // 새 패널을 스택에 추가
            panelStack.Push(panel);
            panel.Show();

            Debug.Log($"[UIManager] 패널 열기: {panel.name}");
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
                currentPanel.Hide();
                Debug.Log($"[UIManager] 패널 닫기: {currentPanel.name}");
            }

            // 이전 패널 다시 표시
            if (panelStack.Count > 0)
            {
                BaseUI previousPanel = panelStack.Peek();
                if (previousPanel != null)
                {
                    previousPanel.Show();
                }
            }
        }

        #endregion

        #region Popup Management

        /// <summary>
        /// 팝업 열기
        /// </summary>
        public void OpenPopup(BaseUI popup)
        {
            if (popup == null) return;

            // 이전 팝업 처리 (숨김)
            if (popupStack.Count > 0)
            {
                BaseUI previousPopup = popupStack.Peek();
                if (previousPopup != null && popup.HidePreviousUI)
                {
                    previousPopup.Hide();
                }
            }

            // 새 팝업을 스택에 추가
            popupStack.Push(popup);
            popup.Show();

            Debug.Log($"[UIManager] 팝업 열기: {popup.name}");
        }

        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public void ClosePopup()
        {
            if (popupStack.Count == 0) return;

            BaseUI currentPopup = popupStack.Pop();
            if (currentPopup != null)
            {
                currentPopup.Hide();
                Debug.Log($"[UIManager] 팝업 닫기: {currentPopup.name}");
            }

            // 이전 팝업 다시 표시
            if (popupStack.Count > 0)
            {
                BaseUI previousPopup = popupStack.Peek();
                if (previousPopup != null)
                {
                    previousPopup.Show();
                }
            }
        }

        #endregion

        #region PopUp Methods (Backward Compatibility)

        /// <summary>
        /// 제네릭 팝업 UI 표시 (기존 호환성을 위한 래퍼)
        /// </summary>
        public T ShowPopUp<T>() where T : BaseUI
        {
            T result = null;
            ShowPopUpAsync<T>((instance) => result = instance);
            return result;
        }

        /// <summary>
        /// 제네릭 팝업 UI 표시 (비동기 버전)
        /// </summary>
        public void ShowPopUpAsync<T>(System.Action<T> onComplete = null) where T : BaseUI
        {
            StartCoroutine(ShowPopUpAsyncCoroutine<T>(onComplete));
        }

        private System.Collections.IEnumerator ShowPopUpAsyncCoroutine<T>(System.Action<T> onComplete) where T : BaseUI
        {
            string prefabName = typeof(T).Name;
            Debug.Log($"[UIManager] Addressable에서 {prefabName} 팝업 로드 시작");
            
            // Addressable에서 팝업 로드 시도
            string addressableKey = $"UI/Popup/{prefabName}";
            
            AsyncOperationHandle<GameObject> handle = default;
            AsyncOperationHandle<GameObject> instanceHandle = default;
            
            try
            {
                handle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] {prefabName} 팝업 로드 중 오류: {e.Message}");
                onComplete?.Invoke(null);
                yield break;
            }
            
            yield return handle;
            
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[UIManager] {prefabName} 팝업 로드 실패");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            try
            {
                instanceHandle = Addressables.InstantiateAsync(handle.Result, popupCanvas.transform);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] {prefabName} 팝업 인스턴스 생성 중 오류: {e.Message}");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            yield return instanceHandle;
            
            if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[UIManager] {prefabName} 팝업 인스턴스 생성 실패");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            T popupInstance = instanceHandle.Result.GetComponent<T>();
            if (popupInstance == null)
            {
                Debug.LogError($"[UIManager] {prefabName}에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
                Addressables.ReleaseInstance(instanceHandle.Result);
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            OpenPopup(popupInstance);
            onComplete?.Invoke(popupInstance);
            Debug.Log($"[UIManager] {prefabName} 팝업 표시 완료");
            
            Addressables.Release(handle);
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
            StartCoroutine(ShowConfirmPopUpAsyncCoroutine(message, confirmText, cancelText, confirmCallback, cancelCallback, onComplete));
        }

        /// <summary>
        /// 간단한 확인 팝업 표시 (비동기 버전)
        /// </summary>
        public void ShowConfirmPopUpAsync(string message, System.Action confirmCallback, System.Action<CheckPopUp> onComplete = null)
        {
            ShowConfirmPopUpAsync(message, "확인", "취소", confirmCallback, null, onComplete);
        }

        private System.Collections.IEnumerator ShowConfirmPopUpAsyncCoroutine(string message, string confirmText, string cancelText,
                                                                             System.Action confirmCallback, System.Action cancelCallback,
                                                                             System.Action<CheckPopUp> onComplete)
        {
            Debug.Log("[UIManager] 확인 팝업 로드 시작");
            
            AsyncOperationHandle<GameObject> handle = default;
            AsyncOperationHandle<GameObject> instanceHandle = default;
            
            try
            {
                // CheckPopUp 프리팹 로드
                handle = Addressables.LoadAssetAsync<GameObject>("UI/Popup/CheckPopUp");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] CheckPopUp 로드 중 오류: {e.Message}");
                onComplete?.Invoke(null);
                yield break;
            }
            
            yield return handle;
            
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[UIManager] CheckPopUp 로드 실패");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            try
            {
                instanceHandle = Addressables.InstantiateAsync(handle.Result, popupCanvas.transform);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] CheckPopUp 인스턴스 생성 중 오류: {e.Message}");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            yield return instanceHandle;
            
            if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[UIManager] CheckPopUp 인스턴스 생성 실패");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            CheckPopUp popupInstance = instanceHandle.Result.GetComponent<CheckPopUp>();
            if (popupInstance == null)
            {
                Debug.LogError("[UIManager] CheckPopUp 컴포넌트를 찾을 수 없습니다.");
                Addressables.ReleaseInstance(instanceHandle.Result);
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            // 팝업 설정
            popupInstance.SetMessage(message);
            popupInstance.SetConfirmText(confirmText);
            popupInstance.SetCancelText(cancelText);
            
            if (confirmCallback != null)
                popupInstance.OnConfirmClicked += confirmCallback;
            if (cancelCallback != null)
                popupInstance.OnCancelClicked += cancelCallback;
            
            OpenPopup(popupInstance);
            onComplete?.Invoke(popupInstance);
            Debug.Log("[UIManager] 확인 팝업 표시 완료");
            
            Addressables.Release(handle);
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
                Util.escPressed = true;
                
                if (canClose)
                {
                    if (popupStack.Count > 0)
                    {
                        ClosePopup();
                    }
                    else if (panelStack.Count > 0)
                    {
                        ClosePanel();
                    }
                }
            }
            else
            {
                Util.escPressed = false;
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
            Debug.Log($"[UIManager] 씬 로드됨: {scene.name}");
            
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
            Debug.Log("[UIManager] 모든 UI 정리 시작");
            
            CloseAllPanels();
            CloseAllPopups();
            
            Debug.Log("[UIManager] 모든 UI가 정리되었습니다.");
        }

        #endregion
    }
}
