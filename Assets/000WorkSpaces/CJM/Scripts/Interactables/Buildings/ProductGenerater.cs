using System.Collections;
using UnityEngine;

public class ProductGenerater : InteractableBase, IWorkStation
{
    public bool isWorkable;
    public bool isReserved;
    HarvestBD originData;

    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() { return isReserved; }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }

    public float ProdTime // 현재 업그레이드 단계에 따른 [생산 속도]
    {
        get
        {
            UpgradeData upgradeData = Manager.buildings.GetUpgradeData(originData.ID);
            int level = upgradeData == null ? 0 : upgradeData.Level_ProdTime.Value;

            return originData.Stat_ProdTime.Values[level];
        }
    }

    public float progressedTime;

    ObjectPool _Pool;

    public IngrediantInstance _SpawnedProduct { get; private set; }

    Coroutine _CultivateRoutine;
    public void Init(GameObject prodPrefab, HarvestBD originData)
    {
        this.originData = originData;

        _Pool = Manager.pool.GetPoolBundle(prodPrefab).instancePool;

        // 생산 루틴 가동(임시)
        if (_CultivateRoutine != null) StopCoroutine(_CultivateRoutine);
        _CultivateRoutine = StartCoroutine(CultivateRoutine());

        Manager.buildings.workStatinLists.productGeneraters.Add(this);
    }

    protected override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();

        // 생산 코루틴 중지
        StopAllCoroutines();

        Manager.buildings?.workStatinLists.productGeneraters?.Remove(this);
    }
    public bool StandByCheck()
    {
        // 생산물이 없을 때
        if (_SpawnedProduct == null)
        {
            //state = GenerateState.StandBy;
            isWorkable = false;
            return true;
        }
        // 생산물이 있을 때
        else
        {
            if (_SpawnedProduct.IsReadyToHarvest())
            {
                isWorkable = true;
                return false;
            }
            else
            {
                isWorkable = false;
                return false;
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
            //state = GenerateState.Generating;
            while (!isWorkable)
            {
                progressedTime += Time.deltaTime;

                // 생산시간 절반 남았을 때 꽃봉오리 소환
                if (progressedTime > ProdTime / 2f)
                {
                    if (_SpawnedProduct == null)
                        SpawnProduct(); 
                }

                if (progressedTime > ProdTime)
                {
                    GenerateComplete(); // 생산 완료
                }

                yield return null;
            }
            yield return null;
        }
    }

    

    void SpawnProduct()
    {
        // 오브젝트 풀에서 활성화
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // 생산물 정보 저장
        _SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();
        _SpawnedProduct.state.Value = ProdState.WaitForComplete;
    }

    void GenerateComplete()
    {
        // 진행도 초기화
        progressedTime = 0;

        // 생산 완료
        _SpawnedProduct.state.Value = ProdState.Completed;
        isWorkable = true;
    }

    public void PickUpProds()
    {
        // 생성된 재료가 없으면 실행 안함
        if (_SpawnedProduct == null) return;

        // "생성 중" 애니메이션 종료된 상태에서만 수확 가능
        if (!_SpawnedProduct.IsReadyToHarvest()) return;

        if (characterRD == null) return;

        // 들고 있는 재료와 다른 재료라면 or 손에 최대 수량만큼 들고 있을 시 => 줍지 않게 만들기
        IngrediantInstance instanceProd;
        if (characterRD.IngrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd == null || instanceProd.Data.ID != _SpawnedProduct.Data.ID) return;
            if (characterRD.IngrediantStack.Count >= characterRD.GetMaxCapacity()) return;
        }

        _SpawnedProduct.state.Value = ProdState.Harvested;

        _SpawnedProduct.AttachToTarget(characterRD.ProdsAttachPoint, characterRD.IngrediantStack.Count, characterRD);
        //Debug.Log($"{pc.ingrediantStack.Count}번째 위치로");
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

