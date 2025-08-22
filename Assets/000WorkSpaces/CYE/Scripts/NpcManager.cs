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
        // 현재 지역의 id를 가져와서
        // npc를 불러와 _currentNpc에 지정함
    }
}
