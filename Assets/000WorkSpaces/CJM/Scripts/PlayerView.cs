using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("방향 전환 시 아바타 회전 속도(도/초)")]
    [SerializeField] private float turnSpeed = 720f;

    // 애니메이터 추가

    private Vector3 lastForward = Vector3.forward;

    public void SetForwardToMoveDir(Vector3 worldMoveDir, float deltaTime)
    {
        // XZ 평면만 사용
        worldMoveDir.y = 0f;

        Vector3 aimDir = worldMoveDir.sqrMagnitude < 0.0001f ? lastForward : worldMoveDir.normalized;

        if (worldMoveDir.sqrMagnitude >= 0.0001f) lastForward = aimDir;

        Quaternion target = Quaternion.LookRotation(aimDir, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * deltaTime);
    }
}
