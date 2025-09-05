using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class BuildingSeller : BuildingInstance
{
    ObjectPool _Pool;
    private void Awake()
    {
        base.BIBaseInit();
        activatePopUI.Init(this);
        Addressables.LoadAssetAsync<GameObject>("it_Building").Completed += task =>
        {
            GameObject product = task.Result;
            _Pool = Manager.pool.GetPoolBundle(product).instancePool;
        };
    }

    public void SpawnBuildingItem()
    {
        // 오브젝트 풀에서 활성화
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);
        Item_Building buildingItem = disposedObject.GetComponent<Item_Building>();

        Addressables.LoadAssetAsync<GameObject>(buildingItem.buildingId).Completed += task =>
        {
            GameObject product = task.Result;
            if (characterRD.IngrediantStack.Count > 0)
            {
                // 손에 뭐가 있으면 구매 불가 판정
            }
            else
            {
                buildingItem.AttachToTarget(characterRD.ProdsAttachPoint);
            }
        };
    }
}
