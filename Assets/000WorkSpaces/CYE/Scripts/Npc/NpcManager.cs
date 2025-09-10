using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNpc;
using UnityEngine;

public class NpcManager : Singleton<NpcManager>
{
    // for test
    public List<CYETestNpcDataSO> _npcRawData = new();
    public NpcData CurrentNpc;
    private void Awake()
    {
        base.SingletonInit();
        Init();
    }
    private void Init()
    {
        // // 초기화
        // SetCurrentNpc("test");
    }
    public void SetCurrentNpc(string regionId)
    {
        CurrentNpc = new NpcData(Manager.data.Npc.Values.FirstOrDefault(item => item.Value.StageId == regionId).Key);
    }
}
