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

    public GameObject owner; // CharacterBase 완성 후 변수 수정

    private void Awake()
    {
        interactable = GetComponent<Interactable_Ingrediant>();
        interactable.SetInstance(this);
    }

    void Update()
    {
        /*if (destroyTime < -90) return; // destroyTime을 -100으로 설정하여, 생존주기 비활성화 가능

        proceededtime += Time.deltaTime;
        if (destroyTime < proceededtime) Despawn();

        // 테스트 용도
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

        ParentPool.ReturnPooledObj(gameObject); // 이 방법으로 디스폰
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

            // 이동
            currentSpeed += absorbAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            // 회전 (가까워질수록 t → 1, 0.3 거리에서 이미 회전 완료)
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist > startDist) startDist = dist; // 타겟이 멀어지면 기준 갱신
            float t = Mathf.InverseLerp(startDist, 0.3f, dist);
            t = Mathf.Clamp01(t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            // 도착 스냅
            if (dist < 0.01f)
            {
                transform.position = targetPos;
                transform.rotation = targetRot; // ← 마지막에 정확히 맞춰주기
                isAttached = true;
                break;
            }

            yield return null;
        }
    }


}
