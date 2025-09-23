using TMPro;
using UnityEngine.UI;

public class SkinButton : KYS.BaseUI
{
    private Button _skinButton => GetUI<Button>("Button");
    private Image _skinImage => GetUI<Image>("SkinImage");
    private TextMeshProUGUI _costText => GetUI<TextMeshProUGUI>("CostText");

    private SkinPanel _skinPanel;
    private SkinTypes _type;
    private string _skinId;

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

        _costText.text = data.Cost.ToString();

        UpdateInfo();
    }

    public void UpdateInfo()
    {
        bool isOwned;
        bool isEquiped;

        if(_type == SkinTypes.Character)
        {
            isOwned = Manager.firebase.UserData.Skin.CharacterSkinList.Get(_skinId) != null;
            isEquiped = Manager.player.Data.CharacterSkinId.Value == _skinId;
        }
        else
        {
            isOwned = Manager.firebase.UserData.Skin.EquipSkinList.Get(_skinId) != null;
            isEquiped = Manager.player.Data.EquipSkinId.Value == _skinId;
        }

        if (isEquiped)
        {
            // TODO : 장착 중 상태
            return;
        }

        if (isOwned)
        {
            // TODO : 보유 중 상태
            return;
        }

        // TODO : 구매 상태
    }

    private void OnClick()
    {
        _skinPanel.SetView(_skinId);
    }
}
