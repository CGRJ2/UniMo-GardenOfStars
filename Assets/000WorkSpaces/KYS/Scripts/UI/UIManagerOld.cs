using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

#if DOTWEEN_AVAILABLE
using DG.Tweening;
#endif

namespace KYS
{
    /// <summary>
    /// ������ UI ������ - ���̾ ���� �� Stack ��� UI ���� ����
    /// </summary>
    public class UIManagerOld : Singleton<UIManagerOld>
    {
        [Header("UI Canvas Settings")]
        [SerializeField] private GameObject popUpCanvasPrefab; // AddressableAssetReference popUpCanvasReference;
        //[SerializeField] private string uiPrefabLabel = "UI"; // Addressables �� (���� ��� ����)
        
        [Header("UI Layer Settings")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private Canvas panelCanvas;
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private Canvas loadingCanvas;
        
        // UI Components
        private PopUpUI popUp;
        
        // Layer�� UI ����
        private Dictionary<UILayerType, List<BaseUI>> layerUIs = new Dictionary<UILayerType, List<BaseUI>>();
        
        // Stack ��� UI ���� (�г� & �˾�)
        private Stack<BaseUI> panelStack = new Stack<BaseUI>();
        private Stack<BaseUI> popupStack = new Stack<BaseUI>();
        
        // ���� ���� �г� ���� (���� ȣȯ��)
        private Dictionary<string, GameObject> mainPanels = new Dictionary<string, GameObject>();
        
        // UI ���� ����
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

                    // �Ϲ� �����տ��� �˾� ĵ���� �ε�
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
                    Debug.LogError("[UIManager] PopUp Canvas Prefab�� �������� �ʾҽ��ϴ�.");
                    return;
                }

                GameObject go = Instantiate(popUpCanvasPrefab);
                popUp = go.GetComponent<PopUpUI>();
                
                if (popUp == null)
                {
                    Debug.LogError("[UIManager] PopUp Canvas �����տ� PopUpUI ������Ʈ�� �����ϴ�");
                    Destroy(go);
                    return;
                }

