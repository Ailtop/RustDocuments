public class ScarecrowBrain : BaseAIBrain
{
	public override void AddStates()
	{
		base.AddStates();
		AddState(new BaseIdleState());
		AddState(new BaseChaseState());
		AddState(new BaseAttackState());
	}

	public override void InitializeAI()
	{
		base.InitializeAI();
		base.ThinkMode = AIThinkMode.Interval;
		thinkRate = 0.25f;
		base.PathFinder = new HumanPathFinder();
		((HumanPathFinder)base.PathFinder).Init(GetBaseEntity());
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}
}
