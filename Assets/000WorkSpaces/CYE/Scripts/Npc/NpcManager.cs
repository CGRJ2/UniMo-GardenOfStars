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
    public void SetCurrentNpc()
    {
        string curStageID = Manager.firebase.UserData.CurStage.Value;
        Debug.LogError(curStageID);
        // 해당하는 regionId의 Npc 데이터를 불러와서
        // CurrentNpc에 넣어줌
        // for test
        // CurrentNpc = new NpcController(_npcRawData.Find(item => item._stageId.Equals(regionId)));
        Debug.LogError($"[NpcManager] {Manager.data.Npc.Values.Count}");
        CurrentNpc = new NpcData(Manager.data.Npc.Values.FirstOrDefault(item => item.Value.StageId == curStageID).Key);
    }
}
