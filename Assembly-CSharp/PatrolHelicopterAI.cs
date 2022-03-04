using System;
using System.Collections.Generic;
using ConVar;
using Oxide.Core;
using Rust;
using UnityEngine;

public class PatrolHelicopterAI : BaseMonoBehaviour
{
	public class targetinfo
	{
		public BasePlayer ply;

		public BaseEntity ent;

		public float lastSeenTime = float.PositiveInfinity;

		public float visibleFor;

		public float nextLOSCheck;

		public targetinfo(BaseEntity initEnt, BasePlayer initPly = null)
		{
			ply = initPly;
			ent = initEnt;
			lastSeenTime = float.PositiveInfinity;
			nextLOSCheck = UnityEngine.Time.realtimeSinceStartup + 1.5f;
		}

		public bool IsVisible()
		{
			return TimeSinceSeen() < 1.5f;
		}

		public float TimeSinceSeen()
		{
			return UnityEngine.Time.realtimeSinceStartup - lastSeenTime;
		}
	}

	public enum aiState
	{
		IDLE = 0,
		MOVE = 1,
		ORBIT = 2,
		STRAFE = 3,
		PATROL = 4,
		GUARD = 5,
		DEATH = 6
	}

	public List<targetinfo> _targetList = new List<targetinfo>();

	public Vector3 interestZoneOrigin;

	public Vector3 destination;

	public bool hasInterestZone;

	public float moveSpeed;

	public float maxSpeed = 25f;

	public float courseAdjustLerpTime = 2f;

	public Quaternion targetRotation;

	public Vector3 windVec;

	public Vector3 targetWindVec;

	public float windForce = 5f;

	public float windFrequency = 1f;

	public float targetThrottleSpeed;

	public float throttleSpeed;

	public float maxRotationSpeed = 90f;

	public float rotationSpeed;

	public float terrainPushForce = 100f;

	public float obstaclePushForce = 100f;

	public HelicopterTurret leftGun;

	public HelicopterTurret rightGun;

	public static PatrolHelicopterAI heliInstance;

	public BaseHelicopter helicopterBase;

	public aiState _currentState;

	private Vector3 _aimTarget;

	private bool movementLockingAiming;

	private bool hasAimTarget;

	private bool aimDoorSide;

	private Vector3 pushVec = Vector3.zero;

	private Vector3 _lastPos;

	private Vector3 _lastMoveDir;

	public bool isDead;

	public bool isRetiring;

	public float spawnTime;

	public float lastDamageTime;

	private float deathTimeout;

	private float destination_min_dist = 2f;

	private float currentOrbitDistance;

	private float currentOrbitTime;

	private bool hasEnteredOrbit;

	private float orbitStartTime;

	private float maxOrbitDuration = 30f;

	private bool breakingOrbit;

	public List<MonumentInfo> _visitedMonuments;

	public float arrivalTime;

	public GameObjectRef rocketProjectile;

	public GameObjectRef rocketProjectile_Napalm;

	public bool leftTubeFiredLast;

	public float lastRocketTime;

	public float timeBetweenRockets = 0.2f;

	public int numRocketsLeft = 12;

	public const int maxRockets = 12;

	public Vector3 strafe_target_position;

	private bool puttingDistance;

	public const float strafe_approach_range = 175f;

	public const float strafe_firing_range = 150f;

	private bool useNapalm;

	[NonSerialized]
	public float lastNapalmTime = float.NegativeInfinity;

	[NonSerialized]
	public float lastStrafeTime = float.NegativeInfinity;

	private float _lastThinkTime;

