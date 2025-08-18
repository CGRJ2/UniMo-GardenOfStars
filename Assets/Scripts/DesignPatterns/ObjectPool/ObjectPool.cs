using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private Stack<GameObject> pool = new Stack<GameObject>();
    public GameObject poolObj; // Ǯ ������Ʈ�� ���� Ǯ �θ� ������Ʈ

    private GameObject _prefab;
    public ObjectPool(GameObject prefab, int size, string poolName = "ObjectPool")
    {
        _prefab = prefab; // ������ ���� ����

        // Ǯ ������Ʈ ���� (���� ��)
        poolObj = new GameObject(poolName);

        for (int i = 0; i < size; i++)
        {
            // Ǯ ������Ʈ�� ��� (������Ʈ)
            GameObject instance = Object.Instantiate(prefab, poolObj.transform);

            // ����Ʈ�� ���
            pool.Push(instance);

            // �ٽ� Ǯ�� ���� �� �ʿ��� ����
            PooledObject PoolChild = instance.GetComponent<PooledObject>();

            // Ǯ�� ���� ������ �ν��Ͻ����� Pooled_Obj ������Ʈ�� ���� �ִ��� ���� �Ǵ�.
            if (PoolChild == null)
            {
                Debug.LogError("�����Ϸ��� Pool�� Prefab�� Pooled_Obj ������Ʈ�� �����ϴ�.");
                return;
            }

            // �ٽ� Ǯ�� ���� �� �ʿ��� ����
            PoolChild.ParentPool = this;
            instance.SetActive(false);
        }
    }

    // Ǯ ������Ʈ ��Ȱ��ȭ(�ı�)
    public void ReturnPooledObj(GameObject obj)
    {
        pool.Push(obj);

        // �ٸ� �θ��� �ڽĿ� �־��ٸ�, ���� Ǯ�� �ڽ����� ����
        if (obj.transform.parent != poolObj.transform)
            obj.transform.SetParent(poolObj.transform);

        obj.SetActive(false);
    }

    // Ǯ ������Ʈ Ȱ��ȭ(����)
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
            Debug.LogWarning("������ƮǮ �� �߰��� ����.");
            
            // Ǯ ������Ʈ�� ��� (��� ������ ������Ʈ)
            GameObject instance = Object.Instantiate(_prefab, poolObj.transform);

            // �ٽ� Ǯ�� ���� �� �ʿ��� ����
            PooledObject PoolChild = instance.GetComponent<PooledObject>();
            if (PoolChild == null){ Debug.LogError("�����Ϸ��� Pool�� Prefab�� Pooled_Obj ������Ʈ�� �����ϴ�."); }
            PoolChild.ParentPool = this;
            instance.SetActive(false); // Ǯ ������Ʈ �ʱ�ȭ�� ���� ù ��Ȱ��ȭ

            // ��ġ �����ְ� �ٽ� Ȱ��ȭ
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);

            return instance;
        }
    }

    public void DestroyAll()
    {
        // Ǯ ����
        Object.Destroy(poolObj);
        pool = null;
    }

}
