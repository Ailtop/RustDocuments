using System.Collections.Generic;
using Rust;
using UnityEngine;

public class SimpleShark : BaseCombatEntity
{
	public class SimpleState
	{
		public SimpleShark entity;

		private float stateEnterTime;

		public SimpleState(SimpleShark owner)
		{
			entity = owner;
		}

		public virtual float State_Weight()
		{
			return 0f;
		}

		public virtual void State_Enter()
		{
			stateEnterTime = Time.realtimeSinceStartup;
		}

		public virtual void State_Think(float delta)
		{
		}

		public virtual void State_Exit()
		{
		}

		public virtual bool CanInterrupt()
		{
			return true;
		}

		public virtual float TimeInState()
		{
			return Time.realtimeSinceStartup - stateEnterTime;
		}
	}

	public class IdleState : SimpleState
	{
		private int patrolTargetIndex;

		public IdleState(SimpleShark owner)
			: base(owner)
		{
		}

		public Vector3 GetTargetPatrolPosition()
		{
			return entity.patrolPath[patrolTargetIndex];
		}

		public override float State_Weight()
		{
			return 1f;
		}

		public override void State_Enter()
		{
			float num = float.PositiveInfinity;
			int num2 = 0;
			for (int i = 0; i < entity.patrolPath.Count; i++)
			{
				float num3 = Vector3.Distance(entity.patrolPath[i], entity.transform.position);
				if (num3 < num)
				{
					num2 = i;
					num = num3;
				}
			}
			patrolTargetIndex = num2;
			base.State_Enter();
		}

		public override void State_Think(float delta)
		{
			if (Vector3.Distance(GetTargetPatrolPosition(), entity.transform.position) < entity.stoppingDistance)
			{
				patrolTargetIndex++;
				if (patrolTargetIndex >= entity.patrolPath.Count)
				{
					patrolTargetIndex = 0;
				}
			}
			if (entity.TimeSinceAttacked() >= 120f && entity.healthFraction < 1f)
			{
				entity.health = entity.MaxHealth();
			}
			entity.destination = entity.WaterClamp(GetTargetPatrolPosition());
		}

		public override void State_Exit()
		{
			base.State_Exit();
		}

		public override bool CanInterrupt()
		{
			return true;
		}
	}

	public class AttackState : SimpleState
	{
		public AttackState(SimpleShark owner)
			: base(owner)
		{
		}

		public override float State_Weight()
		{
			if (!entity.HasTarget() || !entity.CanAttack())
			{
				return 0f;
			}
			return 10f;
		}

		public override void State_Enter()
		{
			base.State_Enter();
		}

		public override void State_Think(float delta)
		{
			BasePlayer target = entity.GetTarget();
			if (target == null)
			{
				return;
			}
			if (TimeInState() >= 10f)
			{
				entity.nextAttackTime = Time.realtimeSinceStartup + 4f;
				entity.Startle();
				return;
			}
			if (entity.CanAttack())
			{
				entity.Startle();
			}
			float num = Vector3.Distance(entity.GetTarget().eyes.position, entity.transform.position);
			bool num2 = num < 4f;
			if (entity.CanAttack() && num <= 2f)
			{
				entity.DoAttack();
			}
			if (!num2)
			{
				Vector3 vector = Vector3Ex.Direction(entity.GetTarget().eyes.position, entity.transform.position);
				Vector3 point = target.eyes.position + vector * 10f;
				point = entity.WaterClamp(point);
				entity.destination = point;
			}
		}

		public override void State_Exit()
		{
			base.State_Exit();
		}

		public override bool CanInterrupt()
		{
			return true;
		}
	}

	public Vector3 destination;

	public float minSpeed;

	public float maxSpeed;

	public float idealDepth;

	public float minTurnSpeed = 0.25f;

	public float maxTurnSpeed = 2f;

	public float attackCooldown = 7f;

	public float aggroRange = 15f;

	public float obstacleDetectionRadius = 1f;

	public Animator animator;

	public GameObjectRef bloodCloud;

	public GameObjectRef corpsePrefab;

	[ServerVar]
	public static float forceSurfaceAmount = 0f;

	[ServerVar]
	public static bool disable = false;

	private Vector3 spawnPos;

	private float stoppingDistance = 3f;

	private float currentSpeed;

	private float lastStartleTime;

	private float startleDuration = 1f;

	private SimpleState[] states;

