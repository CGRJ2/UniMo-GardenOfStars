using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class GameManager : Singleton<GameManager>
{
    private void Awake() => Init();

    void Init()
    {
        base.SingletonInit();
    }
}
