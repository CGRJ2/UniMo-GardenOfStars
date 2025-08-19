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
    public class UIManager_Addressable : Singleton<UIManager_Addressable>
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
        
        // Canvas 인스턴스
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
                Debug.Log("[UIManager_Addressable] Addressable Canvas 초기화 시작");
                
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
                
                Debug.Log("[UIManager_Addressable] Addressable Canvas 초기화 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager_Addressable] Canvas 초기화 중 오류: {e.Message}");
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

        #region Addressable UI Management

        /// <summary>
        /// Addressable에서 UI 프리팹 로드 및 인스턴스화 (문자열 키 사용)
        /// </summary>
        public async System.Threading.Tasks.Task<T> LoadUIAsync<T>(string addressableKey, Transform parent = null) where T : BaseUI
        {
            try
            {
                // 이미 인스턴스화된 UI가 있는지 확인
                if (instantiatedUIs.ContainsKey(addressableKey))
                {
                    return instantiatedUIs[addressableKey].GetComponent<T>();
                }
                
                // Addressable에서 프리팹 로드
                var loadHandle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
                await loadHandle.Task;
                
                if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    // 인스턴스화
                    GameObject uiPrefab = loadHandle.Result;
                    Transform targetParent = parent ?? GetCanvasForUI<T>();
                    
                    var instantiateHandle = Addressables.InstantiateAsync(addressableKey, targetParent);
                    await instantiateHandle.Task;
                    
                    if (instantiateHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject uiInstance = instantiateHandle.Result;
                        T uiComponent = uiInstance.GetComponent<T>();
                        
                        if (uiComponent != null)
                        {
                            // 캐시에 저장
                            instantiatedUIs[addressableKey] = uiInstance;
                            addressableHandles[addressableKey] = instantiateHandle;
                            
                            // 레이어에 등록
                            RegisterUI(uiComponent);
                            
                            Debug.Log($"[UIManager_Addressable] UI 로드 완료: {addressableKey}");
                            return uiComponent;
                        }
                        else
                        {
                            Debug.LogError($"[UIManager_Addressable] {addressableKey}에 {typeof(T).Name} 컴포넌트가 없습니다.");
                            Addressables.ReleaseInstance(instantiateHandle);
                        }
                    }
                    else
                    {
                        Debug.LogError($"[UIManager_Addressable] {addressableKey} 인스턴스화 실패");
                        Addressables.ReleaseInstance(instantiateHandle);
                    }
                }
                else
                {
                    Debug.LogError($"[UIManager_Addressable] {addressableKey} 로드 실패");
                }
                
                Addressables.Release(loadHandle);
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager_Addressable] UI 로드 중 오류: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Addressable에서 UI 프리팹 로드 및 인스턴스화 (AssetReference 사용)
        /// </summary>
        public async System.Threading.Tasks.Task<T> LoadUIAsync<T>(AssetReferenceGameObject assetReference, Transform parent = null) where T : BaseUI
        {
            try
            {
                if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                {
                    Debug.LogError("[UIManager_Addressable] 유효하지 않은 AssetReference입니다.");
                    return null;
                }

                string addressableKey = assetReference.RuntimeKey.ToString();
                
                // 이미 인스턴스화된 UI가 있는지 확인
                if (instantiatedUIs.ContainsKey(addressableKey))
                {
                    return instantiatedUIs[addressableKey].GetComponent<T>();
                }
                
                // AssetReference를 사용하여 인스턴스화
                var instantiateHandle = assetReference.InstantiateAsync(parent);
                await instantiateHandle.Task;
                
                if (instantiateHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject uiInstance = instantiateHandle.Result;
                    T uiComponent = uiInstance.GetComponent<T>();
                    
                    if (uiComponent != null)
                    {
                        // 캐시에 저장
                        instantiatedUIs[addressableKey] = uiInstance;
                        addressableHandles[addressableKey] = instantiateHandle;
                        
                        // 레이어에 등록
                        RegisterUI(uiComponent);
                        
                        Debug.Log($"[UIManager_Addressable] UI 로드 완료: {addressableKey}");
                        return uiComponent;
                    }
                    else
                    {
                        Debug.LogError($"[UIManager_Addressable] {addressableKey}에 {typeof(T).Name} 컴포넌트가 없습니다.");
                        Addressables.ReleaseInstance(instantiateHandle);
                    }
                }
                else
                {
                    Debug.LogError($"[UIManager_Addressable] {addressableKey} 인스턴스화 실패");
                    Addressables.ReleaseInstance(instantiateHandle);
                }
                
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager_Addressable] UI 로드 중 오류: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 라벨로 UI 프리팹들 로드
        /// </summary>
        public async System.Threading.Tasks.Task<List<T>> LoadUIsByLabelAsync<T>(string label) where T : BaseUI
        {
            try
            {
                var loadHandle = Addressables.LoadAssetsAsync<GameObject>(label, null);
                await loadHandle.Task;
                
                List<T> loadedUIs = new List<T>();
                
                if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    foreach (GameObject prefab in loadHandle.Result)
                    {
                        T uiComponent = await LoadUIAsync<T>(prefab.name);
                        if (uiComponent != null)
                        {
                            loadedUIs.Add(uiComponent);
                        }
                    }
                }
                
                Addressables.Release(loadHandle);
                return loadedUIs;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager_Addressable] 라벨 로드 중 오류: {e.Message}");
                return new List<T>();
            }
        }

        /// <summary>
        /// UI 프리팹 미리 로드
        /// </summary>
        public async System.Threading.Tasks.Task PreloadUIAsync<T>(string addressableKey) where T : BaseUI
        {
            try
            {
                var loadHandle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
                await loadHandle.Task;
                
                if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"[UIManager_Addressable] UI 미리 로드 완료: {addressableKey}");
                }
                
                Addressables.Release(loadHandle);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager_Addressable] UI 미리 로드 중 오류: {e.Message}");
            }
        }

        /// <summary>
        /// UI 인스턴스 해제
        /// </summary>
        public void ReleaseUI(string addressableKey)
        {
            if (instantiatedUIs.ContainsKey(addressableKey))
            {
                GameObject uiInstance = instantiatedUIs[addressableKey];
                BaseUI uiComponent = uiInstance.GetComponent<BaseUI>();
                
                if (uiComponent != null)
                {
                    UnregisterUI(uiComponent);
                }
                
                // Addressable 핸들 해제
                if (addressableHandles.ContainsKey(addressableKey))
                {
                    Addressables.ReleaseInstance(addressableHandles[addressableKey]);
                    addressableHandles.Remove(addressableKey);
                }
                
                instantiatedUIs.Remove(addressableKey);
                Debug.Log($"[UIManager_Addressable] UI 해제 완료: {addressableKey}");
            }
        }

        /// <summary>
        /// 모든 Addressable 리소스 해제
        /// </summary>
        public void ReleaseAllAddressables()
        {
            foreach (var kvp in addressableHandles)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.ReleaseInstance(kvp.Value);
                }
            }
            
            addressableHandles.Clear();
            instantiatedUIs.Clear();
            
            Debug.Log("[UIManager_Addressable] 모든 Addressable 리소스 해제 완료");
        }

        #endregion

        #region Canvas Management

        /// <summary>
        /// UI 타입에 따른 적절한 Canvas 반환
        /// </summary>
        private Transform GetCanvasForUI<T>() where T : BaseUI
        {
            // 임시로 T 타입을 기반으로 Canvas 결정
            // 실제로는 BaseUI의 LayerType을 확인해야 함
            if (typeof(T).Name.Contains("HUD"))
                return hudCanvas?.transform;
            else if (typeof(T).Name.Contains("Panel"))
                return panelCanvas?.transform;
            else if (typeof(T).Name.Contains("Popup"))
                return popupCanvas?.transform;
            else if (typeof(T).Name.Contains("Loading"))
                return loadingCanvas?.transform;
            
            // 기본값
            return panelCanvas?.transform;
        }

        /// <summary>
        /// 특정 레이어의 Canvas 반환
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
                    return panelCanvas;
            }
        }

        #endregion

        #region Layer Management

        /// <summary>
        /// 특정 레이어에 UI 등록
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
                Debug.Log($"[UIManager_Addressable] UI 등록: {ui.name} -> {layerType}");
            }
        }

        /// <summary>
        /// 특정 레이어에서 UI 제거
        /// </summary>
        public void UnregisterUI(BaseUI ui)
        {
            if (ui == null) return;
            
            UILayerType layerType = ui.LayerType;
            if (layerUIs.ContainsKey(layerType))
            {
                layerUIs[layerType].Remove(ui);
                Debug.Log($"[UIManager_Addressable] UI 제거: {ui.name} -> {layerType}");
            }
        }

        /// <summary>
        /// 특정 레이어의 모든 UI 가져오기
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

        #region Stack Management

        /// <summary>
        /// 패널 열기 (Stack에 추가)
        /// </summary>
        public void OpenPanel(BaseUI panel)
        {
            if (panel == null) return;

            // 이전 패널 처리
            if (panelStack.Count > 0)
            {
                BaseUI topPanel = panelStack.Peek();
                
                if (panel.HidePreviousUI)
                {
                    topPanel.Hide();
                }
                else if (panel.DisablePreviousUI)
                {
                    topPanel.gameObject.SetActive(false);
                }
            }

            // 새 패널 추가
            panelStack.Push(panel);
            panel.Show();

            Debug.Log($"[UIManager_Addressable] 패널 열기: {panel.name}, 스택 크기: {panelStack.Count}");
        }

        /// <summary>
        /// 패널 닫기 (Stack에서 제거)
        /// </summary>
        public void ClosePanel()
        {
            if (panelStack.Count == 0) return;

            BaseUI panel = panelStack.Pop();
            panel.Hide();

            // 이전 패널 복원
            if (panelStack.Count > 0)
            {
                BaseUI previousPanel = panelStack.Peek();
                
                if (!previousPanel.gameObject.activeInHierarchy)
                {
                    previousPanel.gameObject.SetActive(true);
                }
                
                if (!previousPanel.IsActive)
                {
                    previousPanel.Show();
                }
            }

            Debug.Log($"[UIManager_Addressable] 패널 닫기: {panel.name}, 스택 크기: {panelStack.Count}");
        }

        /// <summary>
        /// 팝업 열기 (Stack에 추가)
        /// </summary>
        public void OpenPopup(BaseUI popup)
        {
            if (popup == null) return;

            // 이전 팝업 처리
            if (popupStack.Count > 0)
            {
                BaseUI topPopup = popupStack.Peek();
                
                if (popup.HidePreviousUI)
                {
                    topPopup.Hide();
                }
                else if (popup.DisablePreviousUI)
                {
                    topPopup.gameObject.SetActive(false);
                }
            }

            // 새 팝업 추가
            popupStack.Push(popup);
            popup.Show();

            Debug.Log($"[UIManager_Addressable] 팝업 열기: {popup.name}, 스택 크기: {popupStack.Count}");
        }

        /// <summary>
        /// 팝업 닫기 (Stack에서 제거)
        /// </summary>
        public void ClosePopup()
        {
            if (popupStack.Count == 0) return;

            BaseUI popup = popupStack.Pop();
            popup.Hide();

            // 이전 팝업 복원
            if (popupStack.Count > 0)
            {
                BaseUI previousPopup = popupStack.Peek();
                
                if (!previousPopup.gameObject.activeInHierarchy)
                {
                    previousPopup.gameObject.SetActive(true);
                }
                
                if (!previousPopup.IsActive)
                {
                    previousPopup.Show();
                }
            }

            Debug.Log($"[UIManager_Addressable] 팝업 닫기: {popup.name}, 스택 크기: {popupStack.Count}");
        }

        #endregion

        #region Input Handling

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canClose)
            {
                if (IsCurrentUINonClosable())
                {
                    Debug.Log("[UIManager_Addressable] 이 UI는 ESC로 닫을 수 없습니다.");
                    return;
                }

                // 우선순위: 팝업 > 패널
                if (popupStack.Count > 0)
                {
                    ClosePopup();
                }
                else if (panelStack.Count > 0)
                {
                    ClosePanel();
                }
                
                Util.ConsumeESC();
            }
            Util.ResetESC();
        }

        private bool IsCurrentUINonClosable()
        {
            // 팝업 스택 확인
            if (popupStack.Count > 0)
            {
                BaseUI topPopup = popupStack.Peek();
                if (!topPopup.CanCloseWithESC)
                    return true;
            }
            
            // 패널 스택 확인
            if (panelStack.Count > 0)
            {
                BaseUI topPanel = panelStack.Peek();
                if (!topPanel.CanCloseWithESC)
                    return true;
            }
            
            return false;
        }

        #endregion

        #region Scene Management

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[UIManager_Addressable] 씬 로드: {scene.name}");
            
            // Stack 초기화
            panelStack.Clear();
            popupStack.Clear();
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
            Debug.Log("[UIManager_Addressable] CleanAllUI 시작");
            
            CloseAllPanels();
            CloseAllPopups();
            
            Debug.Log("[UIManager_Addressable] 모든 UI가 정리되었습니다.");
        }

        #endregion
    }
}
