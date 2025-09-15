using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngrediantInstance : PooledObject
{
    [field: SerializeField] public IngrediantData Data { get; private set; }
    [SerializeField] float absorbAcceleration = 3f;
    [SerializeField] Vector3 stackOffset;

    [Header("출렁 효과 설정값")]
    CharaterRuntimeData ownerCharacterRD;
    int myOrder;
    Transform wobbleParent;
    bool isOnHand;
    [SerializeField] AnimationCurve baseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float minStretch = 0.5f; // myOrder=0 → 빠르게
    [SerializeField] float maxStretch = 10.0f; // myOrder=9 → 느리게

    protected override void OnPooledEnable()
    {
        base.OnPooledEnable();

        // 생성 효과음
    }

    protected override void OnPooledDisable()
    {
        base.OnPooledDisable();

        ownerCharacterRD = null;
        isOnHand = false;
        // 소멸 효과음
    }

    public void Despawn()
    {
        ParentPool.ReturnPooledObj(gameObject); // 이 방법으로 디스폰
    }

    public void SetIngrediantSO(IngrediantData ingrediantSO)
    {
        this.Data = ingrediantSO;
    }

    public void AttachToTarget(Transform parent, int stackCount = 0, CharaterRuntimeData characterRD = null)
    {
        if (characterRD == null)
        {
            transform.SetParent(parent);
            isOnHand = false;
            ownerCharacterRD = null;
        }
        else
        {
            ownerCharacterRD = characterRD;
        }
        StartCoroutine(AttachToTargetRoutine(parent, stackCount));
    }

    public void MoveToTargetAndShrink(Transform parent, Action completed = null)
    {
        //transform.SetParent(parent);
        ownerCharacterRD = null;
        isOnHand = false;
        StartCoroutine(MoveToTargetPosAndShrinkRoutine(parent, completed));
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

                // 출렁 모션을 위한 필드
                if (ownerCharacterRD != null)
                    SetupWobbleParent(targetAttachTransform, stackOrder);

                break;
            }
            yield return null;
        }
    }

    IEnumerator MoveToTargetPosAndShrinkRoutine(Transform targetAttachTransform, Action completed)
    {
        if (targetAttachTransform == null) yield break;

        float currentSpeed = 0f;
        Quaternion startRot = transform.rotation;

        Vector3 firstTargetPos = targetAttachTransform.position;
        float startDist = Vector3.Distance(transform.position, firstTargetPos);
        if (startDist < 0.0001f) startDist = 0.0001f;

        bool isAttached = false;
        while (!isAttached)
        {
            if (targetAttachTransform == null) yield break;

            Vector3 targetPos = targetAttachTransform.position;
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
                while (transform.localScale.x > 0f)
                {
                    transform.localScale -= Time.deltaTime * new Vector3(1, 1, 1) / 0.2f/*(축소 시간)*/;
                    yield return null;
                }
                Despawn();
                transform.localScale = new Vector3(1, 1, 1);
                isAttached = true;

                completed?.Invoke();
                break;
            }

            yield return null;
        }
    }

    void SetupWobbleParent(Transform parent, int stackOrder)
    {
        myOrder = stackOrder;

        if (stackOrder == 0)
            wobbleParent = parent; // 맨 아래는 AttachPoint
        else
        {
            // 바로 아래 재료를 wobbleParent로
            // push순서의 스택을 리스트로
            List<IngrediantInstance> list = ownerCharacterRD?.IngrediantStack.ToList();
            list.Reverse();
            wobbleParent = list[myOrder - 1].gameObject.transform;
        }

        isOnHand = true;
    }



    public void UpdateTransform()
    {
        if (!isOnHand) return;

        Vector3 offset = (myOrder > 0) ? stackOffset : Vector3.zero;
        Vector3 targetPos = wobbleParent.position + offset;
        Quaternion targetRot = wobbleParent.rotation;

        float order01 = Mathf.Clamp01((float)myOrder / 11f); // 최대 스택 가능 개수 나눠주기
        float t = Time.fixedDeltaTime * moveSpeed;
        // 커브 적용
        float eased = baseCurve.Evaluate(t);
        eased = Mathf.Lerp(1f - order01, 1f, eased);

        // 보간
        transform.position = Vector3.Lerp(transform.position, targetPos, eased);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, eased);
    }
}

