using UnityEngine;

[CreateAssetMenu(fileName = "NewCondition", menuName = "State Machines/Conditions/New Condition")]
public class NewConditionSO : StateConditionSO
{
	protected override Condition CreateCondition() => new NewCondition();
}

public class NewCondition : Condition
{
	public override void Awake(StateMachine stateMachine)
	{
	}
		
	protected override bool Statement()
	{
		return true;
	}
	
	// public override void OnStateEnter()
	// {
	// }
	
	// public override void OnStateExit()
	// {
	// }
}
