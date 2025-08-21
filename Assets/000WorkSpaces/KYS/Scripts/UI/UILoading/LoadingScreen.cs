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
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image[] loadingImages; // 전환 가능한 이미지들
        [SerializeField] private TextMeshProUGUI centerMessageText;
        [SerializeField] private Slider loadingProgressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        
        [Header("Loading Settings")]
        [SerializeField] private float imageSwitchInterval = 2f; // 이미지 전환 간격
        [SerializeField] private float messageSwitchInterval = 3f; // 메시지 전환 간격
        [SerializeField] private bool autoSwitchImages = true;
        [SerializeField] private bool autoSwitchMessages = true;
        
        [Header("Loading Messages")]
        [SerializeField] private string[] loadingMessages = {
            "로딩 중...",
            "잠시만 기다려주세요...",
            "데이터를 불러오는 중...",
            "거의 완료되었습니다..."
        };
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableImageFade = true;
        [SerializeField] private float fadeDuration = 0.5f;
        
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
            layerType = UILayerType.Loading;
            hidePreviousUI = true;
            disablePreviousUI = true;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            SetupLoadingScreen();
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            StopAllCoroutines();
        }
        
        private void SetupLoadingScreen()
        {
            Debug.Log("[LoadingScreen] 로딩 화면 설정 시작");
            
            // 초기 상태 설정
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = 0f;
                loadingProgressBar.minValue = 0f;
                loadingProgressBar.maxValue = 1f;
            }
            
            if (progressText != null)
            {
                progressText.text = "0%";
            }
            
            // 첫 번째 메시지 설정
            SetCenterMessage(loadingMessages[0]);
            
            // 자동 전환 시작
            if (autoSwitchImages && loadingImages.Length > 1)
            {
                imageSwitchCoroutine = StartCoroutine(SwitchImagesCoroutine());
            }
            
            if (autoSwitchMessages && loadingMessages.Length > 1)
            {
                messageSwitchCoroutine = StartCoroutine(SwitchMessagesCoroutine());
            }
            
            Debug.Log("[LoadingScreen] 로딩 화면 설정 완료");
        }
        
        #region Public Methods
        
        /// <summary>
        /// 로딩 진행률 설정 (0.0 ~ 1.0)
        /// </summary>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = progress;
            }
            
            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
            
            OnProgressChanged?.Invoke(progress);
            
            // 100% 완료 시 완료 이벤트 호출
            if (progress >= 1f)
            {
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
        
        #region Private Methods
        
        private void SwitchImageDirectly(int imageIndex)
        {
            // 모든 이미지 숨기기
            for (int i = 0; i < loadingImages.Length; i++)
            {
                if (loadingImages[i] != null)
                {
                    loadingImages[i].gameObject.SetActive(i == imageIndex);
                }
            }
            
            currentImageIndex = imageIndex;
        }
        
        private IEnumerator SwitchImageWithFade(int targetIndex)
        {
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
                fadeCoroutines.Add(StartCoroutine(FadeImage(loadingImages[currentImageIndex], 1f, 0f)));
            }
            
            // 새 이미지 페이드 인
            if (loadingImages[targetIndex] != null)
            {
                loadingImages[targetIndex].gameObject.SetActive(true);
                loadingImages[targetIndex].color = new Color(1f, 1f, 1f, 0f);
                fadeCoroutines.Add(StartCoroutine(FadeImage(loadingImages[targetIndex], 0f, 1f)));
            }
            
            currentImageIndex = targetIndex;
            
            yield return new WaitForSeconds(fadeDuration);
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
            while (autoSwitchImages && loadingImages.Length > 1)
            {
                yield return new WaitForSeconds(imageSwitchInterval);
                
                int nextIndex = (currentImageIndex + 1) % loadingImages.Length;
                SwitchToImage(nextIndex);
            }
        }
        
        private IEnumerator SwitchMessagesCoroutine()
        {
            while (autoSwitchMessages && loadingMessages.Length > 1)
            {
                yield return new WaitForSeconds(messageSwitchInterval);
                
                currentMessageIndex = (currentMessageIndex + 1) % loadingMessages.Length;
                SetCenterMessage(loadingMessages[currentMessageIndex]);
            }
        }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// 로딩 화면 표시 (정적 메서드)
        /// </summary>
        public static void ShowLoadingScreen(string initialMessage = "로딩 중...")
        {
            UIManager.Instance.ShowPopUpAsync<LoadingScreen>((loadingScreen) => {
                if (loadingScreen != null)
                {
                    loadingScreen.SetCenterMessage(initialMessage);
                }
            });
        }
        
        /// <summary>
        /// 로딩 화면 숨기기 (정적 메서드)
        /// </summary>
        public static void HideLoadingScreen()
        {
            UIManager.Instance.ClosePopup();
        }
        
        #endregion
    }
}
