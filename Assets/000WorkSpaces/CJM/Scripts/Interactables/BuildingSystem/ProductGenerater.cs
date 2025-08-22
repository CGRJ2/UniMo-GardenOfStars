using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ProductGenerater : InteractableBase
{
    public GenerateState state;
    public float productionTime;
    public float progressedTime;
    
    ObjectPool _Pool;

    [SerializeField] IngrediantInstance _SpawnedProduct;

    Coroutine _CultivateRoutine;

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();

        // ���� �ڷ�ƾ ����
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
            state = GenerateState.Completed;
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
            state = GenerateState.Generating;
            while(state == GenerateState.Generating)
            {
                progressedTime += Time.deltaTime;

                if (progressedTime > productionTime)
                {
                    SpawnProduct(); // ���� �Ϸ�
                    state = GenerateState.Completed;
                } 

                yield return null;
            }
            yield return null;
        }
    }

    public void Init(GameObject prodPrefab, float productionTime)
    {
        this.productionTime = productionTime;

        _Pool = Manager.pool.GetPoolBundle(prodPrefab).instancePool;

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

    public void PickUpProds()
    {
        // ������ ��ᰡ ������ ���� ����
        if (_SpawnedProduct == null) return;

        // �ϲ��� ��쿡�� �߰��ؾ���

        // ��� �ִ� ���� �ٸ� ���� ���� �ʰ� �����
        IngrediantInstance instanceProd;
        if (characterRuntimeData.ingrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd.Data.ID != _SpawnedProduct.Data.ID) return;
        }

        _SpawnedProduct.owner = characterRuntimeData.gameObject;
        _SpawnedProduct.AttachToTarget(characterRuntimeData.prodsAttachPoint, characterRuntimeData.ingrediantStack.Count);
        //Debug.Log($"{pc.ingrediantStack.Count}��° ��ġ��");
        characterRuntimeData.ingrediantStack.Push(_SpawnedProduct);
        _SpawnedProduct = null;
    }


    public override void EnterInteract(PlayerController characterRuntimeData)
    {
        base.EnterInteract(characterRuntimeData);

        PickUpProds();
    }
}

public enum GenerateState { StandBy, Generating, Completed }

