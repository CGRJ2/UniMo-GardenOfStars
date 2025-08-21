using System.Collections;
using UnityEngine;

public class Interactable_Insert : InteractableBase
{
    BuildingInstance_Work buildingInstance;

    // ������ ���� ������ �� ����
    public void Init(BuildingInstance_Work buildingInstance)
    {
        this.buildingInstance = buildingInstance;
    }

    IEnumerator AutoStacking()
    {
        while (pc != null)
        {
            bool isStackable = false;

            // �ǹ��� ���� ������ �ִ� ������ŭ �׿��ִٸ� ���� ���
            if (buildingInstance.maxStackableCount > buildingInstance.prodsStack.Count)
                isStackable = true;
            else isStackable = false;

            // ���� �ڸ��� �� ������ ���
            yield return new WaitUntil(() => isStackable);

            // ������ ������� �÷��̾ ������ ���� �ʰ� break;
            if (pc == null) yield break;

            // �÷��̾� �տ� ��ᰡ �ִ��� üũ
            IngrediantInstance instanceProd;
            if (pc.ingrediantStack.TryPeek(out instanceProd))
            {
                // �� ���� ���� ���� ���� ��ᰡ ���� ������ �� �־��ֱ�
                if (instanceProd.Data == buildingInstance.insertableProdData)
                {
                    IngrediantInstance popedProd = pc.ingrediantStack.Pop();
                    popedProd.AttachToTarget(buildingInstance.attachPoint, buildingInstance.prodsStack.Count);
                    buildingInstance.prodsStack.Push(instanceProd);
                }

                // ���� ���Ա��� ������ �ð� ����
                yield return new WaitForSeconds(buildingInstance.insertDelayTime);
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
