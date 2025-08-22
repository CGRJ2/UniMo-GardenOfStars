using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Move : PlayerStateBase
{
    private Rigidbody _rb;

    public PlayerState_Move(StateMachine<PlayerStates> stateMachine, PlayerRunTimeData data) : base(stateMachine, data)
    {
        HasPhysics = true;
        _rb = PlayerData.gameObject.GetComponent<Rigidbody>();
    }

    public override void Enter()
    {
        PlayerData.IsMove.Value = true;
    }

    public override void Update()
    {
        if(PlayerData.Direction == Vector3.zero)
        {
            if (PlayerData.IsWork.Value)
            {
                StateMachine.ChangeState(PlayerStates.Work);
            }
            else
            {
                StateMachine.ChangeState(PlayerStates.Idle);
            }
        }
    }

    public override void FixedUpdate()
    {
        _rb.velocity = PlayerData.Direction * Manager.player.Data.MoveSpeed;
    }

    public override void Exit()
    {
        PlayerData.IsMove.Value = false;
        _rb.velocity = Vector3.zero;
    }

}
