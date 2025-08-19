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
    /// 개선된 UI 관리자 - 레이어별 관리 및 Stack 기반 UI 관리 지원
    /// </summary>
    public class UIManagerOld : Singleton<UIManagerOld>
    {
        [Header("UI Canvas Settings")]
        [SerializeField] private GameObject popUpCanvasPrefab; // AddressableAssetReference popUpCanvasReference;
        [SerializeField] private string uiPrefabLabel = "UI"; // Addressables 라벨 (향후 사용 예정)
        
        [Header("UI Layer Settings")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private Canvas panelCanvas;
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private Canvas loadingCanvas;
        
        // UI Components
        private PopUpUI popUp;
        
        // Layer별 UI 관리
        private Dictionary<UILayerType, List<BaseUI>> layerUIs = new Dictionary<UILayerType, List<BaseUI>>();
        
        // Stack 기반 UI 관리 (패널 & 팝업)
        private Stack<BaseUI> panelStack = new Stack<BaseUI>();
        private Stack<BaseUI> popupStack = new Stack<BaseUI>();
        
        // 기존 메인 패널 관리 (하위 호환성)
        private Dictionary<string, GameObject> mainPanels = new Dictionary<string, GameObject>();
        
        // UI 상태 관리
        public static int selectIndexUI { get; set; } = 0;
        public static bool canClosePopUp = true;
        bool canClose => (PopUpUI.IsPopUpActive || panelStack.Count > 0 || popupStack.Count > 0) && 
                        !Util.escPressed && canClosePopUp && !IsCurrentUINonClosable();

        #region Properties

        public PopUpUI PopUp
        {
            get
            {
                if (popUp == null)
                {
                    popUp = FindObjectOfType<PopUpUI>();
                    if (popUp != null) return popUp;

                    // 일반 프리팹에서 팝업 캔버스 로드
                    LoadPopUpCanvas();
                }
                return popUp;
            }
        }

        private void LoadPopUpCanvas()
        {
            try
            {
                if (popUpCanvasPrefab == null)
                {
                    Debug.LogError("[UIManager] PopUp Canvas Prefab이 설정되지 않았습니다.");
                    return;
                }

                GameObject go = Instantiate(popUpCanvasPrefab);
                popUp = go.GetComponent<PopUpUI>();
                
                if (popUp == null)
                {
                    Debug.LogError("[UIManager] PopUp Canvas 프리팹에 PopUpUI 컴포넌트가 없습니다");
                    Destroy(go);
                    return;
                }

                DontDestroyOnLoad(go);
                Debug.Log("[UIManager] PopUp Canvas 로드 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] PopUp Canvas 로드 중 오류 발생: {e.Message}");
            }
        }

        #endregion

        #region Initialization

        private void Awake()
        {
            InitializeLayerUIs();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void InitializeLayerUIs()
        {
            foreach (UILayerType layerType in System.Enum.GetValues(typeof(UILayerType)))
            {
                layerUIs[layerType] = new List<BaseUI>();
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
                Debug.Log($"[UIManager] UI 등록: {ui.name} -> {layerType}");
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
                Debug.Log($"[UIManager] UI 제거: {ui.name} -> {layerType}");
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
                // 둘 다 false면 이전 UI는 그대로 유지
            }

            // 새 패널 추가
            panelStack.Push(panel);
            panel.Show();

            Debug.Log($"[UIManager] 패널 열기: {panel.name}, 스택 크기: {panelStack.Count}");
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
                
                // 이전 패널이 비활성화되어 있었다면 다시 활성화
                if (!previousPanel.gameObject.activeInHierarchy)
                {
                    previousPanel.gameObject.SetActive(true);
                }
                
                // 이전 패널이 숨겨져 있었다면 다시 표시
                if (!previousPanel.IsActive)
                {
                    previousPanel.Show();
                }
            }

            Debug.Log($"[UIManager] 패널 닫기: {panel.name}, 스택 크기: {panelStack.Count}");
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
                // 둘 다 false면 이전 UI는 그대로 유지
            }

            // 새 팝업 추가
            popupStack.Push(popup);
            popup.Show();

            Debug.Log($"[UIManager] 팝업 열기: {popup.name}, 스택 크기: {popupStack.Count}");
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
                
                // 이전 팝업이 비활성화되어 있었다면 다시 활성화
                if (!previousPopup.gameObject.activeInHierarchy)
                {
                    previousPopup.gameObject.SetActive(true);
                }
                
                // 이전 팝업이 숨겨져 있었다면 다시 표시
                if (!previousPopup.IsActive)
                {
                    previousPopup.Show();
                }
            }

            Debug.Log($"[UIManager] 팝업 닫기: {popup.name}, 스택 크기: {popupStack.Count}");
        }

        #endregion

        #region Group Management

        /// <summary>
        /// HUD 그룹 기본 상태로 설정
        /// </summary>
        public void SetHUDToDefault()
        {
            var hudUIs = GetUIsByLayer(UILayerType.HUD);
            foreach (var ui in hudUIs)
            {
                // HUD는 기본 정보만 표시하도록 설정
                ui.Show();
            }
        }

        /// <summary>
        /// 패널 그룹의 모든 활성화된 UI 닫기
        /// </summary>
        public void CloseAllPanels()
        {
            while (panelStack.Count > 0)
            {
                ClosePanel();
            }
        }

        /// <summary>
        /// 팝업 그룹의 모든 활성화된 UI 닫기
        /// </summary>
        public void CloseAllPopups()
        {
            while (popupStack.Count > 0)
            {
                ClosePopup();
            }
        }

        /// <summary>
        /// 특정 그룹의 모든 UI 닫기
        /// </summary>
        public void CloseAllUIsByGroup(UIPanelGroup group)
        {
            var panelUIs = GetUIsByLayer(UILayerType.Panel);
            foreach (var ui in panelUIs)
            {
                if (ui.PanelGroup == group && ui.IsActive)
                {
                    ui.Hide();
                }
            }
        }

        #endregion

        #region Generic UI Management

        /// <summary>
        /// 제네릭 팝업 UI 표시 (비동기 버전 - Resources 폴더에서 로드)
        /// </summary>
        public void ShowPopUpAsync<T>(System.Action<T> onComplete = null) where T : BaseUI
        {
            StartCoroutine(ShowPopUpAsyncCoroutine<T>(onComplete));
        }

        private System.Collections.IEnumerator ShowPopUpAsyncCoroutine<T>(System.Action<T> onComplete) where T : BaseUI
        {
            string prefabName = typeof(T).Name;
            Debug.Log($"[UIManager] Resources에서 {prefabName} 프리팹 로드 시작");
            
            // Resources 폴더에서 프리팹 로드
            GameObject prefab = Resources.Load<GameObject>($"UI/Popup/{prefabName}");
            
            if (prefab == null)
            {
                // 다른 경로에서도 시도
                prefab = Resources.Load<GameObject>($"UI/{prefabName}");
            }
            
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Resources에서 {prefabName} 프리팹을 찾을 수 없습니다.");
                onComplete?.Invoke(null);
                yield break;
            }
            
            // 팝업 인스턴스 생성
            GameObject popupInstance = Instantiate(prefab, popupCanvas.transform);
            T popupUI = popupInstance.GetComponent<T>();
            
            if (popupUI == null)
            {
                Debug.LogError($"[UIManager] {prefabName}에 {typeof(T).Name} 컴포넌트가 없습니다.");
                Destroy(popupInstance);
                onComplete?.Invoke(null);
                yield break;
            }
            
            // 팝업 스택에 추가
            OpenPopup(popupUI);
            
            Debug.Log($"[UIManager] {prefabName} 팝업 생성 완료");
            onComplete?.Invoke(popupUI);
        }

        /// <summary>
        /// 제네릭 팝업 UI 표시 (동기 버전 - 레이블 사용) (Resources 폴더에서 로드)
        /// </summary>
        public void ShowPopUpByLabel<T>(string label, System.Action<T> onComplete = null) where T : BaseUI
        {
            StartCoroutine(ShowPopUpByLabelCoroutine<T>(label, onComplete));
        }

        private System.Collections.IEnumerator ShowPopUpByLabelCoroutine<T>(string label, System.Action<T> onComplete) where T : BaseUI
        {
            Debug.Log($"[UIManager] Resources에서 {label} 라벨로 {typeof(T).Name} 프리팹 로드 시작");
            
            // Resources 폴더에서 해당 라벨의 모든 프리팹 로드
            GameObject[] prefabs = Resources.LoadAll<GameObject>($"UI/{label}");
            
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogError($"[UIManager] Resources에서 {label} 라벨의 프리팹을 찾을 수 없습니다.");
                onComplete?.Invoke(null);
                yield break;
            }
            
            // T 타입의 프리팹 찾기
            GameObject targetPrefab = null;
            foreach (GameObject prefab in prefabs)
            {
                if (prefab.GetComponent<T>() != null)
                {
                    targetPrefab = prefab;
                    break;
                }
            }
            
            if (targetPrefab == null)
            {
                Debug.LogError($"[UIManager] {label} 라벨에서 {typeof(T).Name} 타입의 프리팹을 찾을 수 없습니다.");
                onComplete?.Invoke(null);
                yield break;
            }
            
            // 팝업 인스턴스 생성
            GameObject popupInstance = Instantiate(targetPrefab, popupCanvas.transform);
            T popupUI = popupInstance.GetComponent<T>();
            
            // 팝업 스택에 추가
            OpenPopup(popupUI);
            
            Debug.Log($"[UIManager] {label} 라벨로 {typeof(T).Name} 팝업 생성 완료");
            onComplete?.Invoke(popupUI);
        }

        /// <summary>
        /// 제네릭 팝업 UI 표시 (기존 호환성을 위한 래퍼 - Resources 폴더에서 로드)
        /// </summary>
        public T ShowPopUp<T>() where T : BaseUI
        {
            T result = null;
            string prefabName = typeof(T).Name;
            
            // Resources 폴더에서 프리팹 로드
            GameObject prefab = Resources.Load<GameObject>($"UI/Popup/{prefabName}");
            
            if (prefab == null)
            {
                // 다른 경로에서도 시도
                prefab = Resources.Load<GameObject>($"UI/{prefabName}");
            }
            
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Resources에서 {prefabName} 프리팹을 찾을 수 없습니다.");
                return null;
            }
            
            // 팝업 인스턴스 생성
            GameObject popupInstance = Instantiate(prefab, popupCanvas.transform);
            result = popupInstance.GetComponent<T>();
            
            if (result == null)
            {
                Debug.LogError($"[UIManager] {prefabName}에 {typeof(T).Name} 컴포넌트가 없습니다.");
                Destroy(popupInstance);
                return null;
            }
            
            // 팝업 스택에 추가
            OpenPopup(result);
            
            Debug.Log($"[UIManager] {prefabName} 팝업 생성 완료 (동기)");
            return result;
        }

        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public void ClosePopUp()
        {
            if (popupStack.Count > 0)
            {
                ClosePopup();
            }
            else if (PopUp != null)
            {
                PopUp.PopUIStack();
            }
        }

        /// <summary>
        /// 모든 팝업 정리
        /// </summary>
        public void CleanPopUp()
        {
            CloseAllPopups();
            
            if (PopUp != null)
            {
                while (PopUp.StackCount() > 0)
                {
                    PopUp.PopUIStack();
                }
            }
        }

        /// <summary>
        /// 모든 UI 정리
        /// </summary>
        public void CleanAllUI()
        {
            Debug.Log("[UIManager] CleanAllUI 시작");
            
            CloseAllPanels();
            CloseAllPopups();
            
            if (PopUp != null)
            {
                PopUp.ForceCleanAll();
            }
            
            mainPanels.Clear();
            Debug.Log("[UIManager] 모든 UI가 정리되었습니다.");
        }

        #endregion

        #region Scene Management

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[UIManager] 씬 로드: {scene.name}");
            
            // 씬 전환 시 모든 UI 정리
            if (PopUp != null)
            {
                PopUp.ForceCleanAll();
            }
            
            // Stack 초기화
            panelStack.Clear();
            popupStack.Clear();
            mainPanels.Clear();
            
            // 게임 씬으로 전환되는 경우 로딩 블로커 숨기기
            if (scene.name.Contains("Game") || scene.name.Contains("Arena") || 
                scene.name.Contains("Jump") || scene.name.Contains("Racing") ||
                scene.name.Contains("Tile") || scene.name.Contains("Rope") ||
                scene.name.Contains("Receive"))
            {
                if (PopUp != null)
                {
                    PopUp.HideLoadingBlocker();
                }
            }
        }

        #endregion

        #region Input Handling

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canClose)
            {
                if (IsCurrentUINonClosable())
                {
                    Debug.Log("[UIManager] 이 UI는 ESC로 닫을 수 없습니다.");
                    return;
                }

                // 우선순위: 팝업 > 패널 > 기존 팝업
                if (popupStack.Count > 0)
                {
                    ClosePopup();
                }
                else if (panelStack.Count > 0)
                {
                    ClosePanel();
                }
                else
                {
                    ClosePopUp(); // 기존 팝업 시스템
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

        #region Legacy Support (하위 호환성)

        // 기존 메인 패널 등록
        public void RegisterMainPanel(string panelName, GameObject panel)
        {
            if (string.IsNullOrEmpty(panelName) || panel == null)
            {
                Debug.LogError("[UIManager] 패널 이름이나 GameObject가 null입니다.");
                return;
            }

            if (mainPanels.ContainsKey(panelName))
            {
                Debug.LogWarning($"[UIManager] 이미 등록된 메인 패널: {panelName}");
                mainPanels[panelName] = panel;
            }
            else
            {
                mainPanels.Add(panelName, panel);
            }
        }

        // 기존 메인 패널 제거
        public void UnregisterMainPanel(string panelName)
        {
            if (mainPanels.Remove(panelName))
            {
                Debug.Log($"[UIManager] 메인 패널 제거: {panelName}");
            }
        }

        // 기존 메인 패널 가져오기
        public GameObject GetMainPanel(string panelName)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                Debug.LogError("[UIManager] 패널 이름이 null입니다.");
                return null;
            }

            if (mainPanels.TryGetValue(panelName, out GameObject panel))
            {
                return panel;
            }
            Debug.LogWarning($"[UIManager] 등록되지 않은 메인 패널: {panelName}");
            return null;
        }

        // 모든 메인 패널 가져오기
        public Dictionary<string, GameObject> GetAllMainPanels()
        {
            return new Dictionary<string, GameObject>(mainPanels);
        }

        #endregion

        #region Confirmation Popup

        // 확인 팝업 표시 (비동기)
        public void ShowConfirmPopUpAsync(string message, string confirmText = "확인", string cancelText = "취소",
                                               System.Action confirmCallback = null, System.Action cancelCallback = null,
                                               System.Action<CheckPopUp> onComplete = null)
        {
            ShowPopUpAsync<CheckPopUp>((checkPopUp) => {
                if (checkPopUp != null)
                {
                    //checkPopUp.SetMessage(message, confirmText, cancelText, confirmCallback, cancelCallback);
                    onComplete?.Invoke(checkPopUp);
                }
            });
        }

        // 간단한 확인 팝업 (비동기)
        public void ShowConfirmPopUpAsync(string message, System.Action confirmCallback, System.Action<CheckPopUp> onComplete = null)
        {
            ShowConfirmPopUpAsync(message, "확인", "취소", confirmCallback, null, onComplete);
        }

        // 확인 팝업 표시 (기존 호환성을 위한 래퍼)
        public CheckPopUp ShowConfirmPopUp(string message, string confirmText = "확인", string cancelText = "취소",
                                          System.Action confirmCallback = null, System.Action cancelCallback = null)
        {
            CheckPopUp result = null;
            ShowConfirmPopUpAsync(message, confirmText, cancelText, confirmCallback, cancelCallback, (popup) => result = popup);
            return result;
        }

        // 간단한 확인 팝업 (기존 호환성을 위한 래퍼)
        public CheckPopUp ShowConfirmPopUp(string message, System.Action confirmCallback)
        {
            return ShowConfirmPopUp(message, "확인", "취소", confirmCallback, null);
        }

        #endregion

        #region Utility Methods

        // 특정 타입의 활성 팝업 찾기
        public T FindActivePopUp<T>() where T : BaseUI
        {
            if (PopUp == null) return null;
            return PopUp.GetComponentInChildren<T>();
        }

        // 특정 팝업 업데이트
        public void UpdatePopUp<T>() where T : BaseUI
        {
            T popUp = FindActivePopUp<T>();
            if (popUp != null)
            {
                var loginInfoMethod = typeof(T).GetMethod("LoginInfo");
                if (loginInfoMethod != null)
                {
                    loginInfoMethod.Invoke(popUp, null);
                }
            }
        }

        /// <summary>
        /// 어드레서블에서 UI 프리팹 미리 로드 (임시)
        /// </summary>
        public void PreloadUIPrefabs(string label, System.Action onComplete = null)
        {
            Debug.LogWarning($"[UIManager] {label} 라벨 - Addressables 설정이 완료되지 않아 미리 로드를 건너뜁니다.");
            onComplete?.Invoke();
        }

        /// <summary>
        /// 특정 UI 프리팹 미리 로드 (임시)
        /// </summary>
        public void PreloadUIPrefab<T>(System.Action onComplete = null) where T : BaseUI
        {
            Debug.LogWarning($"[UIManager] {typeof(T).Name} - Addressables 설정이 완료되지 않아 미리 로드를 건너뜁니다.");
            onComplete?.Invoke();
        }

        // 게임 씬 로드 시작
        public void StartGameSceneLoad(string sceneName, int selectedGameIndex)
        {
            Debug.Log($"[UIManager] StartGameSceneLoad 호출 - Scene: {sceneName}, GameIndex: {selectedGameIndex}");
        }

        #endregion
    }
}
