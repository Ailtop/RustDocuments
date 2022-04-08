using UnityEngine;

public class CH47AIBrain : BaseAIBrain<CH47HelicopterAIController>
{
	public class DropCrate : BasicAIState
	{
		private float nextDropTime;

		public DropCrate()
			: base(AIState.DropCrate)
		{
		}

		public override bool CanInterrupt()
		{
			if (base.CanInterrupt())
			{
				return !CanDrop();
			}
			return false;
		}

		public bool CanDrop()
		{
			if (Time.time > nextDropTime)
			{
				return brain.GetEntity().CanDropCrate();
			}
			return false;
		}

		public override float GetWeight()
		{
			if (!CanDrop())
			{
				return 0f;
			}
			if (IsInState())
			{
				return 10000f;
			}
			if (brain.CurrentState != null && brain.CurrentState.StateType == AIState.Orbit && brain.CurrentState.TimeInState > 60f)
			{
				CH47DropZone closest = CH47DropZone.GetClosest(brain.mainInterestPoint);
				if ((bool)closest && Vector3Ex.Distance2D(closest.transform.position, brain.mainInterestPoint) < 200f)
				{
					CH47AIBrain component = brain.GetComponent<CH47AIBrain>();
					if (component != null)
					{
						float num = Mathf.InverseLerp(300f, 600f, component.Age);
						return 1000f * num;
					}
				}
			}
			return 0f;
		}

		public override void StateEnter()
		{
			brain.GetEntity().SetDropDoorOpen(open: true);
			brain.GetEntity().EnableFacingOverride(enabled: false);
			CH47DropZone closest = CH47DropZone.GetClosest(brain.GetEntity().transform.position);
			if (closest == null)
			{
				nextDropTime = Time.time + 60f;
			}
			brain.mainInterestPoint = closest.transform.position;
			brain.GetEntity().SetMoveTarget(brain.mainInterestPoint);
			base.StateEnter();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (CanDrop() && Vector3Ex.Distance2D(brain.mainInterestPoint, brain.GetEntity().transform.position) < 5f && brain.GetEntity().rigidBody.velocity.magnitude < 5f)
			{
				brain.GetEntity().DropCrate();
				nextDropTime = Time.time + 120f;
			}
			return StateStatus.Running;
		}

		public override void StateLeave()
		{
			brain.GetEntity().SetDropDoorOpen(open: false);
			nextDropTime = Time.time + 60f;
			base.StateLeave();
		}
	}

	public class EgressState : BasicAIState
	{
		private bool killing;

		private bool egressAltitueAchieved;

		public EgressState()
			: base(AIState.Egress)
		{
		}

		public override bool CanInterrupt()
		{
			return false;
		}

		public override float GetWeight()
		{
			if (brain.GetEntity().OutOfCrates() && !brain.GetEntity().ShouldLand())
			{
				return 10000f;
			}
			CH47AIBrain component = brain.GetComponent<CH47AIBrain>();
			if (component != null)
			{
				if (!(component.Age > 1800f))
				{
					return 0f;
				}
				return 10000f;
			}
			return 0f;
		}

		public override void StateEnter()
		{
			brain.GetEntity().EnableFacingOverride(enabled: false);
			Transform transform = brain.GetEntity().transform;
			Rigidbody rigidBody = brain.GetEntity().rigidBody;
			Vector3 rhs = ((rigidBody.velocity.magnitude < 0.1f) ? transform.forward : rigidBody.velocity.normalized);
			Vector3 vector = Vector3.Cross(Vector3.Cross(transform.up, rhs), Vector3.up);
			brain.mainInterestPoint = transform.position + vector * 8000f;
			brain.mainInterestPoint.y = 100f;
			brain.GetEntity().SetMoveTarget(brain.mainInterestPoint);
			base.StateEnter();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			if (killing)
			{
				return StateStatus.Running;
			}
			Vector3 position = brain.GetEntity().transform.position;
			if (position.y < 85f && !egressAltitueAchieved)
			{
				CH47LandingZone closest = CH47LandingZone.GetClosest(position);
				if (closest != null && Vector3Ex.Distance2D(closest.transform.position, position) < 20f)
				{
					float num = 0f;
					if (TerrainMeta.HeightMap != null && TerrainMeta.WaterMap != null)
					{
						num = Mathf.Max(TerrainMeta.WaterMap.GetHeight(position), TerrainMeta.HeightMap.GetHeight(position));
					}
					num += 100f;
					Vector3 moveTarget = position;
					moveTarget.y = num;
					brain.GetEntity().SetMoveTarget(moveTarget);
					return StateStatus.Running;
				}
			}
			egressAltitueAchieved = true;
			brain.GetEntity().SetMoveTarget(brain.mainInterestPoint);
			if (base.TimeInState > 300f)
			{
				brain.GetEntity().Invoke("DelayedKill", 2f);
				killing = true;
			}
			return StateStatus.Running;
		}

