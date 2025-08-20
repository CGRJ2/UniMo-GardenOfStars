using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("���� ��ȯ �� �ƹ�Ÿ ȸ�� �ӵ�(��/��)")]
    [SerializeField] private float turnSpeed = 720f;

    // �ִϸ����� �߰�

    private Vector3 lastForward = Vector3.forward;

    public void SetForwardToMoveDir(Vector3 worldMoveDir, float deltaTime)
    {
        // XZ ��鸸 ���
        worldMoveDir.y = 0f;

        Vector3 aimDir = worldMoveDir.sqrMagnitude < 0.0001f ? lastForward : worldMoveDir.normalized;

        if (worldMoveDir.sqrMagnitude >= 0.0001f) lastForward = aimDir;

        Quaternion target = Quaternion.LookRotation(aimDir, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * deltaTime);
    }
}
