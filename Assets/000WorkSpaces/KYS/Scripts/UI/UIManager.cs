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
    /// Addressable ?? ??? UI ???
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Addressable Canvas References")]
        [SerializeField] private AssetReferenceGameObject hudCanvasReference;
        [SerializeField] private AssetReferenceGameObject panelCanvasReference;
        [SerializeField] private AssetReferenceGameObject popupCanvasReference;
        [SerializeField] private AssetReferenceGameObject loadingCanvasReference;
        
        [Header("Addressable UI Settings")]
        [SerializeField] private string uiPrefabLabel = "UI";
        [SerializeField] private string hudPrefabLabel = "UI_HUD";
        [SerializeField] private string panelPrefabLabel = "UI_Panel";
        [SerializeField] private string popupPrefabLabel = "UI_Popup";
        
        // Canvas ????
        private Canvas hudCanvas;
        private Canvas panelCanvas;
        private Canvas popupCanvas;
        private Canvas loadingCanvas;
        
        // Layer? UI ??
        private Dictionary<UILayerType, List<BaseUI>> layerUIs = new Dictionary<UILayerType, List<BaseUI>>();
        
        // Stack ?? UI ??
        private Stack<BaseUI> panelStack = new Stack<BaseUI>();
        private Stack<BaseUI> popupStack = new Stack<BaseUI>();
        
        // Addressable ?? ??
        private Dictionary<string, AsyncOperationHandle> addressableHandles = new Dictionary<string, AsyncOperationHandle>();
        private Dictionary<string, GameObject> instantiatedUIs = new Dictionary<string, GameObject>();
        
        // UI ?? ??
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
        /// Addressable Canvas ???
        /// </summary>
        private async System.Threading.Tasks.Task InitializeAddressableCanvases()
        {
            try
            {
                Debug.Log("[UIManager] Addressable Canvas ??? ??");
                
                // HUD Canvas ??
                if (hudCanvasReference != null && hudCanvasReference.RuntimeKeyIsValid())
                {
                    var hudHandle = hudCanvasReference.InstantiateAsync();
                    await hudHandle.Task;
                    hudCanvas = hudHandle.Result.GetComponent<Canvas>();
                    addressableHandles["HUDCanvas"] = hudHandle;
                    DontDestroyOnLoad(hudHandle.Result);
                }
                
                // Panel Canvas ??
                if (panelCanvasReference != null && panelCanvasReference.RuntimeKeyIsValid())
                {
                    var panelHandle = panelCanvasReference.InstantiateAsync();
                    await panelHandle.Task;
                    panelCanvas = panelHandle.Result.GetComponent<Canvas>();
                    addressableHandles["PanelCanvas"] = panelHandle;
                    DontDestroyOnLoad(panelHandle.Result);
                }
                
                // Popup Canvas ??
                if (popupCanvasReference != null && popupCanvasReference.RuntimeKeyIsValid())
                {
                    var popupHandle = popupCanvasReference.InstantiateAsync();
                    await popupHandle.Task;
                    popupCanvas = popupHandle.Result.GetComponent<Canvas>();
                    addressableHandles["PopupCanvas"] = popupHandle;
                    DontDestroyOnLoad(popupHandle.Result);
                }
                
                // Loading Canvas ??
                if (loadingCanvasReference != null && loadingCanvasReference.RuntimeKeyIsValid())
                {
                    var loadingHandle = loadingCanvasReference.InstantiateAsync();
                    await loadingHandle.Task;
                    loadingCanvas = loadingHandle.Result.GetComponent<Canvas>();
                    addressableHandles["LoadingCanvas"] = loadingHandle;
                    DontDestroyOnLoad(loadingHandle.Result);
                }
                
                ApplySafeAreaToCanvases();
                Debug.Log("[UIManager] Addressable Canvas ??? ??");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] Canvas ??? ? ??: {e.Message}");
            }
        }

        /// <summary>
        /// SafeArea? ?? Canvas? ??
        /// </summary>
        private void ApplySafeAreaToCanvases()
        {
            try
            {
                var safeAreaManager = FindObjectOfType<SafeAreaManager>();
                if (safeAreaManager != null)
                {
                    if (hudCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(hudCanvas);
                    if (panelCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(panelCanvas);
                    if (popupCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(popupCanvas);
                    if (loadingCanvas != null) safeAreaManager.ApplySafeAreaToCanvas(loadingCanvas);
                    Debug.Log("[UIManager] SafeArea ?? ??");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[UIManager] SafeArea ?? ? ??: {e.Message}");
            }
        }

        /// <summary>
        /// Layer? UI ???
        /// </summary>
        private void InitializeLayerUIs()
        {
            layerUIs.Clear();
            foreach (UILayerType layerType in System.Enum.GetValues(typeof(UILayerType)))
            {
                layerUIs[layerType] = new List<BaseUI>();
            }
        }

        #endregion

        #region Addressable UI Loading

        /// <summary>
        /// ??? ?? UI ??
        /// </summary>
        public async System.Threading.Tasks.Task<T> LoadUIAsync<T>(string addressableKey, Transform parent = null) where T : BaseUI
        {
            try
            {
                if (string.IsNullOrEmpty(addressableKey))
                {
                    Debug.LogError("[UIManager] Addressable ?? null??? ??????.");
                    return null;
                }

                // ?? ??? UI? ??? ??
                if (instantiatedUIs.ContainsKey(addressableKey))
                {
                    GameObject existingUI = instantiatedUIs[addressableKey];
                    if (existingUI != null)
                    {
                        return existingUI.GetComponent<T>();
                    }
                }

                // UI ??
                var handle = Addressables.InstantiateAsync(addressableKey);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject uiObject = handle.Result;
                    T uiComponent = uiObject.GetComponent<T>();

                    if (uiComponent == null)
                    {
                        Debug.LogError($"[UIManager] {addressableKey}?? {typeof(T).Name} ????? ?? ? ????.");
                        Addressables.ReleaseInstance(handle);
                        return null;
                    }

                    // ?? ??
                    Transform targetParent = parent ?? GetCanvasForUI<T>();
                    if (targetParent != null)
                    {
                        uiObject.transform.SetParent(targetParent, false);
                    }

                    // ??? ??
                    addressableHandles[addressableKey] = handle;
                    instantiatedUIs[addressableKey] = uiObject;
                    RegisterUI(uiComponent);

                    Debug.Log($"[UIManager] UI ?? ??: {addressableKey}");
                    return uiComponent;
                }
                else
                {
                    Debug.LogError($"[UIManager] UI ?? ??: {addressableKey}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI ?? ? ??: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// AssetReference? UI ??
        /// </summary>
        public async System.Threading.Tasks.Task<T> LoadUIAsync<T>(AssetReferenceGameObject assetReference, Transform parent = null) where T : BaseUI
        {
            try
            {
                if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                {
                    Debug.LogError("[UIManager] ???? ?? AssetReference???.");
                    return null;
                }

                string key = assetReference.RuntimeKey.ToString();

                // ?? ??? UI? ??? ??
                if (instantiatedUIs.ContainsKey(key))
                {
                    GameObject existingUI = instantiatedUIs[key];
                    if (existingUI != null)
                    {
                        return existingUI.GetComponent<T>();
                    }
                }

                // UI ??
                var handle = assetReference.InstantiateAsync();
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject uiObject = handle.Result;
                    T uiComponent = uiObject.GetComponent<T>();

                    if (uiComponent == null)
                    {
                        Debug.LogError($"[UIManager] {key}?? {typeof(T).Name} ????? ?? ? ????.");
                        Addressables.ReleaseInstance(handle);
                        return null;
                    }

                    // ?? ??
                    Transform targetParent = parent ?? GetCanvasForUI<T>();
                    if (targetParent != null)
                    {
                        uiObject.transform.SetParent(targetParent, false);
                    }

                    // ??? ??
                    addressableHandles[key] = handle;
                    instantiatedUIs[key] = uiObject;
                    RegisterUI(uiComponent);

                    Debug.Log($"[UIManager] UI ?? ??: {key}");
                    return uiComponent;
                }
                else
                {
                    Debug.LogError($"[UIManager] UI ?? ??: {key}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI ?? ? ??: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// ??? UI? ?? ??
        /// </summary>
        public async System.Threading.Tasks.Task<List<T>> LoadUIsByLabelAsync<T>(string label) where T : BaseUI
        {
            try
            {
                var loadHandle = Addressables.LoadAssetsAsync<GameObject>(label, null);
                await loadHandle.Task;

                List<T> loadedUIs = new List<T>();

                foreach (var asset in loadHandle.Result)
                {
                    T uiComponent = await LoadUIAsync<T>(asset.name);
                    if (uiComponent != null)
                    {
                        loadedUIs.Add(uiComponent);
                    }
                }

                Addressables.Release(loadHandle);
                Debug.Log($"[UIManager] ?? '{label}'? {loadedUIs.Count}?? UI ?? ??");
                return loadedUIs;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] ?? ?? ? ??: {e.Message}");
                return new List<T>();
            }
        }

        /// <summary>
        /// UI ?? ?? (??)
        /// </summary>
        public async System.Threading.Tasks.Task PreloadUIAsync<T>(string addressableKey) where T : BaseUI
        {
            try
            {
                if (instantiatedUIs.ContainsKey(addressableKey))
                {
                    Debug.Log($"[UIManager] ?? ??? UI: {addressableKey}");
                    return;
                }

                T uiComponent = await LoadUIAsync<T>(addressableKey);
                if (uiComponent != null)
                {
                    uiComponent.Hide(); // ??? ??? ??
                    Debug.Log($"[UIManager] UI ?? ?? ??: {addressableKey}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI ?? ?? ? ??: {e.Message}");
            }
        }

        #endregion

        #region Addressable Resource Management

        /// <summary>
        /// ?? UI ??
        /// </summary>
        public void ReleaseUI(string addressableKey)
        {
            try
            {
                if (addressableHandles.ContainsKey(addressableKey))
                {
                    var handle = addressableHandles[addressableKey];
                    Addressables.ReleaseInstance(handle);
                    addressableHandles.Remove(addressableKey);
                }

                if (instantiatedUIs.ContainsKey(addressableKey))
                {
                    GameObject uiObject = instantiatedUIs[addressableKey];
                    if (uiObject != null)
                    {
                        BaseUI uiComponent = uiObject.GetComponent<BaseUI>();
                        if (uiComponent != null)
                        {
                            UnregisterUI(uiComponent);
                        }
                        DestroyImmediate(uiObject);
                    }
                    instantiatedUIs.Remove(addressableKey);
                }

                Debug.Log($"[UIManager] UI ?? ??: {addressableKey}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] UI ?? ? ??: {e.Message}");
            }
        }

        /// <summary>
        /// ?? Addressable ??? ??
        /// </summary>
        public void ReleaseAllAddressables()
        {
            try
            {
                Debug.Log("[UIManager] ?? Addressable ??? ?? ??");

                // ?? ?? ??
                foreach (var handle in addressableHandles.Values)
                {
                    if (handle.IsValid())
                    {
                        Addressables.ReleaseInstance(handle);
                    }
                }
                addressableHandles.Clear();

                // ?? ???? ??
                foreach (var uiObject in instantiatedUIs.Values)
                {
                    if (uiObject != null)
                    {
                        BaseUI uiComponent = uiObject.GetComponent<BaseUI>();
                        if (uiComponent != null)
                        {
                            UnregisterUI(uiComponent);
                        }
                        DestroyImmediate(uiObject);
                    }
                }
                instantiatedUIs.Clear();

                Debug.Log("[UIManager] ?? Addressable ??? ?? ??");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] ??? ?? ? ??: {e.Message}");
            }
        }

        #endregion

        #region Canvas Management

        /// <summary>
        /// UI ??? ?? ??? Canvas ??
        /// </summary>
        private Transform GetCanvasForUI<T>() where T : BaseUI
        {
            // ?? ???? ???? LayerType ??
            GameObject tempObject = new GameObject("Temp");
            T tempComponent = tempObject.AddComponent<T>();
            UILayerType layerType = tempComponent.LayerType;
            DestroyImmediate(tempObject);

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
        /// ???? Canvas ??
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
        /// UI ??
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
                Debug.Log($"[UIManager] UI ??: {ui.name} -> {layerType}");
            }
        }

        /// <summary>
        /// UI ?? ??
        /// </summary>
        public void UnregisterUI(BaseUI ui)
        {
            if (ui == null) return;

            UILayerType layerType = ui.LayerType;
            if (layerUIs.ContainsKey(layerType))
            {
                layerUIs[layerType].Remove(ui);
                Debug.Log($"[UIManager] UI ?? ??: {ui.name} -> {layerType}");
            }
        }

        /// <summary>
        /// ???? UI ?? ??
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
        /// ?? ??
        /// </summary>
        public void OpenPanel(BaseUI panel)
        {
            if (panel == null) return;

            // ?? ?? ??? (??)
            if (panelStack.Count > 0)
            {
                BaseUI previousPanel = panelStack.Peek();
                if (previousPanel != null && panel.HidePreviousUI)
                {
                    previousPanel.Hide();
                }
            }

            // ? ??? ??? ??
            panelStack.Push(panel);
            panel.Show();

            Debug.Log($"[UIManager] ?? ??: {panel.name}");
        }

        /// <summary>
        /// ?? ??
        /// </summary>
        public void ClosePanel()
        {
            if (panelStack.Count == 0) return;

            BaseUI currentPanel = panelStack.Pop();
            if (currentPanel != null)
            {
                currentPanel.Hide();
                Debug.Log($"[UIManager] ?? ??: {currentPanel.name}");
            }

            // ?? ?? ?? ???
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
        /// ?? ??
        /// </summary>
        public void OpenPopup(BaseUI popup)
        {
            if (popup == null) return;

            // ?? ?? ??? (??)
            if (popupStack.Count > 0)
            {
                BaseUI previousPopup = popupStack.Peek();
                if (previousPopup != null && popup.HidePreviousUI)
                {
                    previousPopup.Hide();
                }
            }

            // ? ??? ??? ??
            popupStack.Push(popup);
            popup.Show();

            Debug.Log($"[UIManager] ?? ??: {popup.name}");
        }

        /// <summary>
        /// ?? ??
        /// </summary>
        public void ClosePopup()
        {
            if (popupStack.Count == 0) return;

            BaseUI currentPopup = popupStack.Pop();
            if (currentPopup != null)
            {
                currentPopup.Hide();
                Debug.Log($"[UIManager] ?? ??: {currentPopup.name}");
            }

            // ?? ?? ?? ???
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
        /// ??? ?? UI ?? (?? ???? ?? ??)
        /// </summary>
        public T ShowPopUp<T>() where T : BaseUI
        {
            T result = null;
            ShowPopUpAsync<T>((instance) => result = instance);
            return result;
        }

        /// <summary>
        /// ??? ?? UI ?? (??? ??)
        /// </summary>
        public void ShowPopUpAsync<T>(System.Action<T> onComplete = null) where T : BaseUI
        {
            StartCoroutine(ShowPopUpAsyncCoroutine<T>(onComplete));
        }

        private System.Collections.IEnumerator ShowPopUpAsyncCoroutine<T>(System.Action<T> onComplete) where T : BaseUI
        {
            string prefabName = typeof(T).Name;
            Debug.Log($"[UIManager] Addressable?? {prefabName} ?? ?? ??");
            
            // Addressable?? ?? ?? ??
            string addressableKey = $"UI/Popup/{prefabName}";
            
            AsyncOperationHandle<GameObject> handle = default;
            AsyncOperationHandle<GameObject> instanceHandle = default;
            
            try
            {
                handle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] {prefabName} ?? ?? ? ??: {e.Message}");
                onComplete?.Invoke(null);
                yield break;
            }
            
            yield return handle;
            
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"[UIManager] Addressable?? {prefabName} ??? ?? ? ????: {addressableKey}");
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
                Debug.LogError($"[UIManager] {prefabName} ?? ???? ?? ? ??: {e.Message}");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            yield return instanceHandle;
            
            if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[UIManager] {prefabName} ?? ???? ?? ??");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            T popupInstance = instanceHandle.Result.GetComponent<T>();
            if (popupInstance == null)
            {
                Debug.LogError($"[UIManager] {prefabName} ????? ?? ? ????.");
                Addressables.ReleaseInstance(instanceHandle.Result);
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            OpenPopup(popupInstance);
            onComplete?.Invoke(popupInstance);
            Debug.Log($"[UIManager] {prefabName} ?? ?? ??");
            
            Addressables.Release(handle);
        }

        /// <summary>
        /// ?? ?? ?? (?? ???)
        /// </summary>
        public CheckPopUp ShowConfirmPopUp(string message, string confirmText = "??", string cancelText = "??",
                                          System.Action confirmCallback = null, System.Action cancelCallback = null)
        {
            CheckPopUp result = null;
            ShowConfirmPopUpAsync(message, confirmText, cancelText, confirmCallback, cancelCallback, (popup) => result = popup);
            return result;
        }

        /// <summary>
        /// ?? ?? ?? (?? ??)
        /// </summary>
        public CheckPopUp ShowConfirmPopUp(string message, System.Action confirmCallback)
        {
            return ShowConfirmPopUp(message, "??", "??", confirmCallback, null);
        }

        /// <summary>
        /// ?? ?? ?? (??? ??)
        /// </summary>
        public void ShowConfirmPopUpAsync(string message, string confirmText = "??", string cancelText = "??",
                                         System.Action confirmCallback = null, System.Action cancelCallback = null,
                                         System.Action<CheckPopUp> onComplete = null)
        {
            StartCoroutine(ShowConfirmPopUpAsyncCoroutine(message, confirmText, cancelText, confirmCallback, cancelCallback, onComplete));
        }

        /// <summary>
        /// ?? ?? ?? (?? ??? ??)
        /// </summary>
        public void ShowConfirmPopUpAsync(string message, System.Action confirmCallback, System.Action<CheckPopUp> onComplete = null)
        {
            ShowConfirmPopUpAsync(message, "??", "??", confirmCallback, null, onComplete);
        }

        private System.Collections.IEnumerator ShowConfirmPopUpAsyncCoroutine(string message, string confirmText, string cancelText,
                                                                             System.Action confirmCallback, System.Action cancelCallback,
                                                                             System.Action<CheckPopUp> onComplete)
        {
            Debug.Log("[UIManager] ?? ?? ?? ??");
            
            AsyncOperationHandle<GameObject> handle = default;
            AsyncOperationHandle<GameObject> instanceHandle = default;
            
            try
            {
                // CheckPopUp ??? ??
                handle = Addressables.LoadAssetAsync<GameObject>("UI/Popup/CheckPopUp");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] ?? ?? ?? ? ??: {e.Message}");
                onComplete?.Invoke(null);
                yield break;
            }
            
            yield return handle;
            
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError("[UIManager] Addressable?? CheckPopUp? ?? ? ????.");
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
                Debug.LogError($"[UIManager] ?? ?? ???? ?? ? ??: {e.Message}");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            yield return instanceHandle;
            
            if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[UIManager] ?? ?? ???? ?? ??");
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            CheckPopUp popupInstance = instanceHandle.Result.GetComponent<CheckPopUp>();
            if (popupInstance == null)
            {
                Debug.LogError("[UIManager] CheckPopUp ????? ?? ? ????.");
                Addressables.ReleaseInstance(instanceHandle.Result);
                Addressables.Release(handle);
                onComplete?.Invoke(null);
                yield break;
            }
            
            // ?? ??
            popupInstance.SetMessage(message);
            popupInstance.SetConfirmText(confirmText);
            popupInstance.SetCancelText(cancelText);
            
            if (confirmCallback != null)
                popupInstance.OnConfirmClicked += confirmCallback;
            if (cancelCallback != null)
                popupInstance.OnCancelClicked += cancelCallback;
            
            OpenPopup(popupInstance);
            onComplete?.Invoke(popupInstance);
            Debug.Log("[UIManager] ?? ?? ?? ??");
            
            Addressables.Release(handle);
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// ESC ? ??
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
        /// ?? UI? ?? ? ??? ??
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
        /// ? ?? ? ??
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[UIManager] ? ???: {scene.name}");
            
            // ? ?? ? ?? UI ??
            CleanAllUI();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// ?? ?? ??
        /// </summary>
        public void CloseAllPanels()
        {
            while (panelStack.Count > 0)
            {
                ClosePanel();
            }
        }

        /// <summary>
        /// ?? ?? ??
        /// </summary>
        public void CloseAllPopups()
        {
            while (popupStack.Count > 0)
            {
                ClosePopup();
            }
        }

        /// <summary>
        /// ?? UI ??
        /// </summary>
        public void CleanAllUI()
        {
            Debug.Log("[UIManager] ?? UI ?? ??");
            
            CloseAllPanels();
            CloseAllPopups();
            
            Debug.Log("[UIManager] ?? UI? ???????.");
        }

        #endregion
    }
}
