using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ProdsArea : InteractableBase, IWorkStation
{
    public bool isWorkable { get { return ProdsCount > 0; } }
    public bool isReserved;
    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() { return isReserved; }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }

    [HideInInspector] public ManufactureBuilding ownerInstance;
    ObjectPool _Pool;

    public int ProdsCount;
    IngrediantInstance _ProdsResultInstance;
    [SerializeField] Canvas canvas_ProdsResult;
    [SerializeField] TMP_Text tmp_Count;

    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        string key = instance.originData.ProductID; // 임시로 Id를 이름으로 설정

        // 생산될 재료 인스턴스 풀 지정 or 생성
        Addressables.LoadAssetAsync<GameObject>(key).Completed += task =>
        {
            GameObject product = task.Result;

            _Pool = Manager.pool.GetPoolBundle(product).instancePool;
        };
        
        Manager.buildings.workStatinLists.prodsAreas.Add(this);
    }


    public void PickUp()
    {
        // 손에 있는거랑 다른 재료면 회수 못함
        IngrediantInstance instanceProd;
        if (characterRD.IngrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd.Data.ID != ownerInstance.originData.ProductID) return;
        }

        // 오브젝트 풀에서 활성화
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);
        IngrediantInstance _SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();

        _SpawnedProduct.AttachToTarget(characterRD.ProdsAttachPoint, characterRD.IngrediantStack.Count);
        //Debug.Log($"{pc.ingrediantStack.Count}번째 위치로");
        characterRD.IngrediantStack.Push(_SpawnedProduct);
        ProdsCount -= 1;
    }

    IEnumerator PickUpRoutine()
    {
        while (characterRD != null)
        {
            yield return new WaitUntil(() => ProdsCount > 0 || characterRD == null);

            if (characterRD == null) break;

            PickUp();
            //_SpawnedProduct = null;
            tmp_Count.text = $"{ProdsCount}";
            yield return new WaitForSeconds(ownerInstance.prodsAbsorbDelayTime);
        }
    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        StartCoroutine(PickUpRoutine());
    }

    public override void Enter_PersonalTask(CharaterRuntimeData singleInteracter)
    {
        base.Enter_PersonalTask(singleInteracter);
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.prodsAreas?.Remove(this);
    }
}
