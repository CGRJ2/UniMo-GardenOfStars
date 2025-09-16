using System.Collections.Generic;
using GameNpc;

public class NpcManager : Singleton<NpcManager>
{
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
