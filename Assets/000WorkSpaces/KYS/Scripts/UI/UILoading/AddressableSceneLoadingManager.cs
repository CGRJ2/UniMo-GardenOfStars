using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace KYS
{
    /// <summary>
    /// Addressables를 사용한 씬 로딩과 LoadingScreen을 연동하는 매니저
    /// </summary>
    public class AddressableSceneLoadingManager : MonoBehaviour
    {
        [Header("Addressable Scene Settings")]
        [SerializeField] private AssetReference[] sceneReferences; // Addressable 씬 참조들
        
        [Header("Loading Screen Settings")]
        [SerializeField] private float minLoadingTime = 2f; // 최소 로딩 시간을 2초로 증가
        [SerializeField] private bool simulateProgress = true;
        [SerializeField] private float loadingScreenInitDelay = 0.5f; // 로딩 화면 초기화 대기 시간
        [SerializeField] private float loadingScreenHideDelay = 1f; // 로딩 화면 숨김 지연 시간
        [SerializeField] private bool waitForUIInitialization = true; // UI 초기화 완료 대기 여부
        
        [Header("Scene Loading Messages (Optional)")]
        [SerializeField] private bool useCustomMessages = false; // 커스텀 메시지 사용 여부
        [SerializeField] private string[] sceneLoadingMessages = {
            "씬을 불러오는 중...",
            "오브젝트들을 초기화하는 중...",
            "리소스를 로드하는 중...",
            "거의 완료되었습니다..."
        };

        // 다중 진행률 요소 지원
        [System.Serializable]
        public class LoadingStage
        {
            public string stageName;
            public float weight; // 전체 진행률에서의 비중 (0.0 ~ 1.0)
            public float currentProgress;
            public bool isCompleted;
        }
        
        [Header("Multi-Stage Loading (Optional)")]
        [SerializeField] private bool useMultiStageLoading = false;
        [SerializeField] private LoadingStage[] loadingStages = {
            new LoadingStage { stageName = "씬 로딩", weight = 0.4f },
            new LoadingStage { stageName = "리소스 다운로드", weight = 0.3f },
            new LoadingStage { stageName = "오브젝트 초기화", weight = 0.2f },
            new LoadingStage { stageName = "최종 설정", weight = 0.1f }
        };
        
        // 전체 진행률 계산
        public float GetTotalProgress()
        {
            if (!useMultiStageLoading)
            {
                return CurrentProgress; // 기존 단일 진행률 반환
            }
            
            float totalProgress = 0f;
            foreach (var stage in loadingStages)
            {
                totalProgress += stage.currentProgress * stage.weight;
            }
            return Mathf.Clamp01(totalProgress);
        }
        
        // 특정 단계 진행률 설정
        public void SetStageProgress(int stageIndex, float progress)
        {
            if (!useMultiStageLoading || stageIndex < 0 || stageIndex >= loadingStages.Length)
                return;
                
            loadingStages[stageIndex].currentProgress = Mathf.Clamp01(progress);
            loadingStages[stageIndex].isCompleted = (progress >= 1f);
            
            // 전체 진행률 업데이트
            CurrentProgress = GetTotalProgress();
            OnProgressUpdated?.Invoke(CurrentProgress);
            
            Debug.Log($"[AddressableSceneLoadingManager] 단계 {stageIndex} 진행률: {progress * 100:F1}% (전체: {CurrentProgress * 100:F1}%)");
        }

        private LoadingScreen currentLoadingScreen;
        private bool isLoadingScene = false;
        private AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> currentSceneHandle;

        // 이벤트
        public static System.Action<float> OnProgressUpdated;
        public static System.Action<string> OnMessageUpdated;
        public static System.Action OnLoadingStarted;
        public static System.Action OnLoadingCompleted;
        
        // 현재 진행률 정보
        public float CurrentProgress { get; private set; }
        public string CurrentMessage { get; private set; }
        public bool IsLoading => isLoadingScene;
        
        // 현재 로딩 핸들 정보
        public AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> CurrentHandle => currentSceneHandle;

        private void Awake()
        {
            // 싱글톤 패턴
            if (FindObjectsOfType<AddressableSceneLoadingManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            
            // 초기 진행률을 0%로 설정
            CurrentProgress = 0f;
            CurrentMessage = "";
        }

        private void Start()
        {
            // 씬 로딩 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            // 현재 로딩 중인 씬 핸들 해제
            if (currentSceneHandle.IsValid())
            {
                try
                {
                    Addressables.Release(currentSceneHandle);
                    Debug.Log("[AddressableSceneLoadingManager] 씬 핸들 해제 완료");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[AddressableSceneLoadingManager] 씬 핸들 해제 중 오류: {e.Message}");
                }
            }
            
            // 이벤트 정리
            OnProgressUpdated = null;
            OnMessageUpdated = null;
            OnLoadingStarted = null;
            OnLoadingCompleted = null;
        }

        #region Addressable Scene Loading Methods

        /// <summary>
        /// Addressable 씬 로딩 시작 (AssetReference 사용)
        /// </summary>
        public void LoadAddressableSceneAsync(AssetReference sceneReference, bool showLoading = true)
        {
            if (isLoadingScene) return;

            StartCoroutine(LoadAddressableSceneCoroutine(sceneReference, showLoading));
        }

        /// <summary>
        /// Addressable 씬 로딩 시작 (씬 이름 사용)
        /// </summary>
        public void LoadAddressableSceneAsync(string sceneName, bool showLoading = true)
        {
            if (isLoadingScene) return;

            StartCoroutine(LoadAddressableSceneByNameCoroutine(sceneName, showLoading));
        }

        /// <summary>
        /// Addressable 씬 로딩 시작 (인덱스 사용)
        /// </summary>
        public void LoadAddressableSceneAsync(int sceneIndex, bool showLoading = true)
        {
            if (isLoadingScene || sceneIndex < 0 || sceneIndex >= sceneReferences.Length) return;

            LoadAddressableSceneAsync(sceneReferences[sceneIndex], showLoading);
        }

        /// <summary>
        /// Addressable 씬 로딩 코루틴 (AssetReference)
        /// </summary>
        private IEnumerator LoadAddressableSceneCoroutine(AssetReference sceneReference, bool showLoading)
        {
            isLoadingScene = true;
            
            // 로딩 시작 시 진행률 완전 초기화
            CurrentProgress = 0f;
            CurrentMessage = "";
            
            // 이벤트 발생으로 외부에 초기화 알림
            OnProgressUpdated?.Invoke(0f);
            OnMessageUpdated?.Invoke("");
            
            Debug.Log("[AddressableSceneLoadingManager] 로딩 시작 - 진행률 완전 초기화");

            if (showLoading)
            {
                // 씬 전환 중 Addressable 시스템 안정성을 위한 추가 대기
                yield return new WaitForSeconds(0.2f);
                
                // 로딩 화면 표시 (UIManager의 전용 메서드 사용)
                string initialMessage = useCustomMessages ? sceneLoadingMessages[0] : null;
                UIManager.Instance.ShowLoadingScreen(initialMessage);
                
                // 로딩 화면 초기화 대기 (UI 컴포넌트들이 준비될 때까지)
                yield return new WaitForSeconds(loadingScreenInitDelay);

                currentLoadingScreen = UIManager.Instance.GetCurrentLoadingScreen();
                if (currentLoadingScreen != null)
                {
                    currentLoadingScreen.OnLoadingComplete += OnLoadingComplete;
                    Debug.Log("[AddressableSceneLoadingManager] 로딩 화면 초기화 완료");
                }
                else
                {
                    Debug.LogWarning("[AddressableSceneLoadingManager] LoadingScreen을 찾을 수 없습니다.");
                }
            }

            float startTime = Time.time;

            // Addressable 씬 로딩 시작
            currentSceneHandle = Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Single, false);

            // 로딩 진행률 업데이트
            while (currentSceneHandle.Status == AsyncOperationStatus.None)
            {
                float progress = currentSceneHandle.PercentComplete;
                
                // 진행률이 실제로 변경된 경우에만 업데이트
                if (Mathf.Abs(CurrentProgress - progress) > 0.001f)
                {
                    CurrentProgress = progress; // 현재 진행률 저장
                    Debug.Log($"[AddressableSceneLoadingManager] 진행률 변경: {CurrentProgress * 100:F1}%");
                }

                if (showLoading && currentLoadingScreen != null)
                {
                    Debug.Log($"[AddressableSceneLoadingManager] 진행률 업데이트: {progress * 100}%");
                    
                    // 진행률 시뮬레이션
                    float displayProgress;
                    if (simulateProgress)
                    {
                        displayProgress = Mathf.Lerp(0f, 0.9f, progress);
                        Debug.Log($"[AddressableSceneLoadingManager] 시뮬레이션 진행률: {displayProgress * 100}%");
                    }
                    else
                    {
                        displayProgress = Mathf.Lerp(0f, 0.95f, progress);
                        Debug.Log($"[AddressableSceneLoadingManager] 실제 진행률 (표시): {displayProgress * 100}%");
                    }
                    
                    currentLoadingScreen.SetProgress(displayProgress);

                    // 메시지 업데이트 (커스텀 메시지 사용 시에만)
                    if (useCustomMessages)
                    {
                        int messageIndex = Mathf.FloorToInt(progress * (sceneLoadingMessages.Length - 1));
                        if (messageIndex < sceneLoadingMessages.Length)
                        {
                            string message = sceneLoadingMessages[messageIndex];
                            CurrentMessage = message;
                            currentLoadingScreen.SetCenterMessage(message);
                            OnMessageUpdated?.Invoke(message); // 이벤트 발생
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[AddressableSceneLoadingManager] showLoading: {showLoading}, currentLoadingScreen: {currentLoadingScreen != null}");
                }
                
                // 진행률 이벤트 발생
                OnProgressUpdated?.Invoke(CurrentProgress);

                yield return null;
            }

            // 로딩 완료 확인
            if (currentSceneHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // 최소 로딩 시간 보장 (사용자 경험 개선)
                float elapsedTime = Time.time - startTime;
                float remainingTime = minLoadingTime - elapsedTime;
                
                if (remainingTime > 0)
                {
                    Debug.Log($"[AddressableSceneLoadingManager] 최소 로딩 시간 보장: {remainingTime:F2}초 대기");
                    yield return new WaitForSeconds(remainingTime);
                }

                // 로딩 완료 처리
                if (showLoading && currentLoadingScreen != null)
                {
                    currentLoadingScreen.SetProgress(1f);
                    yield return new WaitForSeconds(0.5f);
                    
                    Debug.Log("[AddressableSceneLoadingManager] 로딩 완료 처리 완료 - 씬 활성화 대기");
                }

                // 씬 활성화
                var sceneInstance = currentSceneHandle.Result;
                sceneInstance.ActivateAsync();
            }
            else
            {
                Debug.LogError($"[AddressableSceneLoadingManager] 씬 로딩 실패: {currentSceneHandle.OperationException?.Message}");

                if (showLoading && currentLoadingScreen != null)
                {
                    currentLoadingScreen.SetCenterMessage("로딩 실패!");
                    yield return new WaitForSeconds(2f);
                }
            }

            isLoadingScene = false;
        }

        /// <summary>
        /// Addressable 씬 로딩 코루틴 (씬 이름)
        /// </summary>
        private IEnumerator LoadAddressableSceneByNameCoroutine(string sceneName, bool showLoading)
        {
            isLoadingScene = true;

            if (showLoading)
            {
                // 로딩 화면 표시 (UIManager의 전용 메서드 사용)
                string initialMessage = useCustomMessages ? sceneLoadingMessages[0] : null;
                UIManager.Instance.ShowLoadingScreen(initialMessage);
                yield return new WaitForSeconds(0.1f);

                currentLoadingScreen = UIManager.Instance.GetCurrentLoadingScreen();
                if (currentLoadingScreen != null)
                {
                    currentLoadingScreen.OnLoadingComplete += OnLoadingComplete;
                }
            }

            float startTime = Time.time;

            // Addressable 씬 로딩 시작 (씬 이름으로)
            currentSceneHandle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single, false);

            while (currentSceneHandle.Status == AsyncOperationStatus.None)
            {
                float progress = currentSceneHandle.PercentComplete;

                if (showLoading && currentLoadingScreen != null)
                {
                    Debug.Log($"[AddressableSceneLoadingManager] 진행률 업데이트: {progress * 100}%");
                    
                    // 실제 로딩 시에도 시뮬레이션 적용 (사용자 경험 개선)
                    float displayProgress;
                    if (simulateProgress)
                    {
                        displayProgress = Mathf.Lerp(0f, 0.9f, progress);
                        Debug.Log($"[AddressableSceneLoadingManager] 시뮬레이션 진행률: {displayProgress * 100}%");
                    }
                    else
                    {
                        // 실제 진행률을 더 부드럽게 표시
                        displayProgress = Mathf.Lerp(0f, 0.95f, progress);
                        Debug.Log($"[AddressableSceneLoadingManager] 실제 진행률 (표시): {displayProgress * 100}%");
                    }
                    
                    currentLoadingScreen.SetProgress(displayProgress);

                    // 메시지 업데이트 (커스텀 메시지 사용 시에만)
                    if (useCustomMessages)
                    {
                        int messageIndex = Mathf.FloorToInt(progress * (sceneLoadingMessages.Length - 1));
                        if (messageIndex < sceneLoadingMessages.Length)
                        {
                            currentLoadingScreen.SetCenterMessage(sceneLoadingMessages[messageIndex]);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[AddressableSceneLoadingManager] showLoading: {showLoading}, currentLoadingScreen: {currentLoadingScreen != null}");
                }

                yield return null;
            }

            if (currentSceneHandle.Status == AsyncOperationStatus.Succeeded)
            {
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < minLoadingTime)
                {
                    yield return new WaitForSeconds(minLoadingTime - elapsedTime);
                }

                if (showLoading && currentLoadingScreen != null)
                {
                    currentLoadingScreen.SetProgress(1f);
                    yield return new WaitForSeconds(0.5f);
                }

                var sceneInstance = currentSceneHandle.Result;
                sceneInstance.ActivateAsync();
            }
            else
            {
                Debug.LogError($"[AddressableSceneLoadingManager] 씬 로딩 실패: {currentSceneHandle.OperationException?.Message}");

                if (showLoading && currentLoadingScreen != null)
                {
                    currentLoadingScreen.SetCenterMessage("로딩 실패!");
                    yield return new WaitForSeconds(2f);
                }
            }

            isLoadingScene = false;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 씬 로딩 완료 이벤트
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[AddressableSceneLoadingManager] 씬 로딩 완료: {scene.name}");

            // 로딩 화면 지연 숨김 시작
            StartCoroutine(HideLoadingScreenWithDelay());
        }
        
        /// <summary>
        /// 로딩 화면을 지연 후 숨기는 코루틴
        /// </summary>
        private IEnumerator HideLoadingScreenWithDelay()
        {
            Debug.Log($"[AddressableSceneLoadingManager] 로딩 화면 지연 숨김 시작: {loadingScreenHideDelay}초 대기");
            
            // 지정된 시간만큼 대기
            yield return new WaitForSeconds(loadingScreenHideDelay);
            
            // 추가로 UI 초기화 완료를 기다림
            if (waitForUIInitialization)
            {
                yield return StartCoroutine(WaitForUIInitialization());
            }
            
            // 로딩 화면 숨기기 (UIManager의 전용 메서드 사용)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideLoadingScreen();
                Debug.Log("[AddressableSceneLoadingManager] UIManager를 통해 로딩 화면 숨김");
            }
            else
            {
                LoadingScreen.HideLoadingScreen();
                Debug.Log("[AddressableSceneLoadingManager] LoadingScreen 정적 메서드로 로딩 화면 숨김");
            }
            
            currentLoadingScreen = null;
            Debug.Log("[AddressableSceneLoadingManager] 로딩 화면 숨김 완료");
        }
        
        /// <summary>
        /// UI 초기화 완료를 기다리는 코루틴
        /// </summary>
        private IEnumerator WaitForUIInitialization()
        {
            Debug.Log("[AddressableSceneLoadingManager] UI 초기화 완료 대기 시작");
            
            // UIManager가 준비될 때까지 대기
            float waitTime = 0f;
            const float maxWaitTime = 3f; // 최대 3초 대기
            
            while (UIManager.Instance == null && waitTime < maxWaitTime)
            {
                waitTime += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
            
            if (UIManager.Instance != null)
            {
                Debug.Log("[AddressableSceneLoadingManager] UIManager 초기화 완료 감지");
                
                // 추가로 한 프레임 더 대기 (UI 컴포넌트들이 완전히 준비되도록)
                yield return null;
            }
            else
            {
                Debug.LogWarning("[AddressableSceneLoadingManager] UIManager 초기화 타임아웃");
            }
            
            Debug.Log("[AddressableSceneLoadingManager] UI 초기화 대기 완료");
        }

        /// <summary>
        /// 씬 언로딩 이벤트
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"[AddressableSceneLoadingManager] 씬 언로딩: {scene.name}");
        }

        /// <summary>
        /// 로딩 완료 이벤트
        /// </summary>
        private void OnLoadingComplete()
        {
            Debug.Log("[AddressableSceneLoadingManager] 로딩 완료 이벤트 발생");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 현재 로딩 상태 확인
        /// </summary>
        public bool IsLoadingScene => isLoadingScene;

        /// <summary>
        /// 로딩 화면 즉시 숨기기
        /// </summary>
        public void HideLoadingScreen()
        {
            LoadingScreen.HideLoadingScreen();
        }

        /// <summary>
        /// 현재 로딩 중인 씬 해제
        /// </summary>
        public void UnloadCurrentScene()
        {
            if (currentSceneHandle.IsValid())
            {
                Addressables.UnloadSceneAsync(currentSceneHandle);
                currentSceneHandle = default;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Addressable 씬 로딩 (정적 메서드, AssetReference)
        /// </summary>
        public static void LoadAddressableScene(AssetReference sceneReference, bool showLoading = true)
        {
            AddressableSceneLoadingManager manager = FindObjectOfType<AddressableSceneLoadingManager>();
            if (manager != null)
            {
                manager.LoadAddressableSceneAsync(sceneReference, showLoading);
            }
            else
            {
                Debug.LogError("[AddressableSceneLoadingManager] AddressableSceneLoadingManager를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// Addressable 씬 로딩 (정적 메서드, 씬 이름)
        /// </summary>
        public static void LoadAddressableScene(string sceneName, bool showLoading = true)
        {
            AddressableSceneLoadingManager manager = FindObjectOfType<AddressableSceneLoadingManager>();
            if (manager != null)
            {
                manager.LoadAddressableSceneAsync(sceneName, showLoading);
            }
            else
            {
                Debug.LogError("[AddressableSceneLoadingManager] AddressableSceneLoadingManager를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// Addressable 씬 로딩 (정적 메서드, 인덱스)
        /// </summary>
        public static void LoadAddressableScene(int sceneIndex, bool showLoading = true)
        {
            AddressableSceneLoadingManager manager = FindObjectOfType<AddressableSceneLoadingManager>();
            if (manager != null)
            {
                manager.LoadAddressableSceneAsync(sceneIndex, showLoading);
            }
            else
            {
                Debug.LogError("[AddressableSceneLoadingManager] AddressableSceneLoadingManager를 찾을 수 없습니다.");
            }
        }

        #endregion

        [ContextMenu("SceneTrasition Test[1]")]
        public void SceneTrasitonTest1()
        {   // 씬 전환 테스트용 메서드
            LoadAddressableSceneAsync(sceneReferences[1], true);
        }

        [ContextMenu("SceneTrasition Test[0]")]
        public void SceneTrasitonTest2()
        {   // 씬 전환 테스트용 메서드
            LoadAddressableSceneAsync(sceneReferences[0], true);
        }
    }
}