	private SimpleState _currentState;

	public List<Vector3> patrolPath = new List<Vector3>();

	private BasePlayer target;

	private float lastSeenTargetTime;

	private float nextTargetSearchTime;

	private static BasePlayer[] playerQueryResults = new BasePlayer[64];

	private float minFloorDist = 2f;

	private float minSurfaceDist = 1f;

	private float lastTimeAttacked;

	private float nextAttackTime;

	private Vector3 cachedObstacleNormal;

	private float cachedObstacleDistance;

	private float obstacleAvoidanceScale;

	private float obstacleDetectionRange = 5f;

	private float lastObstacleCheckTime;

	private void GenerateIdlePoints(Vector3 center, float radius, float heightOffset, float staggerOffset = 0f)
	{
		patrolPath.Clear();
		float num = 0f;
		int num2 = 32;
		int layerMask = 10551553;
		float height = TerrainMeta.WaterMap.GetHeight(center);
		float height2 = TerrainMeta.HeightMap.GetHeight(center);
		for (int i = 0; i < num2; i++)
		{
			num += 360f / (float)num2;
			float radius2 = 1f;
			Vector3 vector = BasePathFinder.GetPointOnCircle(center, radius2, num);
			Vector3 vector2 = Vector3Ex.Direction(vector, center);
			RaycastHit hitInfo;
			vector = ((!Physics.SphereCast(center, obstacleDetectionRadius, vector2, out hitInfo, radius + staggerOffset, layerMask)) ? (center + vector2 * radius) : (center + vector2 * (hitInfo.distance - 6f)));
			if (staggerOffset != 0f)
			{
				vector += vector2 * Random.Range(0f - staggerOffset, staggerOffset);
			}
			vector.y += Random.Range(0f - heightOffset, heightOffset);
			vector.y = Mathf.Clamp(vector.y, height2 + 3f, height - 3f);
			patrolPath.Add(vector);
		}
	}

