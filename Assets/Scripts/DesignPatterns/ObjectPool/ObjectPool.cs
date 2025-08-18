using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private Stack<GameObject> pool = new Stack<GameObject>();
    public GameObject poolObj; // 풀 오브젝트들 담을 풀 부모 오브젝트

    private GameObject _prefab;
    public ObjectPool(GameObject prefab, int size, string poolName = "ObjectPool")
    {
        _prefab = prefab; // 프리펩 정보 저장

        // 풀 오브젝트 생성 (담을 곳)
        poolObj = new GameObject(poolName);

        for (int i = 0; i < size; i++)
        {
            // 풀 오브젝트에 담기 (오브젝트)
            GameObject instance = Object.Instantiate(prefab, poolObj.transform);

            // 리스트에 담기
            pool.Push(instance);

            // 다시 풀에 넣을 때 필요한 참조
            PooledObject PoolChild = instance.GetComponent<PooledObject>();

            // 풀에 넣을 프리펩 인스턴스들이 Pooled_Obj 컴포넌트를 갖고 있는지 여부 판단.
            if (PoolChild == null)
            {
                Debug.LogError("생성하려는 Pool의 Prefab에 Pooled_Obj 컴포넌트가 없습니다.");
                return;
            }

            // 다시 풀에 넣을 때 필요한 참조
            PoolChild.ParentPool = this;
            instance.SetActive(false);
        }
    }

    // 풀 오브젝트 비활성화(파괴)
    public void ReturnPooledObj(GameObject obj)
    {
        pool.Push(obj);

        // 다른 부모의 자식에 있었다면, 원래 풀의 자식으로 들어가기
        if (obj.transform.parent != poolObj.transform)
            obj.transform.SetParent(poolObj.transform);

        obj.SetActive(false);
    }

    // 풀 오브젝트 활성화(생성)
    public GameObject DisposePooledObj(Vector3 position, Quaternion rotation)
    {
        if (pool.Count > 0)
        {
            GameObject instance = pool.Pop();
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);
            return instance;
        }
        else
        {
            Debug.LogWarning("오브젝트풀 비어서 추가로 생성.");
            
            // 풀 오브젝트에 담기 (즉시 생성한 오브젝트)
            GameObject instance = Object.Instantiate(_prefab, poolObj.transform);

            // 다시 풀에 넣을 때 필요한 참조
            PooledObject PoolChild = instance.GetComponent<PooledObject>();
            if (PoolChild == null){ Debug.LogError("생성하려는 Pool의 Prefab에 Pooled_Obj 컴포넌트가 없습니다."); }
            PoolChild.ParentPool = this;
            instance.SetActive(false); // 풀 오브젝트 초기화를 위한 첫 비활성화

            // 위치 맞춰주고 다시 활성화
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);

            return instance;
        }
    }

    public void DestroyAll()
    {
        // 풀 삭제
        Object.Destroy(poolObj);
        pool = null;
    }

}
