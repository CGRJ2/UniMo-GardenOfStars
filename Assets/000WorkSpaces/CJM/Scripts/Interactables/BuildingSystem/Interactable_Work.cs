using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Interactable_Work : InteractableBase
{
    // �۾������� �ΰ� �̻��� ������Ʈ�� ��ġ�� ���¿���, ó�� �۾����̴� ��ü�� ������ �� �����۵��ϴ��� �׽�Ʈ �ʿ�

    BuildingInstance_Work buildingInstance;
    PlayerController curWorker; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
    [SerializeField] Slider progressBar;
    ObjectPool _Pool;

    public void Init(BuildingInstance_Work buildingInstance)
    {
        this.buildingInstance = buildingInstance;
        _Pool = Manager.pool.GetPoolBundle(buildingInstance.resultProdData.InstancePrefab).instancePool;
    }

    IEnumerator ProgressingTask()
    {
        curWorker = pc; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
        while (pc != null)
        {
            // �׿��ִ� ��ᰡ �������� ����
            if (buildingInstance.prodsStack.Count > 0)
            {
                buildingInstance.progressedTime += Time.deltaTime;

                if (buildingInstance.taskTime < buildingInstance.progressedTime)
                {
                    CompleteTask(); // ����� ����
                    buildingInstance.progressedTime = 0; // ���൵ �ʱ�ȭ
                }
                
                // ���൵ ������ ������Ʈ
                progressBar.value = buildingInstance.progressedTime / buildingInstance.taskTime;

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
        // �ƿ� ����������� �����ϴ� ������ �����͸� �־��ִ� �͵� ��������
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // ���깰 ���� ����
        //_SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();

        // ��� �Ҹ�
        buildingInstance.prodsStack.Pop().Despawn();
    }


    public override void EnterInteract_Player()
    {
        base.EnterInteract_Player();
        //Debug.Log($"�ǹ��۾�����({buildingInstance.name}): ����� ��ȣ�ۿ� ����");

        StartCoroutine(ProgressingTask());
    }

    public override void DeactiveInteract_Player()
    {
        base.DeactiveInteract_Player();
        //Debug.Log($"�ǹ��۾�����({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}
