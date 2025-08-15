using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private void Awake() => Init();

    void Init()
    {
        base.SingletonInit();
    }
}
