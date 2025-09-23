using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : Singleton<PoolManager>
{
    // 씬 전환 시 초기화
    private Dictionary<string,PoolBundle> poolsDic = new();
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        poolsDic = new();
    }

    [Header("풀 생성 시 기본 개수 설정")]
    public int count;

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public PoolBundle GetPoolBundle(GameObject prefab)
    {
        // 해당 풀이 있는지 체크
        if (poolsDic.ContainsKey(prefab.name)) return poolsDic[prefab.name];
        else
        {
            Debug.Log("해당 프리펩을 담아둔 오브젝트 풀이 없음 => 없으면 풀 만들기");
            PoolBundle bundle = new($"{prefab.name} Pool", prefab, count);
            bundle.Init();
            poolsDic.Add(prefab.name, bundle);
            return bundle;
        }
    }

    public PoolBundle GetPoolBundle(GameObject prefab, int customCount)
    {
        // 해당 풀이 있는지 체크
        if (poolsDic.ContainsKey(prefab.name)) return poolsDic[prefab.name];
        else
        {
            Debug.Log("해당 프리펩을 담아둔 오브젝트 풀이 없음 => 없으면 풀 만들기");
            PoolBundle bundle = new($"{prefab.name} Pool", prefab, customCount);
            bundle.Init();
            poolsDic.Add(prefab.name, bundle);
            return bundle;
        }
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