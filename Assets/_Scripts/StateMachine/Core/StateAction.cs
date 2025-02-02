using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateAction : IStateComponent
{
    internal StateActionSO _originSO;
    protected StateActionSO OriginSO => _originSO;
    public abstract void OnUpdate();
    
    public virtual void Awake(StateMachine stateMachine){}
    
    public virtual void OnStateEnter(){}
    public virtual void OnStateExit(){}

    public enum SpecificMoment
    {
        OnStateEnter, OnStateExit, OnUpdate
    }
}
