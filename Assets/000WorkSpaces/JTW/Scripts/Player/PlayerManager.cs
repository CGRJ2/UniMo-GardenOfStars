using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public PlayerData Data = new PlayerData();


    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();
    }

    public void InitPlayerData()
    {
        // TODO : 플레이어 데이터를 파이어베이스의 값으로 초기화


    }
}
