using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private GameObject _playerPrefab;

    public PlayerData Data => Manager.firebase.UserData.Player;

    private GameObject _playerObj;
    public GameObject PlayerObj => _playerObj;
    private GameObject _characterSkin;
    private GameObject _equipSkin;

    private Transform _characterskinParent;
    private PlayerView _playerView;

    public bool IsControl = true;

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();
    }

    public void SpawnPlayer(Vector3 position = default)
    {
        _playerObj = Instantiate(_playerPrefab, position, Quaternion.identity);
        _characterskinParent = _playerObj.transform.Find("AvatarRoot");
        _playerView = _playerObj.GetComponent<PlayerView>();

        SetCharacterSkin(Manager.player.Data.CharacterSkinId.Value);
        SetEquipSkin(Manager.player.Data.EquipSkinId.Value);
    }

    public void SetCharacterSkin(string id)
    {
        Data.CharacterSkinId.Value = id;

        if(_characterSkin != null)
        {
            Destroy(_characterSkin);
        }

        GameObject skin = Manager.data.CharacterSkin.Values[id].Skin;

        _characterSkin = Instantiate(skin, _characterskinParent);
        _playerView.SetCharacterAnimator(_characterSkin.GetComponent<Animator>());
    }

    public void SetEquipSkin(string id)
    {
        Data.EquipSkinId.Value = id;

        if (_equipSkin != null)
        {
            Destroy(_equipSkin);
        }

        GameObject skin = Manager.data.EquipSkin.Values[id].Skin;

        _equipSkin = Instantiate(skin, transform);
        _playerView.SetEquipAnimator(_equipSkin.GetComponent<Animator>());
    }
}
