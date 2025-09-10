using GoogleMobileAds.Api;
using TMPro;
using UnityEngine;

public class TestUi : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshPro;
    private RectTransform uiRoot; //렉트트렌스폼을 조정.
    // Start is called before the first frame update
    void Start()
    {
        uiRoot = GetComponent<RectTransform>();
        
    }
    public void Reward()
    {
        textMeshPro.text = "골드100 수령(test)";
    }

    void OnEnable()
    {
        AdManager.OnBannerHeightChanged += AdjustUI;
        Debug.Log("실행됨?");
    }

    void OnDisable()
    {
        AdManager.OnBannerHeightChanged -= AdjustUI;
    }

    void AdjustUI(float bannerHeight, AdPosition position)
    {
        if(bannerHeight == 0)
        {
            uiRoot.offsetMax = new Vector2(uiRoot.offsetMax.x, 0);
            uiRoot.offsetMin = new Vector2(uiRoot.offsetMin.x, 0);
        }

        else if (position == AdPosition.Top || position == AdPosition.TopLeft || position ==AdPosition.TopRight)
        {//서로 반대로 주면 해결?
            //uiRoot.anchoredPosition += new Vector2(0, -bannerHeight);

            //uiRoot.offsetMax = new Vector2(uiRoot.offsetMax.x, -bannerHeight);  //상단여백
            Debug.Log("상단적용");
            uiRoot.offsetMax = new Vector2(uiRoot.offsetMax.x, uiRoot.offsetMax.y - (bannerHeight+20));  //상단여백

        }
        else
        {
            uiRoot.offsetMin = new Vector2(uiRoot.offsetMin.x, uiRoot.offsetMin.y + (bannerHeight+20));  //하단여백
            
            //uiRoot.offsetMin = new Vector2(uiRoot.offsetMin.x, bannerHeight);  //하단여백
             //uiRoot.offsetMin = new Vector2(0, bannerHeight);  //하단여백
            Debug.Log("하단적용");
            Debug.Log($"[UI 조정] bannerHeight: {bannerHeight}");
            Debug.Log($"offsetMin: {uiRoot.offsetMin}");
            Debug.Log($"anchoredPosition: {uiRoot.anchoredPosition}");
            Debug.Log($"sizeDelta: {uiRoot.sizeDelta}");

        }
    }


}
