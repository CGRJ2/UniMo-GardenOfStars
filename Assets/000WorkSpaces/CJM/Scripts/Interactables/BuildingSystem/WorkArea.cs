using System.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UI;

public class WorkArea : InteractableBase
{
    public bool isWorkable { get { return curWorker == null & ownerInstance.ingrediantStack.Count > 0; } }

    [HideInInspector] public ManufactureBuilding ownerInstance;

    CharaterRuntimeData curWorker; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
    [SerializeField] Slider progressBar;

    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        Manager.buildings.workStatinLists.workAreas.Add(this);
    }

    IEnumerator ProgressingTask()
    {
        curWorker = characterRD; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
        curWorker.IsWork.Value = true;

        // ���� ���±��� ����ߴٰ� �۾� ����
        yield return new WaitUntil(() => !curWorker.IsMove.Value);

        while (curWorker == characterRD) // ���� �۾��ڰ� �ִ� ���� ��� ����
        {
            // �۾� ���� ��, ���� ������ ������ ��� ���
            if (curWorker.IsMove.Value)
                yield return new WaitUntil(() => !curWorker.IsMove.Value);
            
            // ��� ���� ��, ��ᰡ ä���� �� ���� ���
            if (ownerInstance.ingrediantStack.Count <= 0)
                yield return new WaitUntil(() => ownerInstance.ingrediantStack.Count > 0);

            // �۾� ���� ������ ������ ���
            if (curWorker != characterRD) break;

            // �׿��ִ� ��ᰡ �������� ����
            if (ownerInstance.ingrediantStack.Count > 0)
            {
                ownerInstance.progressedTime += Time.deltaTime;

                if (ownerInstance.runtimeData.productionTime < ownerInstance.progressedTime)
                {
                    CompleteTask(); // ����� ����
                    ownerInstance.progressedTime = 0; // ���൵ �ʱ�ȭ
                }
                
                // ���൵ ������ ������Ʈ
                progressBar.value = ownerInstance.progressedTime / ownerInstance.runtimeData.productionTime;

                yield return null;
            }
            else yield return null;
        }

        // �۾� ���� ��, ���� �۾��� ���� �ʱ�ȭ
        yield return null;
        curWorker.IsWork.Value = false;
        curWorker = null;
    }

    public void CompleteTask()
    {
        // ����� �ν��Ͻ� ����(Ȱ��ȭ) ----> �̰Ÿ� ���� ��ü�� ���� ���·� �Ұ��� ���� ������
        //GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // ȸ�������� ���� �÷��ֱ�
        ownerInstance.prodsArea.ProdsCount += 1;

        // ��� �Ҹ�
        ownerInstance.ingrediantStack.Pop().Despawn();
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);

        if (curWorker == null)
        {
            StartCoroutine(ProgressingTask());
        }
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.workAreas?.Remove(this);
    }
}
