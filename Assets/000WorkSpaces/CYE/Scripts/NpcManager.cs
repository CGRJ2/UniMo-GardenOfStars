using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
