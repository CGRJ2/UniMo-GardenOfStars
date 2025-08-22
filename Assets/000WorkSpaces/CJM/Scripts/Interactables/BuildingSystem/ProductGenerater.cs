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

        // 생산 코루틴 중지
        StopAllCoroutines();
    }
    public bool StandByCheck()
    {
        // 생산물이 없을 때
        if (_SpawnedProduct == null)
        {
            state = GenerateState.StandBy;
            return true;
        }
        // 생산물이 있을 때
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
            // 매 프레임마다, 스탠바이 상태 체크
            yield return new WaitUntil(() => StandByCheck());

            // 생산 시작
            state = GenerateState.Generating;
            while(state == GenerateState.Generating)
            {
                progressedTime += Time.deltaTime;

                if (progressedTime > productionTime)
                {
                    SpawnProduct(); // 생산 완료
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

        // 생산 루틴 가동(임시)
        if (_CultivateRoutine != null) StopCoroutine(_CultivateRoutine);
        _CultivateRoutine = StartCoroutine(CultivateRoutine());
    }

    public void SpawnProduct()
    {
        // 중복 방지
        if (state != GenerateState.Generating) return; 

        // 오브젝트 풀에서 활성화
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // 진행도 초기화
        progressedTime = 0;

        // 생산물 정보 저장
        _SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();
    }

    public void PickUpProds()
    {
        // 생성된 재료가 없으면 실행 안함
        if (_SpawnedProduct == null) return;

        // 일꾼일 경우에도 추가해야함

        // 들고 있는 재료와 다른 재료는 줍지 않게 만들기
        IngrediantInstance instanceProd;
        if (characterRuntimeData.ingrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd.Data.ID != _SpawnedProduct.Data.ID) return;
        }

        _SpawnedProduct.owner = characterRuntimeData.gameObject;
        _SpawnedProduct.AttachToTarget(characterRuntimeData.prodsAttachPoint, characterRuntimeData.ingrediantStack.Count);
        //Debug.Log($"{pc.ingrediantStack.Count}번째 위치로");
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

