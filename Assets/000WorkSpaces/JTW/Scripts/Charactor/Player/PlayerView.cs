using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController _avatarController;
    [SerializeField] private RuntimeAnimatorController _equipController;

    private Animator _avatarAnimator;
    private Animator _equipAnimator;

    private float _turnSpeed = 720f;
    private PlayerRunTimeData _data;

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

    private void OnEnable()
    {
        _data.IsMove.Subscribe(OnMoveChanged);
        _data.IsWork.Subscribe(OnWorkChanged);
    }

    private void OnDisable()
    {
        _data.IsMove.Unsubscribe(OnMoveChanged);
        _data.IsWork.Unsubscribe(OnWorkChanged);
    }

    private void OnMoveChanged(bool value)
    {
        _avatarAnimator.SetBool("IsMove", value);
        _equipAnimator.SetBool("IsMove", value);
    }

    private void OnWorkChanged(bool value)
    {
        _avatarAnimator.SetBool("IsWork", value);
        _equipAnimator.SetBool("IsWork", value);
    }

    public void SetCharacterAnimator(Animator animator)
    {
        _avatarAnimator = animator;
        _avatarAnimator.runtimeAnimatorController = _avatarController;
    }

    public void SetEquipAnimator(Animator animator)
    {
        _equipAnimator = animator;
        _equipAnimator.runtimeAnimatorController = _equipController;
    }
}
