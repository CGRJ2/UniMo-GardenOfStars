using KYS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinShopPanel : KYS.BaseUI
{
    [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
    [SerializeField] private string moneyTextName = "RunMoneyButtonText";
    [SerializeField] private string moneyButtonName = "MoneyButton";
    [SerializeField] private string gemButtonName = "GemButton";
    [SerializeField] private string gemTextName = "RunGemButtonText";
    [SerializeField] private string closeButtonName = "CloseButton";

    // UI 요소 참조 (GetUI<T>() 메서드로 동적 참조)
    private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);
    private TextMeshProUGUI gemText => GetUI<TextMeshProUGUI>(gemTextName);
    private Button closeButton => GetUI<Button>(closeButtonName);

    private int currentMoney = 0;
    private int currentGem = 0;
    private int GetCurrentMoneyValue() => currentMoney;
    private int GetCurrentGemValue() => currentGem;

    private bool isInitialized = false;
    protected override void Awake()
    {

        base.Awake();

        // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
        if (layerType == UILayerType.Panel) // BaseUI의 기본값
        {
            layerType = UILayerType.HUD;
        }

        Initialize();


    }

    public override void Initialize()
    {
        base.Initialize();

        SetupButtons();
        SetupAutoLocalization();

        // 언어 변경 이벤트 구독
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }

        // 초기 값 설정
        UpdateMoney(Manager.player.Data.Money.Value);
        UpdateGem(Manager.player.Data.Gem.Value);

        // ObservableProperty 구독 - 실시간 돈 업데이트
        Manager.player.Data.Money.Subscribe(OnMoneyChanged);
        Manager.player.Data.Gem.Subscribe(OnGemChanged);

        isInitialized = true;
        Debug.Log("[HUDAllPanel] HUD 완전 초기화 완료");
    }
    public override void Cleanup()
    {
        // ObservableProperty 구독 해제
        Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);
        Manager.player?.Data?.Gem.Unsubscribe(OnGemChanged);

        // 언어 변경 이벤트 구독 해제
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }

        base.Cleanup();

    }



    private void SetupButtons()
    {
        var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClickBack");
        if (closeEventHandler != null)
        {
            closeEventHandler.Click += (data) => OnCloseButtonClicked();
        }

        isButtonsSetup = true; // 설정 완료 플래그
    }


    private void OnCloseButtonClicked()
    {
        Debug.Log("[PlayerUpgradePanel] 패널 닫기");
        Manager.ui.ClosePanel();
    }

    /// <summary>
    /// ObservableProperty Money 값 변경 시 호출되는 콜백
    /// </summary>
    private void OnMoneyChanged(int newMoneyValue)
    {
        UpdateMoney(newMoneyValue);
    }

    /// <summary>
    /// ObservableProperty Gem 값 변경 시 호출되는 콜백
    /// </summary>
    private void OnGemChanged(int newGemValue)
    {
        UpdateGem(newGemValue);
    }


    public void UpdateMoney(int amount)
    {

        currentMoney = amount; // 현재 값 저장
        if (moneyText != null)
        {
            // BaseUI의 돈 포맷팅 사용 (소수점 없음)
            moneyText.text = FormatMoney(amount, false);
        }
    }

    public void UpdateGem(int amount)
    {
        currentGem = amount; // 현재 값 저장
        if (gemText != null)
        {
            // BaseUI의 돈 포맷팅 사용 (소수점 없음)
            gemText.text = FormatMoney(amount, false);
        }
    }

    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        // 언어 변경 시 현재 값으로 텍스트 다시 업데이트
        if (moneyText != null)
        {
            UpdateMoney(GetCurrentMoneyValue());
        }
        if (gemText != null)
        {
            UpdateGem(GetCurrentGemValue());
        }

    }

}
