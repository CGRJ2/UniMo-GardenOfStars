using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStates
{
    Idle, Move, Work, Size
}

public abstract class PlayerStateBase : BaseState<PlayerStates>
{
    protected PlayerRunTimeData PlayerData { get; private set; }

    public PlayerStateBase(StateMachine<PlayerStates> stateMachine, PlayerRunTimeData data) : base(stateMachine)
    {
        PlayerData = data;
    }
}
