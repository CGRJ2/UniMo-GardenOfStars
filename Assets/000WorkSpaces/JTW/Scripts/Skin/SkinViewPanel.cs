using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SkinViewStates
{
    Buy, Moneyless, Owned, Equiped
}

public enum SkinTypes
{
    Character, Equip
}

public class SkinViewPanel : KYS.BaseUI
{
    [SerializeField] private SkinPanel _characterSkinPanel;
    [SerializeField] private SkinPanel _equipSkinPanel;

    [SerializeField] private RuntimeAnimatorController _characterController;
    [SerializeField] private RuntimeAnimatorController _equipController;
    private RuntimeAnimatorController TargetController => _type == SkinTypes.Character ? _characterController : _equipController;

    [SerializeField] private Transform _equipParent;
    [SerializeField] private Transform _characterParent;
    private Transform TargetParent => _type == SkinTypes.Character ? _characterParent : _equipParent;

    private TextMeshProUGUI _skinNameText => GetUI<TextMeshProUGUI>("SkinNameText");
    private TextMeshProUGUI _costText => GetUI<TextMeshProUGUI>("CostText");
    private TextMeshProUGUI _buybuttonText => GetUI<TextMeshProUGUI>("BuyButtonText");
    private TextMeshProUGUI _equipbuttonText => GetUI<TextMeshProUGUI>("EquipButtonText");

    private Button _buybutton => GetUI<Button>("BuyButton");
    private Image _buyBGImage => GetUI<Image>("BuyButtonBG");

    private Button _equipbutton => GetUI<Button>("EquipButton");
    private Image _equipBGImage => GetUI<Image>("EquipButtonBG");
    private Image _equipImage => GetUI<Image>("EquipIcon");

    private GameObject _characterObj; 
    private GameObject _equipObj;
    private GameObject TargetObject
    {
        get
        {
            return _type == SkinTypes.Character ? _characterObj : _equipObj;
        }

        set
        {
            if(_type == SkinTypes.Character)
            {
                _characterObj = value;
            }
            else
            {
                _equipObj = value;
            }
        }
    }

    private SkinViewStates _state;

    private string _skinId;
    private SkinTypes _type;
    private int _cost;

    private bool _isClicked;

    protected override void Awake()
    {
        base.Awake();

        ClearAvatar();

        _buybutton.onClick.AddListener(OnBuyClick);
        _equipbutton.onClick.AddListener(OnEquipClick);
    }

    public void SetInfo(string id, SkinTypes type)
    {
        if (_skinId == id && _type == type) return;

        _type = type;
        if(TargetObject != null)
        {
            Destroy(TargetObject);
        }

        SkinDataCsv data;

        if (type == SkinTypes.Character)
        {
            data = Manager.data.CharacterSkin.Values[id];
        }
        else
        {
            data = Manager.data.EquipSkin.Values[id];
        }

        _skinId = data.Id;
        _cost = data.Cost;

        _skinNameText.text = data.Name;

        GameObject Prefab = data.Skin;
        TargetObject = Instantiate(Prefab, TargetParent);

        Animator anim = TargetObject.GetComponent<Animator>();
        anim.runtimeAnimatorController = TargetController;
        anim.enabled = true;

        UpdateInfo();
    }

    public void UpdateInfo()
    {
        // 초기화
        _equipbutton.gameObject.SetActive(false);
        _buybutton.gameObject.SetActive(false);
        _equipImage.gameObject.SetActive(false);

        _skinNameText.gameObject.SetActive(true);
        _buybutton.gameObject.SetActive(true);

        bool isOwned;
        bool isEquiped;

        Color color;

        if (_type == SkinTypes.Character)
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
            _state = SkinViewStates.Equiped;
            _equipbutton.gameObject.SetActive(true);
            _equipImage.gameObject.SetActive(true);
            _equipbuttonText.text = "장착중";
            _equipbuttonText.color = Color.black;

            if (ColorUtility.TryParseHtmlString("#4FD209", out color))
            {
                _equipbutton.image.color = color;
            }
            if (ColorUtility.TryParseHtmlString("#C2FFA1", out color))
            {
                _equipBGImage.color = color;
            }
            return;
        }

        if (isOwned)
        {
            _state = SkinViewStates.Owned;
            _equipbutton.gameObject.SetActive(true);
            _equipbuttonText.text = "장착";
            _equipbuttonText.color = Color.white;

            if (ColorUtility.TryParseHtmlString("#45C500", out color))
            {
                _equipbutton.image.color = color;
            }
            if (ColorUtility.TryParseHtmlString("#6FEC2B", out color))
            {
                _equipBGImage.color = color;
            }
            return;
        }

