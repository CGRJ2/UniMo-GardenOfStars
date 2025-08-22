using System.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UI;

public class WorkArea : InteractableBase
{
    public bool isWorkable { get { return curWorker == null & instance.ingrediantStack.Count > 0; } }

    [HideInInspector] public ManufactureBuilding instance;

    CharaterRuntimeData curWorker; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
    [SerializeField] Slider progressBar;

    public void Init(ManufactureBuilding instance)
    {
        this.instance = instance;
    }

    IEnumerator ProgressingTask()
    {
        curWorker = characterRD; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�

        // ���� ���±��� ����ߴٰ� �۾� ����
        yield return new WaitUntil(() => !curWorker.IsMove.Value);

        while (!curWorker.IsMove.Value) // ���� ������ ������ ����
        {
            // �۾� ���� ������ ������ ���
            if (curWorker != characterRD) break;

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

        curWorker = null;
    }

    public void CompleteTask()
    {
        // ����� �ν��Ͻ� ����(Ȱ��ȭ) ----> �̰Ÿ� ���� ��ü�� ���� ���·� �Ұ��� ���� ������
        //GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // ȸ�������� ���� �÷��ֱ�
        instance.prodsArea.ProdsCount += 1;

        // ��� �Ҹ�
        instance.ingrediantStack.Pop().Despawn();
    }


    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        //Debug.Log($"�ǹ��۾�����({buildingInstance.name}): ����� ��ȣ�ۿ� ����");
        if (curWorker == null)
        {
            StartCoroutine(ProgressingTask());
        }
    }

    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //Debug.Log($"�ǹ��۾�����({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}
