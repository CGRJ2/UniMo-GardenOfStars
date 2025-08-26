using KYS;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingPopUp : BaseUI
{

    [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
    [SerializeField] private string BGMText = "BGMText";
    [SerializeField] private string SFXText = "SFXText";
    [SerializeField] private string VibrationText = "VibrationText";
    [SerializeField] private string closeButtonName = "CloseButton";



    private new void Awake()
    {
        base.Awake();
        // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
        if (layerType == UILayerType.Panel) // BaseUI의 기본값
        {
            layerType = UILayerType.Popup;
        }



    }

    private TextMeshProUGUI BGU => GetUI<TextMeshProUGUI>(BGMText);
    private TextMeshProUGUI SFXT => GetUI<TextMeshProUGUI>(SFXText);
    private TextMeshProUGUI Vibration => GetUI<TextMeshProUGUI>(VibrationText);

    public override string[] GetAutoLocalizeKeys()
    {
        return new string[]
        {
            "setting_bgm",
            "setting_sfx",
            "setting_vibration"
        };
    }


    public override void Initialize()
    {
        base.Initialize();
        SetupAutoLocalization();
        SetupButtons();
    }


    private void SetupButtons()
    {
        var confirmEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
        if (confirmEventHandler != null)
        {
            confirmEventHandler.Click += OnCloseButton;

        }
        else
        {
            Debug.LogError($"[TitlePanel] 확인 버튼 이벤트 설정 실패: {closeButtonName}");
        }

    }

    private void OnCloseButton(PointerEventData data)
    {
        Manager.ui.ClosePopup();
    }


}
