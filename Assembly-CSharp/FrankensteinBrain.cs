using UnityEngine;

public class FrankensteinBrain : PetBrain
{
	public class MoveToPointState : BasicAIState
	{
		private float originalStopDistance;

		public MoveToPointState()
			: base(AIState.MoveToPoint)
		{
		}

		public override void StateEnter()
		{
			base.StateEnter();
			BaseNavigator navigator = GetEntity().Brain.Navigator;
			originalStopDistance = navigator.StoppingDistance;
			navigator.StoppingDistance = 0.5f;
		}

		public override void StateLeave()
		{
			base.StateLeave();
			GetEntity().Brain.Navigator.StoppingDistance = originalStopDistance;
			Stop();
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			Vector3 pos = brain.Events.Memory.Position.Get(6);
			if (!brain.Navigator.SetDestination(pos, BaseNavigator.NavigationSpeed.Normal, MoveTowardsRate))
			{
				return StateStatus.Error;
			}
			if (!brain.Navigator.Moving)
			{
				brain.LoadDefaultAIDesign();
			}
			if (!brain.Navigator.Moving)
			{
				return StateStatus.Finished;
			}
			return StateStatus.Running;
		}
	}

	public class MoveTorwardsState : BasicAIState
	{
		public MoveTorwardsState()
			: base(AIState.MoveTowards)
		{
		}

		public override void StateLeave()
		{
			base.StateLeave();
			Stop();
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity == null)
			{
				Stop();
				return StateStatus.Error;
			}
			if (!brain.Navigator.SetDestination(baseEntity.transform.position, BaseNavigator.NavigationSpeed.Normal, MoveTowardsRate))
			{
				return StateStatus.Error;
			}
			if (!brain.Navigator.Moving)
			{
				return StateStatus.Finished;
			}
			return StateStatus.Running;
		}
	}

	[ServerVar]
	public static float MoveTowardsRate = 1f;

	public override void AddStates()
	{
		base.AddStates();
		AddState(new BaseIdleState());
		AddState(new MoveTorwardsState());
		AddState(new BaseChaseState());
		AddState(new BaseAttackState());
		AddState(new MoveToPointState());
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
