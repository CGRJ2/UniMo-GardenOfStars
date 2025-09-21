using System.Collections.Generic;
using GameNpc;

public class NpcManager : Singleton<NpcManager>
{
    public NpcData CurStageNpc => Manager.firebase.UserData.CurStageData.Npc;
    public FirebaseProperty<string> CurNpcId => Manager.firebase.UserData.CurStageData.Npc.NpcID;

    private void Awake()
    {
        base.SingletonInit();
        Init();
    }
    private void Init()
    {
        // 초기화
    }

    // 현재 스테이지의 NpcID 등록 및 반환
    public string CurStageNpcDataInit(string curStageID)
    {
        CurNpcId.Value = Manager.data.Stage.Values[curStageID].NpcID;
        return CurNpcId.Value;
    }
}
