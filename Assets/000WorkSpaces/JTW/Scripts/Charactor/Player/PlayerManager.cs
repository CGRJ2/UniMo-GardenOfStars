using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private GameObject _playerPrefab;

    public PlayerData Data => Manager.firebase.UserData.Player;

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();
    }

    public void SpawnPlayer(Vector3 position = default)
    {
        Instantiate(_playerPrefab, position, Quaternion.identity);
    }
}
