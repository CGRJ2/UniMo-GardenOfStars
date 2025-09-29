using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Work : PlayerStateBase
{
    private Quaternion _target;
    private PlayerView _view;

    public PlayerState_Work(StateMachine<PlayerStates> stateMachine, PlayerRunTimeData data) : base(stateMachine, data)
    {
        _view = PlayerData.GetComponent<PlayerView>();
    }

    public override void Enter()
    {
        if (PlayerData.CurWorkStation is WorkArea_SwitchType workArea)
        {
            Vector3 aimDir = workArea.ownerInstance.viewPoint.position - _view.Avatar.position;
            aimDir = aimDir.normalized;

            _target = Quaternion.LookRotation(aimDir, Vector3.up);
        }
        else
        {
            _target = default;
        }
    }

    public override void Update()
    {
        if (_target != default)
        {
            _view.Avatar.rotation = Quaternion.RotateTowards(_view.Avatar.rotation, _target, 720f * Time.deltaTime);
        }

        if (PlayerData.Direction != Vector3.zero && Manager.player.IsControl)
        {
            StateMachine.ChangeState(PlayerStates.Move);
        }
        else if (!PlayerData.IsWork.Value)
        {
            StateMachine.ChangeState(PlayerStates.Idle);
        }
    }

    public override void Exit()
    {
        _target = default;
    }
}
