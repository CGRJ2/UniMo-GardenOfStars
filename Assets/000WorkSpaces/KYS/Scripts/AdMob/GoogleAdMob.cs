using GoogleMobileAds.Api;
using System;
using UnityEngine;

namespace KYS
{
    public class GoogleAdMob : MonoBehaviour
    {
        // 전체 광고 관리 스크립트
        public string fullScreenadUnitId = "ca-app-pub-3940256099942544/1033173712"; // 테스트 광고 유닛 아이디
        public InterstitialAd loadedFullScreenAd;

        // 배너 광고 관련 변수들
        public string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // 테스트 광고 유닛 아이디
        public BannerView _bannerView;


        // 보상형 광고 관련 변수들
        public string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // 테스트 광고 유닛 아이디
        public RewardedAd rewardedAd;

        // 네이티브 광고 관련 변수들
        public string nativeAdUnitId = "ca-app-pub-3940256099942544/2247696110"; // 테스트 광고 유닛 아이디

        public NativeOverlayAd _nativeOverlayAd;

      

        // 보상형 광고 관련 변수들
        private Action<Reward> _userRewardEarnedCallback;

        // 보상형 광고 이벤트들
        public event Action<AdValue> OnAdPaid;
        public event Action OnAdClicked;
        public event Action OnAdImpressionRecorded;
        public event Action OnAdFullScreenContentOpened;
        public event Action OnAdFullScreenContentClosed;
        public event Action<AdError> OnAdFullScreenContentFailed;



        #region GoogleAdMob Intitalize

        private void Awake()
        {
            // 이벤트를 메인 스레드로
            MobileAds.RaiseAdEventsOnUnityMainThread = true; 
            MobileAds.Initialize(OnInitialzed);

        }


        private void OnInitialzed(InitializationStatus initStatus)
        {

            if (initStatus == null)
            {
                Debug.LogError("AdMob 초기화 실패...");
            }


            Debug.Log("AdMob 초기화 완료");
        }

        #endregion

        #region Full Screen Ad

        [ContextMenu("FullScreenLoadTest")]
        public void LoadAdFullScreend()
        {
            var adRequest = new AdRequest();

            InterstitialAd.Load(fullScreenadUnitId, adRequest, (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogError("광고 로드 실패: " + error.GetMessage());
                    return;
                }

                Debug.Log("광고 로드 성공");

                loadedFullScreenAd = ad;
                loadedFullScreenAd.OnAdFullScreenContentClosed -= LoadAdFullScreend; // 중복 등록 방지
                loadedFullScreenAd.OnAdFullScreenContentClosed += LoadAdFullScreend; // 광고 닫히면 다시 로드
            });

        }

        [ContextMenu("FullScreenShowTest")]
        public void ShowAdFullScreen()
        {
            if (loadedFullScreenAd != null && loadedFullScreenAd.CanShowAd())
            {
                loadedFullScreenAd.Show();
            }
            else
            {
                Debug.LogWarning("광고가 로드되지 않았거나 준비되지 않았습니다.");
            }


        }

        public void DestroyFullScreenAd()
        {
            if (loadedFullScreenAd != null)
            {
                loadedFullScreenAd.Destroy();
            }
        }

        #endregion

        #region Banner Ad

        [ContextMenu("LoadNShowBannerAdTest")]
        public void LoadNShowBannerAd()
        {
            var adRequest = new AdRequest();

            if (_bannerView != null)
            {
                _bannerView.Destroy();
            }

            _bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);



            _bannerView.LoadAd(adRequest);


