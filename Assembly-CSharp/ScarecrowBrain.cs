public class ScarecrowBrain : BaseAIBrain<ScarecrowNPC>
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
		((HumanPathFinder)base.PathFinder).Init(GetEntity());
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}
}
