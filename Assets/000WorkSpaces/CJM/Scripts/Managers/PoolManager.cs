using System;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    public PoolBundle[] pools;

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();

        foreach (PoolBundle poolBundle in pools)
        {
            poolBundle.Init();
        }
    }

    public PoolBundle GetPoolBundle(GameObject prefab)
    {
        foreach (PoolBundle poolBundle in pools)
        {
            if (poolBundle.IsInThisPool(prefab))
                return poolBundle;
        }

        Debug.Log("�ش� �������� ��Ƶ� ������Ʈ Ǯ�� ����");
        return null;
    }

}

[Serializable]
public class PoolBundle
{
    public string poolName;
    public ObjectPool instancePool;
    public GameObject pooledObjectPrefab;
    public int count;

    public void Init()
    {
        instancePool = new ObjectPool(pooledObjectPrefab, count, poolName);
    }

    public bool IsInThisPool(GameObject prefab)
    {
        if (prefab == pooledObjectPrefab)
            return true;
        else return false;
    }
}