using System.Collections;
using UnityEngine;

public class Interactable_Insert : InteractableBase
{
    public BuildingInstance_Manufacture instance;

    // ������ ���� ������ �� ����
    public void Init(BuildingInstance_Manufacture instance)
    {
        this.instance = instance;
    }

    IEnumerator AutoStacking()
    {
        while (pc != null)
        {
            bool isStackable = false;


            // �ǹ��� ���� ������ �ִ� ������ŭ �׿��ִٸ� ���� ���
            if (instance.runtimeData.maxStackableCount > instance.prodsStack.Count)
                isStackable = true;
            else isStackable = false;

            // ���� �ڸ��� �� ������ ���
            yield return new WaitUntil(() => isStackable);

            // ������ ������� �÷��̾ ������ ���� �ʰ� break;
            if (pc == null) yield break;

            // �÷��̾� �տ� ��ᰡ �ִ��� üũ
            IngrediantInstance instanceProd;
            Debug.Log(1);
            if (pc.ingrediantStack.TryPeek(out instanceProd))
            {
                Debug.Log(2);

                // �� ���� ���� ���� ���� ��ᰡ ���� ������ �� �־��ֱ�
                if (instanceProd.Data.ID == instance.originData.RequireProdID)
                {
                    Debug.Log(3);

                    IngrediantInstance popedProd = pc.ingrediantStack.Pop();
                    popedProd.AttachToTarget(instance.attachPoint, instance.prodsStack.Count);
                    instance.prodsStack.Push(instanceProd);
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

    public override void EnterInteract_Player()
    {
        base.EnterInteract_Player();
        //Debug.Log($"�ǹ������Կ���({buildingInstance.name}): ����� ��ȣ�ۿ� ����");

        StartCoroutine(AutoStacking());
    }


    public override void DeactiveInteract_Player()
    {
        base.DeactiveInteract_Player();
        //Debug.Log($"�ǹ������Կ���({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}
