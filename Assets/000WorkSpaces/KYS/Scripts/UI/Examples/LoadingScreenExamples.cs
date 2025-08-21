using UnityEngine;
using System.Collections;

namespace KYS
{
    /// <summary>
    /// LoadingScreen 사용 예제 클래스
    /// </summary>
    public class LoadingScreenExamples : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool testOnStart = false;
        [SerializeField] private float testDuration = 5f;
        
        private LoadingScreen currentLoadingScreen;
        
        private void Start()
        {
            if (testOnStart)
            {
                StartCoroutine(TestLoadingScreen());
            }
        }
        
        /// <summary>
        /// 기본 로딩 화면 테스트
        /// </summary>
        [ContextMenu("Test Basic Loading")]
        public void TestBasicLoading()
        {
            StartCoroutine(TestLoadingScreen());
        }
        
        /// <summary>
        /// 진행률 업데이트 테스트
        /// </summary>
        [ContextMenu("Test Progress Loading")]
        public void TestProgressLoading()
        {
            StartCoroutine(TestProgressLoadingCoroutine());
        }
        
        /// <summary>
        /// 커스텀 메시지 테스트
        /// </summary>
        [ContextMenu("Test Custom Messages")]
        public void TestCustomMessages()
        {
            StartCoroutine(TestCustomMessagesCoroutine());
        }
        
        private IEnumerator TestLoadingScreen()
        {
            Debug.Log("[LoadingScreenExamples] 기본 로딩 화면 테스트 시작");
            
            // 로딩 화면 표시
            LoadingScreen.ShowLoadingScreen("게임을 시작하는 중...");
            
            // 로딩 화면 인스턴스 가져오기
            yield return new WaitForSeconds(0.1f);
            currentLoadingScreen = FindObjectOfType<LoadingScreen>();
            
            if (currentLoadingScreen != null)
            {
                // 이벤트 구독
                currentLoadingScreen.OnProgressChanged += OnProgressChanged;
                currentLoadingScreen.OnLoadingComplete += OnLoadingComplete;
                
                // 진행률 시뮬레이션
                float progress = 0f;
                while (progress < 1f)
                {
                    progress += 0.1f;
                    currentLoadingScreen.SetProgress(progress);
                    yield return new WaitForSeconds(0.2f);
                }
                
                // 완료
                currentLoadingScreen.CompleteLoading();
            }
            
            yield return new WaitForSeconds(1f);
            
            // 로딩 화면 숨기기
            LoadingScreen.HideLoadingScreen();
            
            Debug.Log("[LoadingScreenExamples] 기본 로딩 화면 테스트 완료");
        }
        
        private IEnumerator TestProgressLoadingCoroutine()
        {
            Debug.Log("[LoadingScreenExamples] 진행률 로딩 테스트 시작");
            
            LoadingScreen.ShowLoadingScreen("데이터를 불러오는 중...");
            
            yield return new WaitForSeconds(0.1f);
            currentLoadingScreen = FindObjectOfType<LoadingScreen>();
            
            if (currentLoadingScreen != null)
            {
                currentLoadingScreen.OnProgressChanged += OnProgressChanged;
                currentLoadingScreen.OnLoadingComplete += OnLoadingComplete;
                
                // 단계별 진행률 업데이트
                string[] steps = {
                    "초기화 중...",
                    "리소스 로드 중...",
                    "설정 적용 중...",
                    "완료!"
                };
                
                for (int i = 0; i < steps.Length; i++)
                {
                    currentLoadingScreen.SetCenterMessage(steps[i]);
                    currentLoadingScreen.SetProgress((float)i / (steps.Length - 1));
                    yield return new WaitForSeconds(1f);
                }
            }
            
            yield return new WaitForSeconds(1f);
            LoadingScreen.HideLoadingScreen();
            
            Debug.Log("[LoadingScreenExamples] 진행률 로딩 테스트 완료");
        }
        
        private IEnumerator TestCustomMessagesCoroutine()
        {
            Debug.Log("[LoadingScreenExamples] 커스텀 메시지 테스트 시작");
            
            LoadingScreen.ShowLoadingScreen("시스템 점검 중...");
            
            yield return new WaitForSeconds(0.1f);
            currentLoadingScreen = FindObjectOfType<LoadingScreen>();
            
            if (currentLoadingScreen != null)
            {
                currentLoadingScreen.OnProgressChanged += OnProgressChanged;
                currentLoadingScreen.OnLoadingComplete += OnLoadingComplete;
                
                // 자동 메시지 전환 비활성화
                currentLoadingScreen.ToggleAutoMessageSwitch(false);
                
                // 수동으로 메시지 변경
                string[] customMessages = {
                    "네트워크 연결 확인 중...",
                    "서버에 연결 중...",
                    "사용자 정보 확인 중...",
                    "게임 데이터 동기화 중...",
                    "완료되었습니다!"
                };
                
                for (int i = 0; i < customMessages.Length; i++)
                {
                    currentLoadingScreen.SetCenterMessage(customMessages[i]);
                    currentLoadingScreen.SetProgress((float)i / (customMessages.Length - 1));
                    yield return new WaitForSeconds(1.5f);
                }
            }
            
            yield return new WaitForSeconds(1f);
            LoadingScreen.HideLoadingScreen();
            
            Debug.Log("[LoadingScreenExamples] 커스텀 메시지 테스트 완료");
        }
        
        /// <summary>
        /// 진행률 변경 이벤트 핸들러
        /// </summary>
        private void OnProgressChanged(float progress)
        {
            Debug.Log($"[LoadingScreenExamples] 진행률: {progress * 100:F1}%");
        }
        
        /// <summary>
        /// 로딩 완료 이벤트 핸들러
        /// </summary>
        private void OnLoadingComplete()
        {
            Debug.Log("[LoadingScreenExamples] 로딩이 완료되었습니다!");
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
    }
}
