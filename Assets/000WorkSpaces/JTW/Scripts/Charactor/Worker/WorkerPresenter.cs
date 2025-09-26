using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerPresenter : MonoBehaviour
{
    [Header("AnimationController")]
    [SerializeField] private RuntimeAnimatorController _characterController;
    [SerializeField] private RuntimeAnimatorController _equipController;

    [Header("Particle")]
    [SerializeField] private ParticleSystem _moveParticle;
    [SerializeField] private ParticleSystem _workParticle;
    [SerializeField] private ParticleSystem _stunParticle;
    [SerializeField] private ParticleSystem _stunBreakParticle;

    private Animator _characterAnimator;
    private Animator _equipAnimator;

    private WorkerRuntimeData _data;

    private GameObject _avatar;

    public bool IsInit;

    void Start()
    {
        _data = GetComponent<WorkerRuntimeData>();

        GameObject avatarPrefab = Manager.data.Character.Values[_data.Id].Avatar;

        _avatar = Instantiate(avatarPrefab, _data.transform);

        if (_avatar.transform.Find("Equip") == null) return;

        _characterAnimator = _avatar.transform.Find("CharacterRoot/Character").GetComponent<Animator>();
        _characterAnimator.runtimeAnimatorController = _characterController;
        _characterAnimator.enabled = true;

        _equipAnimator = _avatar.transform.Find("Equip").GetComponent<Animator>();
        _equipAnimator.runtimeAnimatorController = _equipController;
        _equipAnimator.enabled = true;

        _data.IsMove.Subscribe(OnMoveChanged);
        _data.IsWork.Subscribe(OnWorkChanged);
        _data.IsStun.Subscribe(OnStunChanged);
        _data.IsAwake.Subscribe(OnAwakeChanged);

        IsInit = true;
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

        _moveParticle.gameObject.SetActive(value);
    }

    private void OnWorkChanged(bool value)
    {
        _characterAnimator.SetBool("IsWork", value);
        _equipAnimator.SetBool("IsWork", value);

        _workParticle.gameObject.SetActive(value);
    }

    private void OnStunChanged(bool value)
    {
        _characterAnimator.SetBool("IsStun", value);
        _equipAnimator.SetBool("IsStun", value);

        _stunParticle.gameObject.SetActive(value);
    }

    private void OnAwakeChanged(bool value)
    {
        _characterAnimator.SetBool("IsAwake", value);
        _equipAnimator.SetBool("IsAwake", value);

        _stunBreakParticle.gameObject.SetActive(value);
    }
}
