using System.Collections;
using UnityEngine;

public class InsertArea : InteractableBase, IWorkStation
{
    [HideInInspector] public ManufactureBuilding ownerInstance;

    public bool isWorkable
    { get { return ownerInstance.ingrediantStack.Count < ownerInstance.runtimeData.capacity; }}
    public bool isReserved;
    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() { return isReserved; }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }
    // ������ ���� ������ �� ����
    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        Manager.buildings.workStatinLists.insertAreas.Add(this);
    }

    IEnumerator AutoStacking()
    {
        while (characterRD != null)
        {
            bool isStackable = false;


            // �ǹ��� ���� ������ �ִ� ������ŭ �׿��ִٸ� ���� ���
            if (ownerInstance.runtimeData.capacity > ownerInstance.ingrediantStack.Count)
                isStackable = true;
            else isStackable = false;

            // ���� �ڸ��� �� ������ ���
            yield return new WaitUntil(() => isStackable);

            // ������ ������� �÷��̾ ������ ���� �ʰ� break;
            if (characterRD == null) yield break;

            // �÷��̾� �տ� ��ᰡ �ִ��� üũ
            IngrediantInstance instanceProd;
            if (characterRD.IngrediantStack.TryPeek(out instanceProd))
            {
                // �� ���� ���� ���� ���� ��ᰡ ���� ������ �� �־��ֱ�
                if (instanceProd.Data.ID == ownerInstance.originData.RequireProdID)
                {
                    IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                    popedProd.AttachToTarget(ownerInstance.attachPoint, ownerInstance.ingrediantStack.Count);
                    ownerInstance.ingrediantStack.Push(instanceProd);
                }

                // ���� ���Ա��� ������ �ð� ����
                yield return new WaitForSeconds(ownerInstance.insertDelayTime);
            }
            // �÷��̾� �տ� ��ᰡ ������ �ٷ� return
            else
            {
                yield return null;
            }
        }
    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        //Debug.Log($"�ǹ������Կ���({buildingInstance.name}): ����� ��ȣ�ۿ� ����");

        if (characterRD is WorkerRuntimeData worker)
        {
            if (worker.CurWorkstation.Value != this as IWorkStation) return;
        }

        StartCoroutine(AutoStacking());
    }


    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //Debug.Log($"�ǹ������Կ���({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.insertAreas?.Remove(this);
    }
}
