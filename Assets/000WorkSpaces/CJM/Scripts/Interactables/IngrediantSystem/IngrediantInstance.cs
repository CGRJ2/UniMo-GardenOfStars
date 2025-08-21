using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class IngrediantInstance : PooledObject
{
    [field: SerializeField] public IngrediantSO Data { get; private set; }
    public int count;
    Interactable_Ingrediant interactable;
    [SerializeField] float absorbAcceleration = 3f;

    [SerializeField] Vector3 stackOffset;


    //[SerializeField] float destroyTime = 120f;
    //float proceededtime;

    public GameObject owner; // CharacterBase �ϼ� �� ���� ����

    private void Awake()
    {
        interactable = GetComponent<Interactable_Ingrediant>();
        interactable.SetInstance(this);
    }

    void Update()
    {
        /*if (destroyTime < -90) return; // destroyTime�� -100���� �����Ͽ�, �����ֱ� ��Ȱ��ȭ ����

        proceededtime += Time.deltaTime;
        if (destroyTime < proceededtime) Despawn();

        // �׽�Ʈ �뵵
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (owner != null) Despawn();
        }*/
    }

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
        interactable.isInteracted = false;
    }

    public void SetIngrediantSO(IngrediantSO ingrediantSO)
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
