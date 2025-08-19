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
        // 현재 생산 진행도 저장 후 코루틴 중지
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
            // 생산물을 아무도 가져가지 않았다면
            if (_SpawnedProduct.owner == null)
            {
                state = GenerateState.Completed;
                return false;
            }
            // 누군가 생산물을 가져갔다면
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
            // 매 프레임마다, 스탠바이 상태 체크
            yield return new WaitUntil(() => StandByCheck());

            // 생산 시작
            state = GenerateState.Generating;
            while(state == GenerateState.Generating)
            {
                progressedTime += Time.deltaTime;

                if (progressedTime > cultivatingTime)
                {
                    SpawnProduct(); // 생산 완료
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
}

public enum GenerateState { StandBy,  Generating, Completed }