                DontDestroyOnLoad(go);
                Debug.Log("[UIManager] PopUp Canvas �ε� �Ϸ�");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIManager] PopUp Canvas �ε� �� ���� �߻�: {e.Message}");
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
        /// Ư�� ���̾ UI ���
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
                Debug.Log($"[UIManager] UI ���: {ui.name} -> {layerType}");
            }
        }

        /// <summary>
        /// Ư�� ���̾�� UI ����
        /// </summary>
        public void UnregisterUI(BaseUI ui)
        {
            if (ui == null) return;
            
            UILayerType layerType = ui.LayerType;
            if (layerUIs.ContainsKey(layerType))
            {
                layerUIs[layerType].Remove(ui);
                Debug.Log($"[UIManager] UI ����: {ui.name} -> {layerType}");
            }
        }

        /// <summary>
        /// Ư�� ���̾��� ��� UI ��������
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
        /// �г� ���� (Stack�� �߰�)
        /// </summary>
        public void OpenPanel(BaseUI panel)
        {
            if (panel == null) return;

            // ���� �г� ó��
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
                // �� �� false�� ���� UI�� �״�� ����
            }

            // �� �г� �߰�
            panelStack.Push(panel);
            panel.Show();

            Debug.Log($"[UIManager] �г� ����: {panel.name}, ���� ũ��: {panelStack.Count}");
        }

                /// <summary>
        /// �г� �ݱ� (Stack���� ����)
        /// </summary>
        public void ClosePanel()
        {
            if (panelStack.Count == 0) return;

            BaseUI panel = panelStack.Pop();
            panel.Hide();

            // ���� �г� ����
            if (panelStack.Count > 0)
            {
                BaseUI previousPanel = panelStack.Peek();
                
                // ���� �г��� ��Ȱ��ȭ�Ǿ� �־��ٸ� �ٽ� Ȱ��ȭ
                if (!previousPanel.gameObject.activeInHierarchy)
                {
                    previousPanel.gameObject.SetActive(true);
                }
                
                // ���� �г��� ������ �־��ٸ� �ٽ� ǥ��
                if (!previousPanel.IsActive)
                {
                    previousPanel.Show();
                }
            }

            Debug.Log($"[UIManager] �г� �ݱ�: {panel.name}, ���� ũ��: {panelStack.Count}");
        }

                /// <summary>
        /// �˾� ���� (Stack�� �߰�)
        /// </summary>
        public void OpenPopup(BaseUI popup)
        {
            if (popup == null) return;

            // ���� �˾� ó��
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
                // �� �� false�� ���� UI�� �״�� ����
            }

            // �� �˾� �߰�
            popupStack.Push(popup);
            popup.Show();

            Debug.Log($"[UIManager] �˾� ����: {popup.name}, ���� ũ��: {popupStack.Count}");
        }

                /// <summary>
        /// �˾� �ݱ� (Stack���� ����)
        /// </summary>
        public void ClosePopup()
        {
            if (popupStack.Count == 0) return;

            BaseUI popup = popupStack.Pop();
            popup.Hide();

            // ���� �˾� ����
            if (popupStack.Count > 0)
            {
                BaseUI previousPopup = popupStack.Peek();
                
                // ���� �˾��� ��Ȱ��ȭ�Ǿ� �־��ٸ� �ٽ� Ȱ��ȭ
                if (!previousPopup.gameObject.activeInHierarchy)
                {
                    previousPopup.gameObject.SetActive(true);
                }
                
                // ���� �˾��� ������ �־��ٸ� �ٽ� ǥ��
                if (!previousPopup.IsActive)
                {
                    previousPopup.Show();
                }
            }

            Debug.Log($"[UIManager] �˾� �ݱ�: {popup.name}, ���� ũ��: {popupStack.Count}");
        }

        #endregion

        #region Group Management

        /// <summary>
        /// HUD �׷� �⺻ ���·� ����
        /// </summary>
        public void SetHUDToDefault()
        {
            var hudUIs = GetUIsByLayer(UILayerType.HUD);
            foreach (var ui in hudUIs)
            {
                // HUD�� �⺻ ������ ǥ���ϵ��� ����
                ui.Show();
            }
        }

        /// <summary>
        /// �г� �׷��� ��� Ȱ��ȭ�� UI �ݱ�
        /// </summary>
        public void CloseAllPanels()
        {
            while (panelStack.Count > 0)
            {
                ClosePanel();
            }
        }

        /// <summary>
        /// �˾� �׷��� ��� Ȱ��ȭ�� UI �ݱ�
        /// </summary>
        public void CloseAllPopups()
        {
            while (popupStack.Count > 0)
            {
                ClosePopup();
            }
        }

        /// <summary>
        /// Ư�� �׷��� ��� UI �ݱ�
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
        /// ���׸� �˾� UI ǥ�� (�񵿱� ���� - Resources �������� �ε�)
        /// </summary>
        public void ShowPopUpAsync<T>(System.Action<T> onComplete = null) where T : BaseUI
        {
            StartCoroutine(ShowPopUpAsyncCoroutine<T>(onComplete));
        }

        private System.Collections.IEnumerator ShowPopUpAsyncCoroutine<T>(System.Action<T> onComplete) where T : BaseUI
        {
            string prefabName = typeof(T).Name;
            Debug.Log($"[UIManager] Resources���� {prefabName} ������ �ε� ����");
            
            // Resources �������� ������ �ε�
            GameObject prefab = Resources.Load<GameObject>($"UI/Popup/{prefabName}");
            
            if (prefab == null)
            {
                // �ٸ� ��ο����� �õ�
                prefab = Resources.Load<GameObject>($"UI/{prefabName}");
            }
            
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Resources���� {prefabName} �������� ã�� �� �����ϴ�.");
                onComplete?.Invoke(null);
                yield break;
            }
            
            // �˾� �ν��Ͻ� ����
            GameObject popupInstance = Instantiate(prefab, popupCanvas.transform);
            T popupUI = popupInstance.GetComponent<T>();
            
            if (popupUI == null)
            {
                Debug.LogError($"[UIManager] {prefabName}�� {typeof(T).Name} ������Ʈ�� �����ϴ�.");
                Destroy(popupInstance);
                onComplete?.Invoke(null);
                yield break;
            }
            
            // �˾� ���ÿ� �߰�
            OpenPopup(popupUI);
            
            Debug.Log($"[UIManager] {prefabName} �˾� ���� �Ϸ�");
            onComplete?.Invoke(popupUI);
        }

        /// <summary>
        /// ���׸� �˾� UI ǥ�� (���� ���� - ���̺� ���) (Resources �������� �ε�)
        /// </summary>
        public void ShowPopUpByLabel<T>(string label, System.Action<T> onComplete = null) where T : BaseUI
        {
            StartCoroutine(ShowPopUpByLabelCoroutine<T>(label, onComplete));
        }

        private System.Collections.IEnumerator ShowPopUpByLabelCoroutine<T>(string label, System.Action<T> onComplete) where T : BaseUI
        {
            Debug.Log($"[UIManager] Resources���� {label} �󺧷� {typeof(T).Name} ������ �ε� ����");
            
            // Resources �������� �ش� ���� ��� ������ �ε�
            GameObject[] prefabs = Resources.LoadAll<GameObject>($"UI/{label}");
            
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogError($"[UIManager] Resources���� {label} ���� �������� ã�� �� �����ϴ�.");
                onComplete?.Invoke(null);
                yield break;
            }
            
            // T Ÿ���� ������ ã��
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
                Debug.LogError($"[UIManager] {label} �󺧿��� {typeof(T).Name} Ÿ���� �������� ã�� �� �����ϴ�.");
                onComplete?.Invoke(null);
                yield break;
            }
            
            // �˾� �ν��Ͻ� ����
            GameObject popupInstance = Instantiate(targetPrefab, popupCanvas.transform);
            T popupUI = popupInstance.GetComponent<T>();
            
            // �˾� ���ÿ� �߰�
            OpenPopup(popupUI);
            
            Debug.Log($"[UIManager] {label} �󺧷� {typeof(T).Name} �˾� ���� �Ϸ�");
            onComplete?.Invoke(popupUI);
        }

        /// <summary>
        /// ���׸� �˾� UI ǥ�� (���� ȣȯ���� ���� ���� - Resources �������� �ε�)
        /// </summary>
        public T ShowPopUp<T>() where T : BaseUI
        {
            T result = null;
            string prefabName = typeof(T).Name;
            
            // Resources �������� ������ �ε�
            GameObject prefab = Resources.Load<GameObject>($"UI/Popup/{prefabName}");
            
            if (prefab == null)
            {
                // �ٸ� ��ο����� �õ�
                prefab = Resources.Load<GameObject>($"UI/{prefabName}");
            }
            
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Resources���� {prefabName} �������� ã�� �� �����ϴ�.");
                return null;
            }
            
            // �˾� �ν��Ͻ� ����
            GameObject popupInstance = Instantiate(prefab, popupCanvas.transform);
            result = popupInstance.GetComponent<T>();
            
            if (result == null)
            {
                Debug.LogError($"[UIManager] {prefabName}�� {typeof(T).Name} ������Ʈ�� �����ϴ�.");
                Destroy(popupInstance);
                return null;
            }
            
            // �˾� ���ÿ� �߰�
            OpenPopup(result);
            
            Debug.Log($"[UIManager] {prefabName} �˾� ���� �Ϸ� (����)");
            return result;
        }

        /// <summary>
        /// �˾� �ݱ�
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
        /// ��� �˾� ����
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
        /// ��� UI ����
        /// </summary>
        public void CleanAllUI()
        {
            Debug.Log("[UIManager] CleanAllUI ����");
            
            CloseAllPanels();
            CloseAllPopups();
            
            if (PopUp != null)
            {
                PopUp.ForceCleanAll();
            }
            
            mainPanels.Clear();
            Debug.Log("[UIManager] ��� UI�� �����Ǿ����ϴ�.");
        }

        #endregion

        #region Scene Management

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[UIManager] �� �ε�: {scene.name}");
            
            // �� ��ȯ �� ��� UI ����
            if (PopUp != null)
            {
                PopUp.ForceCleanAll();
            }
            
            // Stack �ʱ�ȭ
            panelStack.Clear();
            popupStack.Clear();
            mainPanels.Clear();
            
            // ���� ������ ��ȯ�Ǵ� ��� �ε� ���Ŀ �����
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
                    Debug.Log("[UIManager] �� UI�� ESC�� ���� �� �����ϴ�.");
                    return;
                }

                // �켱����: �˾� > �г� > ���� �˾�
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
                    ClosePopUp(); // ���� �˾� �ý���
                }
                
                Util.ConsumeESC();
            }
            Util.ResetESC();
        }

        private bool IsCurrentUINonClosable()
        {
            // �˾� ���� Ȯ��
            if (popupStack.Count > 0)
            {
                BaseUI topPopup = popupStack.Peek();
                if (!topPopup.CanCloseWithESC)
                    return true;
            }
            
            // �г� ���� Ȯ��
            if (panelStack.Count > 0)
            {
                BaseUI topPanel = panelStack.Peek();
                if (!topPanel.CanCloseWithESC)
                    return true;
            }
            
            return false;
        }

        #endregion

        #region Legacy Support (���� ȣȯ��)

        // ���� ���� �г� ���
        public void RegisterMainPanel(string panelName, GameObject panel)
        {
            if (string.IsNullOrEmpty(panelName) || panel == null)
            {
                Debug.LogError("[UIManager] �г� �̸��̳� GameObject�� null�Դϴ�.");
                return;
            }

            if (mainPanels.ContainsKey(panelName))
            {
                Debug.LogWarning($"[UIManager] �̹� ��ϵ� ���� �г�: {panelName}");
                mainPanels[panelName] = panel;
            }
            else
            {
                mainPanels.Add(panelName, panel);
            }
        }

        // ���� ���� �г� ����
        public void UnregisterMainPanel(string panelName)
        {
            if (mainPanels.Remove(panelName))
            {
                Debug.Log($"[UIManager] ���� �г� ����: {panelName}");
            }
        }

        // ���� ���� �г� ��������
        public GameObject GetMainPanel(string panelName)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                Debug.LogError("[UIManager] �г� �̸��� null�Դϴ�.");
                return null;
            }

            if (mainPanels.TryGetValue(panelName, out GameObject panel))
            {
                return panel;
            }
            Debug.LogWarning($"[UIManager] ��ϵ��� ���� ���� �г�: {panelName}");
            return null;
        }

        // ��� ���� �г� ��������
        public Dictionary<string, GameObject> GetAllMainPanels()
        {
            return new Dictionary<string, GameObject>(mainPanels);
        }

        #endregion

        #region Confirmation Popup

        // Ȯ�� �˾� ǥ�� (�񵿱�)
        public void ShowConfirmPopUpAsync(string message, string confirmText = "Ȯ��", string cancelText = "���",
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

        // ������ Ȯ�� �˾� (�񵿱�)
        public void ShowConfirmPopUpAsync(string message, System.Action confirmCallback, System.Action<CheckPopUp> onComplete = null)
        {
            ShowConfirmPopUpAsync(message, "Ȯ��", "���", confirmCallback, null, onComplete);
        }

        // Ȯ�� �˾� ǥ�� (���� ȣȯ���� ���� ����)
        public CheckPopUp ShowConfirmPopUp(string message, string confirmText = "Ȯ��", string cancelText = "���",
                                          System.Action confirmCallback = null, System.Action cancelCallback = null)
        {
            CheckPopUp result = null;
            ShowConfirmPopUpAsync(message, confirmText, cancelText, confirmCallback, cancelCallback, (popup) => result = popup);
            return result;
        }

        // ������ Ȯ�� �˾� (���� ȣȯ���� ���� ����)
        public CheckPopUp ShowConfirmPopUp(string message, System.Action confirmCallback)
        {
            return ShowConfirmPopUp(message, "Ȯ��", "���", confirmCallback, null);
        }

        #endregion

        #region Utility Methods

        // Ư�� Ÿ���� Ȱ�� �˾� ã��
        public T FindActivePopUp<T>() where T : BaseUI
        {
            if (PopUp == null) return null;
            return PopUp.GetComponentInChildren<T>();
        }

        // Ư�� �˾� ������Ʈ
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
        /// ��巹������� UI ������ �̸� �ε� (�ӽ�)
        /// </summary>
        public void PreloadUIPrefabs(string label, System.Action onComplete = null)
        {
            Debug.LogWarning($"[UIManager] {label} �� - Addressables ������ �Ϸ���� �ʾ� �̸� �ε带 �ǳʶݴϴ�.");
            onComplete?.Invoke();
        }

        /// <summary>
        /// Ư�� UI ������ �̸� �ε� (�ӽ�)
        /// </summary>
        public void PreloadUIPrefab<T>(System.Action onComplete = null) where T : BaseUI
        {
            Debug.LogWarning($"[UIManager] {typeof(T).Name} - Addressables ������ �Ϸ���� �ʾ� �̸� �ε带 �ǳʶݴϴ�.");
            onComplete?.Invoke();
        }

        // ���� �� �ε� ����
        public void StartGameSceneLoad(string sceneName, int selectedGameIndex)
        {
            Debug.Log($"[UIManager] StartGameSceneLoad ȣ�� - Scene: {sceneName}, GameIndex: {selectedGameIndex}");
        }

        #endregion
    }
}