		public override void StateLeave()
		{
			base.StateLeave();
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
			brain.GetEntity().SetMoveTarget(brain.GetEntity().GetPosition() + brain.GetEntity().rigidBody.velocity.normalized * 10f);
			base.StateEnter();
		}
	}

	public class LandState : BasicAIState
	{
		private float landedForSeconds;

		private float lastLandtime;

		private float landingHeight = 20f;

		private float nextDismountTime;

		public LandState()
			: base(AIState.Land)
		{
		}

		public override float GetWeight()
		{
			if (!GetEntity().ShouldLand())
			{
				return 0f;
			}
			float num = Time.time - lastLandtime;
			if (IsInState() && landedForSeconds < 12f)
			{
				return 1000f;
			}
			if (!IsInState() && num > 10f)
			{
				return 9000f;
			}
			return 0f;
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			Vector3 position = brain.GetEntity().transform.position;
			_ = brain.GetEntity().transform.forward;
			CH47LandingZone closest = CH47LandingZone.GetClosest(brain.GetEntity().landingTarget);
			if (!closest)
			{
				return StateStatus.Error;
			}
			float magnitude = brain.GetEntity().rigidBody.velocity.magnitude;
			float num = Vector3Ex.Distance2D(closest.transform.position, position);
			bool enabled = num < 40f;
			bool altitudeProtection = num > 15f && position.y < closest.transform.position.y + 10f;
			brain.GetEntity().EnableFacingOverride(enabled);
			brain.GetEntity().SetAltitudeProtection(altitudeProtection);
			int num2;
			if (Mathf.Abs(closest.transform.position.y - position.y) < 3f && num <= 5f)
			{
				num2 = ((magnitude < 1f) ? 1 : 0);
				if (num2 != 0)
				{
					landedForSeconds += delta;
					if (lastLandtime == 0f)
					{
						lastLandtime = Time.time;
					}
				}
			}
			else
			{
				num2 = 0;
			}
			float num3 = 1f - Mathf.InverseLerp(0f, 7f, num);
			landingHeight -= 4f * num3 * Time.deltaTime;
			if (landingHeight < -5f)
			{
				landingHeight = -5f;
			}
			brain.GetEntity().SetAimDirection(closest.transform.forward);
			Vector3 moveTarget = brain.mainInterestPoint + new Vector3(0f, landingHeight, 0f);
			if (num < 100f && num > 15f)
			{
				Vector3 vector = Vector3Ex.Direction2D(closest.transform.position, position);
				if (Physics.SphereCast(position, 15f, vector, out var hitInfo, num, 1218511105))
				{
					Vector3 vector2 = Vector3.Cross(vector, Vector3.up);
					moveTarget = hitInfo.point + vector2 * 50f;
				}
			}
			brain.GetEntity().SetMoveTarget(moveTarget);
			if (num2 != 0)
			{
				if (landedForSeconds > 1f && Time.time > nextDismountTime)
				{
					foreach (BaseVehicle.MountPointInfo mountPoint in brain.GetEntity().mountPoints)
					{
						if ((bool)mountPoint.mountable && mountPoint.mountable.IsMounted())
						{
							nextDismountTime = Time.time + 0.5f;
							mountPoint.mountable.DismountAllPlayers();
							break;
						}
					}
				}
				if (landedForSeconds > 8f)
				{
					brain.GetComponent<CH47AIBrain>().ForceSetAge(float.PositiveInfinity);
				}
			}
			return StateStatus.Running;
		}