	private void GenerateIdlePoints_Shrinkwrap(Vector3 center, float radius, float heightOffset, float staggerOffset = 0f)
	{
		patrolPath.Clear();
		float num = 0f;
		int num2 = 32;
		int layerMask = 10551553;
		float height = TerrainMeta.WaterMap.GetHeight(center);
		float height2 = TerrainMeta.HeightMap.GetHeight(center);
		for (int i = 0; i < num2; i++)
		{
			num += 360f / (float)num2;
			float radius2 = radius * 2f;
			Vector3 vector = BasePathFinder.GetPointOnCircle(center, radius2, num);
			Vector3 vector2 = Vector3Ex.Direction(center, vector);
			RaycastHit hitInfo;
			if (Physics.SphereCast(vector, obstacleDetectionRadius, vector2, out hitInfo, radius + staggerOffset, layerMask))
			{
				vector = hitInfo.point - vector2 * 6f;
			}
			else
			{
				vector += vector2 * radius;
			}
			if (staggerOffset != 0f)
			{
				vector += vector2 * Random.Range(0f - staggerOffset, staggerOffset);
			}
			vector.y += Random.Range(0f - heightOffset, heightOffset);
			vector.y = Mathf.Clamp(vector.y, height2 + 3f, height - 3f);
			patrolPath.Add(vector);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (disable)
		{
			Invoke(base.KillMessage, 0.01f);
			return;
		}
		base.transform.position = WaterClamp(base.transform.position);
		Init();
	}

	public void Init()
	{
		GenerateIdlePoints_Shrinkwrap(base.transform.position, 20f, 2f, 3f);
		states = new SimpleState[2];
		states[0] = new IdleState(this);
		states[1] = new AttackState(this);
		base.transform.position = patrolPath[0];
	}

	private void Think(float delta)
	{
		if (states == null)
		{
			return;
		}
		if (disable)
		{
			if (!IsInvoking(base.KillMessage))
			{
				Invoke(base.KillMessage, 0.01f);
			}
			return;
		}
		SimpleState simpleState = null;
		float num = -1f;
		SimpleState[] array = states;
		foreach (SimpleState simpleState2 in array)
		{
			float num2 = simpleState2.State_Weight();
			if (num2 > num)
			{
				simpleState = simpleState2;
				num = num2;
			}
		}
		if (simpleState != _currentState && (_currentState == null || _currentState.CanInterrupt()))
		{
			if (_currentState != null)
			{
				_currentState.State_Exit();
			}
			simpleState.State_Enter();
			_currentState = simpleState;
		}
		UpdateTarget(delta);
		_currentState.State_Think(delta);
		UpdateObstacleAvoidance(delta);
		UpdateDirection(delta);
		UpdateSpeed(delta);
		UpdatePosition(delta);
		SetFlag(Flags.Open, HasTarget() && CanAttack());
	}

	public Vector3 WaterClamp(Vector3 point)
	{
		float height = WaterSystem.GetHeight(point);
		float min = TerrainMeta.HeightMap.GetHeight(point) + minFloorDist;
		float max = height - minSurfaceDist;
		if (forceSurfaceAmount != 0f)
		{
			min = (max = WaterSystem.GetHeight(point) + forceSurfaceAmount);
		}
		point.y = Mathf.Clamp(point.y, min, max);
		return point;
	}

	public bool ValidTarget(BasePlayer newTarget)
	{
		float maxDistance = Vector3.Distance(newTarget.eyes.position, base.transform.position);
		Vector3 direction = Vector3Ex.Direction(newTarget.eyes.position, base.transform.position);
		int layerMask = 10551552;
		if (Physics.Raycast(base.transform.position, direction, maxDistance, layerMask))
		{
			return false;
		}
		if (newTarget.isMounted)
		{
			if ((bool)newTarget.GetMountedVehicle())
			{
				return false;
			}
			if (!newTarget.GetMounted().GetComponent<WaterInflatable>().buoyancy.enabled)
			{
				return false;
			}
		}
		else if (!WaterLevel.Test(newTarget.CenterPoint(), true, newTarget))
		{
			return false;
		}
		return true;
	}

	public void ClearTarget()
	{
		target = null;
		lastSeenTargetTime = 0f;
	}

	public override void OnKilled(HitInfo hitInfo = null)
	{
		if (base.isServer)
		{
			BaseCorpse baseCorpse = DropCorpse(corpsePrefab.resourcePath);
			if ((bool)baseCorpse)
			{
				baseCorpse.Spawn();
				baseCorpse.TakeChildren(this);
			}
			Invoke(base.KillMessage, 0.5f);
		}
		base.OnKilled(hitInfo);
	}

	public void UpdateTarget(float delta)
	{
		if (target != null)
		{
			bool flag = Vector3.Distance(target.eyes.position, base.transform.position) > aggroRange * 2f;
			bool flag2 = Time.realtimeSinceStartup > lastSeenTargetTime + 4f;
			if (!ValidTarget(target) || flag || flag2)
			{
				ClearTarget();
			}
			else
			{
				lastSeenTargetTime = Time.realtimeSinceStartup;
			}
		}
		if (Time.realtimeSinceStartup < nextTargetSearchTime || !(target == null))
		{
			return;
		}
		nextTargetSearchTime = Time.realtimeSinceStartup + 1f;
		if (!BaseNetworkable.HasCloseConnections(base.transform.position, aggroRange))
		{
			return;
		}
		int playersInSphere = Query.Server.GetPlayersInSphere(base.transform.position, aggroRange, playerQueryResults);
		for (int i = 0; i < playersInSphere; i++)
		{
			BasePlayer basePlayer = playerQueryResults[i];
			if (!basePlayer.isClient && ValidTarget(basePlayer))
			{
				target = basePlayer;
				lastSeenTargetTime = Time.realtimeSinceStartup;
				break;
			}
		}
	}

	public float TimeSinceAttacked()
	{
		return Time.realtimeSinceStartup - lastTimeAttacked;
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		lastTimeAttacked = Time.realtimeSinceStartup;
		if (info.damageTypes.Total() > 20f)
		{
			Startle();
		}
		if (info.InitiatorPlayer != null && target == null && ValidTarget(info.InitiatorPlayer))
		{
			target = info.InitiatorPlayer;
			lastSeenTargetTime = Time.realtimeSinceStartup;
		}
	}

	public bool HasTarget()
	{
		return target != null;
	}

	public BasePlayer GetTarget()
	{
		return target;
	}

	public bool CanAttack()
	{
		return Time.realtimeSinceStartup > nextAttackTime;
	}

	public void DoAttack()
	{
		if (HasTarget())
		{
			GetTarget().Hurt(Random.Range(30f, 70f), DamageType.Bite, this);
			Vector3 posWorld = WaterClamp(GetTarget().CenterPoint());
			Effect.server.Run(bloodCloud.resourcePath, posWorld, Vector3.forward);
			nextAttackTime = Time.realtimeSinceStartup + attackCooldown;
		}
	}

	public void Startle()
	{
		lastStartleTime = Time.realtimeSinceStartup;
	}

	public bool IsStartled()
	{
		return lastStartleTime + startleDuration > Time.realtimeSinceStartup;
	}

	private float GetDesiredSpeed()
	{
		if (!IsStartled())
		{
			return minSpeed;
		}
		return maxSpeed;
	}

	public float GetTurnSpeed()
	{
		if (IsStartled())
		{
			return maxTurnSpeed;
		}
		if (obstacleAvoidanceScale != 0f)
		{
			return Mathf.Lerp(minTurnSpeed, maxTurnSpeed, obstacleAvoidanceScale);
		}
		return minTurnSpeed;
	}

	private float GetCurrentSpeed()
	{
		return currentSpeed;
	}

	private void UpdateObstacleAvoidance(float delta)
	{
		delta = Time.realtimeSinceStartup - lastObstacleCheckTime;
		if (delta < 0.5f)
		{
			return;
		}
		Vector3 forward = base.transform.forward;
		Vector3 position = base.transform.position;
		int layerMask = 1503764737;
		RaycastHit hitInfo;
		if (Physics.SphereCast(position, obstacleDetectionRadius, forward, out hitInfo, obstacleDetectionRange, layerMask))
		{
			Vector3 point = hitInfo.point;
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			RaycastHit hitInfo2;
			if (Physics.SphereCast(position + Vector3.down * 0.25f + base.transform.right * 0.25f, obstacleDetectionRadius, forward, out hitInfo2, obstacleDetectionRange, layerMask))
			{
				vector = hitInfo2.point;
			}
			RaycastHit hitInfo3;
			if (Physics.SphereCast(position + Vector3.down * 0.25f - base.transform.right * 0.25f, obstacleDetectionRadius, forward, out hitInfo3, obstacleDetectionRange, layerMask))
			{
				vector2 = hitInfo2.point;
			}
			if (vector != Vector3.zero && vector2 != Vector3.zero)
			{
				Vector3 normal = new Plane(point, vector, vector2).normal;
				if (normal != Vector3.zero)
				{
					hitInfo.normal = normal;
				}
			}
			cachedObstacleNormal = hitInfo.normal;
			cachedObstacleDistance = hitInfo.distance;
			obstacleAvoidanceScale = 1f - Mathf.InverseLerp(2f, obstacleDetectionRange * 0.75f, hitInfo.distance);
		}
		else
		{
			obstacleAvoidanceScale = Mathf.MoveTowards(obstacleAvoidanceScale, 0f, delta * 2f);
			if (obstacleAvoidanceScale == 0f)
			{
				cachedObstacleDistance = 0f;
			}
		}
	}

	private void UpdateDirection(float delta)
	{
		Vector3 forward2 = base.transform.forward;
		Vector3 forward = Vector3Ex.Direction(WaterClamp(destination), base.transform.position);
		if (obstacleAvoidanceScale != 0f)
		{
			Vector3 vector;
			if (cachedObstacleNormal != Vector3.zero)
			{
				Vector3 lhs = QuaternionEx.LookRotationForcedUp(cachedObstacleNormal, Vector3.up) * Vector3.forward;
				vector = ((!(Vector3.Dot(lhs, base.transform.right) > Vector3.Dot(lhs, -base.transform.right))) ? (-base.transform.right) : base.transform.right);
			}
			else
			{
				vector = base.transform.right;
			}
			forward = vector * obstacleAvoidanceScale;
			forward.Normalize();
		}
		Quaternion b = Quaternion.LookRotation(forward, Vector3.up);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, delta * GetTurnSpeed());
	}

	private void UpdatePosition(float delta)
	{
		Vector3 forward = base.transform.forward;
		Vector3.Distance(WaterClamp(destination), base.transform.position);
		Vector3 point = base.transform.position + forward * GetCurrentSpeed() * delta;
		point = WaterClamp(point);
		base.transform.position = point;
	}

	private void UpdateSpeed(float delta)
	{
		currentSpeed = Mathf.Lerp(currentSpeed, GetDesiredSpeed(), delta * 4f);
	}

	public void Update()
	{
		if (base.isServer)
		{
			Think(Time.deltaTime);
		}
	}
}
