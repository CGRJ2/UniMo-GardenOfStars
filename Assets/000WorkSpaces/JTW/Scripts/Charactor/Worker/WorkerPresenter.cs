using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerPresenter : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController _characterController;
    [SerializeField] private RuntimeAnimatorController _equipController;

    private Animator _characterAnimator;
    private Animator _equipAnimator;

    private WorkerRuntimeData _data;

    void Start()
    {
        _data = GetComponent<WorkerRuntimeData>();

        GameObject avatarPrefab = Manager.data.Character.Values[$"{_data.Id}_{Manager.firebase.UserData.CurStage.Value}"].Avatar;

        GameObject avatar = Instantiate(avatarPrefab, _data.transform);

        if (avatar.transform.Find("Equip") == null) return;

        _characterAnimator = avatar.transform.Find("Character").GetComponent<Animator>();
        _characterAnimator.runtimeAnimatorController = _characterController;
        _characterAnimator.enabled = true;

        _equipAnimator = avatar.transform.Find("Equip").GetComponent<Animator>();
        _equipAnimator.runtimeAnimatorController = _equipController;
        _equipAnimator.enabled = true;
    }

    private void OnEnable()
    {
        _data.IsMove.Subscribe(OnMoveChanged);
        _data.IsWork.Subscribe(OnWorkChanged);
        _data.IsStun.Subscribe(OnStunChanged);
        _data.IsAwake.Subscribe(OnAwakeChanged);
    }

    private void OnDisable()
    {
        _data.IsMove.Unsubscribe(OnMoveChanged);
        _data.IsWork.Unsubscribe(OnWorkChanged);
        _data.IsStun.Unsubscribe(OnStunChanged);
        _data.IsAwake.Unsubscribe(OnAwakeChanged);
    }

    private void OnMoveChanged(bool value)
    {
        _characterAnimator.SetBool("IsMove", value);
        _equipAnimator.SetBool("IsMove", value);
    }

    private void OnWorkChanged(bool value)
    {
        _characterAnimator.SetBool("IsWork", value);
        _equipAnimator.SetBool("IsWork", value);
    }

    private void OnStunChanged(bool value)
    {
        if (!value) return;

        _characterAnimator.SetTrigger("StunTrigger");
        _equipAnimator.SetTrigger("StunTrigger");
    }

    private void OnAwakeChanged(bool value)
    {
        _characterAnimator.SetBool("IsAwake", value);
        _equipAnimator.SetBool("IsAwake", value);
    }
}
