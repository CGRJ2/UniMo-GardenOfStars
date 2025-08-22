using System.Collections;
using UnityEngine;

public class InsertArea : InteractableBase
{
    [HideInInspector] public ManufactureBuilding instance;

    public bool isWorkable
    { get { return instance.ingrediantStack.Count < instance.runtimeData.maxStackableCount; }}

    // ������ ���� ������ �� ����
    public void Init(ManufactureBuilding instance)
    {
        this.instance = instance;
    }

    IEnumerator AutoStacking()
    {
        while (characterRD != null)
        {
            bool isStackable = false;


            // �ǹ��� ���� ������ �ִ� ������ŭ �׿��ִٸ� ���� ���
            if (instance.runtimeData.maxStackableCount > instance.ingrediantStack.Count)
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
                if (instanceProd.Data.ID == instance.originData.RequireProdID)
                {
                    IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                    popedProd.AttachToTarget(instance.attachPoint, instance.ingrediantStack.Count);
                    instance.ingrediantStack.Push(instanceProd);
                }

                // ���� ���Ա��� ������ �ð� ����
                yield return new WaitForSeconds(instance.insertDelayTime);
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

        StartCoroutine(AutoStacking());
    }


    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //Debug.Log($"�ǹ������Կ���({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}
