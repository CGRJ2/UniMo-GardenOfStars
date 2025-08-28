using System.Collections;
using System.Collections.Generic;
using GameNpc;
using UnityEngine;

public class NpcManager : Singleton<NpcManager>
{
    // for test
    public CYETestNpcDataSO _npcRawData;
    public Npc CurrentNpc;
    private void Awake()
    {
        base.SingletonInit();
        Init();
    }
    private void Init()
    {
        // 초기화
        SetCurrentNpc("test");
    }
    public void SetCurrentNpc(string regionId)
    {
        // 해당하는 regionId의 Npc 데이터를 불러와서
        // CurrentNpc에 넣어줌
        // for test
        CurrentNpc = new Npc(_npcRawData);
    }
}
