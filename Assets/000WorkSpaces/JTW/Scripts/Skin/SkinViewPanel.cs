using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
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
    private TextMeshProUGUI _buttonText => GetUI<TextMeshProUGUI>("ButtonText");

    private Button _button => GetUI<Button>("Button");

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

        _button.onClick.AddListener(OnClick);
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

        GameObject Prefab = data.Skin;
        TargetObject = Instantiate(Prefab, TargetParent);



        Animator anim = TargetObject.GetComponent<Animator>();
        anim.runtimeAnimatorController = TargetController;
        anim.enabled = true;

        UpdateInfo();
    }

    public void UpdateInfo()
    {
        _skinNameText.gameObject.SetActive(true);
        _button.gameObject.SetActive(true);

        bool isOwned;
        bool isEquiped;

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
            // TODO : 장착 중 상태
            return;
        }

        if (isOwned)
        {
            _state = SkinViewStates.Owned;
            // TODO : 보유 중 상태
            return;
        }

        if(Manager.player.Data.Gem.Value < _cost)
        {

            _state = SkinViewStates.Moneyless;
            // TODO : 구매 불가 상태
            return;
        }

        _state = SkinViewStates.Buy;
        // TODO : 구매 가능 상태
    }

    public void ClearAvatar()
    {
        if (_characterObj != null) Destroy(_characterObj);
        if (_equipObj != null) Destroy(_equipObj);

        string characterSkinId = Manager.player.Data.CharacterSkinId.Value;
        string equipSkinId = Manager.player.Data.EquipSkinId.Value;

        GameObject characterPrefab = Manager.data.CharacterSkin.Values[characterSkinId].Skin;
        GameObject equipPrefab = Manager.data.EquipSkin.Values[characterSkinId].Skin;

        _characterObj = Instantiate(characterPrefab, _characterParent);
        _equipObj = Instantiate(equipPrefab, _equipParent);

        Animator charAnim = _characterObj.GetComponent<Animator>();
        Animator equipAnim = _equipObj.GetComponent<Animator>();

        charAnim.runtimeAnimatorController = _characterController;
        equipAnim.runtimeAnimatorController = _equipController;

        charAnim.enabled = true;
        equipAnim.enabled = true;

        _skinNameText.gameObject.SetActive(false);
        _button.gameObject.SetActive(false);
    }

    private void OnClick()
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
            case SkinViewStates.Owned:
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
                break;
            case SkinViewStates.Moneyless:
                Manager.ui.ShowPanelAsync<ShopPanel>();
                _isClicked = false;
                break;
        }
    }

    private IEnumerator WaitServerUpdate(Action OnComplete)
    {
        yield return new WaitUntil(() => Manager.firebase.UserData.Skin.CharacterSkinList.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.Skin.EquipSkinList.IsInit);
        yield return new WaitUntil(() => !Manager.player.Data.CharacterSkinId.IsInUpdate);
        yield return new WaitUntil(() => !Manager.player.Data.EquipSkinId.IsInUpdate);
    }
}
