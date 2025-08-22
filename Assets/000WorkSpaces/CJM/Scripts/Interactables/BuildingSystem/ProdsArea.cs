using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ProdsArea : InteractableBase
{
    [HideInInspector] public ManufactureBuilding instance;
    ObjectPool _Pool;

    public int prodsCount;

    public void Init(ManufactureBuilding instance)
    {
        this.instance = instance;
        string key = instance.originData.ProductID; // 임시로 Id를 이름으로 설정

        // 생산될 재료 인스턴스 풀 지정 or 생성
        Addressables.LoadAssetAsync<GameObject>(key).Completed += task =>
        {
            GameObject product = task.Result;

            _Pool = Manager.pool.GetPoolBundle(product).instancePool;
        };
    }
    public void PickUp()
    {
        // 오브젝트 풀에서 활성화
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

    }

    public override void EnterInteract(PlayerController characterRuntimeData)
    {
        base.EnterInteract(characterRuntimeData);


    }

    public override void EnterInteractSingleOnly(PlayerController singleInteracter)
    {
        base.EnterInteractSingleOnly(singleInteracter);
    }
}
