using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Interactable_Work : InteractableBase
{
    // �۾������� �ΰ� �̻��� ������Ʈ�� ��ġ�� ���¿���, ó�� �۾����̴� ��ü�� ������ �� �����۵��ϴ��� �׽�Ʈ �ʿ�

    BuildingInstance_Manufacture instance;

    PlayerController curWorker; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
    [SerializeField] Slider progressBar;
    ObjectPool _Pool;

    public void Init(BuildingInstance_Manufacture instance)
    {
        this.instance = instance;
        string key = instance.originData.ProductID; // �ӽ÷� Id�� �̸����� ����
        Addressables.LoadAssetAsync<GameObject>(key).Completed += task =>
        {
            GameObject product = task.Result;

            _Pool = Manager.pool.GetPoolBundle(product).instancePool;
        };
        
    }

    IEnumerator ProgressingTask()
    {
        curWorker = pc; // �ӽ�. �ϲ۱��� ������ ������ ���� �ʿ�
        while (pc != null)
        {
            // �׿��ִ� ��ᰡ �������� ����
            if (instance.prodsStack.Count > 0)
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
        // �ƿ� ����������� �����ϴ� ������ �����͸� �־��ִ� �͵� ��������
        GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);

        // ���깰 ���� ����
        //_SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();

        // ��� �Ҹ�
        instance.prodsStack.Pop().Despawn();
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
