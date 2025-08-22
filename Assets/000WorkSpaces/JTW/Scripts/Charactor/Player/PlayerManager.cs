using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private GameObject _playerPrefab;

    public PlayerData Data { get; private set; } = new PlayerData();

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();
    }

    public void InitPlayerData()
    {
        // TODO : 플레이어 데이터를 파이어베이스의 값으로 초기화


    }

    public void SpawnPlayer(Vector3 position = default)
    {
        Instantiate(_playerPrefab, position, Quaternion.identity);
    }
}