            _bannerView.OnAdFullScreenContentClosed -= LoadNShowBannerAd;
            _bannerView.OnAdFullScreenContentClosed += LoadNShowBannerAd;

        }

        [ContextMenu("DestoryBannerAd")]
        public void DestroyBannerAd()
        {
            if (_bannerView != null)
            {
                _bannerView.Destroy();
            }
        }


        [ContextMenu("BannerFullWidethTest")]
        public void RequestBanner()
        {


            // Clean up banner ad before creating a new one.
            if (_bannerView != null)
            {
                _bannerView.Destroy();
            }

            AdSize adaptiveSize =
                    AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            
            _bannerView = new BannerView(bannerAdUnitId, adaptiveSize, AdPosition.Bottom);

            // Register for ad events.
            _bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
            _bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;

            AdRequest adRequest = new AdRequest();

            // Load a banner ad.
            _bannerView.LoadAd(adRequest);

            _bannerView.OnAdFullScreenContentClosed -= RequestBanner;
            _bannerView.OnAdFullScreenContentClosed += RequestBanner;

        }

        #endregion


        #region Banner callback handlers

        private void OnBannerAdLoaded()
        {
            Debug.Log("Banner view loaded an ad with response : "
                     + _bannerView.GetResponseInfo());
            Debug.Log($"Ad Height: {_bannerView.GetHeightInPixels()}, width: {_bannerView.GetWidthInPixels()}");

        }

        private void OnBannerAdLoadFailed(LoadAdError error)
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                    + error);
        }





        #endregion

        #region Rewarded Ad

        public void LoadRewardedAd()
        {
            var adRequest = new AdRequest();

            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
            }

            RewardedAd.Load(rewardedAdUnitId, adRequest, (ad, error) =>
            {
                if (error != null)
                {
                    Debug.LogError("보상형 광고 로드 실패: " + error.GetMessage());
                    return;
                }
                Debug.Log("보상형 광고 로드 성공");
                rewardedAd = ad;
                SetupRewardedAdEvents();
            });
        }

        private void SetupRewardedAdEvents()
        {
            if (rewardedAd == null) return;

            // 이벤트 등록
            rewardedAd.OnAdFullScreenContentClosed += OnRewardedAdClosed;
            rewardedAd.OnAdFullScreenContentFailed += OnRewardedAdFailed;
            rewardedAd.OnAdFullScreenContentOpened += OnRewardedAdOpened;
            rewardedAd.OnAdClicked += OnRewardedAdClicked;
            rewardedAd.OnAdImpressionRecorded += OnRewardedAdImpressionRecorded;
            rewardedAd.OnAdPaid += OnRewardedAdPaid;
            // OnUserEarnedReward는 Show() 메서드의 콜백으로 처리됨
        }

        // 광고 완료 대기 시간 설정
        [Header("광고 시간 설정")]
        public float adCompletionTimeout = 30f; // 광고 완료 대기 시간 (초)
        public bool showAdProgress = true; // 광고 진행률 표시 여부

        public void ShowRewardedAd(Action<Reward> onRewardEarned = null)
        {
            if (rewardedAd == null)
            {
                Debug.LogWarning("보상형 광고가 로드되지 않았습니다. 다시 로드합니다.");
                LoadRewardedAd();
                return;
            }

            if (rewardedAd.CanShowAd())
            {
                _userRewardEarnedCallback = onRewardEarned;

                // 광고 시작 시간 기록
                float adStartTime = Time.time;

                rewardedAd.Show((Reward reward) =>
                {
                    // 광고 완료 시간 계산 및 저장
                    lastAdDuration = Time.time - adStartTime;
                    Debug.Log($"보상 획득: {reward.Type} - {reward.Amount} (광고 시간: {lastAdDuration:F1}초)");

                    _userRewardEarnedCallback?.Invoke(reward);
                    _userRewardEarnedCallback = null;
                });
            }
            else
            {
                Debug.LogWarning("보상형 광고를 표시할 수 없습니다.");
            }
        }

        // 보상형 광고 이벤트 핸들러들
        private void OnRewardedAdClosed()
        {
            Debug.Log("보상형 광고가 닫혔습니다.");
            OnAdFullScreenContentClosed?.Invoke();
            LoadRewardedAd(); // 다음 광고 미리 로드
        }

        private void OnRewardedAdFailed(AdError error)
        {
            Debug.LogError($"보상형 광고 실패: {error.GetMessage()}");
            OnAdFullScreenContentFailed?.Invoke(error);
        }

        private void OnRewardedAdOpened()
        {
            Debug.Log("보상형 광고가 열렸습니다.");
            OnAdFullScreenContentOpened?.Invoke();
        }

        private void OnRewardedAdClicked()
        {
            Debug.Log("보상형 광고가 클릭되었습니다.");
            OnAdClicked?.Invoke();
        }

        private void OnRewardedAdImpressionRecorded()
        {
            Debug.Log("보상형 광고 인상이 기록되었습니다.");
            OnAdImpressionRecorded?.Invoke();
        }

        private void OnRewardedAdPaid(AdValue adValue)
        {
            Debug.Log($"보상형 광고 수익: {adValue.Value} {adValue.CurrencyCode}");
            OnAdPaid?.Invoke(adValue);
        }



        // 보상형 광고 상태 확인
        public bool IsRewardedAdReady()
        {
            return rewardedAd != null && rewardedAd.CanShowAd();
        }

        public void DestroyRewardedAd()
        {
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
        }

        // 보상형 광고 사용 예시 메서드들
        [ContextMenu("보상형 광고 로드")]
        public void LoadRewardedAdTest()
        {
            LoadRewardedAd();
        }

        [ContextMenu("보상형 광고 표시")]
        public void ShowRewardedAdTest()
        {
            ShowRewardedAd((reward) =>
            {
                Debug.Log($"[테스트] 보상 획득: {reward.Type} - {reward.Amount}");
                // 여기에 실제 보상 지급 로직 추가
                GiveReward(reward);
            });
        }

        // 테스트용: 직접 코인 지급 (광고 없이)
        [ContextMenu("테스트: 직접 코인 100개 지급")]
        public void TestGiveCoinsDirectly()
        {
            Debug.Log($"[테스트] 직접 코인 지급 테스트 시작");
            Debug.Log($"[테스트] 현재 코인: {Manager.player.Data.Money.Value}");

            Manager.player.Data.Money.Value += 100;

            Debug.Log($"[테스트] 코인 100개 지급 후: {Manager.player.Data.Money.Value}");
        }

        // 테스트용: 보상 계산 확인
        [ContextMenu("테스트: 보상 계산 확인")]
        public void TestRewardCalculation()
        {
            Debug.Log($"[테스트] 보상 계산 테스트");
            Debug.Log($"[테스트] 코인 배율: {coinRewardMultiplier}");
            Debug.Log($"[테스트] 코인 범위: {minCoinReward}~{maxCoinReward}");

            int testAmount = 50;
            int calculatedAmount = CalculateRewardAmount(testAmount, coinRewardMultiplier, minCoinReward, maxCoinReward);

            Debug.Log($"[테스트] 원본 보상: {testAmount}, 계산된 보상: {calculatedAmount}");
        }

        [ContextMenu("테스트: 시간별 보상 계산")]
        public void TestTimeBasedReward()
        {
            Debug.Log($"[테스트] 시간별 보상 계산 테스트");
            Debug.Log($"[테스트] 시간별 보상 활성화: {enableTimeBasedReward}");
            Debug.Log($"[테스트] 짧은 광고 기준: {shortAdTime}초, 배율: {shortAdMultiplier}");
            Debug.Log($"[테스트] 일반 광고 배율: {normalAdMultiplier}");
            Debug.Log($"[테스트] 긴 광고 기준: {longAdTime}초, 배율: {longAdMultiplier}");

            // 다양한 광고 시간으로 테스트
            float[] testDurations = { 10f, 15f, 25f, 35f, 45f };

            foreach (float duration in testDurations)
            {
                float multiplier = CalculateTimeBasedMultiplier(duration);
                Debug.Log($"[테스트] 광고 시간 {duration:F1}초 → 배율: {multiplier:F2}");
            }
        }

        // 보상형 광고 사용 예시들
        [ContextMenu("코인 보상형 광고")]
        public void ShowCoinRewardAd()
        {
            ShowRewardedAd((reward) =>
            {
                Debug.Log($"[코인 보상형 광고] 보상 타입: {reward.Type}, 보상 양: {reward.Amount}");

                if (reward.Type == "coins")
                {
                    GiveReward(reward);
                    // UIManager의 확인 팝업 사용
                    UIManager.Instance.ShowConfirmPopUpAsync($"코인 {reward.Amount}개를 획득했습니다!", "확인");
                }
                else
                {
                    Debug.LogWarning($"[코인 보상형 광고] 예상한 보상 타입이 아닙니다. 받은 타입: {reward.Type}");
                }
            });
        }

        [ContextMenu("젬 보상형 광고")]
        public void ShowGemRewardAd()
        {
            ShowRewardedAd((reward) =>
            {
                Debug.Log($"[젬 보상형 광고] 보상 타입: {reward.Type}, 보상 양: {reward.Amount}");

                if (reward.Type == "gems")
                {
                    GiveReward(reward);
                    UIManager.Instance.ShowConfirmPopUpAsync($"젬 {reward.Amount}개를 획득했습니다!", "확인");
                }
                else
                {
                    Debug.LogWarning($"[젬 보상형 광고] 예상한 보상 타입이 아닙니다. 받은 타입: {reward.Type}");
                }
            });
        }

        [ContextMenu("에너지 보상형 광고")]
        public void ShowEnergyRewardAd()
        {
            ShowRewardedAd((reward) =>
            {
                Debug.Log($"[에너지 보상형 광고] 보상 타입: {reward.Type}, 보상 양: {reward.Amount}");

                if (reward.Type == "energy")
                {
                    GiveReward(reward);
                    UIManager.Instance.ShowConfirmPopUpAsync($"에너지 {reward.Amount}개를 회복했습니다!", "확인");
                }
                else
                {
                    Debug.LogWarning($"[에너지 보상형 광고] 예상한 보상 타입이 아닙니다. 받은 타입: {reward.Type}");
                }
            });
        }

        // 보상 배율 설정 (게임 밸런스 조절용)
        [Header("보상 배율 설정")]
        public float coinRewardMultiplier = 1.0f; // 코인 보상 배율
        public float gemRewardMultiplier = 1.0f;  // 젬 보상 배율
        public float energyRewardMultiplier = 1.0f; // 에너지 보상 배율

        // 광고 시간에 따른 보상 조절
        [Header("광고 시간별 보상 조절")]
        public bool enableTimeBasedReward = false; // 시간별 보상 조절 활성화
        public float shortAdTime = 15f; // 짧은 광고 기준 시간 (초)
        public float longAdTime = 30f;  // 긴 광고 기준 시간 (초)
        public float shortAdMultiplier = 0.8f; // 짧은 광고 보상 배율
        public float normalAdMultiplier = 1.0f; // 일반 광고 보상 배율
        public float longAdMultiplier = 1.2f;   // 긴 광고 보상 배율

        // 보상 최소/최대 값 설정
        [Header("보상 범위 설정")]
        public int minCoinReward = 10;   // 최소 코인 보상
        public int maxCoinReward = 100;  // 최대 코인 보상
        public int minGemReward = 1;     // 최소 젬 보상
        public int maxGemReward = 10;    // 최대 젬 보상
        public int minEnergyReward = 5;  // 최소 에너지 보상
        public int maxEnergyReward = 20; // 최대 에너지 보상

        private float lastAdDuration = 0f; // 마지막 광고 시간 저장

        private void GiveReward(Reward reward)
        {
            Debug.Log($"[GiveReward] 보상 지급 시작 - 타입: {reward.Type}, 원본 양: {reward.Amount}");

            // 광고 시간에 따른 보상 배율 계산
            float timeBasedMultiplier = CalculateTimeBasedMultiplier(lastAdDuration);

            // 보상 타입에 따른 실제 보상 지급 (배율 적용)
            switch (reward.Type)
            {
                case "coins":
                    float finalCoinMultiplier = coinRewardMultiplier * timeBasedMultiplier;
                    int coinAmount = CalculateRewardAmount((int)reward.Amount, finalCoinMultiplier, minCoinReward, maxCoinReward);

                    // 현재 코인 확인
                    int currentCoins = Manager.player.Data.Money.Value;
                    Debug.Log($"[GiveReward] 현재 코인: {currentCoins}");
                    Debug.Log($"[GiveReward] 광고 시간: {lastAdDuration:F1}초, 시간별 배율: {timeBasedMultiplier:F2}");

                    // 코인 지급
                    Manager.player.Data.Money.Value += coinAmount;

                    // 지급 후 코인 확인
                    int newCoins = Manager.player.Data.Money.Value;
                    Debug.Log($"[GiveReward] 코인 {coinAmount}개 지급됨 (원본: {reward.Amount}, 기본 배율: {coinRewardMultiplier}, 최종 배율: {finalCoinMultiplier:F2})");
                    Debug.Log($"[GiveReward] 지급 후 코인: {newCoins} (변화량: {newCoins - currentCoins})");
                    break;

                case "gems":
                    float finalGemMultiplier = gemRewardMultiplier * timeBasedMultiplier;
                    int gemAmount = CalculateRewardAmount((int)reward.Amount, finalGemMultiplier, minGemReward, maxGemReward);
                    // 젬 지급 로직
                    Debug.Log($"[GiveReward] 젬 {gemAmount}개 지급됨 (원본: {reward.Amount}, 기본 배율: {gemRewardMultiplier}, 최종 배율: {finalGemMultiplier:F2})");
                    break;

                case "energy":
                    float finalEnergyMultiplier = energyRewardMultiplier * timeBasedMultiplier;
                    int energyAmount = CalculateRewardAmount((int)reward.Amount, finalEnergyMultiplier, minEnergyReward, maxEnergyReward);
                    // 에너지 지급 로직
                    Debug.Log($"[GiveReward] 에너지 {energyAmount}개 지급됨 (원본: {reward.Amount}, 기본 배율: {energyRewardMultiplier}, 최종 배율: {finalEnergyMultiplier:F2})");
                    break;

                default:
                    Debug.LogWarning($"[GiveReward] 알 수 없는 보상 타입: {reward.Type}");
                    break;
            }
        }

        // 광고 시간에 따른 보상 배율 계산
        private float CalculateTimeBasedMultiplier(float adDuration)
        {
            if (!enableTimeBasedReward)
                return 1.0f;

            if (adDuration <= shortAdTime)
            {
                Debug.Log($"[시간별 보상] 짧은 광고 ({adDuration:F1}초) - 배율: {shortAdMultiplier}");
                return shortAdMultiplier;
            }
            else if (adDuration >= longAdTime)
            {
                Debug.Log($"[시간별 보상] 긴 광고 ({adDuration:F1}초) - 배율: {longAdMultiplier}");
                return longAdMultiplier;
            }
            else
            {
                Debug.Log($"[시간별 보상] 일반 광고 ({adDuration:F1}초) - 배율: {normalAdMultiplier}");
                return normalAdMultiplier;
            }
        }

        // 보상 양 계산 메서드
        private int CalculateRewardAmount(int originalAmount, float multiplier, int minAmount, int maxAmount)
        {
            int calculatedAmount = Mathf.RoundToInt(originalAmount * multiplier);
            return Mathf.Clamp(calculatedAmount, minAmount, maxAmount);
        }

        // 보상 배율 동적 조절 메서드들
        public void SetCoinRewardMultiplier(float multiplier)
        {
            coinRewardMultiplier = multiplier;
            Debug.Log($"코인 보상 배율이 {multiplier}로 설정되었습니다.");
        }

        public void SetGemRewardMultiplier(float multiplier)
        {
            gemRewardMultiplier = multiplier;
            Debug.Log($"젬 보상 배율이 {multiplier}로 설정되었습니다.");
        }

        public void SetEnergyRewardMultiplier(float multiplier)
        {
            energyRewardMultiplier = multiplier;
            Debug.Log($"에너지 보상 배율이 {multiplier}로 설정되었습니다.");
        }

        // 보상 범위 동적 조절 메서드들
        public void SetCoinRewardRange(int min, int max)
        {
            minCoinReward = min;
            maxCoinReward = max;
            Debug.Log($"코인 보상 범위가 {min}~{max}로 설정되었습니다.");
        }

        public void SetGemRewardRange(int min, int max)
        {
            minGemReward = min;
            maxGemReward = max;
            Debug.Log($"젬 보상 범위가 {min}~{max}로 설정되었습니다.");
        }

        public void SetEnergyRewardRange(int min, int max)
        {
            minEnergyReward = min;
            maxEnergyReward = max;
            Debug.Log($"에너지 보상 범위가 {min}~{max}로 설정되었습니다.");
        }

        #endregion


        #region NativeOverlayAd Ad

        [ContextMenu("LoadNativeAdTest")]
        public void LoadNativeAd()
        {
            if (_nativeOverlayAd != null)
            {
                _nativeOverlayAd.Destroy();
            }

            var adRequest = new AdRequest();

            // Optional: Define native ad options.
            // AdMob 예시와 변경된 부분 있음.
            var options = new NativeAdOptions
            {
                AdChoicesPlacement = AdChoicesPlacement.TopRightCorner,
                MediaAspectRatio = MediaAspectRatio.Any,
                VideoOptions = new VideoOptions
                {
                    StartMuted = true,
                    CustomControlsRequested = false,
                    ClickToExpandRequested = true
                }

            };



            // Send the request to load the ad.
            NativeOverlayAd.Load(nativeAdUnitId, adRequest, options,
                (NativeOverlayAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogError("Native Overlay ad failed to load an ad " +
                                   " with error: " + error);
                    return;
                }

                // The ad should always be non-null if the error is null, but
                // double-check to avoid a crash.
                if (ad == null)
                {
                    Debug.LogError("Unexpected error: Native Overlay ad load event " +
                                   " fired with null ad and null error.");
                    return;
                }
Debug.Log("Native Overlay ad loading...");
                // The operation completed successfully.
                Debug.Log("Native Overlay ad loaded with response : " +
                           ad.GetResponseInfo());
                _nativeOverlayAd = ad;


                _nativeOverlayAd.OnAdFullScreenContentClosed += OnNativeOverlayAdClosed;
                _nativeOverlayAd.OnAdFullScreenContentOpened += OnNativeOverlayAdOpened;
                _nativeOverlayAd.OnAdClicked += OnNativeOverlayAdClicked;
                _nativeOverlayAd.OnAdImpressionRecorded += OnNativeOverlayAdImpressionRecorded;
                _nativeOverlayAd.OnAdPaid += OnNativeOverlayAdPaid;


            });
        }



        /// <summary>
        /// Renders the ad.
        /// </summary>
        [ContextMenu("RenderNativeAdTest")]
        public void RenderAdNativeAd()
        {
            


            if (_nativeOverlayAd != null)
            {
                Debug.Log("Rendering Native Overlay ad.");

                // Define a native template style with a custom style.
                var style = new NativeTemplateStyle
                {
                    // AdMob 예시와 변경된 부분 있음.
                    TemplateId = NativeTemplateId.Medium,
                    MainBackgroundColor = Color.red,
                    CallToActionText = new NativeTemplateTextStyle
                    {
                        BackgroundColor = Color.green,
                        TextColor = Color.white,
                        FontSize = 9,
                        Style = NativeTemplateFontStyle.Bold
                    }
                };

                // Renders a native overlay ad at the default size
                // and anchored to the bottom of the screne.
                _nativeOverlayAd.RenderTemplate(style, AdPosition.Bottom);
            }
        }


        /// <summary>
        /// Shows the ad.
        /// </summary>
        [ContextMenu("ShowNativeAdTest")]
        public void ShowAd()
        {
            if (_nativeOverlayAd != null)
            {
                Debug.Log("Showing Native Overlay ad.");
                _nativeOverlayAd.Show();
            }
        }


        /// <summary>
        /// Hides the ad.
        /// </summary>
        [ContextMenu("HideNativeAdTest")]
        public void HideAdNativeAd()
        {
            if (_nativeOverlayAd != null)
            {
                Debug.Log("Hiding Native Overlay ad.");
                _nativeOverlayAd.Hide();
            }
        }

        /// <summary>
        /// Destroys the native overlay ad.
        /// </summary>
        [ContextMenu("DestroyNativeAdTest")]
        public void DestroyAdNativeAd()
        {
            if (_nativeOverlayAd != null)
            {
                Debug.Log("Destroying native overlay ad.");
                _nativeOverlayAd.Destroy();
                _nativeOverlayAd = null;
            }
        }

        private void OnNativeOverlayAdClosed()
        {
            Debug.Log("Native Overlay ad closed.");
        }



        private void OnNativeOverlayAdOpened()
        {
            Debug.Log("Native Overlay ad opened.");
        }

        private void OnNativeOverlayAdClicked()
        {
            Debug.Log("Native Overlay ad clicked.");
        }

        private void OnNativeOverlayAdImpressionRecorded()
        {
            Debug.Log("Native Overlay ad impression recorded.");
        }

        private void OnNativeOverlayAdPaid(AdValue adValue)
        {
            Debug.Log("Native Overlay ad paid event with value: " +
                       adValue.Value + " " + adValue.CurrencyCode);
        }


        #endregion


        [ContextMenu("DetroyAllAd")]
        public void DestroyAllAd()
        {
            DestroyFullScreenAd();
            DestroyBannerAd();
            DestroyRewardedAd();
            DestroyAdNativeAd();
        }

    }

}


