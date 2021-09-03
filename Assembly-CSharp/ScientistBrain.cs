using UnityEngine;

public class ScientistBrain : BaseAIBrain<HumanNPCNew>
{
	public class ChaseState : BasicAIState
	{
		private StateStatus status = StateStatus.Error;

		private float nextPositionUpdateTime;

		public ChaseState()
			: base(AIState.Chase)
		{
			base.AgrresiveState = true;
		}

		public override void StateLeave()
		{
			base.StateLeave();
			Stop();
		}

		public override void StateEnter()
		{
			base.StateEnter();
			status = StateStatus.Error;
			if (brain.PathFinder != null)
			{
				status = StateStatus.Running;
				nextPositionUpdateTime = 0f;
			}
		}

		private void Stop()
		{
			brain.Navigator.Stop();
			brain.Navigator.ClearFacingDirectionOverride();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (status == StateStatus.Error)
			{
				return status;
			}
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity == null)
			{
				return StateStatus.Error;
			}
			HumanNPCNew entity = GetEntity();
			float num = Vector3.Distance(baseEntity.transform.position, entity.transform.position);
			if (brain.Senses.Memory.IsLOS(baseEntity) || num <= 10f)
			{
				brain.Navigator.SetFacingDirectionEntity(baseEntity);
			}
			else
			{
				brain.Navigator.ClearFacingDirectionOverride();
			}
			if (num <= 10f)
			{
				brain.Navigator.SetCurrentSpeed(BaseNavigator.NavigationSpeed.Normal);
			}
			else
			{
				brain.Navigator.SetCurrentSpeed(BaseNavigator.NavigationSpeed.Fast);
			}
			if (Time.time > nextPositionUpdateTime)
			{
				nextPositionUpdateTime = Time.time + Random.Range(0.5f, 1f);
				Vector3 pos = GetEntity().transform.position;
				AIInformationZone informationZone = entity.GetInformationZone(baseEntity.transform.position);
				bool flag = false;
				if (informationZone != null)
				{
					AIMovePoint bestMovePointNear = informationZone.GetBestMovePointNear(baseEntity.transform.position, entity.transform.position, 0f, brain.Navigator.BestMovementPointMaxDistance, true, entity, true);
					if ((bool)bestMovePointNear)
					{
						bestMovePointNear.SetUsedBy(entity, 5f);
						pos = brain.PathFinder.GetRandomPositionAround(bestMovePointNear.transform.position, 0f, bestMovePointNear.radius - 0.3f);
						flag = true;
					}
				}
				if (!flag)
				{
					return StateStatus.Error;
				}
				if (num < 10f)
				{
					brain.Navigator.SetDestination(pos, BaseNavigator.NavigationSpeed.Normal);
				}
				else
				{
					brain.Navigator.SetDestination(pos, BaseNavigator.NavigationSpeed.Fast);
				}
			}
			if (brain.Navigator.Moving)
			{
				return StateStatus.Running;
			}
			return StateStatus.Finished;
		}
	}

	public class CombatState : BasicAIState
	{
		private float nextActionTime;

		private Vector3 combatStartPosition;

		public CombatState()
			: base(AIState.Combat)
		{
			base.AgrresiveState = true;
		}

		public override void StateEnter()
		{
			base.StateEnter();
			combatStartPosition = GetEntity().transform.position;
			FaceTarget();
		}

		public override void StateLeave()
		{
			base.StateLeave();
			GetEntity().SetDucked(false);
			brain.Navigator.ClearFacingDirectionOverride();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			FaceTarget();
			if (Time.time > nextActionTime)
			{
				HumanNPCNew entity = GetEntity();
				if (Random.Range(0, 3) == 1)
				{
					nextActionTime = Time.time + Random.Range(2f, 3f);
					entity.SetDucked(true);
					brain.Navigator.Stop();
				}
				else
				{
					nextActionTime = Time.time + Random.Range(3f, 4f);
					entity.SetDucked(false);
					brain.Navigator.SetDestination(brain.PathFinder.GetRandomPositionAround(combatStartPosition, 1f), BaseNavigator.NavigationSpeed.Normal);
				}
			}
			return StateStatus.Running;
		}

		private void FaceTarget()
		{
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity == null)
			{
				brain.Navigator.ClearFacingDirectionOverride();
			}
			else
			{
				brain.Navigator.SetFacingDirectionEntity(baseEntity);
			}
		}
	}

	public class CombatStationaryState : BasicAIState
	{
		public CombatStationaryState()
			: base(AIState.CombatStationary)
		{
			base.AgrresiveState = true;
		}

		public override void StateLeave()
		{
			base.StateLeave();
			brain.Navigator.ClearFacingDirectionOverride();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity != null)
			{
				brain.Navigator.SetFacingDirectionEntity(baseEntity);
			}
			else
			{
				brain.Navigator.ClearFacingDirectionOverride();
			}
			return StateStatus.Running;
		}
	}

	public class CoverState : BasicAIState
	{
		public CoverState()
			: base(AIState.Cover)
		{
		}

		public override void StateEnter()
		{
			base.StateEnter();
			HumanNPCNew entity = GetEntity();
			entity.SetDucked(true);
			AIPoint aIPoint = brain.Events.Memory.AIPoint.Get(4);
			if (aIPoint != null)
			{
				aIPoint.SetUsedBy(entity);
			}
		}

		public override void StateLeave()
		{
			base.StateLeave();
			HumanNPCNew entity = GetEntity();
			entity.SetDucked(false);
			brain.Navigator.ClearFacingDirectionOverride();
			AIPoint aIPoint = brain.Events.Memory.AIPoint.Get(4);
			if (aIPoint != null)
			{
				aIPoint.ClearIfUsedBy(entity);
			}
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			HumanNPCNew entity = GetEntity();
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			float num = entity.AmmoFractionRemaining();
			if (num == 0f || (baseEntity != null && !brain.Senses.Memory.IsLOS(baseEntity) && num < 0.25f))
			{
				entity.AttemptReload();
			}
			if (baseEntity != null)
			{
				brain.Navigator.SetFacingDirectionEntity(baseEntity);
			}
			return StateStatus.Running;
		}
	}

	public class DismountedState : BaseDismountedState
	{
		private StateStatus status = StateStatus.Error;

		public override void StateEnter()
		{
			base.StateEnter();
			status = StateStatus.Error;
			HumanNPCNew entity = GetEntity();
			if (brain.PathFinder == null)
			{
				return;
			}
			AIInformationZone informationZone = entity.GetInformationZone(entity.transform.position);
			if (!(informationZone == null))
			{
				AICoverPoint bestCoverPoint = informationZone.GetBestCoverPoint(entity.transform.position, entity.transform.position, 25f, 50f, entity);
				if ((bool)bestCoverPoint)
				{
					bestCoverPoint.SetUsedBy(entity, 10f);
				}
				Vector3 pos = ((bestCoverPoint == null) ? entity.transform.position : bestCoverPoint.transform.position);
				if (brain.Navigator.SetDestination(pos, BaseNavigator.NavigationSpeed.Fast))
				{
					status = StateStatus.Running;
				}
			}
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (status == StateStatus.Error)
			{
				return status;
			}
			if (brain.Navigator.Moving)
			{
				return StateStatus.Running;
			}
			return StateStatus.Finished;
		}
	}

	public class IdleState : BaseIdleState
	{
	}

	public class MountedState : BaseMountedState
	{
	}

	public class RoamState : BaseRoamState
	{
		private StateStatus status = StateStatus.Error;

		private AIMovePoint roamPoint;

		public override void StateLeave()
		{
			base.StateLeave();
			Stop();
			ClearRoamPointUsage();
		}

		public override void StateEnter()
		{
			base.StateEnter();
			status = StateStatus.Error;
			ClearRoamPointUsage();
			HumanNPCNew entity = GetEntity();
			if (brain.PathFinder == null)
			{
				return;
			}
			status = StateStatus.Error;
			roamPoint = brain.PathFinder.GetBestRoamPoint(GetRoamAnchorPosition(), entity.transform.position, entity.eyes.BodyForward(), brain.Navigator.MaxRoamDistanceFromHome, brain.Navigator.BestRoamPointMaxDistance);
			if (roamPoint != null)
			{
				if (brain.Navigator.SetDestination(roamPoint.transform.position, BaseNavigator.NavigationSpeed.Slow))
				{
					roamPoint.SetUsedBy(GetEntity());
					status = StateStatus.Running;
				}
				else
				{
					roamPoint.SetUsedBy(entity, 600f);
				}
			}
		}

		private void ClearRoamPointUsage()
		{
			if (roamPoint != null)
			{
				roamPoint.ClearIfUsedBy(GetEntity());
				roamPoint = null;
			}
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta)
		{
			if (status == StateStatus.Error)
			{
				return status;
			}
			if (brain.Navigator.Moving)
			{
				return StateStatus.Running;
			}
			return StateStatus.Finished;
		}
	}

	public class TakeCoverState : BasicAIState
	{
		private StateStatus status = StateStatus.Error;

		private BaseEntity coverFromEntity;

		public TakeCoverState()
			: base(AIState.TakeCover)
		{
		}

		public override void StateEnter()
		{
			base.StateEnter();
			status = StateStatus.Running;
			if (!StartMovingToCover())
			{
				status = StateStatus.Error;
			}
		}

		public override void StateLeave()
		{
			base.StateLeave();
			brain.Navigator.ClearFacingDirectionOverride();
			ClearCoverPointUsage();
		}

		private void ClearCoverPointUsage()
		{
			AIPoint aIPoint = brain.Events.Memory.AIPoint.Get(4);
			if (aIPoint != null)
			{
				aIPoint.ClearIfUsedBy(GetEntity());
			}
		}

		private bool StartMovingToCover()
		{
			coverFromEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (coverFromEntity == null)
			{
				return false;
			}
			HumanNPCNew entity = GetEntity();
			Vector3 hideFromPosition = (coverFromEntity ? coverFromEntity.transform.position : (entity.transform.position + entity.LastAttackedDir * 30f));
			AIInformationZone informationZone = entity.GetInformationZone(entity.transform.position);
			if (informationZone == null)
			{
				return false;
			}
			float minRange = ((entity.SecondsSinceAttacked < 2f) ? 2f : 0f);
			float bestCoverPointMaxDistance = brain.Navigator.BestCoverPointMaxDistance;
			AICoverPoint bestCoverPoint = informationZone.GetBestCoverPoint(entity.transform.position, hideFromPosition, minRange, bestCoverPointMaxDistance, entity);
			if (bestCoverPoint == null)
			{
				return false;
			}
			Vector3 position = bestCoverPoint.transform.position;
			if (!brain.Navigator.SetDestination(position, BaseNavigator.NavigationSpeed.Normal))
			{
				return false;
			}
			FaceCoverFromEntity();
			brain.Events.Memory.AIPoint.Set(bestCoverPoint, 4);
			bestCoverPoint.SetUsedBy(entity);
			return true;
		}

		public override void DrawGizmos()
		{
			base.DrawGizmos();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			FaceCoverFromEntity();
			if (status == StateStatus.Error)
			{
				return status;
			}
			if (brain.Navigator.Moving)
			{
				return StateStatus.Running;
			}
			return StateStatus.Finished;
		}

		private void FaceCoverFromEntity()
		{
			coverFromEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (!(coverFromEntity == null))
			{
				brain.Navigator.SetFacingDirectionEntity(coverFromEntity);
			}
		}
	}

	public static int Count;

	public override void AddStates()
	{
		base.AddStates();
		AddState(new BaseIdleState());
		AddState(new RoamState());
		AddState(new ChaseState());
		AddState(new CombatState());
		AddState(new TakeCoverState());
		AddState(new CoverState());
		AddState(new MountedState());
		AddState(new DismountedState());
		AddState(new BaseFollowPathState());
		AddState(new BaseNavigateHomeState());
		AddState(new CombatStationaryState());
		AddState(new BaseMoveTorwardsState());
	}

	public override void InitializeAI()
	{
		base.InitializeAI();
		base.ThinkMode = AIThinkMode.Interval;
		thinkRate = 0.25f;
		base.PathFinder = new HumanPathFinder();
		((HumanPathFinder)base.PathFinder).Init(GetEntity());
		Count++;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Count--;
	}

	protected override void OnStateChanged()
	{
		base.OnStateChanged();
		if (base.CurrentState != null)
		{
			switch (base.CurrentState.StateType)
			{
			case AIState.Idle:
			case AIState.Roam:
			case AIState.Patrol:
			case AIState.FollowPath:
			case AIState.Cooldown:
				GetEntity().SetPlayerFlag(BasePlayer.PlayerFlags.Relaxed, true);
				break;
			default:
				GetEntity().SetPlayerFlag(BasePlayer.PlayerFlags.Relaxed, false);
				break;
			}
		}
	}
}
