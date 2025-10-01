using UnityEngine;
using UnityEngine.AI;

public class WorkerState_Work : WorkerStateBase
{
    private NavMeshAgent _navAgent;
    private Quaternion _target;

    public WorkerState_Work(StateMachine<WorkerStates> stateMachine, WorkerRuntimeData data) : base(stateMachine, data)
    {
        _navAgent = WorkerData.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        if (WorkerData.CurWorkstation.Value is WorkArea_SwitchType)
        {
            Vector3 aimDir = (WorkerData.CurWorkstation.Value as WorkArea_SwitchType).ownerInstance.viewPoint.position - WorkerData.transform.position;
            aimDir = aimDir.normalized;

            _target = Quaternion.LookRotation(aimDir, Vector3.up);
        }
        else
        {
            _target = default;
        }

        _navAgent.avoidancePriority = 1;

        InteractableBase interact = (WorkerData.CurWorkstation.Value as InteractableBase);

        interact.Enter(WorkerData);
        if (interact.personalTaskOwner == null)
        {
            interact.Enter_PersonalTask(WorkerData);
        }
    }

    public override void Update()
    {
        if (_target != default)
        {
            WorkerData.transform.rotation = Quaternion.RotateTowards(WorkerData.transform.rotation, _target, 720f * Time.deltaTime);
        }


        if (!CanWork())
        {
            StateMachine.ChangeState(WorkerStates.Idle);
        }
    }

    public override void Exit()
    {
        InteractableBase interact = (WorkerData.CurWorkstation.Value as InteractableBase);

        interact.Exit(WorkerData);
        if (interact.personalTaskOwner == WorkerData)
        {
            interact.Exit_PersonalTask(WorkerData);
        }
        _navAgent.avoidancePriority = 30;
        // 시작할 때는 작업영역이 아닐 수도 있어서 true로 만들지는 않음.
        WorkerData.IsWork.Value = false;
    }

    private bool CanWork()
    {
        IWorkStation workstation = WorkerData.CurWorkstation.Value;

        if (!workstation.GetWorkableState()) return false;

        if (workstation is InsertArea)
        {
            if (WorkerData.IngrediantStack.Count == 0) return false;
        }
        else if (workstation is ProductGenerater || workstation is ProdsArea)
        {
            if (WorkerData.IngrediantStack.Count >= WorkerData.MaxCapacity) return false;
        }
        else if (workstation is WorkArea_SwitchType)
        {
            if ((workstation as WorkArea_SwitchType).curWorker != null
                && (workstation as WorkArea_SwitchType).curWorker != WorkerData)
            {
                return false;
            }
        }

        return true;
    }
}