	public void UpdateTargetList()
	{
		Vector3 strafePos = Vector3.zero;
		bool flag = false;
		bool shouldUseNapalm = false;
		for (int num = _targetList.Count - 1; num >= 0; num--)
		{
			targetinfo targetinfo = _targetList[num];
			if (targetinfo == null || targetinfo.ent == null)
			{
				_targetList.Remove(targetinfo);
			}
			else
			{
				if (UnityEngine.Time.realtimeSinceStartup > targetinfo.nextLOSCheck)
				{
					targetinfo.nextLOSCheck = UnityEngine.Time.realtimeSinceStartup + 1f;
					if (PlayerVisible(targetinfo.ply))
					{
						targetinfo.lastSeenTime = UnityEngine.Time.realtimeSinceStartup;
						targetinfo.visibleFor += 1f;
					}
					else
					{
						targetinfo.visibleFor = 0f;
					}
				}
				bool flag2 = (targetinfo.ply ? targetinfo.ply.IsDead() : (targetinfo.ent.Health() <= 0f));
				if (targetinfo.TimeSinceSeen() >= 6f || flag2)
				{
					bool flag3 = UnityEngine.Random.Range(0f, 1f) >= 0f;
					if ((CanStrafe() || CanUseNapalm()) && IsAlive() && !flag && !flag2 && (targetinfo.ply == leftGun._target || targetinfo.ply == rightGun._target) && flag3)
					{
						shouldUseNapalm = !ValidStrafeTarget(targetinfo.ply) || UnityEngine.Random.Range(0f, 1f) > 0.75f;
						flag = true;
						strafePos = targetinfo.ply.transform.position;
					}
					_targetList.Remove(targetinfo);
				}
			}
		}
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (activePlayer.HasPlayerFlag(BasePlayer.PlayerFlags.SafeZone) || Vector3Ex.Distance2D(base.transform.position, activePlayer.transform.position) > 150f)
			{
				continue;
			}
			bool flag4 = false;
			foreach (targetinfo target in _targetList)
			{
				if (target.ply == activePlayer)
				{
					flag4 = true;
					break;
				}
			}
			if (!flag4 && activePlayer.GetThreatLevel() > 0.5f && PlayerVisible(activePlayer))
			{
				_targetList.Add(new targetinfo(activePlayer, activePlayer));
			}
		}
		if (flag)
		{
			ExitCurrentState();
			State_Strafe_Enter(strafePos, shouldUseNapalm);
		}
	}

	public bool PlayerVisible(BasePlayer ply)
	{
		object obj = Interface.CallHook("CanHelicopterTarget", this, ply);
		if (obj is bool)
		{
			return (bool)obj;
		}
		Vector3 position = ply.eyes.position;
		if (TOD_Sky.Instance.IsNight && Vector3.Distance(position, interestZoneOrigin) > 40f)
		{
			return false;
		}
		Vector3 vector = base.transform.position - Vector3.up * 6f;
		float num = Vector3.Distance(position, vector);
		Vector3 normalized = (position - vector).normalized;
		RaycastHit hitInfo;
		if (GamePhysics.Trace(new Ray(vector + normalized * 5f, normalized), 0f, out hitInfo, num * 1.1f, 1218652417) && GameObjectEx.ToBaseEntity(hitInfo.collider.gameObject) == ply)
		{
			return true;
		}
		return false;
	}

	public void WasAttacked(HitInfo info)
	{
		BasePlayer basePlayer = info.Initiator as BasePlayer;
		if (basePlayer != null)
		{
			_targetList.Add(new targetinfo(basePlayer, basePlayer));
		}
	}

	public void Awake()
	{
		if (PatrolHelicopter.lifetimeMinutes == 0f)
		{
			Invoke(DestroyMe, 1f);
			return;
		}
		InvokeRepeating(UpdateWind, 0f, 1f / windFrequency);
		_lastPos = base.transform.position;
		spawnTime = UnityEngine.Time.realtimeSinceStartup;
		InitializeAI();
	}

	public void SetInitialDestination(Vector3 dest, float mapScaleDistance = 0.25f)
	{
		hasInterestZone = true;
		interestZoneOrigin = dest;
		float x = TerrainMeta.Size.x;
		float y = dest.y + 25f;
		Vector3 position = Vector3Ex.Range(-1f, 1f);
		position.y = 0f;
		position.Normalize();
		position *= x * mapScaleDistance;
		position.y = y;
		if (mapScaleDistance == 0f)
		{
			position = interestZoneOrigin + new Vector3(0f, 10f, 0f);
		}
		base.transform.position = position;
		ExitCurrentState();
		State_Move_Enter(dest);
	}

	public void Retire()
	{
		if (!isRetiring && Interface.CallHook("OnHelicopterRetire", this) == null)
		{
			isRetiring = true;
			Invoke(DestroyMe, 240f);
			float x = TerrainMeta.Size.x;
			float y = 200f;
			Vector3 newPos = Vector3Ex.Range(-1f, 1f);
			newPos.y = 0f;
			newPos.Normalize();
			newPos *= x * 20f;
			newPos.y = y;
			ExitCurrentState();
			State_Move_Enter(newPos);
		}
	}

	public void SetIdealRotation(Quaternion newTargetRot, float rotationSpeedOverride = -1f)
	{
		float num = ((rotationSpeedOverride == -1f) ? Mathf.Clamp01(moveSpeed / (maxSpeed * 0.5f)) : rotationSpeedOverride);
		rotationSpeed = num * maxRotationSpeed;
		targetRotation = newTargetRot;
	}

	public Quaternion GetYawRotationTo(Vector3 targetDest)
	{
		Vector3 vector = targetDest;
		vector.y = 0f;
		Vector3 position = base.transform.position;
		position.y = 0f;
		return Quaternion.LookRotation((vector - position).normalized);
	}

	public void SetTargetDestination(Vector3 targetDest, float minDist = 5f, float minDistForFacingRotation = 30f)
	{
		destination = targetDest;
		destination_min_dist = minDist;
		float num = Vector3.Distance(targetDest, base.transform.position);
		if (num > minDistForFacingRotation && !IsTargeting())
		{
			SetIdealRotation(GetYawRotationTo(destination));
		}
		targetThrottleSpeed = GetThrottleForDistance(num);
	}

	public bool AtDestination()
	{
		return Vector3.Distance(base.transform.position, destination) < destination_min_dist;
	}

	public void MoveToDestination()
	{
		Vector3 vector = (_lastMoveDir = Vector3.Lerp(_lastMoveDir, (destination - base.transform.position).normalized, UnityEngine.Time.deltaTime / courseAdjustLerpTime));
		throttleSpeed = Mathf.Lerp(throttleSpeed, targetThrottleSpeed, UnityEngine.Time.deltaTime / 3f);
		float num = throttleSpeed * maxSpeed;
		TerrainPushback();
		base.transform.position += vector * num * UnityEngine.Time.deltaTime;
		windVec = Vector3.Lerp(windVec, targetWindVec, UnityEngine.Time.deltaTime);
		base.transform.position += windVec * windForce * UnityEngine.Time.deltaTime;
		moveSpeed = Mathf.Lerp(moveSpeed, Vector3.Distance(_lastPos, base.transform.position) / UnityEngine.Time.deltaTime, UnityEngine.Time.deltaTime * 2f);
		_lastPos = base.transform.position;
	}

	public void TerrainPushback()
	{
		if (_currentState != aiState.DEATH)
		{
			Vector3 vector = base.transform.position + new Vector3(0f, 2f, 0f);
			Vector3 normalized = (destination - vector).normalized;
			float b = Vector3.Distance(destination, base.transform.position);
			Ray ray = new Ray(vector, normalized);
			float num = 5f;
			float num2 = Mathf.Min(100f, b);
			int mask = LayerMask.GetMask("Terrain", "World", "Construction");
			Vector3 b2 = Vector3.zero;
			RaycastHit hitInfo;
			if (UnityEngine.Physics.SphereCast(ray, num, out hitInfo, num2 - num * 0.5f, mask))
			{
				float num3 = 1f - hitInfo.distance / num2;
				float num4 = terrainPushForce * num3;
				b2 = Vector3.up * num4;
			}
			Ray ray2 = new Ray(vector, _lastMoveDir);
			float num5 = Mathf.Min(10f, b);
			RaycastHit hitInfo2;
			if (UnityEngine.Physics.SphereCast(ray2, num, out hitInfo2, num5 - num * 0.5f, mask))
			{
				float num6 = 1f - hitInfo2.distance / num5;
				float num7 = obstaclePushForce * num6;
				b2 += _lastMoveDir * num7 * -1f;
				b2 += Vector3.up * num7;
			}
			pushVec = Vector3.Lerp(pushVec, b2, UnityEngine.Time.deltaTime);
			base.transform.position += pushVec * UnityEngine.Time.deltaTime;
		}
	}

	public void UpdateRotation()
	{
		if (hasAimTarget)
		{
			Vector3 position = base.transform.position;
			position.y = 0f;
			Vector3 aimTarget = _aimTarget;
			aimTarget.y = 0f;
			Vector3 normalized = (aimTarget - position).normalized;
			Vector3 vector = Vector3.Cross(normalized, Vector3.up);
			float num = Vector3.Angle(normalized, base.transform.right);
			float num2 = Vector3.Angle(normalized, -base.transform.right);
			if (aimDoorSide)
			{
				if (num < num2)
				{
					targetRotation = Quaternion.LookRotation(vector);
				}
				else
				{
					targetRotation = Quaternion.LookRotation(-vector);
				}
			}
			else
			{
				targetRotation = Quaternion.LookRotation(normalized);
			}
		}
		rotationSpeed = Mathf.Lerp(rotationSpeed, maxRotationSpeed, UnityEngine.Time.deltaTime / 2f);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetRotation, rotationSpeed * UnityEngine.Time.deltaTime);
	}

	public void UpdateSpotlight()
	{
		if (hasInterestZone)
		{
			helicopterBase.spotlightTarget = new Vector3(interestZoneOrigin.x, TerrainMeta.HeightMap.GetHeight(interestZoneOrigin), interestZoneOrigin.z);
		}
		else
		{
			helicopterBase.spotlightTarget = Vector3.zero;
		}
	}

	public void Update()
	{
		if (helicopterBase.isClient)
		{
			return;
		}
		heliInstance = this;
		UpdateTargetList();
		MoveToDestination();
		UpdateRotation();
		UpdateSpotlight();
		AIThink();
		DoMachineGuns();
		if (!isRetiring)
		{
			float num = Mathf.Max(spawnTime + PatrolHelicopter.lifetimeMinutes * 60f, lastDamageTime + 120f);
			if (UnityEngine.Time.realtimeSinceStartup > num)
			{
				Retire();
			}
		}
	}

	public void WeakspotDamaged(BaseHelicopter.weakspot weak, HitInfo info)
	{
		float num = UnityEngine.Time.realtimeSinceStartup - lastDamageTime;
		lastDamageTime = UnityEngine.Time.realtimeSinceStartup;
		BasePlayer basePlayer = info.Initiator as BasePlayer;
		bool num2 = ValidStrafeTarget(basePlayer);
		bool flag = num2 && CanStrafe();
		bool flag2 = !num2 && CanUseNapalm();
		if (num < 5f && basePlayer != null && (flag || flag2))
		{
			ExitCurrentState();
			State_Strafe_Enter(info.Initiator.transform.position, flag2);
		}
	}

	public void CriticalDamage()
	{
		isDead = true;
		ExitCurrentState();
		State_Death_Enter();
	}

	public void DoMachineGuns()
	{
		if (_targetList.Count > 0)
		{
			if (leftGun.NeedsNewTarget())
			{
				leftGun.UpdateTargetFromList(_targetList);
			}
			if (rightGun.NeedsNewTarget())
			{
				rightGun.UpdateTargetFromList(_targetList);
			}
		}
		leftGun.TurretThink();
		rightGun.TurretThink();
	}

	public void FireGun(Vector3 targetPos, float aimCone, bool left)
	{
		if (PatrolHelicopter.guns == 0)
		{
			return;
		}
		Vector3 position = (left ? helicopterBase.left_gun_muzzle.transform : helicopterBase.right_gun_muzzle.transform).position;
		Vector3 normalized = (targetPos - position).normalized;
		position += normalized * 2f;
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(aimCone, normalized);
		RaycastHit hitInfo;
		if (GamePhysics.Trace(new Ray(position, modifiedAimConeDirection), 0f, out hitInfo, 300f, 1219701521))
		{
			targetPos = hitInfo.point;
			if ((bool)hitInfo.collider)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
				if ((bool)entity && entity != helicopterBase)
				{
					BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
					HitInfo info = new HitInfo(helicopterBase, entity, DamageType.Bullet, helicopterBase.bulletDamage * PatrolHelicopter.bulletDamageScale, hitInfo.point);
					if ((bool)baseCombatEntity)
					{
						baseCombatEntity.OnAttacked(info);
						if (baseCombatEntity is BasePlayer)
						{
							Effect.server.ImpactEffect(new HitInfo
							{
								HitPositionWorld = hitInfo.point - modifiedAimConeDirection * 0.25f,
								HitNormalWorld = -modifiedAimConeDirection,
								HitMaterial = StringPool.Get("Flesh")
							});
						}
					}
					else
					{
						entity.OnAttacked(info);
					}
				}
			}
		}
		else
		{
			targetPos = position + modifiedAimConeDirection * 300f;
		}
		helicopterBase.ClientRPC(null, "FireGun", left, targetPos);
	}

	public bool CanInterruptState()
	{
		if (_currentState != aiState.STRAFE)
		{
			return _currentState != aiState.DEATH;
		}
		return false;
	}

	public bool IsAlive()
	{
		return !isDead;
	}

	public void DestroyMe()
	{
		helicopterBase.Kill();
	}

	public Vector3 GetLastMoveDir()
	{
		return _lastMoveDir;
	}

	public Vector3 GetMoveDirection()
	{
		return (destination - base.transform.position).normalized;
	}

	public float GetMoveSpeed()
	{
		return moveSpeed;
	}

	public float GetMaxRotationSpeed()
	{
		return maxRotationSpeed;
	}

	public bool IsTargeting()
	{
		return hasAimTarget;
	}

	public void UpdateWind()
	{
		targetWindVec = UnityEngine.Random.onUnitSphere;
	}

	public void SetAimTarget(Vector3 aimTarg, bool isDoorSide)
	{
		if (!movementLockingAiming)
		{
			hasAimTarget = true;
			_aimTarget = aimTarg;
			aimDoorSide = isDoorSide;
		}
	}

	public void ClearAimTarget()
	{
		hasAimTarget = false;
		_aimTarget = Vector3.zero;
	}

	public void State_Death_Think(float timePassed)
	{
		float num = UnityEngine.Time.realtimeSinceStartup * 0.25f;
		float x = Mathf.Sin((float)Math.PI * 2f * num) * 10f;
		float z = Mathf.Cos((float)Math.PI * 2f * num) * 10f;
		Vector3 vector = new Vector3(x, 0f, z);
		SetAimTarget(base.transform.position + vector, true);
		Ray ray = new Ray(base.transform.position, GetLastMoveDir());
		int mask = LayerMask.GetMask("Terrain", "World", "Construction", "Water");
		RaycastHit hitInfo;
		if (UnityEngine.Physics.SphereCast(ray, 3f, out hitInfo, 5f, mask) || UnityEngine.Time.realtimeSinceStartup > deathTimeout)
		{
			helicopterBase.Hurt(helicopterBase.health * 2f, DamageType.Generic, null, false);
		}
	}

	public void State_Death_Enter()
	{
		maxRotationSpeed *= 8f;
		_currentState = aiState.DEATH;
		Vector3 randomOffset = GetRandomOffset(base.transform.position, 20f, 60f);
		int num = 1236478737;
		Vector3 pos;
		Vector3 normal;
		TransformUtil.GetGroundInfo(randomOffset - Vector3.up * 2f, out pos, out normal, 500f, num);
		SetTargetDestination(pos);
		targetThrottleSpeed = 0.5f;
		deathTimeout = UnityEngine.Time.realtimeSinceStartup + 10f;
	}

	public void State_Death_Leave()
	{
	}

	public void State_Idle_Think(float timePassed)
	{
		ExitCurrentState();
		State_Patrol_Enter();
	}

	public void State_Idle_Enter()
	{
		_currentState = aiState.IDLE;
	}

	public void State_Idle_Leave()
	{
	}

	public void State_Move_Think(float timePassed)
	{
		float distToTarget = Vector3.Distance(base.transform.position, destination);
		targetThrottleSpeed = GetThrottleForDistance(distToTarget);
		if (AtDestination())
		{
			ExitCurrentState();
			State_Idle_Enter();
		}
	}

	public void State_Move_Enter(Vector3 newPos)
	{
		_currentState = aiState.MOVE;
		destination_min_dist = 5f;
		SetTargetDestination(newPos);
		float distToTarget = Vector3.Distance(base.transform.position, destination);
		targetThrottleSpeed = GetThrottleForDistance(distToTarget);
	}

	public void State_Move_Leave()
	{
	}

	public void State_Orbit_Think(float timePassed)
	{
		if (breakingOrbit)
		{
			if (AtDestination())
			{
				ExitCurrentState();
				State_Idle_Enter();
			}
		}
		else
		{
			if (Vector3Ex.Distance2D(base.transform.position, destination) > 15f)
			{
				return;
			}
			if (!hasEnteredOrbit)
			{
				hasEnteredOrbit = true;
				orbitStartTime = UnityEngine.Time.realtimeSinceStartup;
			}
			float num = (float)Math.PI * 2f * currentOrbitDistance;
			float num2 = 0.5f * maxSpeed;
			float num3 = num / num2;
			currentOrbitTime += timePassed / (num3 * 1.01f);
			float rate = currentOrbitTime;
			Vector3 orbitPosition = GetOrbitPosition(rate);
			ClearAimTarget();
			SetTargetDestination(orbitPosition, 0f, 1f);
			targetThrottleSpeed = 0.5f;
		}
		if (UnityEngine.Time.realtimeSinceStartup - orbitStartTime > maxOrbitDuration && !breakingOrbit)
		{
			breakingOrbit = true;
			Vector3 appropriatePosition = GetAppropriatePosition(base.transform.position + base.transform.forward * 75f, 40f, 50f);
			SetTargetDestination(appropriatePosition, 10f, 0f);
		}
	}

	public Vector3 GetOrbitPosition(float rate)
	{
		float x = Mathf.Sin((float)Math.PI * 2f * rate) * currentOrbitDistance;
		float z = Mathf.Cos((float)Math.PI * 2f * rate) * currentOrbitDistance;
		Vector3 vector = new Vector3(x, 20f, z);
		return interestZoneOrigin + vector;
	}

	public void State_Orbit_Enter(float orbitDistance)
	{
		_currentState = aiState.ORBIT;
		breakingOrbit = false;
		hasEnteredOrbit = false;
		orbitStartTime = UnityEngine.Time.realtimeSinceStartup;
		Vector3 vector = base.transform.position - interestZoneOrigin;
		currentOrbitTime = Mathf.Atan2(vector.x, vector.z);
		currentOrbitDistance = orbitDistance;
		ClearAimTarget();
		SetTargetDestination(GetOrbitPosition(currentOrbitTime), 20f, 0f);
	}

	public void State_Orbit_Leave()
	{
		breakingOrbit = false;
		hasEnteredOrbit = false;
		currentOrbitTime = 0f;
		ClearAimTarget();
	}

	public Vector3 GetRandomPatrolDestination()
	{
		Vector3 vector = Vector3.zero;
		if (TerrainMeta.Path != null && TerrainMeta.Path.Monuments != null && TerrainMeta.Path.Monuments.Count > 0)
		{
			MonumentInfo monumentInfo = null;
			if (_visitedMonuments.Count > 0)
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					bool flag = false;
					foreach (MonumentInfo visitedMonument in _visitedMonuments)
					{
						if (monument == visitedMonument)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						monumentInfo = monument;
						break;
					}
				}
			}
			if (monumentInfo == null)
			{
				_visitedMonuments.Clear();
				monumentInfo = TerrainMeta.Path.Monuments[UnityEngine.Random.Range(0, TerrainMeta.Path.Monuments.Count)];
			}
			if ((bool)monumentInfo)
			{
				vector = monumentInfo.transform.position;
				_visitedMonuments.Add(monumentInfo);
				vector.y = TerrainMeta.HeightMap.GetHeight(vector) + 200f;
				RaycastHit hitOut;
				if (TransformUtil.GetGroundInfo(vector, out hitOut, 300f, 1235288065))
				{
					vector.y = hitOut.point.y;
				}
				vector.y += 30f;
			}
		}
		else
		{
			float x = TerrainMeta.Size.x;
			float y = 30f;
			vector = Vector3Ex.Range(-1f, 1f);
			vector.y = 0f;
			vector.Normalize();
			vector *= x * UnityEngine.Random.Range(0f, 0.75f);
			vector.y = y;
		}
		return vector;
	}

	public void State_Patrol_Think(float timePassed)
	{
		float num = Vector3.Distance(base.transform.position, destination);
		if (num <= 25f)
		{
			targetThrottleSpeed = GetThrottleForDistance(num);
		}
		else
		{
			targetThrottleSpeed = 0.5f;
		}
		if (AtDestination() && arrivalTime == 0f)
		{
			arrivalTime = UnityEngine.Time.realtimeSinceStartup;
			ExitCurrentState();
			maxOrbitDuration = 20f;
			State_Orbit_Enter(75f);
		}
		if (_targetList.Count > 0)
		{
			interestZoneOrigin = _targetList[0].ply.transform.position + new Vector3(0f, 20f, 0f);
			ExitCurrentState();
			maxOrbitDuration = 10f;
			State_Orbit_Enter(75f);
		}
	}

	public void State_Patrol_Enter()
	{
		_currentState = aiState.PATROL;
		Vector3 randomPatrolDestination = GetRandomPatrolDestination();
		SetTargetDestination(randomPatrolDestination, 10f);
		interestZoneOrigin = randomPatrolDestination;
		arrivalTime = 0f;
	}

	public void State_Patrol_Leave()
	{
	}

	public int ClipRocketsLeft()
	{
		return numRocketsLeft;
	}

	public bool CanStrafe()
	{
		object obj = Interface.CallHook("CanHelicopterStrafe", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (UnityEngine.Time.realtimeSinceStartup - lastStrafeTime >= 20f)
		{
			return CanInterruptState();
		}
		return false;
	}

	public bool CanUseNapalm()
	{
		object obj = Interface.CallHook("CanHelicopterUseNapalm", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return UnityEngine.Time.realtimeSinceStartup - lastNapalmTime >= 30f;
	}

	public void State_Strafe_Enter(Vector3 strafePos, bool shouldUseNapalm = false)
	{
		if (Interface.CallHook("OnHelicopterStrafeEnter", this, strafePos) == null)
		{
			if (CanUseNapalm() && shouldUseNapalm)
			{
				useNapalm = shouldUseNapalm;
				lastNapalmTime = UnityEngine.Time.realtimeSinceStartup;
			}
			lastStrafeTime = UnityEngine.Time.realtimeSinceStartup;
			_currentState = aiState.STRAFE;
			int mask = LayerMask.GetMask("Terrain", "World", "Construction", "Water");
			Vector3 pos;
			Vector3 normal;
			if (TransformUtil.GetGroundInfo(strafePos, out pos, out normal, 100f, mask, base.transform))
			{
				strafe_target_position = pos;
			}
			else
			{
				strafe_target_position = strafePos;
			}
			numRocketsLeft = 12;
			lastRocketTime = 0f;
			movementLockingAiming = true;
			Vector3 randomOffset = GetRandomOffset(strafePos, 175f, 192.5f);
			SetTargetDestination(randomOffset, 10f);
			SetIdealRotation(GetYawRotationTo(randomOffset));
			puttingDistance = true;
		}
	}

	public void State_Strafe_Think(float timePassed)
	{
		if (puttingDistance)
		{
			if (AtDestination())
			{
				puttingDistance = false;
				SetTargetDestination(strafe_target_position + new Vector3(0f, 40f, 0f), 10f);
				SetIdealRotation(GetYawRotationTo(strafe_target_position));
			}
			return;
		}
		SetIdealRotation(GetYawRotationTo(strafe_target_position));
		float num = Vector3Ex.Distance2D(strafe_target_position, base.transform.position);
		if (num <= 150f && ClipRocketsLeft() > 0 && UnityEngine.Time.realtimeSinceStartup - lastRocketTime > timeBetweenRockets)
		{
			float num2 = Vector3.Distance(strafe_target_position, base.transform.position) - 10f;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			if (!UnityEngine.Physics.Raycast(base.transform.position, (strafe_target_position - base.transform.position).normalized, num2, LayerMask.GetMask("Terrain", "World")))
			{
				FireRocket();
			}
		}
		if (ClipRocketsLeft() <= 0 || num <= 15f)
		{
			ExitCurrentState();
			State_Move_Enter(GetAppropriatePosition(strafe_target_position + base.transform.forward * 120f));
		}
	}

	public bool ValidStrafeTarget(BasePlayer ply)
	{
		object obj = Interface.CallHook("CanHelicopterStrafeTarget", this, ply);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return !ply.IsNearEnemyBase();
	}

	public void State_Strafe_Leave()
	{
		lastStrafeTime = UnityEngine.Time.realtimeSinceStartup;
		if (useNapalm)
		{
			lastNapalmTime = UnityEngine.Time.realtimeSinceStartup;
		}
		useNapalm = false;
		movementLockingAiming = false;
	}

	public void FireRocket()
	{
		numRocketsLeft--;
		lastRocketTime = UnityEngine.Time.realtimeSinceStartup;
		float num = 4f;
		bool flag = leftTubeFiredLast;
		leftTubeFiredLast = !leftTubeFiredLast;
		Transform transform = (flag ? helicopterBase.rocket_tube_left.transform : helicopterBase.rocket_tube_right.transform);
		Vector3 vector = transform.position + transform.forward * 1f;
		Vector3 vector2 = (strafe_target_position - vector).normalized;
		if (num > 0f)
		{
			vector2 = AimConeUtil.GetModifiedAimConeDirection(num, vector2);
		}
		float maxDistance = 1f;
		RaycastHit hitInfo;
		if (UnityEngine.Physics.Raycast(vector, vector2, out hitInfo, maxDistance, 1236478737))
		{
			maxDistance = hitInfo.distance - 0.1f;
		}
		Effect.server.Run(helicopterBase.rocket_fire_effect.resourcePath, helicopterBase, StringPool.Get(flag ? "rocket_tube_left" : "rocket_tube_right"), Vector3.zero, Vector3.forward, null, true);
		BaseEntity baseEntity = GameManager.server.CreateEntity(useNapalm ? rocketProjectile_Napalm.resourcePath : rocketProjectile.resourcePath, vector);
		if (!(baseEntity == null))
		{
			ServerProjectile component = baseEntity.GetComponent<ServerProjectile>();
			if ((bool)component)
			{
				component.InitializeVelocity(vector2 * component.speed);
			}
			baseEntity.Spawn();
		}
	}

	public void InitializeAI()
	{
		_lastThinkTime = UnityEngine.Time.realtimeSinceStartup;
	}

	public void OnCurrentStateExit()
	{
		switch (_currentState)
		{
		default:
			State_Idle_Leave();
			break;
		case aiState.MOVE:
			State_Move_Leave();
			break;
		case aiState.STRAFE:
			State_Strafe_Leave();
			break;
		case aiState.ORBIT:
			State_Orbit_Leave();
			break;
		case aiState.PATROL:
			State_Patrol_Leave();
			break;
		}
	}

	public void ExitCurrentState()
	{
		OnCurrentStateExit();
		_currentState = aiState.IDLE;
	}

	public float GetTime()
	{
		return UnityEngine.Time.realtimeSinceStartup;
	}

	public void AIThink()
	{
		float time = GetTime();
		float timePassed = time - _lastThinkTime;
		_lastThinkTime = time;
		switch (_currentState)
		{
		default:
			State_Idle_Think(timePassed);
			break;
		case aiState.MOVE:
			State_Move_Think(timePassed);
			break;
		case aiState.STRAFE:
			State_Strafe_Think(timePassed);
			break;
		case aiState.ORBIT:
			State_Orbit_Think(timePassed);
			break;
		case aiState.PATROL:
			State_Patrol_Think(timePassed);
			break;
		case aiState.DEATH:
			State_Death_Think(timePassed);
			break;
		}
	}

	public Vector3 GetRandomOffset(Vector3 origin, float minRange, float maxRange = 0f, float minHeight = 20f, float maxHeight = 30f)
	{
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		onUnitSphere.y = 0f;
		onUnitSphere.Normalize();
		maxRange = Mathf.Max(minRange, maxRange);
		Vector3 origin2 = origin + onUnitSphere * UnityEngine.Random.Range(minRange, maxRange);
		return GetAppropriatePosition(origin2, minHeight, maxHeight);
	}

	public Vector3 GetAppropriatePosition(Vector3 origin, float minHeight = 20f, float maxHeight = 30f)
	{
		float num = 100f;
		Ray ray = new Ray(origin + new Vector3(0f, num, 0f), Vector3.down);
		float num2 = 5f;
		int mask = LayerMask.GetMask("Terrain", "World", "Construction", "Water");
		RaycastHit hitInfo;
		if (UnityEngine.Physics.SphereCast(ray, num2, out hitInfo, num * 2f - num2, mask))
		{
			origin = hitInfo.point;
		}
		origin.y += UnityEngine.Random.Range(minHeight, maxHeight);
		return origin;
	}

	public float GetThrottleForDistance(float distToTarget)
	{
		float num = 0f;
		if (distToTarget >= 75f)
		{
			return 1f;
		}
		if (distToTarget >= 50f)
		{
			return 0.75f;
		}
		if (distToTarget >= 25f)
		{
			return 0.33f;
		}
		if (distToTarget >= 5f)
		{
			return 0.05f;
		}
		return 0.05f * (1f - distToTarget / 5f);
	}
}
