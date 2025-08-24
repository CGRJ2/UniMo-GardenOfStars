using System.Collections;
using UnityEngine;

public class QuestProdsInsertArea : InteractableBase
{
    public ConstellationBuilding ownerInstance;
    [SerializeField] Transform attachPoint;
    [SerializeField] float insertDelayTime = 0.1f;
    // ������ ���� ������ �� ����
    public void Init(ConstellationBuilding instance)
    {
        this.ownerInstance = instance;
        //Manager.buildings.workStatinLists.insertAreas.Add(this);
    }

    IEnumerator AutoInserting()
    {
        while (characterRD != null)
        {

            // �÷��̾� �տ� �ִ� ��ᰡ ����Ʈ ���ǿ� ���ԵǴ��� üũ
            IngrediantInstance instanceProd;
            if (characterRD.IngrediantStack.TryPeek(out instanceProd))
            {
                Temp_Requirement targetRequirement = null;
                foreach (Temp_Requirement requirement in ownerInstance.requirements)
                {
                    // �տ� �ִ� ��ᰡ ����Ʈ ���ǿ� �ִ� ����̰� && �������� ���� ��Ȳ�̸�
                    if (instanceProd.Data.ID == requirement.prodId && !requirement.isClear)
                    {
                        targetRequirement = requirement;    // Ÿ������ ����
                        break;
                    }
                }


                if (targetRequirement != null)
                {
                    // ���� ���൵�� ���� �߰�
                    if (targetRequirement.curCount < targetRequirement.needCount)
                    {
                        IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                        targetRequirement.curCount += 1;
                        popedProd.MoveToTargetAndShrink(attachPoint);
                    }
                    else // �ʿ� ��� ������ŭ �� ������ ���� �Ϸ�ó�� �� ����
                    {
                        targetRequirement.isClear = true;
                        break;
                    }
                }

                // ���� ���Ա��� ������ �ð� ����
                yield return new WaitForSeconds(insertDelayTime);

                // ��� ���� �� ����Ʈ ���� ������Ʈ
                // 
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

        StartCoroutine(AutoInserting());
    }


    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //Debug.Log($"�ǹ������Կ���({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        //Manager.buildings?.workStatinLists.insertAreas?.Remove(this);
    }
}
