using System.Collections;
using System.Collections.Generic;
using GameNpc;
using UnityEngine;

public class NpcManager : Singleton<NpcManager>
{
    private NpcController _currentNpc;
    private void Awake()
    {
        base.SingletonInit();
        Init();
    }
    private void Init()
    {
        // 초기화

    }
    public void SetCurrentNpc(string regionId)
    { 
        
    }
}
