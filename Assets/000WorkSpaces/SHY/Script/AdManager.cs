using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdManager : Singleton<AdManager>
{
    public static event Action<float, AdPosition> OnBannerHeightChanged;


    [SerializeField] private float bannerRefreshInterval = 60f; // 배너 광고 리프레시 간격 (초)
    private bool isBannerRefreshing = false;
    // 리프레시 중복 방지 플래그 (트루일경우 중복실행안되고 리프레쉬 중임을 의미)
    [SerializeField] private float rewardedRefreshInterval = 3500f; // 보상형 광고 리프레시 간격 (초)
    private bool isRefreshingRewardedAd = false;
    private float lastRewardedLoadTime;
    private bool showrewardchack =true;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private BannerView bannerView;
    private AppOpenAd appOpenAd;
    private bool isShowingAd = false;


    [Header("광고 ID")]
    [SerializeField] private string interstitialID;
    [SerializeField] private string rewardedID;
    [SerializeField] private string bannerID;
    [SerializeField] private string appopenId; //개발중지


    [Header("배너광고 조정")]
    [SerializeField] public BannerSize bannerSize = BannerSize.BANNER;
    [SerializeField] private BannerPosition bannerPosition = BannerPosition.Bottom;

    // Start is called before the first frame update
    private void Awake()
    {
        Init();
    }
    void Init()
    {
        base.SingletonInit();
        StartCoroutine(WaitInit());
    }

    IEnumerator WaitInit()
    {
        yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
        yield return new WaitUntil(() => Manager.firebase.UserData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.AdRemoved.IsInit);

        Manager.firebase.UserDataProperty.Subscribe(data => data.AdRemoved.Subscribe(ApplyBannerState));
        Manager.firebase.UserData.AdRemoved.Subscribe(ApplyBannerState);
        ApplyBannerState(Manager.firebase.UserData.AdRemoved.Value);
        if (!isRefreshingRewardedAd) StartCoroutine(AutoRefreshRewardedAd());

    }


    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            LoadInterstitialAd();
            LoadRewardedAd();
            LoadAppOpenAd();

        });
    }
    public void LoadAppOpenAd()
    {
        AdRequest request = new AdRequest();
        AppOpenAd.Load(appopenId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("앱 열기 광고 로딩 실패: " + error);
                return;
            }

            appOpenAd = ad;
            Debug.Log("앱 열기 광고 로딩 완료");
        });

    }
    public void ShowAppOpenAdIfAvailable()
    {
        appOpenAd.Show();
        StartCoroutine(WaitAndReloadAd());

    }
    IEnumerator WaitAndReloadAd()
    {
        yield return new WaitForSeconds(1.5f); // 광고 길이에 따라 조정
        isShowingAd = false;
        LoadAppOpenAd();
    }

    // 전면 광고
    public void LoadInterstitialAd()
    {
        interstitialAd?.Destroy();
        var request = new AdRequest();
        InterstitialAd.Load(interstitialID, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("전면 광고 로딩 실패: " + error);
                return;
            }
            interstitialAd = ad;
        });
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
            interstitialAd.Show();
        else
            Debug.Log("전면 광고 준비 안됨");
    }

    // 보상형 광고
    public void LoadRewardedAd()
    {
        lastRewardedLoadTime = Time.time; //광고 로딩 시점 저장.
        var request = new AdRequest();
        RewardedAd.Load(rewardedID, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("보상형 광고 로딩 실패: " + error);
                return;
            }

            rewardedAd = ad;
            rewardedAd.OnAdFullScreenContentClosed += LoadRewardedAd;
            Debug.Log("보상형 광고 로딩 완료");
        });

    }

    public async void ShowRewardedAd(System.Action onReward)
    {
        if (showrewardchack)
        {

            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"보상 지급! 어떤걸{reward.Type},얼만큼 {reward.Amount}");
                    onReward?.Invoke();
                });
            }
            else
            {
                Debug.Log("보상형 광고 준비 안됨");
                LoadRewardedAd();
                showrewardchack = false;
                await Task.Delay(5000);
                if (rewardedAd != null && rewardedAd.CanShowAd())
                {
                    rewardedAd.Show((Reward reward) =>
                    {
                        Debug.Log($"보상 지급! 어떤걸{reward.Type},얼만큼 {reward.Amount}");
                        onReward?.Invoke();
                    });
                }
                showrewardchack = true;
            }
        }
    }
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadBannerAd();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            HideBannerAd();
        }
    }*/

    // 배너 광고
    public void LoadBannerAd()
    {
        float dpi = Screen.dpi;
        if (dpi == 0) dpi = 160f; // DPI fallback

        int widthDp = (int)(Screen.width / (dpi / 160f));

        AdSize selectedSize = GetAdSize(bannerSize);
        AdPosition selectedPosition = GetAdPosition(bannerPosition);

        bannerView = new BannerView(bannerID, selectedSize, selectedPosition);
        bannerView.LoadAd(new AdRequest());
        // 광고 높이 이벤트 발행
        // int bannerHeightPx = selectedSize.Height;
        //OnBannerHeightChanged?.Invoke(selectedSize.Height,selectedPosition);
        StartCoroutine(NotifyBannerHeightDelayed(selectedPosition));
    }

    IEnumerator NotifyBannerHeightDelayed(AdPosition position)
    {
        yield return new WaitForSeconds(0.5f); // 광고 로딩 시간 확보
        float height = bannerView?.GetHeightInPixels() ?? 0;
        Debug.Log($"[AdManager] Banner Height: {height}");
        OnBannerHeightChanged?.Invoke(height, position);
    }


    public void HideBannerAd()
    {
        bannerView?.Destroy(); //광고 파괴
        //bannerView?.Hide(); //숨기고 있었지만 파괴로 변경.
        OnBannerHeightChanged?.Invoke(0, GetAdPosition(bannerPosition)); // 광고 숨김 → UI 복원

    }

    // 옵션: 사이즈 선택

    private AdSize GetAdSize(BannerSize size)
    {
        switch (size)
        {
            case BannerSize.MEDIUMRECTANGLE: return AdSize.MediumRectangle;
            case BannerSize.LEADERBOARD: return AdSize.Leaderboard;
            case BannerSize.IABBANNER: return AdSize.IABBanner;
            case BannerSize.ADAPTIVE:
                int width = (int)(Screen.width / (Screen.dpi / 160));
                return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(width);

            default: return AdSize.Banner;
        }
    }
    // 옵션: 위치 선택
    private AdPosition GetAdPosition(BannerPosition position)
    {
        switch (position)
        {
            case BannerPosition.Top: return AdPosition.Top;
            case BannerPosition.Bottom: return AdPosition.Bottom;
            case BannerPosition.TopLeft: return AdPosition.TopLeft;
            case BannerPosition.TopRight: return AdPosition.TopRight;
            case BannerPosition.BottomLeft: return AdPosition.BottomLeft;
            case BannerPosition.BottomRight: return AdPosition.BottomRight;
            default: return AdPosition.Bottom;
        }
    }


    public void ApplyBannerState(bool adRemoved)
    {
        if (adRemoved)
        {
            HideBannerAd();
            Debug.Log("광고 제거 상태 적용됨");
            // 광고 제거 ui 비 활성화
            //noadsbutton.interactable = false;
            //noadsbutton.GetComponentInChildren<TextMeshProUGUI>().text = "구매완료";

        }
        else
        {

            if (GameObject.Find($"{bannerSize}(Clone)") == null)
            {
                LoadBannerAd();
                Debug.Log("광고 표시 상태 적용됨");
            }
            else
            {
                Debug.Log("광고가 이미 표시중입니다.");
            }
            if (!isBannerRefreshing)
            {
                StartCoroutine(AutoRefreshBanner());
                isBannerRefreshing = true;

            }
        }
    }
    IEnumerator AutoRefreshBanner()
    {
        while (true)
        {
            yield return new WaitForSeconds(bannerRefreshInterval);

            if (Manager.firebase.UserData.AdRemoved.Value)
            {
                Debug.Log("광고 제거 상태이므로 배너 리프레시 중단");
                isBannerRefreshing = false;
                yield break; // 코루틴 종료

            }

            bannerView?.Destroy(); // 기존 광고 제거
            LoadBannerAd();        // 새 광고 로딩
            Debug.Log("배너 광고 리프레시됨");
        }
    }
    private IEnumerator AutoRefreshRewardedAd() //보상형 광고 리프레쉬
    {
        isRefreshingRewardedAd = true;

        while (true)
        {
            yield return new WaitForSeconds(60f); // 1분마다 체크

            float timeSinceLastLoad = Time.time - lastRewardedLoadTime;

            if (rewardedAd == null || !rewardedAd.CanShowAd() || timeSinceLastLoad >= rewardedRefreshInterval)
            {
                LoadRewardedAd();
                lastRewardedLoadTime = Time.time;
                Debug.Log("보상형 광고 자동 리프레시됨");
            }
        }
    }

}
public enum BannerSize
{
    BANNER,
    MEDIUMRECTANGLE,
    LEADERBOARD,
    IABBANNER,
    ADAPTIVE
}
public enum BannerPosition
{
    Top,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}