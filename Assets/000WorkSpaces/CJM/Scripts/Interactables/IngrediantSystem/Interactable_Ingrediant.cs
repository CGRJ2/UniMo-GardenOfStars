using System.Collections;
using UnityEngine;

// Ǯ ������Ʈ�� �ν��Ͻ����� ��ӹޱ� ������ ��ȣ�ۿ� ������ ������ MonoBehavior Ŭ������ ������
public class Interactable_Ingrediant : InteractableBase
{
    IngrediantInstance ingrediantInstance;
    [SerializeField] float absorbAcceleration = 3f;
    [SerializeField] Vector3 stackOffset;

    bool isInteracted = false;
    bool isAttached = false;

    

    private void Awake()
    {
        ingrediantInstance = GetComponent<IngrediantInstance>();
    }

    IEnumerator AttachToTarget(Transform targetAttachTransform, int stackOrder)
    {
        if (targetAttachTransform == null) yield break; // Ÿ���� ������� ���� ���� ������ġ

        float currentSpeed = 0;
        /*Quaternion startRot = transform.rotation;
        float startDist = (transform.position - (targetAttachTransform.position + stackOffset * stackOrder)).magnitude;
        if (startDist < 0.0001f) startDist = 0.0001f; // 0 ����*/

        while (!isAttached)
        {
            if (targetAttachTransform == null) yield break; // Ÿ���� ������� ���� ���� ������ġ

            Vector3 targetPos = targetAttachTransform.position + stackOffset * stackOrder;
            Quaternion targetRot = targetAttachTransform.rotation;

            // ������ �̵�
            currentSpeed += absorbAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            // ȸ��
            /*float dist = Vector3.Distance(transform.position, targetPos);
            if (dist > startDist) startDist = dist; // Ÿ���� �־����� ���� ����(Ÿ�� �̵� ����)
            float t = 1f - Mathf.InverseLerp(startDist, 0.3f, dist); // 0��1
            t = Mathf.Clamp01(t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);*/

            Debug.Log("Ÿ�� ��ġ�� �̵� ��");
            if ((targetPos - transform.position).magnitude < 0.1f)
            {
                transform.position = targetPos;
                isAttached = true;
                break;
            }

            yield return null;
        }
    }

    public override void ImediateInteract()
    {
        base.ImediateInteract();

        // 1ȸ ��ȣ�ۿ� �� ����
        if (isInteracted) return;
        isInteracted = true;

        Debug.Log($"��� �ν��Ͻ�({ingrediantInstance.ingrediantSO.Name}): ����� ��ȣ�ۿ� ����");

        // �÷��̾� ��ġ�� ����
        // �ӽ�----------------------------------
        transform.SetParent(pc.transform); 
        ingrediantInstance.owner = pc.gameObject;
        StartCoroutine(AttachToTarget(pc.prodsAttachPoint, pc.TempStackorder));
        pc.TempStackorder += 1;
        //---------------------------------------
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        
        // 1ȸ ��ȣ�ۿ� �� ����
        if (isInteracted) return;
        isInteracted = true;

        //Debug.Log($"��� �ν��Ͻ�({ingrediantInstance.ingrediantSO.Name}): �˾��� ��ȣ�ۿ� Ȱ��ȭ");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();

        // 1ȸ ��ȣ�ۿ� �� ����
        if (isInteracted) return;
        isInteracted = true;

        //Debug.Log($"��� �ν��Ͻ�({ingrediantInstance.ingrediantSO.Name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}
