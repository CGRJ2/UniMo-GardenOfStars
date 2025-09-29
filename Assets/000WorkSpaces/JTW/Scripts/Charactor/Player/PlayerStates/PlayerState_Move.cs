using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerState_Move : PlayerStateBase
{
    private Rigidbody _rb;
    private NavMeshAgent _navAgent;

    public PlayerState_Move(StateMachine<PlayerStates> stateMachine, PlayerRunTimeData data) : base(stateMachine, data)
    {
        _rb = PlayerData.gameObject.GetComponent<Rigidbody>();
        _navAgent = PlayerData.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        Application.targetFrameRate = 50;
        QualitySettings.vSyncCount = 0;
        PlayerData.IsMove.Value = true;
    }

    public override void Update()
    {
        Vector3 move = PlayerData.Direction * Manager.player.Data.MoveSpeed * Time.deltaTime;
        _navAgent.Move(move);

        if (PlayerData.Direction == Vector3.zero || !Manager.player.IsControl)
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

    public override void Exit()
    {
        PlayerData.IsMove.Value = false;
        _rb.velocity = Vector3.zero;
    }

}
