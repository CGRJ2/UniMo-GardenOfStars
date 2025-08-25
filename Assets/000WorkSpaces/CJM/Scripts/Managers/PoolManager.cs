using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Header("생성된 풀 관리")]    // 씬 전환 시 초기화 필요한가?
    public List<PoolBundle> pools;

    [Header("풀 생성 시 기본 개수 설정")]
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

        Debug.Log("해당 프리펩을 담아둔 오브젝트 풀이 없음 => 없으면 풀 만들기");
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