		public override void StateEnter()
		{
			brain.mainInterestPoint = GetEntity().landingTarget;
			landingHeight = 15f;
			base.StateEnter();
		}

		public override void StateLeave()
		{
			brain.GetEntity().EnableFacingOverride(enabled: false);
			brain.GetEntity().SetAltitudeProtection(on: true);
			brain.GetEntity().SetMinHoverHeight(30f);
			landedForSeconds = 0f;
			base.StateLeave();
		}

		public override bool CanInterrupt()
		{
			return true;
		}
	}

	public class OrbitState : BasicAIState
	{
		public OrbitState()
			: base(AIState.Orbit)
		{
		}

		public Vector3 GetOrbitCenter()
		{
			return brain.mainInterestPoint;
		}

		public override float GetWeight()
		{
			if (IsInState())
			{
				float num = 1f - Mathf.InverseLerp(120f, 180f, base.TimeInState);
				return 5f * num;
			}
			if (brain.CurrentState != null && brain.CurrentState.StateType == AIState.Patrol && brain.CurrentState is PatrolState patrolState && patrolState.AtPatrolDestination())
			{
				return 5f;
			}
			return 0f;
		}

		public override void StateEnter()
		{
			brain.GetEntity().EnableFacingOverride(enabled: true);
			brain.GetEntity().InitiateAnger();
			base.StateEnter();
		}

		public override StateStatus StateThink(float delta)
		{
			Vector3 orbitCenter = GetOrbitCenter();
			CH47HelicopterAIController entity = brain.GetEntity();
			Vector3 position = entity.GetPosition();
			Vector3 vector = Vector3Ex.Direction2D(orbitCenter, position);
			Vector3 vector2 = Vector3.Cross(Vector3.up, vector);
			float num = ((Vector3.Dot(Vector3.Cross(entity.transform.right, Vector3.up), vector2) < 0f) ? (-1f) : 1f);
			float num2 = 75f;
			Vector3 normalized = (-vector + vector2 * num * 0.6f).normalized;
			Vector3 vector3 = orbitCenter + normalized * num2;
			entity.SetMoveTarget(vector3);
			entity.SetAimDirection(Vector3Ex.Direction2D(vector3, position));
			base.StateThink(delta);
			return StateStatus.Running;
		}

		public override void StateLeave()
		{
			brain.GetEntity().EnableFacingOverride(enabled: false);
			brain.GetEntity().CancelAnger();
			base.StateLeave();
		}
	}

	public class PatrolState : BasePatrolState
	{
		protected float patrolApproachDist = 75f;

		public override void StateEnter()
		{
			base.StateEnter();
			brain.mainInterestPoint = brain.PathFinder.GetRandomPatrolPoint();
		}

		public override StateStatus StateThink(float delta)
		{
			base.StateThink(delta);
			brain.GetEntity().SetMoveTarget(brain.mainInterestPoint);
			return StateStatus.Running;
		}

		public bool AtPatrolDestination()
		{
			return Vector3Ex.Distance2D(GetDestination(), brain.GetEntity().transform.position) < patrolApproachDist;
		}

		public Vector3 GetDestination()
		{
			return brain.mainInterestPoint;
		}

		public override bool CanInterrupt()
		{
			if (base.CanInterrupt())
			{
				return AtPatrolDestination();
			}
			return false;
		}

		public override float GetWeight()
		{
			if (IsInState())
			{
				if (AtPatrolDestination() && base.TimeInState > 2f)
				{
					return 0f;
				}
				return 3f;
			}
			float num = Mathf.InverseLerp(70f, 120f, TimeSinceState()) * 5f;
			return 1f + num;
		}
	}

	public override void AddStates()
	{
		base.AddStates();
		AddState(new IdleState());
		AddState(new PatrolState());
		AddState(new OrbitState());
		AddState(new EgressState());
		AddState(new DropCrate());
		AddState(new LandState());
	}

	public override void InitializeAI()
	{
		base.InitializeAI();
		base.ThinkMode = AIThinkMode.FixedUpdate;
		base.PathFinder = new CH47PathFinder();
	}

	public void FixedUpdate()
	{
		if (!(base.baseEntity == null) && !base.baseEntity.isClient)
		{
			Think(Time.fixedDeltaTime);
		}
	}
}