        if(Manager.player.Data.Gem.Value < _cost)
        {
            _state = SkinViewStates.Moneyless;
            _buybutton.gameObject.SetActive(true);
            _buybuttonText.text = "보석 부족";
            _costText.text = _cost.ToString();

            if (ColorUtility.TryParseHtmlString("#FF0000", out color))
            {
                _buybutton.image.color = color;
            }
            if (ColorUtility.TryParseHtmlString("#FF6868", out color))
            {
                _buyBGImage.color = color;
            }
            return;
        }

        _state = SkinViewStates.Buy;
        _buybutton.gameObject.SetActive(true);
        _buybuttonText.text = "구매";
        _costText.text = _cost.ToString();

        if (ColorUtility.TryParseHtmlString("#0077FF", out color))
        {
            _buybutton.image.color = color;
        }
        if (ColorUtility.TryParseHtmlString("#68AFFF", out color))
        {
            _buyBGImage.color = color;
        }
    }

    public void ClearAvatar()
    {
        if (_characterObj != null) Destroy(_characterObj);
        if (_equipObj != null) Destroy(_equipObj);

        string characterSkinId = Manager.player.Data.CharacterSkinId.Value;
        string equipSkinId = Manager.player.Data.EquipSkinId.Value;

        GameObject characterPrefab = Manager.data.CharacterSkin.Values[characterSkinId].Skin;
        GameObject equipPrefab = Manager.data.EquipSkin.Values[equipSkinId].Skin;

        _characterObj = Instantiate(characterPrefab, _characterParent);
        _equipObj = Instantiate(equipPrefab, _equipParent);

        Animator charAnim = _characterObj.GetComponent<Animator>();
        Animator equipAnim = _equipObj.GetComponent<Animator>();

        charAnim.runtimeAnimatorController = _characterController;
        equipAnim.runtimeAnimatorController = _equipController;

        charAnim.enabled = true;
        equipAnim.enabled = true;

        _skinNameText.gameObject.SetActive(false);
        _buybutton.gameObject.SetActive(false);
    }

    private void OnBuyClick()
    {
        if (_isClicked) return;
        _isClicked = true;

        switch (_state)
        {
            case SkinViewStates.Buy:
                if (Manager.player.Data.Gem.Value < _cost || Manager.player.Data.Gem.IsInUpdate) return;
                Manager.player.Data.Gem.Value -= _cost;

                switch (_type)
                {
                    case SkinTypes.Character:
                        Manager.firebase.UserData.Skin.CharacterSkinList.Add(_skinId);
                        Manager.player.SetCharacterSkin(_skinId);
                        break;
                    case SkinTypes.Equip:
                        Manager.firebase.UserData.Skin.EquipSkinList.Add(_skinId);
                        Manager.player.SetEquipSkin(_skinId);
                        break;
                }

                StartCoroutine(WaitServerUpdate(() => 
                {
                    UpdateInfo();
                    _characterSkinPanel.SetButtonsInfo();
                    _equipSkinPanel.SetButtonsInfo();

                    _isClicked = false;
                }));

                break;
            case SkinViewStates.Moneyless:
                Manager.ui.ShowPanelAsync<ShopPanel>();
                _isClicked = false;
                break;
        }
    }

    private void OnEquipClick()
    {
        if (_state != SkinViewStates.Owned) return;

        if (_isClicked) return;
        _isClicked = true;

        switch (_type)
        {
            case SkinTypes.Character:
                Manager.player.SetCharacterSkin(_skinId);
                break;
            case SkinTypes.Equip:
                Manager.player.SetEquipSkin(_skinId);
                break;
        }

        StartCoroutine(WaitServerUpdate(() =>
        {
            UpdateInfo();
            _characterSkinPanel.SetButtonsInfo();
            _equipSkinPanel.SetButtonsInfo();

            _isClicked = false;
        }));
    }

    private IEnumerator WaitServerUpdate(Action OnComplete)
    {
        yield return new WaitUntil(() => Manager.firebase.UserData.Skin.CharacterSkinList.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.Skin.EquipSkinList.IsInit);
        yield return new WaitUntil(() => !Manager.player.Data.CharacterSkinId.IsInUpdate);
        yield return new WaitUntil(() => !Manager.player.Data.EquipSkinId.IsInUpdate);

        OnComplete();
    }
}
