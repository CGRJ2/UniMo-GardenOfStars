using System.Collections.Generic;
using GameNpc;

public class NpcManager : Singleton<NpcManager>
{
    public NpcData CurrentNpc => Manager.firebase.UserData.CurStageData.Npc;
    
    private void Awake()
    {
        base.SingletonInit();
        Init();
    }
    private void Init()
    {
        // 초기화
    }

}
