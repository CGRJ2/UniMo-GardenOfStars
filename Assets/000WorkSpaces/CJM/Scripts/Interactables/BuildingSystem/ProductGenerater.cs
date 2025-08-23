using System.Collections;
using UnityEngine;

public class ProductGenerater : InteractableBase
{
    public bool isWorkable;
    
    //[SerializeField] GenerateState state;
    public float productionTime;
    public float progressedTime;

    ObjectPool _Pool;

    [SerializeField] IngrediantInstance _SpawnedProduct;

    Coroutine _CultivateRoutine;
    public void Init(GameObject prodPrefab, float productionTime)
    {
        this.productionTime = productionTime;

        _Pool = Manager.pool.GetPoolBundle(prodPrefab).instancePool;

        // ���� ��ƾ ����(�ӽ�)
        if (_CultivateRoutine != null) StopCoroutine(_CultivateRoutine);
        _CultivateRoutine = StartCoroutine(CultivateRoutine());

        Manager.buildings.workStatinLists.productGeneraters.Add(this);
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();

        // ���� �ڷ�ƾ ����
        StopAllCoroutines();

        Manager.buildings?.workStatinLists.productGeneraters?.Remove(this);
    }
    public bool StandByCheck()
    {
        // ���깰�� ���� ��
        if (_SpawnedProduct == null)
        {
            //state = GenerateState.StandBy;
            isWorkable = false;
            return true;
        }
        // ���깰�� ���� ��
        else
        {
            //state = GenerateState.Completed;
            isWorkable = true;
            return false;
        }
    }

    IEnumerator CultivateRoutine()
    {
        while (true)
        {
            // �� �����Ӹ���, ���Ĺ��� ���� üũ
            yield return new WaitUntil(() => StandByCheck());

            // ���� ����
            //state = GenerateState.Generating;
            while (!isWorkable)
            {
                progressedTime += Time.deltaTime;

                if (progressedTime > productionTime)
                {
                    SpawnProduct(); // ���� �Ϸ�
                }

                yield return null;
            }
            yield return null;
        }
    }

    

    public void SpawnProduct()
    {
        // ������Ʈ Ǯ���� Ȱ��ȭ
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // ���൵ �ʱ�ȭ
        progressedTime = 0;

        // ���깰 ���� ����
        _SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();

        isWorkable = true;
    }

    public void PickUpProds()
    {
        // ������ ��ᰡ ������ ���� ����
        if (_SpawnedProduct == null) return;

        // �ϲ��� ��쿡�� �߰��ؾ���

        // ��� �ִ� ���� �ٸ� ���� ���� �ʰ� �����
        IngrediantInstance instanceProd;
        if (characterRD.IngrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd.Data.ID != _SpawnedProduct.Data.ID) return;
        }

        _SpawnedProduct.owner = characterRD.gameObject;
        _SpawnedProduct.AttachToTarget(characterRD.ProdsAttachPoint, characterRD.IngrediantStack.Count);
        //Debug.Log($"{pc.ingrediantStack.Count}��° ��ġ��");
        characterRD.IngrediantStack.Push(_SpawnedProduct);
        _SpawnedProduct = null;
    }

    IEnumerator PickUpRoutine()
    {
        while (characterRD != null)
        {
            yield return new WaitUntil(() => _SpawnedProduct != null);

            if (characterRD == null) break;

            PickUpProds();
        }
    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);

        StartCoroutine(PickUpRoutine());
    }
}

public enum GenerateState { StandBy, Generating, Completed }

