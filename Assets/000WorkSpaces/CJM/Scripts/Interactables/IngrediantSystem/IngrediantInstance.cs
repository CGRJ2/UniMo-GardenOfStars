using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class IngrediantInstance : PooledObject
{
    [field: SerializeField] public IngrediantData Data { get; private set; }
    [SerializeField] float absorbAcceleration = 3f;
    [SerializeField] Vector3 stackOffset;

    public GameObject owner; // CharacterBase �ϼ� �� ���� ����

    protected override void OnPooledEnable()
    {
        base.OnPooledEnable();
        //proceededtime = 0;
    }

    protected override void OnPooledDisable()
    {
        base.OnPooledDisable();
    }

    public void Despawn()
    {
        owner = null;

        ParentPool.ReturnPooledObj(gameObject); // �� ������� ����
    }

    public void SetIngrediantSO(IngrediantData ingrediantSO)
    {
        this.Data = ingrediantSO;
    }

    public void AttachToTarget(Transform parent, int stackCount = 0)
    {
        transform.SetParent(parent);
        StartCoroutine(AttachToTargetRoutine(parent, stackCount));
    }

    IEnumerator AttachToTargetRoutine(Transform targetAttachTransform, int stackOrder = 0)
    {
        if (targetAttachTransform == null) yield break;

        float currentSpeed = 0f;
        Quaternion startRot = transform.rotation;

        Vector3 firstTargetPos = targetAttachTransform.position + stackOffset * stackOrder;
        float startDist = Vector3.Distance(transform.position, firstTargetPos);
        if (startDist < 0.0001f) startDist = 0.0001f;

        bool isAttached = false;
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


}
