using System.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UI;

public class WorkArea : InteractableBase, IWorkStation
{
    public bool isWorkable { get { return ownerInstance.ingrediantStack.Count > 0; } }
    public bool isReserved;
    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() { return isReserved; }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }

    [HideInInspector] public ManufactureBuilding ownerInstance;

    public CharaterRuntimeData curWorker; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
    [SerializeField] Slider progressBar;

    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        Manager.buildings.workStatinLists.workAreas.Add(this);
        progressBar.gameObject.SetActive(false);
    }

    IEnumerator ProgressingTask()
    {
        isReserved = false;
        curWorker = characterRD; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
        curWorker.IsWork.Value = true;

        // ���� ���±��� ����ߴٰ� �۾� ����
        yield return new WaitUntil(() => !curWorker.IsMove.Value);

        // �۾� ���� ��, ���൵ ǥ��
        progressBar.gameObject.SetActive(true);

        while (curWorker == characterRD) // ���� �۾��ڰ� �ִ� ���� ��� ����
        {
            // �۾� ���� ��, ���� ������ ������ ��� ���
            if (curWorker.IsMove.Value)
            {
                progressBar.gameObject.SetActive(false);
                yield return new WaitUntil(() => !curWorker.IsMove.Value);
                progressBar.gameObject.SetActive(true);
            }

            // ��� ���� ��, ��ᰡ ä���� �� ���� ���
            if (ownerInstance.ingrediantStack.Count <= 0)
            {
                progressBar.gameObject.SetActive(false);
                yield return new WaitUntil(() => ownerInstance.ingrediantStack.Count > 0);
                progressBar.gameObject.SetActive(true);
            }

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

        // ���൵ ǥ�� ��Ȱ��ȭ
        progressBar.gameObject.SetActive(false);
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

        if (characterRD is WorkerRuntimeData worker)
        {
            if (worker.CurWorkstation.Value != this as IWorkStation) return;
        }

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
