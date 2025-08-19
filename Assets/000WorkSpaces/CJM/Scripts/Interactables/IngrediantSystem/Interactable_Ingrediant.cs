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
        if (targetAttachTransform == null) yield break;

        float currentSpeed = 0f;
        Quaternion startRot = transform.rotation;

        Vector3 firstTargetPos = targetAttachTransform.position + stackOffset * stackOrder;
        float startDist = Vector3.Distance(transform.position, firstTargetPos);
        if (startDist < 0.0001f) startDist = 0.0001f;

        while (!isAttached)
        {
            if (targetAttachTransform == null) yield break;

            Vector3 targetPos = targetAttachTransform.position + stackOffset * stackOrder;
            Quaternion targetRot = targetAttachTransform.rotation;

            // �̵�
            currentSpeed += absorbAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            // ȸ�� (����������� t �� 1, 0.3 �Ÿ����� �̹� ȸ�� �Ϸ�)
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist > startDist) startDist = dist; // Ÿ���� �־����� ���� ����
            float t = Mathf.InverseLerp(startDist, 0.3f, dist); 
            t = Mathf.Clamp01(t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            // ���� ����
            if (dist < 0.01f)
            {
                transform.position = targetPos;
                transform.rotation = targetRot; // �� �������� ��Ȯ�� �����ֱ�
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

        Debug.Log($"��� �ν��Ͻ�({ingrediantInstance?.ingrediantSO?.Name}): ����� ��ȣ�ۿ� ����");

        // �÷��̾� ��ġ�� ����
        // �ӽ�----------------------------------
        transform.SetParent(pc.prodsAttachPoint); 
        ingrediantInstance.owner = pc.gameObject;
        StartCoroutine(AttachToTarget(pc.prodsAttachPoint, pc.ingrediantStack.Count));
        pc.ingrediantStack.Push(ingrediantInstance.ingrediantSO);
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
