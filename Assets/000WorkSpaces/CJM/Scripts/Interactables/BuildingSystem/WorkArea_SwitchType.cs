using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorkArea_SwitchType : InteractableBase, IWorkStation
{
    public bool isWorkable { get { return (curWorker == null & ownerInstance.ingrediantStack.Count > 0 && !isOperating); } }
    public bool isReserved;
    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() { return isReserved; }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }

    [HideInInspector] public ManufactureBuilding ownerInstance;

    CharaterRuntimeData curWorker; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
    [SerializeField] Slider progressBar;
    [SerializeField] Slider temp_PrepareBar;
    [Header("�۾��� Ȱ��ȭ��Ű�� �ð�")]
    [SerializeField] float prepareTime = 3f;
    float prepareProgressedTime = 0f;   // �غ� �ܰ� ���൵


    [SerializeField] bool isOperating;

    

    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        Manager.buildings.workStatinLists.workAreas_SwitchType.Add(this);
        temp_PrepareBar.gameObject.SetActive(false);
        progressBar.gameObject.SetActive(false);
    }

    // �۾� �غ� �ܰ�
    IEnumerator PrepareTask()
    {
        curWorker = characterRD; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
        curWorker.IsWork.Value = true;

        // ���� ���� && �����۾� �Ϸ� ���±��� ����ߴٰ� �۾� ����
        yield return new WaitUntil(() => !curWorker.IsMove.Value && !isOperating);

        while (curWorker == characterRD) // ���� �۾��ڰ� �ִ� ���� ��� ����
        {
            yield return new WaitUntil(() => !isOperating);
            
            // �غ� ���� ��, ���൵ ǥ��
            temp_PrepareBar.gameObject.SetActive(true);

            // �۾� ���� ��, ���� ������ ������ ��� ���
            if (curWorker.IsMove.Value)
            {
                prepareProgressedTime = 0; // ���൵ �ʱ�ȭ
                temp_PrepareBar.gameObject.SetActive(false);
                yield return new WaitUntil(() => !curWorker.IsMove.Value);
                temp_PrepareBar.gameObject.SetActive(true);
            }

            // ��� ���� ��, ��ᰡ ä���� �� ���� ���
            if (ownerInstance.ingrediantStack.Count <= 0)
                yield return new WaitUntil(() => ownerInstance.ingrediantStack.Count > 0);

            // �۾� ���� ������ ������ ���
            if (curWorker != characterRD) break;

            // �׿��ִ� ��ᰡ �������� ����
            if (ownerInstance.ingrediantStack.Count > 0)
            {
                prepareProgressedTime += Time.deltaTime;

                if (prepareTime < prepareProgressedTime)
                {
                    //CompleteTask(); // ����� ����
                    StartCoroutine(ProgressingTask());
                    isOperating = true;
                    prepareProgressedTime = 0; // ���൵ �ʱ�ȭ
                    temp_PrepareBar.gameObject.SetActive(false); // ���൵ ǥ�� ��Ȱ��ȭ
                    continue;
                }

                // ���൵ ������ ������Ʈ
                temp_PrepareBar.value = prepareProgressedTime / prepareTime;

                yield return null;
            }
            else yield return null;
        }

        // �۾� ���� ��, ���� �۾��� ���� �ʱ�ȭ
        yield return null;
        curWorker.IsWork.Value = false;
        curWorker = null;

        // �غ� �۾� ���� ��, ���൵ ǥ�� ��Ȱ��ȭ
        temp_PrepareBar.gameObject.SetActive(false);
    }
    IEnumerator ProgressingTask()
    {
        // �۾��� ������ �� ���� ���
        yield return new WaitUntil(() => isOperating);

        // �۾� ���� ��, ���൵ ǥ��
        progressBar.gameObject.SetActive(true);

        while (isOperating)
        {
            ownerInstance.progressedTime += Time.deltaTime;

            if (ownerInstance.runtimeData.productionTime < ownerInstance.progressedTime)
            {
                CompleteTask(); // ����� ����
                ownerInstance.progressedTime = 0; // ���൵ �ʱ�ȭ
                isOperating = false; // �۾� ó�� ����
            }

            // ���൵ ������ ������Ʈ
            progressBar.value = ownerInstance.progressedTime / ownerInstance.runtimeData.productionTime;
            yield return null;
        }

        // �۾� �Ϸ� ��, ���൵ ǥ�� ��Ȱ��ȭ
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

        if (curWorker == null)
        {
            StartCoroutine(PrepareTask());
        }
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.workAreas_SwitchType?.Remove(this);
    }
}
