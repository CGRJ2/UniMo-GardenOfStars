using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Header("������ Ǯ ����")]    // �� ��ȯ �� �ʱ�ȭ �ʿ��Ѱ�?
    public List<PoolBundle> pools;

    [Header("Ǯ ���� �� �⺻ ���� ����")]
    public int count;

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();

        /*foreach (PoolBundle poolBundle in pools)
        {
            poolBundle.Init();
        }*/
    }

    public PoolBundle GetPoolBundle(GameObject prefab)
    {
        foreach (PoolBundle poolBundle in pools)
        {
            if (poolBundle.IsInThisPool(prefab))
                return poolBundle;
        }

        Debug.Log("�ش� �������� ��Ƶ� ������Ʈ Ǯ�� ���� => ������ Ǯ �����");
        PoolBundle bundle = new($"{prefab.name} Pool", prefab, count);
        bundle.Init();
        pools.Add(bundle);
        return bundle;
    }

}

[Serializable]
public class PoolBundle
{
    public string poolName;
    public ObjectPool instancePool;
    public GameObject pooledObjectPrefab;
    public int count;

    public PoolBundle(string poolName, GameObject pooledObjectPrefab, int count)
    {
        this.poolName = poolName;
        this.pooledObjectPrefab = pooledObjectPrefab;
        this.count = count;
    }

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