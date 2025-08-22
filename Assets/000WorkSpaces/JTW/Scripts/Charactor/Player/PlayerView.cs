using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private GameObject _avatar;

    [Header("방향 전환 시 아바타 회전 속도(도/초)")]
    [SerializeField] private float _turnSpeed = 720f;

    private PlayerRunTimeData _data;

    // 애니메이터 추가

    private Vector3 _lastForward = Vector3.forward;

    private void Awake()
    {
        _data = GetComponent<PlayerRunTimeData>();
    }

    private void Update()
    {
        SetForwardToMoveDir(_data.Direction, Time.deltaTime);
    }

    public void SetForwardToMoveDir(Vector3 worldMoveDir, float deltaTime)
    {
        // XZ 평면만 사용
        worldMoveDir.y = 0f;

        Vector3 aimDir = worldMoveDir.sqrMagnitude < 0.0001f ? _lastForward : worldMoveDir.normalized;

        if (worldMoveDir.sqrMagnitude >= 0.0001f) _lastForward = aimDir;

        Quaternion target = Quaternion.LookRotation(aimDir, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * deltaTime);
    }
}
