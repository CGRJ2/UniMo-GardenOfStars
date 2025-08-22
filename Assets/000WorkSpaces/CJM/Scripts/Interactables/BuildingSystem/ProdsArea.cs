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
        string key = instance.originData.ProductID; // �ӽ÷� Id�� �̸����� ����

        // ����� ��� �ν��Ͻ� Ǯ ���� or ����
        Addressables.LoadAssetAsync<GameObject>(key).Completed += task =>
        {
            GameObject product = task.Result;

            _Pool = Manager.pool.GetPoolBundle(product).instancePool;
        };
    }
    public void PickUp()
    {
        // ������Ʈ Ǯ���� Ȱ��ȭ
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
