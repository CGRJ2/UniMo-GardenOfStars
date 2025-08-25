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
        string key = instance.originData.ProductID; // �ӽ÷� Id�� �̸����� ����

        // ����� ��� �ν��Ͻ� Ǯ ���� or ����
        Addressables.LoadAssetAsync<GameObject>(key).Completed += task =>
        {
            GameObject product = task.Result;

            _Pool = Manager.pool.GetPoolBundle(product).instancePool;
        };
        
        Manager.buildings.workStatinLists.prodsAreas.Add(this);
    }


    public void PickUp()
    {
        // �տ� �ִ°Ŷ� �ٸ� ���� ȸ�� ����
        IngrediantInstance instanceProd;
        if (characterRD.IngrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd.Data.ID != ownerInstance.originData.ProductID) return;
        }

        // ������Ʈ Ǯ���� Ȱ��ȭ
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);
        IngrediantInstance _SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();

        _SpawnedProduct.AttachToTarget(characterRD.ProdsAttachPoint, characterRD.IngrediantStack.Count);
        //Debug.Log($"{pc.ingrediantStack.Count}��° ��ġ��");
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
