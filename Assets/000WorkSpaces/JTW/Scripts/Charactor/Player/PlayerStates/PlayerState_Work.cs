using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Work : PlayerStateBase
{
    public PlayerState_Work(StateMachine<PlayerStates> stateMachine, PlayerRunTimeData data) : base(stateMachine, data)
    {
    }

    public override void Enter()
    {
    }

    public override void Update()
    {
        if(PlayerData.Direction != Vector3.zero)
        {
            StateMachine.ChangeState(PlayerStates.Move);
        }
        else if (PlayerData.IsWork.Value)
        {
            StateMachine.ChangeState(PlayerStates.Idle);
        }
    }

    public override void Exit()
    {
    }
}
