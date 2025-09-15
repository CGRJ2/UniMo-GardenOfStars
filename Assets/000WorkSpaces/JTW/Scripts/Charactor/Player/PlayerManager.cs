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

    public bool IsControl = true;

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();
    }

    public void SpawnPlayer(Vector3 position = default)
    {
        _playerObj = Instantiate(_playerPrefab, position, Quaternion.identity);
    }
}
