using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ProductGenerater : MonoBehaviour
{
    public IngrediantSO ingrediantSO;
    public GenerateState state;
    public float cultivatingTime;
    public float progressedTime;
    
    ObjectPool _Pool;

    [SerializeField] IngrediantInstance _SpawnedProduct;

    Coroutine _CultivateRoutine;

    private void OnDisable()
    {
        // ���� ���� ���൵ ���� �� �ڷ�ƾ ����
        StopAllCoroutines();
    }

    public bool StandByCheck()
    {
        // ���깰�� ���� ��
        if (_SpawnedProduct == null)
        {
            state = GenerateState.StandBy;
            return true;
        }
        // ���깰�� ���� ��
        else
        {
            // ���깰�� �ƹ��� �������� �ʾҴٸ�
            if (_SpawnedProduct.owner == null)
            {
                state = GenerateState.Completed;
                return false;
            }
            // ������ ���깰�� �������ٸ�
            else
            {
                state = GenerateState.StandBy;
                _SpawnedProduct = null;
                return true;
            }
        }
    }

    IEnumerator CultivateRoutine()
    {
        while (true)
        {
            // �� �����Ӹ���, ���Ĺ��� ���� üũ
            yield return new WaitUntil(() => StandByCheck());

            // ���� ����
            state = GenerateState.Generating;
            while(state == GenerateState.Generating)
            {
                progressedTime += Time.deltaTime;

                if (progressedTime > cultivatingTime)
                {
                    SpawnProduct(); // ���� �Ϸ�
                    state = GenerateState.Completed;
                } 

                yield return null;
            }
            yield return null;
        }
    }

    public void Init(IngrediantSO ingrediantData, float cultivateTime)
    {
        this.ingrediantSO = ingrediantData;
        this.cultivatingTime = cultivateTime;

        _Pool = Manager.pool.GetPoolBundle(ingrediantData.InstancePrefab).instancePool;

        // ���� ��ƾ ����(�ӽ�)
        if (_CultivateRoutine != null) StopCoroutine(_CultivateRoutine);
        _CultivateRoutine = StartCoroutine(CultivateRoutine());
    }

    public void SpawnProduct()
    {
        // �ߺ� ����
        if (state != GenerateState.Generating) return; 

        // ������Ʈ Ǯ���� Ȱ��ȭ
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // ���൵ �ʱ�ȭ
        progressedTime = 0;

        // ���깰 ���� ����
        _SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();
    }
}

public enum GenerateState { StandBy,  Generating, Completed }

