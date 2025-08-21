using UnityEngine;
using UnityEngine.UI;

namespace KYS
{
    /// <summary>
    /// 씬 로딩과 LoadingScreen 연동 사용 예제
    /// </summary>
    public class SceneLoadingExamples : MonoBehaviour
    {
        [Header("Scene Loading Examples")]
        [SerializeField] private Button loadSceneButton;
        [SerializeField] private Button loadSceneWithLoadingButton;
        [SerializeField] private Button loadSceneWithoutLoadingButton;
        
        [Header("Scene Names")]
        [SerializeField] private string targetSceneName = "SampleScene";
        [SerializeField] private int targetSceneIndex = 1;
        
        private void Start()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (loadSceneButton != null)
            {
                loadSceneButton.onClick.AddListener(LoadSceneWithLoading);
            }
            
            if (loadSceneWithLoadingButton != null)
            {
                loadSceneWithLoadingButton.onClick.AddListener(LoadSceneWithLoading);
            }
            
            if (loadSceneWithoutLoadingButton != null)
            {
                loadSceneWithoutLoadingButton.onClick.AddListener(LoadSceneWithoutLoading);
            }
        }
        
        #region Scene Loading Examples
        
        /// <summary>
        /// 로딩 화면과 함께 씬 로딩
        /// </summary>
        [ContextMenu("Load Scene With Loading Screen")]
        public void LoadSceneWithLoading()
        {
            Debug.Log("[SceneLoadingExamples] 로딩 화면과 함께 씬 로딩 시작");
            
            // 방법 1: SceneLoadingManager 사용
            SceneLoadingManager.LoadScene(targetSceneName, true);
            
            // 방법 2: 직접 SceneLoadingManager 인스턴스 사용
            // SceneLoadingManager manager = FindObjectOfType<SceneLoadingManager>();
            // if (manager != null)
            // {
            //     manager.LoadSceneAsync(targetSceneName, true);
            // }
        }
        
        /// <summary>
        /// 로딩 화면 없이 씬 로딩
        /// </summary>
        [ContextMenu("Load Scene Without Loading Screen")]
        public void LoadSceneWithoutLoading()
        {
            Debug.Log("[SceneLoadingExamples] 로딩 화면 없이 씬 로딩 시작");
            
            SceneLoadingManager.LoadScene(targetSceneName, false);
        }
        
        /// <summary>
        /// 씬 인덱스로 로딩
        /// </summary>
        [ContextMenu("Load Scene By Index")]
        public void LoadSceneByIndex()
        {
            Debug.Log($"[SceneLoadingExamples] 씬 인덱스 {targetSceneIndex}로 로딩 시작");
            
            SceneLoadingManager.LoadScene(targetSceneIndex, true);
        }
        
        #endregion
        
        #region Custom Loading Examples
        
        /// <summary>
        /// 커스텀 로딩 화면과 함께 씬 로딩
        /// </summary>
        [ContextMenu("Load Scene With Custom Loading")]
        public void LoadSceneWithCustomLoading()
        {
            StartCoroutine(CustomSceneLoadingCoroutine());
        }
        
        private System.Collections.IEnumerator CustomSceneLoadingCoroutine()
        {
            Debug.Log("[SceneLoadingExamples] 커스텀 로딩 화면과 함께 씬 로딩 시작");
            
            // 커스텀 로딩 화면 표시
            LoadingScreen.ShowLoadingScreen("게임을 시작하는 중...");
            
            yield return new WaitForSeconds(0.1f);
            
            LoadingScreen loadingScreen = FindObjectOfType<LoadingScreen>();
            if (loadingScreen != null)
            {
                // 이벤트 구독
                loadingScreen.OnProgressChanged += OnCustomProgressChanged;
                loadingScreen.OnLoadingComplete += OnCustomLoadingComplete;
                
                // 커스텀 메시지 설정
                string[] customMessages = {
                    "게임 데이터를 초기화하는 중...",
                    "플레이어 정보를 불러오는 중...",
                    "월드 맵을 생성하는 중...",
                    "NPC들을 배치하는 중...",
                    "완료!"
                };
                
                // 씬 로딩 시작
                AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetSceneName);
                asyncLoad.allowSceneActivation = false;
                
                float progress = 0f;
                int messageIndex = 0;
                
                while (asyncLoad.progress < 0.9f)
                {
                    progress = asyncLoad.progress / 0.9f;
                    
                    // 진행률 업데이트
                    loadingScreen.SetProgress(progress);
                    
                    // 메시지 업데이트
                    int newMessageIndex = Mathf.FloorToInt(progress * (customMessages.Length - 1));
                    if (newMessageIndex != messageIndex && newMessageIndex < customMessages.Length)
                    {
                        messageIndex = newMessageIndex;
                        loadingScreen.SetCenterMessage(customMessages[messageIndex]);
                    }
                    
                    yield return null;
                }
                
                // 완료 처리
                loadingScreen.SetProgress(1f);
                loadingScreen.SetCenterMessage(customMessages[customMessages.Length - 1]);
                
                yield return new WaitForSeconds(0.5f);
                
                // 씬 활성화
                asyncLoad.allowSceneActivation = true;
                
                // 이벤트 구독 해제
                loadingScreen.OnProgressChanged -= OnCustomProgressChanged;
                loadingScreen.OnLoadingComplete -= OnCustomLoadingComplete;
            }
        }
        
        /// <summary>
        /// 커스텀 진행률 변경 이벤트
        /// </summary>
        private void OnCustomProgressChanged(float progress)
        {
            Debug.Log($"[SceneLoadingExamples] 커스텀 진행률: {progress * 100:F1}%");
        }
        
        /// <summary>
        /// 커스텀 로딩 완료 이벤트
        /// </summary>
        private void OnCustomLoadingComplete()
        {
            Debug.Log("[SceneLoadingExamples] 커스텀 로딩 완료!");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// 현재 씬 정보 출력
        /// </summary>
        [ContextMenu("Print Current Scene Info")]
        public void PrintCurrentSceneInfo()
        {
            UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Debug.Log($"[SceneLoadingExamples] 현재 씬: {currentScene.name} (인덱스: {currentScene.buildIndex})");
        }
        
        /// <summary>
        /// 로딩 화면 즉시 숨기기
        /// </summary>
        [ContextMenu("Hide Loading Screen")]
        public void HideLoadingScreen()
        {
            LoadingScreen.HideLoadingScreen();
        }
        
        /// <summary>
        /// 로딩 화면 즉시 표시
        /// </summary>
        [ContextMenu("Show Loading Screen")]
        public void ShowLoadingScreen()
        {
            LoadingScreen.ShowLoadingScreen("테스트 로딩 중...");
        }
        
        #endregion
    }
}
