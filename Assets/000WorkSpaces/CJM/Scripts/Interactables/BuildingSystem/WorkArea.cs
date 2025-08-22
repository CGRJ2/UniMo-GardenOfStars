using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class WorkArea : InteractableBase
{
    // �۾������� �ΰ� �̻��� ������Ʈ�� ��ġ�� ���¿���, ó�� �۾����̴� ��ü�� ������ �� �����۵��ϴ��� �׽�Ʈ �ʿ�

    [HideInInspector] public ManufactureBuilding instance;

    PlayerController curWorker; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
    [SerializeField] Slider progressBar;

    public void Init(ManufactureBuilding instance)
    {
        this.instance = instance;
    }

    IEnumerator ProgressingTask()
    {
        curWorker = characterRuntimeData; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
        while (characterRuntimeData != null)
        {
            // �׿��ִ� ��ᰡ �������� ����
            if (instance.ingrediantStack.Count > 0)
            {
                instance.progressedTime += Time.deltaTime;

                if (instance.runtimeData.productionTime < instance.progressedTime)
                {
                    CompleteTask(); // ����� ����
                    instance.progressedTime = 0; // ���൵ �ʱ�ȭ
                }
                
                // ���൵ ������ ������Ʈ
                progressBar.value = instance.progressedTime / instance.runtimeData.productionTime;

                yield return null;
            }
            else yield return null;
        }
        // �۾��� ������� curWorker �ʱ�ȭ
        curWorker = null;
    }

    public void CompleteTask()
    {
        // ����� �ν��Ͻ� ����(Ȱ��ȭ) ----> �̰Ÿ� ���� ��ü�� ���� ���·� �Ұ��� ���� ������
        //GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // ȸ�������� ���� �÷��ֱ�
        instance.prodsArea.prodsCount += 1;

        // ��� �Ҹ�
        instance.ingrediantStack.Pop().Despawn();
    }


    public override void EnterInteract(PlayerController characterRuntimeData)
    {
        base.EnterInteract(characterRuntimeData);
        //Debug.Log($"�ǹ��۾�����({buildingInstance.name}): ����� ��ȣ�ۿ� ����");

        StartCoroutine(ProgressingTask());
    }

    public override void ExitInteract(PlayerController characterRuntimeData)
    {
        base.ExitInteract(characterRuntimeData);
        //Debug.Log($"�ǹ��۾�����({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}
