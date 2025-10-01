using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinButton : KYS.BaseUI
{
    private Button _skinButton => GetUI<Button>("Button");
    private Image _skinImage => GetUI<Image>("SkinImage");
    private Image _lockImage => GetUI<Image>("LockImage");
    private Image _equipImage => GetUI<Image>("EquippedIcon");
    private Image _costImage => GetUI<Image>("CostImage");
    private TextMeshProUGUI _costText => GetUI<TextMeshProUGUI>("CostText");

    private SkinPanel _skinPanel;
    private SkinTypes _type;
    private string _skinId;
    private int _cost;

    protected override void Awake()
    {
        base.Awake();
        _skinButton.onClick.AddListener(OnClick);
    }

    public void SetInfo(string Id, SkinTypes type, SkinPanel skinPanel)
    {
        _skinId = Id;
        _type = type;
        _skinPanel = skinPanel;

        SkinDataCsv data;
        if (_type == SkinTypes.Character)
        {
            data = Manager.data.CharacterSkin.Values[_skinId];
        }
        else
        {
            data = Manager.data.EquipSkin.Values[_skinId];
        }

        _skinImage.sprite = data.Sprite;

        UpdateInfo();
    }

    public void UpdateInfo()
    {
        _costImage.gameObject.SetActive(false);
        _equipImage.gameObject.SetActive(false);
        _lockImage.gameObject.SetActive(false);

        bool isOwned;
        bool isEquiped;
        Color color;

        if (_type == SkinTypes.Character)
        {
            isOwned = Manager.firebase.UserData.Skin.CharacterSkinList.Get(_skinId) != null;
            isEquiped = Manager.player.Data.CharacterSkinId.Value == _skinId;
            _cost = Manager.data.CharacterSkin.Values[_skinId].Cost;
        }
        else
        {
            isOwned = Manager.firebase.UserData.Skin.EquipSkinList.Get(_skinId) != null;
            isEquiped = Manager.player.Data.EquipSkinId.Value == _skinId;
            _cost = Manager.data.EquipSkin.Values[_skinId].Cost;
        }

        if (isEquiped)
        {
            _equipImage.gameObject.SetActive(true);
            _costText.text = GetLocalizedText("Equipped");
            if (ColorUtility.TryParseHtmlString("#40B800", out color))
            {
                _skinButton.image.color = color;
            }
            return;
        }

        if (isOwned)
        {
            _costText.text = GetLocalizedText("Owned");
            if (ColorUtility.TryParseHtmlString("#E0883B", out color))
            {
                _skinButton.image.color = color;
            }
            return;
        }

        _costImage.gameObject.SetActive(true);
        _lockImage.gameObject.SetActive(true);
        _costText.text = _cost.ToString();

        if (ColorUtility.TryParseHtmlString("#0077FF", out color))
        {
            _skinButton.image.color = color;
        }
    }

    private void OnClick()
    {
        _skinPanel.SetView(_skinId);
    }
}
