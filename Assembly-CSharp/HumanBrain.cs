using UnityEngine;

public class HumanBrain : BaseAIBrain<HumanNPC>
{
	public class ChaseState : BasicAIState
	{
		private float nextPositionUpdateTime;

		public ChaseState()
			: base(AIState.Chase)
		{
		}

		public override float GetWeight()
		{
			float num = 0f;
			if (!GetEntity().HasTarget())
			{
				return 0f;
			}
			if (GetEntity().AmmoFractionRemaining() < 0.3f || GetEntity().IsReloading())
			{
				num -= 1f;
			}
			if (GetEntity().HasTarget())
			{
				num += 0.5f;
			}
			num = (GetEntity().CanSeeTarget() ? (num - 0.5f) : (num + 1f));
			if (GetEntity().DistanceToTarget() > GetEntity().GetIdealDistanceFromTarget())
			{
				num += 1f;
			}
			return num;
		}

		public override void StateEnter()
		{
			base.StateEnter();
			GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Walk);
		}

		public override void StateLeave()
		{
			base.StateLeave();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (GetEntity().currentTarget == null)
			{
				return StateStatus.Error;
			}
			float num = Vector3.Distance(GetEntity().currentTarget.transform.position, GetEntity().transform.position);
			if (num < 5f)
			{
				GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.SlowWalk);
			}
			else if (num < 10f)
			{
				GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Walk);
			}
			else
			{
				GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Sprint);
			}
			if (Time.time > nextPositionUpdateTime)
			{
				Random.Range(1f, 2f);
				Vector3 position = GetEntity().transform.position;
				AIMovePoint aIMovePoint = GetEntity().GetInformationZone(GetEntity().currentTarget.transform.position).GetBestMovePointNear(maxRange: Mathf.Min(35f, GetEntity().EngagementRange() * 0.75f), targetPosition: GetEntity().currentTarget.transform.position, fromPosition: GetEntity().transform.position, minRange: 0f, checkLOS: true, forObject: GetEntity(), returnClosest: true);
				if ((bool)aIMovePoint)
				{
					aIMovePoint.SetUsedBy(GetEntity(), 5f);
					position = aIMovePoint.transform.position;
					position = GetEntity().GetRandomPositionAround(position, 0f, aIMovePoint.radius - 0.3f);
				}
				else
				{
					position = GetEntity().GetRandomPositionAround(GetEntity().currentTarget.transform.position, 1f);
				}
				GetEntity().SetDestination(position);
				nextPositionUpdateTime = Time.time + 1f;
			}
			return StateStatus.Running;
		}
	}

	public class CombatState : BasicAIState
	{
		private float nextStrafeTime;

		public CombatState()
			: base(AIState.Combat)
		{
		}

		public override void StateEnter()
		{
			base.StateEnter();
			brain.mainInterestPoint = GetEntity().transform.position;
			GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Walk);
		}

		public override float GetWeight()
		{
			if (!GetEntity().HasTarget())
			{
				return 0f;
			}
			if (!GetEntity().TargetInRange())
			{
				return 0f;
			}
			float num = 1f - Mathf.InverseLerp(GetEntity().GetIdealDistanceFromTarget(), GetEntity().EngagementRange(), GetEntity().DistanceToTarget());
			float num2 = 0.5f * num;
			if (GetEntity().CanSeeTarget())
			{
				num2 += 1f;
			}
			return num2;
		}

		public override void StateLeave()
		{
			GetEntity().SetDucked(false);
			base.StateLeave();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (Time.time > nextStrafeTime)
			{
				if (Random.Range(0, 3) == 1)
				{
					nextStrafeTime = Time.time + Random.Range(2f, 3f);
					GetEntity().SetDucked(true);
					GetEntity().Stop();
				}
				else
				{
					nextStrafeTime = Time.time + Random.Range(3f, 4f);
					GetEntity().SetDucked(false);
					GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Walk);
					GetEntity().SetDestination(GetEntity().GetRandomPositionAround(brain.mainInterestPoint, 1f));
				}
			}
			return StateStatus.Running;
		}
	}

	public class CoverState : BasicAIState
	{
		private float lastCoverTime;

		private bool isFleeing;

		private bool inCover;

		private float timeInCover;

		private AICoverPoint currentCover;

		public CoverState()
			: base(AIState.Cover)
		{
		}

		public override float GetWeight()
		{
			float num = 0f;
			if (!GetEntity().currentTarget && GetEntity().SecondsSinceAttacked < 2f)
			{
				return 4f;
			}
			if (GetEntity().DistanceToTarget() > GetEntity().EngagementRange() * 3f)
			{
				return 6f;
			}
			if (!IsInState() && TimeSinceState() < 2f)
			{
				return 0f;
			}
			if (GetEntity().SecondsSinceAttacked < 5f || GetEntity().healthFraction < 0.4f || GetEntity().DistanceToTarget() < 15f)
			{
				if (GetEntity().IsReloading())
				{
					num += 2f;
				}
				num += (1f - Mathf.Lerp(0.1f, 0.35f, GetEntity().AmmoFractionRemaining())) * 1.5f;
			}
			if (isFleeing)
			{
				num += 1f;
			}
			if (GetEntity().healthFraction < 1f)
			{
				float num2 = 1f - Mathf.InverseLerp(0.8f, 1f, GetEntity().healthFraction);
				num += (1f - Mathf.InverseLerp(1f, 2f, GetEntity().SecondsSinceAttacked)) * num2 * 2f;
			}
			return num;
		}

		public override bool CanInterrupt()
		{
			float num = (GetEntity().currentTarget ? 2f : 8f);
			if (base.TimeInState > 5f)
			{
				if (inCover)
				{
					return timeInCover > num;
				}
				return true;
			}
			return false;
		}

		public override void StateEnter()
		{
			GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Walk);
			lastCoverTime = -10f;
			isFleeing = false;
			inCover = false;
			timeInCover = -1f;
			GetEntity().ClearStationaryAimPoint();
			currentCover = null;
			base.StateEnter();
		}

		public override void StateLeave()
		{
			base.StateLeave();
			GetEntity().SetDucked(false);
			GetEntity().ClearStationaryAimPoint();
			if ((bool)currentCover)
			{
				currentCover.ClearIfUsedBy(GetEntity());
				currentCover = null;
			}
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			float num = 2f;
			float num2 = 0f;
			if (Time.time > lastCoverTime + num && !isFleeing)
			{
				Vector3 hideFromPosition = (GetEntity().currentTarget ? GetEntity().currentTarget.transform.position : (GetEntity().transform.position + GetEntity().LastAttackedDir * 30f));
				float num3 = ((GetEntity().currentTarget != null) ? GetEntity().DistanceToTarget() : 30f);
				AIInformationZone informationZone = GetEntity().GetInformationZone(GetEntity().transform.position);
				if (informationZone != null)
				{
					float secondsSinceAttacked = GetEntity().SecondsSinceAttacked;
					float minRange = ((secondsSinceAttacked < 2f) ? 2f : 0f);
					float maxRange = 25f;
					if (currentCover != null)
					{
						currentCover.ClearIfUsedBy(GetEntity());
						currentCover = null;
					}
					AICoverPoint bestCoverPoint = informationZone.GetBestCoverPoint(GetEntity().transform.position, hideFromPosition, minRange, maxRange, GetEntity());
					if ((bool)bestCoverPoint)
					{
						bestCoverPoint.SetUsedBy(GetEntity(), 15f);
						currentCover = bestCoverPoint;
					}
					Vector3 vector = ((bestCoverPoint == null) ? GetEntity().transform.position : bestCoverPoint.transform.position);
					GetEntity().SetDestination(vector);
					float num4 = Vector3.Distance(vector, GetEntity().transform.position);
					bool flag2;
					int num5;
					if (secondsSinceAttacked < 4f)
					{
						flag2 = GetEntity().AmmoFractionRemaining() <= 0.25f;
					}
					else
						num5 = 0;
					bool flag3;
					int num6;
					if (GetEntity().healthFraction < 0.5f && secondsSinceAttacked < 1f)
					{
						flag3 = Time.time > num2;
					}
					else
						num6 = 0;
					if ((num3 > 6f && num4 > 8f) || GetEntity().currentTarget == null)
					{
						isFleeing = true;
						num2 = Time.time + Random.Range(4f, 7f);
					}
					if (num4 > 1f)
					{
						GetEntity().ClearStationaryAimPoint();
					}
				}
				lastCoverTime = Time.time;
			}
			bool flag = Vector3.Distance(GetEntity().transform.position, GetEntity().finalDestination) <= 0.25f;
			if (!inCover && flag)
			{
				if (isFleeing)
				{
					GetEntity().SetStationaryAimPoint(GetEntity().finalDestination + -GetEntity().eyes.BodyForward() * 5f);
				}
				else if ((bool)GetEntity().currentTarget)
				{
					GetEntity().SetStationaryAimPoint(GetEntity().transform.position + Vector3Ex.Direction2D(GetEntity().currentTarget.transform.position, GetEntity().transform.position) * 5f);
				}
			}
			inCover = flag;
			if (inCover)
			{
				timeInCover += delta;
			}
			else
			{
				timeInCover = 0f;
			}
			GetEntity().SetDucked(inCover);
			if (inCover)
			{
				isFleeing = false;
			}
			if (GetEntity().AmmoFractionRemaining() == 0f || isFleeing || (!GetEntity().CanSeeTarget() && inCover && GetEntity().SecondsSinceDealtDamage > 2f && GetEntity().AmmoFractionRemaining() < 0.25f))
			{
				GetEntity().AttemptReload();
			}
			if (!inCover)
			{
				if (base.TimeInState > 1f && isFleeing)
				{
					GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Sprint);
				}
				else
				{
					GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Walk);
				}
			}
			return StateStatus.Running;
		}
	}

	public class ExfilState : BasicAIState
	{
		public ExfilState()
			: base(AIState.Exfil)
		{
		}

		public override float GetWeight()
		{
			if (GetEntity().RecentlyDismounted() && GetEntity().SecondsSinceAttacked > 1f)
			{
				return 100f;
			}
			return 0f;
		}

		public override void StateEnter()
		{
			base.StateEnter();
			GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Sprint);
			AIInformationZone informationZone = GetEntity().GetInformationZone(GetEntity().transform.position);
			if (informationZone != null)
			{
				AICoverPoint bestCoverPoint = informationZone.GetBestCoverPoint(GetEntity().transform.position, GetEntity().transform.position, 25f, 50f, GetEntity());
				if ((bool)bestCoverPoint)
				{
					bestCoverPoint.SetUsedBy(GetEntity(), 10f);
				}
				Vector3 vector = ((bestCoverPoint == null) ? GetEntity().transform.position : bestCoverPoint.transform.position);
				GetEntity().SetDestination(vector);
				brain.mainInterestPoint = vector;
			}
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (GetEntity().CanSeeTarget() && base.TimeInState > 2f && GetEntity().DistanceToTarget() < 10f)
			{
				GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Walk);
			}
			else
			{
				GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Sprint);
			}
			return StateStatus.Running;
		}
	}

	public class IdleState : BaseIdleState
	{
		public override float GetWeight()
		{
			return 0.1f;
		}

		public override void StateEnter()
		{
			GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.SlowWalk);
			base.StateEnter();
		}
	}

	public class MountedState : BasicAIState
	{
		public MountedState()
			: base(AIState.Mounted)
		{
		}

		public override float GetWeight()
		{
			if (GetEntity().isMounted)
			{
				return 100f;
			}
			return 0f;
		}

		public override void StateEnter()
		{
			GetEntity().SetNavMeshEnabled(false);
			GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.Walk);
			base.StateEnter();
		}

		public override void StateLeave()
		{
			GetEntity().SetNavMeshEnabled(true);
			base.StateLeave();
		}
	}

	public class RoamState : BaseRoamState
	{
		public override float GetWeight()
		{
			if (!GetEntity().HasTarget() && GetEntity().SecondsSinceAttacked > 10f)
			{
				return 5f;
			}
			return 0f;
		}

		public override void StateEnter()
		{
			base.StateEnter();
			GetEntity().SetDesiredSpeed(HumanNPC.SpeedType.SlowWalk);
			GetEntity().SetPlayerFlag(BasePlayer.PlayerFlags.Relaxed, true);
		}

		public override void StateLeave()
		{
			base.StateLeave();
			GetEntity().SetPlayerFlag(BasePlayer.PlayerFlags.Relaxed, false);
		}

		public override Vector3 GetDestination()
		{
			return GetEntity().finalDestination;
		}

		public override Vector3 GetForwardDirection()
		{
			return GetEntity().eyes.BodyForward();
		}

		public override void SetDestination(Vector3 destination)
		{
			base.SetDestination(destination);
			GetEntity().SetDestination(destination);
		}
	}

	public override void AddStates()
	{
		base.AddStates();
		AddState(new IdleState());
		AddState(new RoamState());
		AddState(new ChaseState());
		AddState(new CoverState());
		AddState(new CombatState());
		AddState(new MountedState());
		AddState(new ExfilState());
	}

	public override void InitializeAI()
	{
		base.InitializeAI();
		base.ThinkMode = AIThinkMode.Interval;
		thinkRate = 0.25f;
		base.PathFinder = new HumanPathFinder();
		((HumanPathFinder)base.PathFinder).Init(GetEntity());
	}
}
