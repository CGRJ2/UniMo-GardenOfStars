using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace KYS
{
    public class LoadingScreen : BaseUI
    {
        [Header("Loading Screen Elements")]
        [SerializeField] private Image[] loadingImages; // 전환 가능한 이미지들 (배경 이미지 포함)
        [SerializeField] private TextMeshProUGUI centerMessageText;
        [SerializeField] private Slider loadingProgressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        
        [Header("Image Fill Progress Settings")]
        [SerializeField] private Image fillProgressImage; // 체력바처럼 채워지는 이미지
        [SerializeField] private UnityEngine.UI.Image.Type fillImageType = UnityEngine.UI.Image.Type.Filled; // Filled 타입으로 설정
        [SerializeField] private UnityEngine.UI.Image.FillMethod fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal; // 가로 방향 채우기
        [SerializeField] private bool useImageFill = true; // Image Fill 사용 여부
        [SerializeField] private bool useSlider = false; // Slider 사용 여부 (기본값 false로 변경)
        
        [Header("Loading Settings")]
        [SerializeField] private float imageSwitchInterval = 2f; // 이미지 전환 간격
        [SerializeField] private float messageSwitchInterval = 3f; // 메시지 전환 간격
        [SerializeField] private bool autoSwitchImages = true;
        [SerializeField] private bool autoSwitchMessages = true;
        
        [Header("Loading Messages")]
        [SerializeField] private string[] loadingMessageKeys = {
            "loading_progress",
            "loading_wait",
            "loading_data",
            "loading_almost_done"
        };
        
        [Header("Fallback Messages (로컬라이제이션 실패 시 사용)")]
        [SerializeField] private string[] fallbackMessages = {
            "로딩 중...",
            "잠시만 기다려주세요...",
            "데이터를 불러오는 중...",
            "거의 완료되었습니다..."
        };
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableImageFade = true;
        [SerializeField] private float fadeDuration = 0.5f;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableProgressSimulation = true; // 테스트용 진행률 시뮬레이션
        [SerializeField] private bool enableExternalProgressMonitoring = true; // 외부 진행률 모니터링
        
        // 외부 모니터링 상태 관리
        private bool isExternalMonitoringActive = false;
        [SerializeField] private float minDisplayTime = 1.5f; // 최소 표시 시간 (초)
        
        // 외부 진행률 모니터링
        private Coroutine externalProgressCoroutine;
        private float screenStartTime; // 화면 표시 시작 시간
        
        // 내부 상태
        private int currentImageIndex = 0;
        private int currentMessageIndex = 0;
        private Coroutine imageSwitchCoroutine;
        private Coroutine messageSwitchCoroutine;
        private List<Coroutine> fadeCoroutines = new List<Coroutine>();
        
        // 이벤트
        public System.Action<float> OnProgressChanged;
        public System.Action OnLoadingComplete;
        
        protected override void Awake()
        {
            base.Awake();

        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // 초기화가 완료된 후 SetupLoadingScreen 호출
            StartCoroutine(InitializeAfterFrame());
        }
        
        private IEnumerator InitializeAfterFrame()
        {
            // 한 프레임 대기하여 모든 컴포넌트가 준비되도록 함
            yield return null;
            SetupLoadingScreen();
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            StopAllCoroutines();
            
            // 외부 진행률 모니터링 정리
            if (externalProgressCoroutine != null)
            {
                StopCoroutine(externalProgressCoroutine);
                externalProgressCoroutine = null;
            }
            
            // 외부 모니터링 상태 초기화
            isExternalMonitoringActive = false;
        }
        
        private void SetupLoadingScreen()
        {
            Debug.Log("[LoadingScreen] 로딩 화면 설정 시작");
            screenStartTime = Time.time; // 화면 표시 시작 시간 기록
            
            // 로딩 화면 시작 시 진행률을 강제로 0%로 초기화
            ForceResetProgress();
            
            // Image Fill 설정
            if (useImageFill && fillProgressImage != null)
            {
                fillProgressImage.type = fillImageType;
                fillProgressImage.fillMethod = fillMethod;
                fillProgressImage.fillAmount = 0f; // 초기값을 0으로 강제 설정
                fillProgressImage.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Left; // 왼쪽에서 시작
                Debug.Log("[LoadingScreen] Image Fill 설정 완료 - 초기 fillAmount: 0");
            }
            else if (useImageFill && fillProgressImage == null)
            {
                Debug.LogWarning("[LoadingScreen] useImageFill이 true이지만 fillProgressImage가 null입니다.");
            }
            
            // Slider 초기 상태 설정
            if (useSlider && loadingProgressBar != null)
            {
                loadingProgressBar.value = 0f;
                loadingProgressBar.minValue = 0f;
                loadingProgressBar.maxValue = 1f;
                Debug.Log("[LoadingScreen] Slider 설정 완료");
            }
            else if (useSlider && loadingProgressBar == null)
            {
                Debug.LogWarning("[LoadingScreen] useSlider가 true이지만 loadingProgressBar가 null입니다.");
            }
            
            if (progressText != null)
            {
                progressText.text = "0%";
                Debug.Log("[LoadingScreen] Progress Text 설정 완료");
            }
            else
            {
                Debug.LogWarning("[LoadingScreen] progressText가 null입니다.");
            }
            
            // 첫 번째 메시지 설정 (로컬라이제이션 사용)
            SetLoadingMessageByIndex(0);
            
            // 초기 이미지 설정 (첫 번째 이미지만 활성화)
            if (loadingImages.Length > 0)
            {
                for (int i = 0; i < loadingImages.Length; i++)
                {
                    if (loadingImages[i] != null)
                    {
                        loadingImages[i].gameObject.SetActive(i == 0);
                        Debug.Log($"[LoadingScreen] 초기 이미지 설정: 이미지 {i} {(i == 0 ? "활성화" : "비활성화")}");
                    }
                }
                currentImageIndex = 0;
                Debug.Log("[LoadingScreen] 초기 이미지 인덱스: 0");
            }
            else
            {
                Debug.LogWarning("[LoadingScreen] loadingImages 배열이 비어있습니다.");
            }
            
            // 자동 전환 시작
            if (autoSwitchImages && loadingImages.Length > 1)
            {
                imageSwitchCoroutine = StartCoroutine(SwitchImagesCoroutine());
                Debug.Log("[LoadingScreen] 자동 이미지 전환 시작");
            }
            
            if (autoSwitchMessages && loadingMessageKeys.Length > 1)
            {
                messageSwitchCoroutine = StartCoroutine(SwitchMessagesCoroutine());
                Debug.Log("[LoadingScreen] 자동 메시지 전환 시작");
            }
            
            // 외부 진행률 모니터링 시작
            if (enableExternalProgressMonitoring)
            {
                // 외부 모니터링 시작 전에 초기 진행률을 0%로 강제 설정
                SetProgress(0f);
                Debug.Log("[LoadingScreen] 외부 모니터링 시작 전 초기 진행률 0% 설정");
                
                isExternalMonitoringActive = true;
                externalProgressCoroutine = StartCoroutine(MonitorExternalProgress());
                Debug.Log("[LoadingScreen] 외부 진행률 모니터링 시작");
            }
            // 테스트용 진행률 시뮬레이션 시작 (실제 사용 시 제거)
            else if (enableProgressSimulation)
            {
                // 시뮬레이션 시작 전에 초기 진행률을 0%로 강제 설정
                SetProgress(0f);
                Debug.Log("[LoadingScreen] 시뮬레이션 시작 전 초기 진행률 0% 설정");
                
                StartCoroutine(SimulateProgressForTesting());
            }
            else
            {
                // 실제 사용 환경에서는 초기 진행률을 0%로 설정
                SetProgress(0f);
                Debug.Log("[LoadingScreen] 실제 사용 모드: 초기 진행률 0% 설정");
            }
            
            Debug.Log("[LoadingScreen] 로딩 화면 설정 완료");
        }
        
        #region Public Methods
        
        /// <summary>
        /// 로딩 진행률 설정 (0.0 ~ 1.0)
        /// </summary>
        public void SetProgress(float progress)
        {
            float previousProgress = 0f;
            
            // 이전 진행률 가져오기
            if (useImageFill && fillProgressImage != null)
            {
                previousProgress = fillProgressImage.fillAmount;
            }
            
            progress = Mathf.Clamp01(progress);
            
            // 진행률이 이전보다 낮으면 업데이트하지 않음 (뒤로 돌아가는 것 방지)
            if (progress < previousProgress)
            {
                Debug.Log($"[LoadingScreen] 진행률 뒤로 돌아가는 것 방지: {previousProgress * 100:F1}% → {progress * 100:F1}% (업데이트 건너뜀)");
                return;
            }
            
            Debug.Log($"[LoadingScreen] 진행률 설정: {previousProgress * 100:F1}% → {progress * 100:F1}%");
            Debug.Log($"[LoadingScreen] useImageFill: {useImageFill}, fillProgressImage: {fillProgressImage != null}");
            Debug.Log($"[LoadingScreen] useSlider: {useSlider}, loadingProgressBar: {loadingProgressBar != null}");
            Debug.Log($"[LoadingScreen] progressText: {progressText != null}");
            
            // Image Fill 방식으로 진행률 표시
            if (useImageFill && fillProgressImage != null)
            {
                float previousFillAmount = fillProgressImage.fillAmount;
                fillProgressImage.fillAmount = progress;
                Debug.Log($"[LoadingScreen] Image Fill 진행률 업데이트: {previousFillAmount:F3} → {progress:F3} ({progress * 100:F1}%)");
            }
            else if (useImageFill && fillProgressImage == null)
            {
                Debug.LogWarning("[LoadingScreen] useImageFill이 true이지만 fillProgressImage가 null입니다.");
            }
            
            // Slider 방식으로 진행률 표시
            if (useSlider && loadingProgressBar != null)
            {
                loadingProgressBar.value = progress;
                Debug.Log($"[LoadingScreen] Slider 진행률 업데이트: {progress}");
            }
            else if (useSlider && loadingProgressBar == null)
            {
                Debug.LogWarning("[LoadingScreen] useSlider가 true이지만 loadingProgressBar가 null입니다.");
            }
            
            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                Debug.Log($"[LoadingScreen] Progress Text 업데이트: {Mathf.RoundToInt(progress * 100)}%");
            }
            else
            {
                Debug.LogWarning("[LoadingScreen] progressText가 null입니다.");
            }
            
            OnProgressChanged?.Invoke(progress);
            
            // 100% 완료 시 완료 이벤트 호출
            if (progress >= 1f)
            {
                Debug.Log("[LoadingScreen] 로딩 완료 이벤트 호출");
                OnLoadingComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 중앙 메시지 설정
        /// </summary>
        public void SetCenterMessage(string message)
        {
            if (centerMessageText != null)
            {
                centerMessageText.text = message;
            }
        }

        /// <summary>
        /// 로컬라이제이션 키로 중앙 메시지 설정
        /// </summary>
        public void SetCenterMessageByKey(string localizationKey, string fallbackMessage = null)
        {
            string message = GetLocalizedMessage(localizationKey, fallbackMessage);
            SetCenterMessage(message);
        }

        /// <summary>
        /// 로컬라이제이션된 메시지 가져오기
        /// </summary>
        private string GetLocalizedMessage(string key, string fallback = null)
        {
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                string localizedText = LocalizationManager.Instance.GetText(key);
                if (!string.IsNullOrEmpty(localizedText) && localizedText != key)
                {
                    return localizedText;
                }
            }
            
            return fallback ?? key;
        }

        /// <summary>
        /// 특정 인덱스의 로딩 메시지 설정
        /// </summary>
        public void SetLoadingMessageByIndex(int index)
        {
            if (index >= 0 && index < loadingMessageKeys.Length)
            {
                string fallback = index < fallbackMessages.Length ? fallbackMessages[index] : null;
                SetCenterMessageByKey(loadingMessageKeys[index], fallback);
            }
        }
        
        /// <summary>
        /// 특정 이미지로 전환
        /// </summary>
        public void SwitchToImage(int imageIndex)
        {
            if (loadingImages == null || imageIndex < 0 || imageIndex >= loadingImages.Length)
            {
                Debug.LogWarning($"[LoadingScreen] 유효하지 않은 이미지 인덱스: {imageIndex}");
                return;
            }
            
            if (enableImageFade)
            {
                StartCoroutine(SwitchImageWithFade(imageIndex));
            }
            else
            {
                SwitchImageDirectly(imageIndex);
            }
        }
        
        /// <summary>
        /// 로딩 완료 처리
        /// </summary>
        public void CompleteLoading()
        {
            Debug.Log("[LoadingScreen] 로딩 완료");
            SetProgress(1f);
            OnLoadingComplete?.Invoke();
        }
        
        /// <summary>
        /// 자동 이미지 전환 토글
        /// </summary>
        public void ToggleAutoImageSwitch(bool enable)
        {
            autoSwitchImages = enable;
            
            if (enable && imageSwitchCoroutine == null)
            {
                imageSwitchCoroutine = StartCoroutine(SwitchImagesCoroutine());
            }
            else if (!enable && imageSwitchCoroutine != null)
            {
                StopCoroutine(imageSwitchCoroutine);
                imageSwitchCoroutine = null;
            }
        }
        
        /// <summary>
        /// 자동 메시지 전환 토글
        /// </summary>
        public void ToggleAutoMessageSwitch(bool enable)
        {
            autoSwitchMessages = enable;
            
            if (enable && messageSwitchCoroutine == null)
            {
                messageSwitchCoroutine = StartCoroutine(SwitchMessagesCoroutine());
            }
            else if (!enable && messageSwitchCoroutine != null)
            {
                StopCoroutine(messageSwitchCoroutine);
                messageSwitchCoroutine = null;
            }
        }

        #endregion

        #region Image Fill Progress Methods

        /// <summary>
        /// Image Fill 진행률 즉시 설정 (애니메이션 없음)
        /// </summary>
        public void SetFillProgressImmediate(float progress)
        {
            if (useImageFill && fillProgressImage != null)
            {
                fillProgressImage.fillAmount = Mathf.Clamp01(progress);
            }
        }

        /// <summary>
        /// Image Fill 진행률을 애니메이션으로 설정
        /// </summary>
        public void SetFillProgressAnimated(float targetProgress, float duration = 0.5f)
        {
            if (useImageFill && fillProgressImage != null)
            {
                StartCoroutine(AnimateFillProgress(targetProgress, duration));
            }
        }

        /// <summary>
        /// Image Fill 진행률 애니메이션 코루틴
        /// </summary>
        private IEnumerator AnimateFillProgress(float targetProgress, float duration)
        {
            float startProgress = fillProgressImage.fillAmount;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float currentProgress = Mathf.Lerp(startProgress, targetProgress, t);
                fillProgressImage.fillAmount = Mathf.Clamp01(currentProgress);
                yield return null;
            }

            fillProgressImage.fillAmount = Mathf.Clamp01(targetProgress);
        }

        /// <summary>
        /// Image Fill 방향 설정
        /// </summary>
        public void SetFillDirection(UnityEngine.UI.Image.FillMethod direction)
        {
            if (useImageFill && fillProgressImage != null)
            {
                fillProgressImage.fillMethod = direction;
                
                // 방향에 따른 Origin 설정
                switch (direction)
                {
                    case UnityEngine.UI.Image.FillMethod.Horizontal:
                        fillProgressImage.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Left;
                        break;
                    case UnityEngine.UI.Image.FillMethod.Vertical:
                        fillProgressImage.fillOrigin = (int)UnityEngine.UI.Image.OriginVertical.Bottom;
                        break;
                    case UnityEngine.UI.Image.FillMethod.Radial90:
                        fillProgressImage.fillOrigin = (int)UnityEngine.UI.Image.Origin90.BottomLeft;
                        break;
                    case UnityEngine.UI.Image.FillMethod.Radial180:
                        fillProgressImage.fillOrigin = (int)UnityEngine.UI.Image.Origin180.Bottom;
                        break;
                    case UnityEngine.UI.Image.FillMethod.Radial360:
                        fillProgressImage.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Bottom;
                        break;
                }
            }
        }

        /// <summary>
        /// Image Fill 색상 설정
        /// </summary>
        public void SetFillColor(Color color)
        {
            if (useImageFill && fillProgressImage != null)
            {
                fillProgressImage.color = color;
            }
        }

        /// <summary>
        /// Image Fill 색상을 진행률에 따라 그라데이션으로 설정
        /// </summary>
        public void SetFillColorGradient(Color startColor, Color endColor)
        {
            if (useImageFill && fillProgressImage != null)
            {
                float progress = fillProgressImage.fillAmount;
                fillProgressImage.color = Color.Lerp(startColor, endColor, progress);
            }
        }

        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 진행률을 강제로 0%로 초기화 (이전 데이터 클리어)
        /// </summary>
        private void ForceResetProgress()
        {
            Debug.Log("[LoadingScreen] 진행률 강제 초기화 시작");
            
            // Image Fill 강제 초기화
            if (useImageFill && fillProgressImage != null)
            {
                fillProgressImage.fillAmount = 0f;
                Debug.Log("[LoadingScreen] Image Fill 강제 초기화: 0%");
            }
            
            // Slider 강제 초기화
            if (useSlider && loadingProgressBar != null)
            {
                loadingProgressBar.value = 0f;
                Debug.Log("[LoadingScreen] Slider 강제 초기화: 0%");
            }
            
            // Progress Text 강제 초기화
            if (progressText != null)
            {
                progressText.text = "0%";
                Debug.Log("[LoadingScreen] Progress Text 강제 초기화: 0%");
            }
            
            Debug.Log("[LoadingScreen] 진행률 강제 초기화 완료");
        }
        
        private void SwitchImageDirectly(int imageIndex)
        {
            Debug.Log($"[LoadingScreen] 이미지 직접 전환: {imageIndex}번 이미지로 변경");
            
            // 모든 이미지 숨기기
            for (int i = 0; i < loadingImages.Length; i++)
            {
                if (loadingImages[i] != null)
                {
                    bool shouldBeActive = (i == imageIndex);
                    loadingImages[i].gameObject.SetActive(shouldBeActive);
                    Debug.Log($"[LoadingScreen] 이미지 {i}: {(shouldBeActive ? "활성화" : "비활성화")}");
                }
                else
                {
                    Debug.LogWarning($"[LoadingScreen] loadingImages[{i}]가 null입니다.");
                }
            }
            
            currentImageIndex = imageIndex;
            Debug.Log($"[LoadingScreen] 현재 이미지 인덱스: {currentImageIndex}");
        }
        
        private IEnumerator SwitchImageWithFade(int targetIndex)
        {
            Debug.Log($"[LoadingScreen] 이미지 페이드 전환: {currentImageIndex} → {targetIndex}");
            
            // 기존 페이드 코루틴들 정리
            foreach (var coroutine in fadeCoroutines)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            fadeCoroutines.Clear();
            
            // 현재 이미지 페이드 아웃
            if (loadingImages[currentImageIndex] != null)
            {
                Debug.Log($"[LoadingScreen] 현재 이미지({currentImageIndex}) 페이드 아웃 시작");
                fadeCoroutines.Add(StartCoroutine(FadeImage(loadingImages[currentImageIndex], 1f, 0f)));
            }
            else
            {
                Debug.LogWarning($"[LoadingScreen] 현재 이미지({currentImageIndex})가 null입니다.");
            }
            
            // 새 이미지 페이드 인
            if (loadingImages[targetIndex] != null)
            {
                Debug.Log($"[LoadingScreen] 새 이미지({targetIndex}) 페이드 인 시작");
                loadingImages[targetIndex].gameObject.SetActive(true);
                loadingImages[targetIndex].color = new Color(1f, 1f, 1f, 0f);
                fadeCoroutines.Add(StartCoroutine(FadeImage(loadingImages[targetIndex], 0f, 1f)));
            }
            else
            {
                Debug.LogWarning($"[LoadingScreen] 새 이미지({targetIndex})가 null입니다.");
            }
            
            currentImageIndex = targetIndex;
            
            yield return new WaitForSeconds(fadeDuration);
            Debug.Log($"[LoadingScreen] 이미지 페이드 전환 완료");
        }
        
        private IEnumerator FadeImage(Image image, float startAlpha, float targetAlpha)
        {
            float elapsed = 0f;
            Color color = image.color;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                image.color = color;
                yield return null;
            }
            
            color.a = targetAlpha;
            image.color = color;
            
            // 페이드 아웃 완료된 이미지 비활성화
            if (targetAlpha == 0f)
            {
                image.gameObject.SetActive(false);
            }
        }
        
        private IEnumerator SwitchImagesCoroutine()
        {
            Debug.Log($"[LoadingScreen] 자동 이미지 전환 코루틴 시작 (총 {loadingImages.Length}개 이미지)");
            
            while (autoSwitchImages && loadingImages.Length > 1)
            {
                yield return new WaitForSeconds(imageSwitchInterval);
                
                int nextIndex = (currentImageIndex + 1) % loadingImages.Length;
                Debug.Log($"[LoadingScreen] 자동 이미지 전환: {currentImageIndex} → {nextIndex}");
                SwitchToImage(nextIndex);
            }
            
            Debug.Log("[LoadingScreen] 자동 이미지 전환 코루틴 종료");
        }
        
        private IEnumerator SwitchMessagesCoroutine()
        {
            while (autoSwitchMessages && loadingMessageKeys.Length > 1)
            {
                yield return new WaitForSeconds(messageSwitchInterval);
                
                currentMessageIndex = (currentMessageIndex + 1) % loadingMessageKeys.Length;
                SetLoadingMessageByIndex(currentMessageIndex);
            }
        }

        /// <summary>
        /// 외부 진행률 모니터링 (AddressableSceneLoadingManager에서 데이터 가져오기)
        /// </summary>
        private IEnumerator MonitorExternalProgress()
        {
            Debug.Log("[LoadingScreen] 외부 진행률 모니터링 시작");

            // 초기 대기 시간 (로딩 화면이 완전히 준비될 때까지)
            yield return new WaitForSeconds(0.2f);
            Debug.Log("[LoadingScreen] 외부 모니터링 초기 대기 완료");

            // AddressableSceneLoadingManager 찾기
            AddressableSceneLoadingManager loadingManager = null;
            float timeoutTimer = 0f;
            const float TIMEOUT_DURATION = 10f; // 10초 타임아웃

            // 매니저 찾기 (없으면 생성될 때까지 대기)
            while (loadingManager == null && isExternalMonitoringActive)
            {
                loadingManager = FindObjectOfType<AddressableSceneLoadingManager>();
                if (loadingManager == null)
                {
                    timeoutTimer += 0.5f;
                    if (timeoutTimer >= TIMEOUT_DURATION)
                    {
                        Debug.LogError("[LoadingScreen] AddressableSceneLoadingManager를 찾을 수 없습니다. 타임아웃 발생. 시뮬레이션 모드로 전환합니다.");
                        // 타임아웃 시 시뮬레이션 모드로 전환
                        enableExternalProgressMonitoring = false;
                        enableProgressSimulation = true;
                        StartCoroutine(SimulateProgressForTesting());
                        yield break;
                    }

                    Debug.Log($"[LoadingScreen] AddressableSceneLoadingManager를 찾을 수 없습니다. 대기 중... ({timeoutTimer:F1}s)");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    Debug.Log("[LoadingScreen] AddressableSceneLoadingManager 발견!");
                    Debug.Log($"[LoadingScreen] 현재 매니저 진행률: {loadingManager.CurrentProgress * 100:F1}%");
                    Debug.Log($"[LoadingScreen] 현재 매니저 메시지: {loadingManager.CurrentMessage}");
                    Debug.Log($"[LoadingScreen] 현재 매니저 로딩 상태: {loadingManager.IsLoading}");
                }
            }

            // 모니터링 루프
            while (isExternalMonitoringActive && enableExternalProgressMonitoring)
            {
                // 로딩 중인지 확인
                if (loadingManager != null && loadingManager.IsLoading)
                {
                    // 로딩이 완료되었는지 확인
                    if (loadingManager.CurrentProgress >= 1f)
                    {
                        Debug.Log("[LoadingScreen] 외부 모니터링에서 로딩 완료 감지 - 모니터링 중단");
                        isExternalMonitoringActive = false;
                        break;
                    }
                    
                    try
                    {
                        // 현재 진행률 가져오기
                        float progress = loadingManager.CurrentProgress;
                        string message = loadingManager.CurrentMessage;

                        // 진행률이 0%보다 크고 유효한 경우에만 업데이트
                        if (progress > 0f)
                        {
                            Debug.Log($"[LoadingScreen] 외부 진행률 업데이트: {progress * 100:F1}% - {message}");

                            // UI 업데이트
                            SetProgress(progress);
                            if (!string.IsNullOrEmpty(message))
                            {
                                SetCenterMessage(message);
                            }

                            // 진행률이 100%에 도달하면 모니터링 중단
                            if (progress >= 1f)
                            {
                                Debug.Log("[LoadingScreen] 외부 모니터링에서 100% 감지 - 모니터링 중단");
                                isExternalMonitoringActive = false;
                                break;
                            }
                        }
                        else if (progress == 0f)
                        {
                            // 0%인 경우에도 UI를 0%로 확실히 설정
                            Debug.Log("[LoadingScreen] 진행률이 0% - UI 강제 0% 설정");
                            SetProgress(0f);
                        }
                        else
                        {
                            Debug.Log("[LoadingScreen] 진행률이 0%이므로 업데이트 건너뜀");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[LoadingScreen] 외부 진행률 업데이트 중 오류 발생: {e.Message}");
                        // 오류 발생 시 시뮬레이션 모드로 전환
                        enableExternalProgressMonitoring = false;
                        enableProgressSimulation = true;
                        StartCoroutine(SimulateProgressForTesting());
                        yield break;
                    }
                }
                else
                {
                    // 로딩이 완료되면 최소 표시 시간 보장 후 100%로 설정
                    float elapsedTime = Time.time - screenStartTime;
                    float remainingTime = minDisplayTime - elapsedTime;

                    if (remainingTime > 0)
                    {
                        Debug.Log($"[LoadingScreen] 최소 표시 시간 보장: {remainingTime:F2}초 대기");
                        yield return new WaitForSeconds(remainingTime);
                    }

                    SetProgress(1f);
                    Debug.Log("[LoadingScreen] 외부 로딩 완료 감지");
                    break;
                }

                yield return new WaitForSeconds(0.1f); // 0.1초마다 확인
            }

            Debug.Log("[LoadingScreen] 외부 진행률 모니터링 종료");
        }
        
        /// <summary>
        /// 테스트용 진행률 시뮬레이션 (실제 사용 시 제거)
        /// </summary>
        private IEnumerator SimulateProgressForTesting()
        {
            Debug.Log("[LoadingScreen] 테스트용 진행률 시뮬레이션 시작");
            
            float progress = 0f;
            float increment = 0.005f; // 0.5%씩 증가 (더 부드럽게)
            float interval = 0.05f; // 0.05초마다 업데이트 (더 빠르게)
            
            while (progress < 1f)
            {
                yield return new WaitForSeconds(interval);
                progress += increment;
                SetProgress(progress);
                
                // 100% 도달 시 완료
                if (progress >= 1f)
                {
                    Debug.Log("[LoadingScreen] 테스트 진행률 시뮬레이션 완료");
                    break;
                }
            }
        }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// 로딩 화면 표시 (정적 메서드)
        /// </summary>
        public static void ShowLoadingScreen(string initialMessage = null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLoadingScreen(initialMessage);
            }
            else
            {
                Debug.LogError("[LoadingScreen] UIManager.Instance가 null입니다.");
            }
        }

        /// <summary>
        /// 로딩 화면 표시 (로컬라이제이션 키 사용)
        /// </summary>
        public static void ShowLoadingScreenByKey(string localizationKey, string fallbackMessage = null)
        {
            UIManager.Instance.ShowPopUpAsync<LoadingScreen>((loadingScreen) => {
                if (loadingScreen != null)
                {
                    loadingScreen.SetCenterMessageByKey(localizationKey, fallbackMessage);
                }
            });
        }
        
        /// <summary>
        /// 로딩 화면 숨기기 (정적 메서드)
        /// </summary>
        public static void HideLoadingScreen()
        {
            // LoadingScreen 인스턴스 찾기
            LoadingScreen[] loadingScreens = FindObjectsOfType<LoadingScreen>();
            foreach (var loadingScreen in loadingScreens)
            {
                if (loadingScreen != null && loadingScreen.gameObject.activeInHierarchy)
                {
                    loadingScreen.Hide();
                    break;
                }
            }
        }
        
        #endregion
    }
}
