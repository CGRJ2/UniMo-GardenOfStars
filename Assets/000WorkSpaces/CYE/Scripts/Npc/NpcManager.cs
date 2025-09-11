using System.Collections.Generic;

public class NpcManager : Singleton<NpcManager>
{
    // for test
    public List<CYETestNpcDataSO> _npcRawData = new();
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

}
