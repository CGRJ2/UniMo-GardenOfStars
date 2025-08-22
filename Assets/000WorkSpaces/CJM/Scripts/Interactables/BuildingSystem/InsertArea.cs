using System.Collections;
using UnityEngine;

public class InsertArea : InteractableBase
{
    [HideInInspector] public ManufactureBuilding instance;

    // ������ ���� ������ �� ����
    public void Init(ManufactureBuilding instance)
    {
        this.instance = instance;
    }

    IEnumerator AutoStacking()
    {
        while (characterRuntimeData != null)
        {
            bool isStackable = false;


            // �ǹ��� ���� ������ �ִ� ������ŭ �׿��ִٸ� ���� ���
            if (instance.runtimeData.maxStackableCount > instance.ingrediantStack.Count)
                isStackable = true;
            else isStackable = false;

            // ���� �ڸ��� �� ������ ���
            yield return new WaitUntil(() => isStackable);

            // ������ ������� �÷��̾ ������ ���� �ʰ� break;
            if (characterRuntimeData == null) yield break;

            // �÷��̾� �տ� ��ᰡ �ִ��� üũ
            IngrediantInstance instanceProd;
            if (characterRuntimeData.ingrediantStack.TryPeek(out instanceProd))
            {
                // �� ���� ���� ���� ���� ��ᰡ ���� ������ �� �־��ֱ�
                if (instanceProd.Data.ID == instance.originData.RequireProdID)
                {
                    IngrediantInstance popedProd = characterRuntimeData.ingrediantStack.Pop();
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

    public override void EnterInteract(PlayerController characterRuntimeData)
    {
        base.EnterInteract(characterRuntimeData);
        //Debug.Log($"�ǹ������Կ���({buildingInstance.name}): ����� ��ȣ�ۿ� ����");

        StartCoroutine(AutoStacking());
    }


    public override void ExitInteract(PlayerController characterRuntimeData)
    {
        base.ExitInteract(characterRuntimeData);
        //Debug.Log($"�ǹ������Կ���({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}
