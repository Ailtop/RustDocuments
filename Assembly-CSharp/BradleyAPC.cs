using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.AI;
using UnityEngine;
using UnityEngine.AI;

public class BradleyAPC : BaseCombatEntity, TriggerHurtNotChild.IHurtTriggerUser
{
	[Serializable]
	public class TargetInfo : Facepunch.Pool.IPooled
	{
		public float damageReceivedFrom;

		public BaseEntity entity;

		public float lastSeenTime;

		public Vector3 lastSeenPosition;

		public void EnterPool()
		{
			entity = null;
			lastSeenPosition = Vector3.zero;
			lastSeenTime = 0f;
		}

		public void Setup(BaseEntity ent, float time)
		{
			entity = ent;
			lastSeenTime = time;
		}

		public void LeavePool()
		{
		}

		public float GetPriorityScore(BradleyAPC apc)
		{
			BasePlayer basePlayer = entity as BasePlayer;
			if ((bool)basePlayer)
			{
				float value = Vector3.Distance(entity.transform.position, apc.transform.position);
				float num = (1f - Mathf.InverseLerp(10f, 80f, value)) * 50f;
				float value2 = ((basePlayer.GetHeldEntity() == null) ? 0f : basePlayer.GetHeldEntity().hostileScore);
				float num2 = Mathf.InverseLerp(4f, 20f, value2) * 100f;
				float num3 = Mathf.InverseLerp(10f, 3f, UnityEngine.Time.time - lastSeenTime) * 100f;
				float num4 = Mathf.InverseLerp(0f, 100f, damageReceivedFrom) * 50f;
				return num + num2 + num4 + num3;
			}
			return 0f;
		}

		public bool IsVisible()
		{
			if (lastSeenTime != -1f)
			{
				return UnityEngine.Time.time - lastSeenTime < sightUpdateRate * 2f;
			}
			return false;
		}

		public bool IsValid()
		{
			return entity != null;
		}
	}

	[Header("Sound")]
	public BlendedLoopEngineSound engineSound;

	public SoundDefinition treadLoopDef;

	public AnimationCurve treadGainCurve;

	public AnimationCurve treadPitchCurve;

	public AnimationCurve treadFreqCurve;

	private Sound treadLoop;

	private SoundModulation.Modulator treadGain;

	private SoundModulation.Modulator treadPitch;

	public SoundDefinition chasisLurchSoundDef;

	public float chasisLurchAngleDelta = 2f;

	public float chasisLurchSpeedDelta = 2f;

	public float lastAngle;

	public float lastSpeed;

	public SoundDefinition turretTurnLoopDef;

	public float turretLoopGainSpeed = 3f;

	public float turretLoopPitchSpeed = 3f;

	public float turretLoopMinAngleDelta;

	public float turretLoopMaxAngleDelta = 10f;

	public float turretLoopPitchMin = 0.5f;

	public float turretLoopPitchMax = 1f;

	public float turretLoopGainThreshold = 0.0001f;

	private Sound turretTurnLoop;

	private SoundModulation.Modulator turretTurnLoopGain;

	private SoundModulation.Modulator turretTurnLoopPitch;

	public float enginePitch = 0.9f;

	public float rpmMultiplier = 0.6f;

	private TreadAnimator treadAnimator;

	[Header("Wheels")]
	public WheelCollider[] leftWheels;

	public WheelCollider[] rightWheels;

	[Header("Movement Config")]
	public float moveForceMax = 2000f;

	public float brakeForce = 100f;

	public float turnForce = 2000f;

	public float sideStiffnessMax = 1f;

	public float sideStiffnessMin = 0.5f;

	public Transform centerOfMass;

	public float stoppingDist = 5f;

	[Header("Control")]
	public float throttle = 1f;

	public float turning;

	public float rightThrottle;

	public float leftThrottle;

	public bool brake;

	[Header("Other")]
	public Rigidbody myRigidBody;

	public Collider myCollider;

	public Vector3 destination;

	public Vector3 finalDestination;

	public Transform followTest;

	public TriggerHurtEx impactDamager;

	[Header("Weapons")]
	public Transform mainTurretEyePos;

	public Transform mainTurret;

	public Transform CannonPitch;

	public Transform CannonMuzzle;

	public Transform coaxPitch;

	public Transform coaxMuzzle;

	public Transform topTurretEyePos;

	public Transform topTurretYaw;

	public Transform topTurretPitch;

	public Transform topTurretMuzzle;

	public Vector3 turretAimVector = Vector3.forward;

	public Vector3 desiredAimVector = Vector3.forward;

	public Vector3 topTurretAimVector = Vector3.forward;

	public Vector3 desiredTopTurretAimVector = Vector3.forward;

	[Header("Effects")]
	public GameObjectRef explosionEffect;

	public GameObjectRef servergibs;

	public GameObjectRef fireBall;

	public GameObjectRef crateToDrop;

	public GameObjectRef debrisFieldMarker;

	[Header("Loot")]
	public int maxCratesToSpawn;

	public int patrolPathIndex;

	public IAIPath patrolPath;

	public bool DoAI = true;

	public GameObjectRef mainCannonMuzzleFlash;

	public GameObjectRef mainCannonProjectile;

	public float recoilScale = 200f;

	public NavMeshPath navMeshPath;

	public int navMeshPathIndex;

	private LayerMask obstacleHitMask;

	private TimeSince timeSinceSeemingStuck;

	private TimeSince timeSinceStuckReverseStart;

	private const string prefabPath = "assets/prefabs/npc/m2bradley/bradleyapc.prefab";

	public float nextFireTime = 10f;

	public int numBursted;

	public float nextPatrolTime;

	public float nextEngagementPathTime;

	public float currentSpeedZoneLimit;

	[Header("Pathing")]
	public List<Vector3> currentPath;

	public int currentPathIndex;

	public bool pathLooping;

	[Header("Targeting")]
	public float viewDistance = 100f;

	public float searchRange = 100f;

	public float searchFrequency = 2f;

	public float memoryDuration = 20f;

	public static float sightUpdateRate = 0.5f;

	public List<TargetInfo> targetList = new List<TargetInfo>();

	public BaseCombatEntity mainGunTarget;

	[Header("Coax")]
	public float coaxFireRate = 0.06667f;

	public int coaxBurstLength = 10;

	public float coaxAimCone = 3f;

	public float bulletDamage = 15f;

	[Header("TopTurret")]
	public float topTurretFireRate = 0.25f;

	public float nextCoaxTime;

	public int numCoaxBursted;

	public float nextTopTurretTime = 0.3f;

	public GameObjectRef gun_fire_effect;

	public GameObjectRef bulletEffect;

	public float lastLateUpdate;

	public override float PositionTickRate
	{
		protected get
		{
			return 0.1f;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BradleyAPC.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.bradley != null && !info.fromDisk)
		{
			throttle = info.msg.bradley.engineThrottle;
			rightThrottle = info.msg.bradley.throttleRight;
			leftThrottle = info.msg.bradley.throttleLeft;
			desiredAimVector = info.msg.bradley.mainGunVec;
			desiredTopTurretAimVector = info.msg.bradley.topTurretVec;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.bradley = Facepunch.Pool.Get<ProtoBuf.BradleyAPC>();
			info.msg.bradley.engineThrottle = throttle;
			info.msg.bradley.throttleLeft = leftThrottle;
			info.msg.bradley.throttleRight = rightThrottle;
			info.msg.bradley.mainGunVec = turretAimVector;
			info.msg.bradley.topTurretVec = topTurretAimVector;
		}
	}

	public static BradleyAPC SpawnRoadDrivingBradley(Vector3 spawnPos, Quaternion spawnRot)
	{
		RuntimePath runtimePath = new RuntimePath();
		PathList pathList = null;
		float num = float.PositiveInfinity;
		foreach (PathList road in TerrainMeta.Path.Roads)
		{
			_ = Vector3.zero;
			float num2 = float.PositiveInfinity;
			Vector3[] points = road.Path.Points;
			foreach (Vector3 a in points)
			{
				float num3 = Vector3.Distance(a, spawnPos);
				if (num3 < num2)
				{
					num2 = num3;
				}
			}
			if (num2 < num)
			{
				pathList = road;
				num = num2;
			}
		}
		if (pathList == null)
		{
			return null;
		}
		Vector3 startPoint = pathList.Path.GetStartPoint();
		Vector3 endPoint = pathList.Path.GetEndPoint();
		bool flag = startPoint == endPoint;
		int num4 = (flag ? (pathList.Path.Points.Length - 1) : pathList.Path.Points.Length);
		IAIPathNode[] nodes = new RuntimePathNode[num4];
		runtimePath.Nodes = nodes;
		IAIPathNode iAIPathNode = null;
		int num5 = 0;
		int num6 = (flag ? (pathList.Path.MaxIndex - 1) : pathList.Path.MaxIndex);
		for (int j = pathList.Path.MinIndex; j <= num6; j++)
		{
			IAIPathNode iAIPathNode2 = new RuntimePathNode(pathList.Path.Points[j] + Vector3.up * 1f);
			if (iAIPathNode != null)
			{
				iAIPathNode2.AddLink(iAIPathNode);
				iAIPathNode.AddLink(iAIPathNode2);
			}
			runtimePath.Nodes[num5] = iAIPathNode2;
			iAIPathNode = iAIPathNode2;
			num5++;
		}
		if (flag)
		{
			runtimePath.Nodes[0].AddLink(runtimePath.Nodes[runtimePath.Nodes.Length - 1]);
			runtimePath.Nodes[runtimePath.Nodes.Length - 1].AddLink(runtimePath.Nodes[0]);
		}
		else
		{
			RuntimeInterestNode interestNode = new RuntimeInterestNode(startPoint + Vector3.up * 1f);
			runtimePath.AddInterestNode(interestNode);
			RuntimeInterestNode interestNode2 = new RuntimeInterestNode(endPoint + Vector3.up * 1f);
			runtimePath.AddInterestNode(interestNode2);
		}
		int value = Mathf.CeilToInt(pathList.Path.Length / 500f);
		value = Mathf.Clamp(value, 1, 3);
		if (flag)
		{
			value++;
		}
		for (int k = 0; k < value; k++)
		{
			int num7 = UnityEngine.Random.Range(0, pathList.Path.Points.Length);
			RuntimeInterestNode interestNode3 = new RuntimeInterestNode(pathList.Path.Points[num7] + Vector3.up * 1f);
			runtimePath.AddInterestNode(interestNode3);
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/m2bradley/bradleyapc.prefab", spawnPos, spawnRot);
		BradleyAPC bradleyAPC = null;
		if ((bool)baseEntity)
		{
			bradleyAPC = baseEntity.GetComponent<BradleyAPC>();
			if ((bool)bradleyAPC)
			{
				bradleyAPC.Spawn();
				bradleyAPC.InstallPatrolPath(runtimePath);
			}
			else
			{
				baseEntity.Kill();
			}
		}
		return bradleyAPC;
	}

	[ServerVar(Name = "spawnroadbradley")]
	public static string svspawnroadbradley(Vector3 pos, Vector3 dir)
	{
		if (!(SpawnRoadDrivingBradley(pos, Quaternion.LookRotation(dir, Vector3.up)) != null))
		{
			return "Failed to spawn road-driving Bradley.";
		}
		return "Spawned road-driving Bradley.";
	}

	public void SetDestination(Vector3 dest)
	{
		destination = dest;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Initialize();
		InvokeRepeating(UpdateTargetList, 0f, 2f);
		InvokeRepeating(UpdateTargetVisibilities, 0f, sightUpdateRate);
		obstacleHitMask = LayerMask.GetMask("Vehicle World");
		timeSinceSeemingStuck = 0f;
		timeSinceStuckReverseStart = float.MaxValue;
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
	}

	public void Initialize()
	{
		if (Interface.CallHook("OnBradleyApcInitialize", this) == null)
		{
			myRigidBody.centerOfMass = centerOfMass.localPosition;
			destination = base.transform.position;
			finalDestination = base.transform.position;
		}
	}

	public BasePlayer FollowPlayer()
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (activePlayer.IsAdmin && activePlayer.IsAlive() && !activePlayer.IsSleeping() && activePlayer.GetActiveItem() != null && activePlayer.GetActiveItem().info.shortname == "tool.binoculars")
			{
				return activePlayer;
			}
		}
		return null;
	}

	public static Vector3 Direction2D(Vector3 aimAt, Vector3 aimFrom)
	{
		return (new Vector3(aimAt.x, 0f, aimAt.z) - new Vector3(aimFrom.x, 0f, aimFrom.z)).normalized;
	}

	public bool IsAtDestination()
	{
		return Vector3Ex.Distance2D(base.transform.position, destination) <= stoppingDist;
	}

	public bool IsAtFinalDestination()
	{
		return Vector3Ex.Distance2D(base.transform.position, finalDestination) <= stoppingDist;
	}

	public Vector3 ClosestPointAlongPath(Vector3 start, Vector3 end, Vector3 fromPos)
	{
		Vector3 vector = end - start;
		Vector3 rhs = fromPos - start;
		float num = Vector3.Dot(vector, rhs);
		float num2 = Vector3.SqrMagnitude(end - start);
		float num3 = Mathf.Clamp01(num / num2);
		return start + vector * num3;
	}

	public void FireGunTest()
	{
		if (UnityEngine.Time.time < nextFireTime)
		{
			return;
		}
		nextFireTime = UnityEngine.Time.time + 0.25f;
		numBursted++;
		if (numBursted >= 4)
		{
			nextFireTime = UnityEngine.Time.time + 5f;
			numBursted = 0;
		}
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(2f, CannonMuzzle.rotation * Vector3.forward);
		Vector3 normalized = (CannonPitch.transform.rotation * Vector3.back + base.transform.up * -1f).normalized;
		myRigidBody.AddForceAtPosition(normalized * recoilScale, CannonPitch.transform.position, ForceMode.Impulse);
		Effect.server.Run(mainCannonMuzzleFlash.resourcePath, this, StringPool.Get(CannonMuzzle.gameObject.name), Vector3.zero, Vector3.zero);
		BaseEntity baseEntity = GameManager.server.CreateEntity(mainCannonProjectile.resourcePath, CannonMuzzle.transform.position, Quaternion.LookRotation(modifiedAimConeDirection));
		if (!(baseEntity == null))
		{
			ServerProjectile component = baseEntity.GetComponent<ServerProjectile>();
			if ((bool)component)
			{
				component.InitializeVelocity(modifiedAimConeDirection * component.speed);
			}
			baseEntity.Spawn();
		}
	}

	public void InstallPatrolPath(IAIPath path)
	{
		patrolPath = path;
		currentPath = new List<Vector3>();
		currentPathIndex = -1;
	}

	public void UpdateMovement_Patrol()
	{
		if (patrolPath == null || UnityEngine.Time.time < nextPatrolTime)
		{
			return;
		}
		nextPatrolTime = UnityEngine.Time.time + 20f;
		if ((HasPath() && !IsAtFinalDestination()) || Interface.CallHook("OnBradleyApcPatrol", this) != null)
		{
			return;
		}
		IAIPathInterestNode randomInterestNodeAwayFrom = patrolPath.GetRandomInterestNodeAwayFrom(base.transform.position);
		IAIPathNode closestToPoint = patrolPath.GetClosestToPoint(randomInterestNodeAwayFrom.Position);
		bool flag = false;
		List<IAIPathNode> nodes = Facepunch.Pool.GetList<IAIPathNode>();
		IAIPathNode iAIPathNode;
		if (GetEngagementPath(ref nodes))
		{
			flag = true;
			iAIPathNode = nodes[nodes.Count - 1];
		}
		else
		{
			iAIPathNode = patrolPath.GetClosestToPoint(base.transform.position);
		}
		if (!(Vector3.Distance(finalDestination, closestToPoint.Position) > 2f))
		{
			return;
		}
		if (closestToPoint == iAIPathNode)
		{
			currentPath.Clear();
			currentPath.Add(closestToPoint.Position);
			currentPathIndex = -1;
			pathLooping = false;
			finalDestination = closestToPoint.Position;
		}
		else
		{
			if (!AStarPath.FindPath(iAIPathNode, closestToPoint, out var path, out var _))
			{
				return;
			}
			currentPath.Clear();
			if (flag)
			{
				for (int i = 0; i < nodes.Count - 1; i++)
				{
					currentPath.Add(nodes[i].Position);
				}
			}
			foreach (IAIPathNode item in path)
			{
				currentPath.Add(item.Position);
			}
			currentPathIndex = -1;
			pathLooping = false;
			finalDestination = closestToPoint.Position;
		}
	}

	public void UpdateMovement_Hunt()
	{
		if (Interface.CallHook("OnBradleyApcHunt", this) != null || patrolPath == null)
		{
			return;
		}
		TargetInfo targetInfo = targetList[0];
		if (!targetInfo.IsValid())
		{
			return;
		}
		if (HasPath() && targetInfo.IsVisible())
		{
			if (currentPath.Count > 1)
			{
				Vector3 item = currentPath[currentPathIndex];
				ClearPath();
				currentPath.Add(item);
				finalDestination = item;
				currentPathIndex = 0;
			}
		}
		else
		{
			if (!(UnityEngine.Time.time > nextEngagementPathTime) || HasPath() || targetInfo.IsVisible())
			{
				return;
			}
			bool flag = false;
			IAIPathNode start = patrolPath.GetClosestToPoint(base.transform.position);
			List<IAIPathNode> nodes = Facepunch.Pool.GetList<IAIPathNode>();
			if (GetEngagementPath(ref nodes))
			{
				flag = true;
				start = nodes[nodes.Count - 1];
			}
			IAIPathNode iAIPathNode = null;
			List<IAIPathNode> nearNodes = Facepunch.Pool.GetList<IAIPathNode>();
			patrolPath.GetNodesNear(targetInfo.lastSeenPosition, ref nearNodes, 30f);
			Stack<IAIPathNode> stack = null;
			float num = float.PositiveInfinity;
			float y = mainTurretEyePos.localPosition.y;
			foreach (IAIPathNode item2 in nearNodes)
			{
				Stack<IAIPathNode> path = new Stack<IAIPathNode>();
				if (targetInfo.entity.IsVisible(item2.Position + new Vector3(0f, y, 0f)) && AStarPath.FindPath(start, item2, out path, out var pathCost) && pathCost < num)
				{
					stack = path;
					num = pathCost;
					iAIPathNode = item2;
				}
			}
			if (stack == null && nearNodes.Count > 0)
			{
				Stack<IAIPathNode> path2 = new Stack<IAIPathNode>();
				IAIPathNode iAIPathNode2 = nearNodes[UnityEngine.Random.Range(0, nearNodes.Count)];
				if (AStarPath.FindPath(start, iAIPathNode2, out path2, out var pathCost2) && pathCost2 < num)
				{
					stack = path2;
					iAIPathNode = iAIPathNode2;
				}
			}
			if (stack != null)
			{
				currentPath.Clear();
				if (flag)
				{
					for (int i = 0; i < nodes.Count - 1; i++)
					{
						currentPath.Add(nodes[i].Position);
					}
				}
				foreach (IAIPathNode item3 in stack)
				{
					currentPath.Add(item3.Position);
				}
				currentPathIndex = -1;
				pathLooping = false;
				finalDestination = iAIPathNode.Position;
			}
			Facepunch.Pool.FreeList(ref nearNodes);
			Facepunch.Pool.FreeList(ref nodes);
			nextEngagementPathTime = UnityEngine.Time.time + 5f;
		}
	}

	public void DoSimpleAI()
	{
		if (base.isClient)
		{
			return;
		}
		SetFlag(Flags.Reserved5, TOD_Sky.Instance.IsNight);
		if (Interface.CallHook("OnBradleyApcThink", this) != null || !DoAI)
		{
			return;
		}
		if (targetList.Count > 0)
		{
			if (targetList[0].IsValid() && targetList[0].IsVisible())
			{
				mainGunTarget = targetList[0].entity as BaseCombatEntity;
			}
			else
			{
				mainGunTarget = null;
			}
			UpdateMovement_Hunt();
		}
		else
		{
			mainGunTarget = null;
			UpdateMovement_Patrol();
		}
		AdvancePathMovement(force: false);
		float num = Vector3.Distance(base.transform.position, destination);
		float value = Vector3.Distance(base.transform.position, finalDestination);
		if (num > stoppingDist)
		{
			Vector3 lhs = Direction2D(destination, base.transform.position);
			float num2 = Vector3.Dot(lhs, base.transform.right);
			float num3 = Vector3.Dot(lhs, base.transform.right);
			float num4 = Vector3.Dot(lhs, -base.transform.right);
			if (Vector3.Dot(lhs, -base.transform.forward) > num2)
			{
				if (num3 >= num4)
				{
					turning = 1f;
				}
				else
				{
					turning = -1f;
				}
			}
			else
			{
				turning = Mathf.Clamp(num2 * 3f, -1f, 1f);
			}
			float throttleScaleFromTurn = 1f - Mathf.InverseLerp(0f, 0.3f, Mathf.Abs(turning));
			AvoidObstacles(ref throttleScaleFromTurn);
			float num5 = Vector3.Dot(myRigidBody.velocity, base.transform.forward);
			if (!(throttle > 0f) || !(num5 < 0.5f))
			{
				timeSinceSeemingStuck = 0f;
			}
			else if ((float)timeSinceSeemingStuck > 10f)
			{
				timeSinceStuckReverseStart = 0f;
				timeSinceSeemingStuck = 0f;
			}
			float num6 = Mathf.InverseLerp(0.1f, 0.4f, Vector3.Dot(base.transform.forward, Vector3.up));
			if ((float)timeSinceStuckReverseStart < 3f)
			{
				throttle = -0.75f;
				turning = 1f;
			}
			else
			{
				throttle = (0.1f + Mathf.InverseLerp(0f, 20f, value) * 1f) * throttleScaleFromTurn + num6;
			}
		}
		DoWeaponAiming();
		SendNetworkUpdate();
	}

	public void FixedUpdate()
	{
		DoSimpleAI();
		DoPhysicsMove();
		DoWeapons();
		DoHealing();
	}

	private void AvoidObstacles(ref float throttleScaleFromTurn)
	{
		Ray ray = new Ray(base.transform.position + base.transform.forward * (bounds.extents.z - 1f), base.transform.forward);
		if (!GamePhysics.Trace(ray, 3f, out var hitInfo, 20f, obstacleHitMask, QueryTriggerInteraction.Ignore, this))
		{
			return;
		}
		if (hitInfo.point == Vector3.zero)
		{
			hitInfo.point = hitInfo.collider.ClosestPointOnBounds(ray.origin);
		}
		float num = base.transform.AngleToPos(hitInfo.point);
		float num2 = Mathf.Abs(num);
		if (num2 > 75f || !(GameObjectEx.ToBaseEntity(hitInfo.collider) is BradleyAPC))
		{
			return;
		}
		bool flag = false;
		if (num2 < 5f)
		{
			float num3 = ((throttle < 0f) ? 150f : 50f);
			if (Vector3.SqrMagnitude(base.transform.position - hitInfo.point) < num3)
			{
				flag = true;
			}
		}
		if (num > 30f)
		{
			turning = -1f;
		}
		else
		{
			turning = 1f;
		}
		throttleScaleFromTurn = (flag ? (-1f) : 1f);
		int num4 = currentPathIndex;
		_ = currentPathIndex;
		float num5 = Vector3.Distance(base.transform.position, destination);
		while (HasPath() && (double)num5 < 26.6 && currentPathIndex >= 0)
		{
			int num6 = currentPathIndex;
			AdvancePathMovement(force: true);
			num5 = Vector3.Distance(base.transform.position, destination);
			if (currentPathIndex == num4 || currentPathIndex == num6)
			{
				break;
			}
		}
	}

	public void DoPhysicsMove()
	{
		if (base.isClient)
		{
			return;
		}
		Vector3 velocity = myRigidBody.velocity;
		throttle = Mathf.Clamp(throttle, -1f, 1f);
		leftThrottle = throttle;
		rightThrottle = throttle;
		if (turning > 0f)
		{
			rightThrottle = 0f - turning;
			leftThrottle = turning;
		}
		else if (turning < 0f)
		{
			leftThrottle = turning;
			rightThrottle = turning * -1f;
		}
		Vector3.Distance(base.transform.position, GetFinalDestination());
		float num = Vector3.Distance(base.transform.position, GetCurrentPathDestination());
		float num2 = 15f;
		if (num < 20f)
		{
			float value = Vector3.Dot(PathDirection(currentPathIndex), PathDirection(currentPathIndex + 1));
			float num3 = Mathf.InverseLerp(2f, 10f, num);
			float num4 = Mathf.InverseLerp(0.5f, 0.8f, value);
			num2 = 15f - 14f * ((1f - num4) * (1f - num3));
		}
		_ = 20f;
		if (patrolPath != null)
		{
			float num5 = num2;
			foreach (IAIPathSpeedZone speedZone in patrolPath.SpeedZones)
			{
				if (speedZone.WorldSpaceBounds().Contains(base.transform.position))
				{
					num5 = Mathf.Min(num5, speedZone.GetMaxSpeed());
				}
			}
			currentSpeedZoneLimit = Mathf.Lerp(currentSpeedZoneLimit, num5, UnityEngine.Time.deltaTime);
			num2 = Mathf.Min(num2, currentSpeedZoneLimit);
		}
		if (PathComplete())
		{
			num2 = 0f;
		}
		if (ConVar.Global.developer > 1)
		{
			Debug.Log("velocity:" + velocity.magnitude + "max : " + num2);
		}
		brake = velocity.magnitude >= num2;
		ApplyBrakes(brake ? 1f : 0f);
		float num6 = throttle;
		leftThrottle = Mathf.Clamp(leftThrottle + num6, -1f, 1f);
		rightThrottle = Mathf.Clamp(rightThrottle + num6, -1f, 1f);
		float t = Mathf.InverseLerp(2f, 1f, velocity.magnitude * Mathf.Abs(Vector3.Dot(velocity.normalized, base.transform.forward)));
		float torqueAmount = Mathf.Lerp(moveForceMax, turnForce, t);
		float num7 = Mathf.InverseLerp(5f, 1.5f, velocity.magnitude * Mathf.Abs(Vector3.Dot(velocity.normalized, base.transform.forward)));
		ScaleSidewaysFriction(1f - num7);
		SetMotorTorque(leftThrottle, rightSide: false, torqueAmount);
		SetMotorTorque(rightThrottle, rightSide: true, torqueAmount);
		impactDamager.damageEnabled = myRigidBody.velocity.magnitude > 2f;
	}

	public void ApplyBrakes(float amount)
	{
		ApplyBrakeTorque(amount, rightSide: true);
		ApplyBrakeTorque(amount, rightSide: false);
	}

	public float GetMotorTorque(bool rightSide)
	{
		float num = 0f;
		WheelCollider[] array = (rightSide ? rightWheels : leftWheels);
		foreach (WheelCollider wheelCollider in array)
		{
			num += wheelCollider.motorTorque;
		}
		return num / (float)rightWheels.Length;
	}

	public void ScaleSidewaysFriction(float scale)
	{
		float stiffness = 0.75f + 0.75f * scale;
		WheelCollider[] array = rightWheels;
		foreach (WheelCollider obj in array)
		{
			WheelFrictionCurve sidewaysFriction = obj.sidewaysFriction;
			sidewaysFriction.stiffness = stiffness;
			obj.sidewaysFriction = sidewaysFriction;
		}
		array = leftWheels;
		foreach (WheelCollider obj2 in array)
		{
			WheelFrictionCurve sidewaysFriction2 = obj2.sidewaysFriction;
			sidewaysFriction2.stiffness = stiffness;
			obj2.sidewaysFriction = sidewaysFriction2;
		}
	}

	public void SetMotorTorque(float newThrottle, bool rightSide, float torqueAmount)
	{
		newThrottle = Mathf.Clamp(newThrottle, -1f, 1f);
		float num = torqueAmount * newThrottle;
		int num2 = (rightSide ? rightWheels.Length : leftWheels.Length);
		int num3 = 0;
		WheelCollider[] array = (rightSide ? rightWheels : leftWheels);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetGroundHit(out var _))
			{
				num3++;
			}
		}
		float num4 = 1f;
		if (num3 > 0)
		{
			num4 = num2 / num3;
		}
		array = (rightSide ? rightWheels : leftWheels);
		foreach (WheelCollider wheelCollider in array)
		{
			if (wheelCollider.GetGroundHit(out var _))
			{
				wheelCollider.motorTorque = num * num4;
			}
			else
			{
				wheelCollider.motorTorque = num;
			}
		}
	}

	public void ApplyBrakeTorque(float amount, bool rightSide)
	{
		WheelCollider[] array = (rightSide ? rightWheels : leftWheels);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].brakeTorque = brakeForce * amount;
		}
	}

	public void CreateExplosionMarker(float durationMinutes)
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity(debrisFieldMarker.resourcePath, base.transform.position, Quaternion.identity);
		baseEntity.Spawn();
		baseEntity.SendMessage("SetDuration", durationMinutes, SendMessageOptions.DontRequireReceiver);
	}

	public override void OnKilled(HitInfo info)
	{
		if (base.isClient || Interface.CallHook("OnEntityDestroy", this) != null)
		{
			return;
		}
		CreateExplosionMarker(10f);
		Effect.server.Run(explosionEffect.resourcePath, mainTurretEyePos.transform.position, Vector3.up, null, broadcast: true);
		Vector3 zero = Vector3.zero;
		GameObject gibSource = servergibs.Get().GetComponent<ServerGib>()._gibSource;
		List<ServerGib> list = ServerGib.CreateGibs(servergibs.resourcePath, base.gameObject, gibSource, zero, 3f);
		for (int i = 0; i < 12 - maxCratesToSpawn; i++)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(this.fireBall.resourcePath, base.transform.position, base.transform.rotation);
			if (!baseEntity)
			{
				continue;
			}
			float minInclusive = 3f;
			float maxInclusive = 10f;
			Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
			baseEntity.transform.position = base.transform.position + new Vector3(0f, 1.5f, 0f) + onUnitSphere * UnityEngine.Random.Range(-4f, 4f);
			Collider component = baseEntity.GetComponent<Collider>();
			baseEntity.Spawn();
			baseEntity.SetVelocity(zero + onUnitSphere * UnityEngine.Random.Range(minInclusive, maxInclusive));
			foreach (ServerGib item in list)
			{
				UnityEngine.Physics.IgnoreCollision(component, item.GetCollider(), ignore: true);
			}
		}
		for (int j = 0; j < maxCratesToSpawn; j++)
		{
			Vector3 onUnitSphere2 = UnityEngine.Random.onUnitSphere;
			onUnitSphere2.y = 0f;
			onUnitSphere2.Normalize();
			Vector3 pos = base.transform.position + new Vector3(0f, 1.5f, 0f) + onUnitSphere2 * UnityEngine.Random.Range(2f, 3f);
			BaseEntity baseEntity2 = GameManager.server.CreateEntity(crateToDrop.resourcePath, pos, Quaternion.LookRotation(onUnitSphere2));
			baseEntity2.Spawn();
			LootContainer lootContainer = baseEntity2 as LootContainer;
			if ((bool)lootContainer)
			{
				lootContainer.Invoke(lootContainer.RemoveMe, 1800f);
			}
			Collider component2 = baseEntity2.GetComponent<Collider>();
			Rigidbody rigidbody = baseEntity2.gameObject.AddComponent<Rigidbody>();
			rigidbody.useGravity = true;
			rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rigidbody.mass = 2f;
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			rigidbody.velocity = zero + onUnitSphere2 * UnityEngine.Random.Range(1f, 3f);
			rigidbody.angularVelocity = Vector3Ex.Range(-1.75f, 1.75f);
			rigidbody.drag = 0.5f * (rigidbody.mass / 5f);
			rigidbody.angularDrag = 0.2f * (rigidbody.mass / 5f);
			FireBall fireBall = GameManager.server.CreateEntity(this.fireBall.resourcePath) as FireBall;
			if ((bool)fireBall)
			{
				fireBall.SetParent(baseEntity2);
				fireBall.Spawn();
				fireBall.GetComponent<Rigidbody>().isKinematic = true;
				fireBall.GetComponent<Collider>().enabled = false;
			}
			baseEntity2.SendMessage("SetLockingEnt", fireBall.gameObject, SendMessageOptions.DontRequireReceiver);
			foreach (ServerGib item2 in list)
			{
				UnityEngine.Physics.IgnoreCollision(component2, item2.GetCollider(), ignore: true);
			}
		}
		base.OnKilled(info);
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		BasePlayer basePlayer = info.Initiator as BasePlayer;
		if (basePlayer != null)
		{
			AddOrUpdateTarget(basePlayer, info.PointStart, info.damageTypes.Total());
		}
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		base.OnHealthChanged(oldvalue, newvalue);
		if (base.isServer)
		{
			SetFlag(Flags.Reserved2, base.healthFraction <= 0.75f);
			SetFlag(Flags.Reserved3, base.healthFraction < 0.4f);
		}
	}

	public void DoHealing()
	{
		if (!base.isClient && base.healthFraction < 1f && base.SecondsSinceAttacked > 600f)
		{
			float amount = MaxHealth() / 300f * UnityEngine.Time.fixedDeltaTime;
			Heal(amount);
		}
	}

	public BasePlayer GetPlayerDamageInitiator()
	{
		return null;
	}

	public float GetDamageMultiplier(BaseEntity ent)
	{
		float num = ((throttle > 0f) ? 10f : 0f);
		float num2 = Vector3.Dot(myRigidBody.velocity, base.transform.forward);
		if (num2 > 0f)
		{
			num += num2 * 0.5f;
		}
		if (ent is BaseVehicle)
		{
			num *= 10f;
		}
		return num;
	}

	public void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal)
	{
	}

	public bool HasPath()
	{
		if (currentPath != null)
		{
			return currentPath.Count > 0;
		}
		return false;
	}

	public void ClearPath()
	{
		currentPath.Clear();
		currentPathIndex = -1;
	}

	public bool IndexValid(int index)
	{
		if (!HasPath())
		{
			return false;
		}
		if (index >= 0)
		{
			return index < currentPath.Count;
		}
		return false;
	}

	public Vector3 GetFinalDestination()
	{
		if (!HasPath())
		{
			return base.transform.position;
		}
		return finalDestination;
	}

	public Vector3 GetCurrentPathDestination()
	{
		if (!HasPath())
		{
			return base.transform.position;
		}
		return currentPath[currentPathIndex];
	}

	public bool PathComplete()
	{
		if (HasPath())
		{
			if (currentPathIndex == currentPath.Count - 1)
			{
				return AtCurrentPathNode();
			}
			return false;
		}
		return true;
	}

	public bool AtCurrentPathNode()
	{
		if (currentPathIndex < 0 || currentPathIndex >= currentPath.Count)
		{
			return false;
		}
		return Vector3.Distance(base.transform.position, currentPath[currentPathIndex]) <= stoppingDist;
	}

	public int GetLoopedIndex(int index)
	{
		if (!HasPath())
		{
			Debug.LogWarning("Warning, GetLoopedIndex called without a path");
			return 0;
		}
		if (!pathLooping)
		{
			return Mathf.Clamp(index, 0, currentPath.Count - 1);
		}
		if (index >= currentPath.Count)
		{
			return index % currentPath.Count;
		}
		if (index < 0)
		{
			return currentPath.Count - Mathf.Abs(index % currentPath.Count);
		}
		return index;
	}

	public Vector3 PathDirection(int index)
	{
		if (!HasPath() || currentPath.Count <= 1)
		{
			return base.transform.forward;
		}
		index = GetLoopedIndex(index);
		Vector3 vector;
		Vector3 vector2;
		if (pathLooping)
		{
			int loopedIndex = GetLoopedIndex(index - 1);
			vector = currentPath[loopedIndex];
			vector2 = currentPath[GetLoopedIndex(index)];
		}
		else
		{
			vector = ((index - 1 >= 0) ? currentPath[index - 1] : base.transform.position);
			vector2 = currentPath[index];
		}
		return (vector2 - vector).normalized;
	}

	public Vector3 IdealPathPosition()
	{
		if (!HasPath())
		{
			return base.transform.position;
		}
		int loopedIndex = GetLoopedIndex(currentPathIndex - 1);
		if (loopedIndex == currentPathIndex)
		{
			return currentPath[currentPathIndex];
		}
		return ClosestPointAlongPath(currentPath[loopedIndex], currentPath[currentPathIndex], base.transform.position);
	}

	public void AdvancePathMovement(bool force)
	{
		if (HasPath())
		{
			if (force || AtCurrentPathNode() || currentPathIndex == -1)
			{
				currentPathIndex = GetLoopedIndex(currentPathIndex + 1);
			}
			if (PathComplete())
			{
				ClearPath();
				return;
			}
			Vector3 vector = IdealPathPosition();
			Vector3 vector2 = currentPath[currentPathIndex];
			float a = Vector3.Distance(vector, vector2);
			float value = Vector3.Distance(base.transform.position, vector);
			float num = Mathf.InverseLerp(8f, 0f, value);
			vector += Direction2D(vector2, vector) * Mathf.Min(a, num * 20f);
			SetDestination(vector);
		}
	}

	public bool GetPathToClosestTurnableNode(IAIPathNode start, Vector3 forward, ref List<IAIPathNode> nodes)
	{
		float num = float.NegativeInfinity;
		IAIPathNode iAIPathNode = null;
		foreach (IAIPathNode item in start.Linked)
		{
			float num2 = Vector3.Dot(forward, (item.Position - start.Position).normalized);
			if (num2 > num)
			{
				num = num2;
				iAIPathNode = item;
			}
		}
		if (iAIPathNode != null)
		{
			nodes.Add(iAIPathNode);
			if (!iAIPathNode.Straightaway)
			{
				return true;
			}
			return GetPathToClosestTurnableNode(iAIPathNode, (iAIPathNode.Position - start.Position).normalized, ref nodes);
		}
		return false;
	}

	public bool GetEngagementPath(ref List<IAIPathNode> nodes)
	{
		IAIPathNode closestToPoint = patrolPath.GetClosestToPoint(base.transform.position);
		Vector3 normalized = (closestToPoint.Position - base.transform.position).normalized;
		if (Vector3.Dot(base.transform.forward, normalized) > 0f)
		{
			nodes.Add(closestToPoint);
			if (!closestToPoint.Straightaway)
			{
				return true;
			}
		}
		return GetPathToClosestTurnableNode(closestToPoint, base.transform.forward, ref nodes);
	}

	public void AddOrUpdateTarget(BaseEntity ent, Vector3 pos, float damageFrom = 0f)
	{
		if ((AI.ignoreplayers && !ent.IsNpc) || !(ent is BasePlayer))
		{
			return;
		}
		TargetInfo targetInfo = null;
		foreach (TargetInfo target in targetList)
		{
			if (target.entity == ent)
			{
				targetInfo = target;
				break;
			}
		}
		if (targetInfo == null)
		{
			targetInfo = Facepunch.Pool.Get<TargetInfo>();
			targetInfo.Setup(ent, UnityEngine.Time.time - 1f);
			targetList.Add(targetInfo);
		}
		targetInfo.lastSeenPosition = pos;
		targetInfo.damageReceivedFrom += damageFrom;
	}

	public void UpdateTargetList()
	{
		List<BaseEntity> obj = Facepunch.Pool.GetList<BaseEntity>();
		Vis.Entities(base.transform.position, searchRange, obj, 133120);
		foreach (BaseEntity item in obj)
		{
			if ((AI.ignoreplayers && !item.IsNpc) || !(item is BasePlayer))
			{
				continue;
			}
			BasePlayer basePlayer = item as BasePlayer;
			if (basePlayer.IsDead() || basePlayer is HumanNPC || basePlayer is NPCPlayer || (basePlayer.InSafeZone() && !basePlayer.IsHostile()) || !VisibilityTest(item))
			{
				continue;
			}
			bool flag = false;
			foreach (TargetInfo target in targetList)
			{
				if (target.entity == item)
				{
					target.lastSeenTime = UnityEngine.Time.time;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				TargetInfo targetInfo = Facepunch.Pool.Get<TargetInfo>();
				targetInfo.Setup(item, UnityEngine.Time.time);
				targetList.Add(targetInfo);
			}
		}
		for (int num = targetList.Count - 1; num >= 0; num--)
		{
			TargetInfo obj2 = targetList[num];
			BasePlayer basePlayer2 = obj2.entity as BasePlayer;
			if (obj2.entity == null || UnityEngine.Time.time - obj2.lastSeenTime > memoryDuration || basePlayer2.IsDead() || (basePlayer2.InSafeZone() && !basePlayer2.IsHostile()) || (AI.ignoreplayers && !basePlayer2.IsNpc))
			{
				targetList.Remove(obj2);
				Facepunch.Pool.Free(ref obj2);
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		targetList.Sort(SortTargets);
	}

	public int SortTargets(TargetInfo t1, TargetInfo t2)
	{
		return t2.GetPriorityScore(this).CompareTo(t1.GetPriorityScore(this));
	}

	public Vector3 GetAimPoint(BaseEntity ent)
	{
		BasePlayer basePlayer = ent as BasePlayer;
		if (basePlayer != null)
		{
			return basePlayer.eyes.position;
		}
		return ent.CenterPoint();
	}

	public bool VisibilityTest(BaseEntity ent)
	{
		if (ent == null)
		{
			return false;
		}
		if (!(Vector3.Distance(ent.transform.position, base.transform.position) < viewDistance))
		{
			return false;
		}
		bool flag = false;
		if (ent is BasePlayer)
		{
			BasePlayer basePlayer = ent as BasePlayer;
			Vector3 position = mainTurret.transform.position;
			flag = IsVisible(basePlayer.eyes.position, position) || IsVisible(basePlayer.transform.position + Vector3.up * 0.1f, position);
			if (!flag && basePlayer.isMounted && basePlayer.GetMounted().VehicleParent() != null && basePlayer.GetMounted().VehicleParent().AlwaysAllowBradleyTargeting)
			{
				flag = IsVisible(basePlayer.GetMounted().VehicleParent().bounds.center, position);
			}
			if (flag)
			{
				flag = !UnityEngine.Physics.SphereCast(new Ray(position, Vector3Ex.Direction(basePlayer.eyes.position, position)), 0.05f, Vector3.Distance(basePlayer.eyes.position, position), 10551297);
			}
		}
		else
		{
			Debug.LogWarning("Standard vis test!");
			flag = IsVisible(ent.CenterPoint());
		}
		object obj = Interface.CallHook("CanBradleyApcTarget", this, ent);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return flag;
	}

	public void UpdateTargetVisibilities()
	{
		foreach (TargetInfo target in targetList)
		{
			if (target.IsValid() && VisibilityTest(target.entity))
			{
				target.lastSeenTime = UnityEngine.Time.time;
				target.lastSeenPosition = target.entity.transform.position;
			}
		}
	}

	public void DoWeaponAiming()
	{
		desiredAimVector = ((mainGunTarget != null) ? (GetAimPoint(mainGunTarget) - mainTurretEyePos.transform.position).normalized : desiredAimVector);
		BaseEntity baseEntity = null;
		if (targetList.Count > 0)
		{
			if (targetList.Count > 1 && targetList[1].IsValid() && targetList[1].IsVisible())
			{
				baseEntity = targetList[1].entity;
			}
			else if (targetList[0].IsValid() && targetList[0].IsVisible())
			{
				baseEntity = targetList[0].entity;
			}
		}
		desiredTopTurretAimVector = ((baseEntity != null) ? (GetAimPoint(baseEntity) - topTurretEyePos.transform.position).normalized : base.transform.forward);
	}

	public void DoWeapons()
	{
		if (mainGunTarget != null && Vector3.Dot(turretAimVector, (GetAimPoint(mainGunTarget) - mainTurretEyePos.transform.position).normalized) >= 0.99f)
		{
			bool flag = VisibilityTest(mainGunTarget);
			float num = Vector3.Distance(mainGunTarget.transform.position, base.transform.position);
			if (UnityEngine.Time.time > nextCoaxTime && flag && num <= 40f)
			{
				numCoaxBursted++;
				FireGun(GetAimPoint(mainGunTarget), 3f, isCoax: true);
				nextCoaxTime = UnityEngine.Time.time + coaxFireRate;
				if (numCoaxBursted >= coaxBurstLength)
				{
					nextCoaxTime = UnityEngine.Time.time + 1f;
					numCoaxBursted = 0;
				}
			}
			if (num >= 10f && flag)
			{
				FireGunTest();
			}
		}
		if (targetList.Count > 1)
		{
			BaseEntity entity = targetList[1].entity;
			if (entity != null && UnityEngine.Time.time > nextTopTurretTime && VisibilityTest(entity))
			{
				FireGun(GetAimPoint(targetList[1].entity), 3f, isCoax: false);
				nextTopTurretTime = UnityEngine.Time.time + topTurretFireRate;
			}
		}
	}

	public void FireGun(Vector3 targetPos, float aimCone, bool isCoax)
	{
		Transform transform = (isCoax ? coaxMuzzle : topTurretMuzzle);
		Vector3 vector = transform.transform.position - transform.forward * 0.25f;
		Vector3 normalized = (targetPos - vector).normalized;
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(aimCone, normalized);
		targetPos = vector + modifiedAimConeDirection * 300f;
		List<RaycastHit> obj = Facepunch.Pool.GetList<RaycastHit>();
		GamePhysics.TraceAll(new Ray(vector, modifiedAimConeDirection), 0f, obj, 300f, 1220225809);
		for (int i = 0; i < obj.Count; i++)
		{
			RaycastHit hit = obj[i];
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if (!(entity != null) || (!(entity == this) && !entity.EqualNetID(this)))
			{
				BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
				if (baseCombatEntity != null)
				{
					ApplyDamage(baseCombatEntity, hit.point, modifiedAimConeDirection);
				}
				if (!(entity != null) || entity.ShouldBlockProjectiles())
				{
					targetPos = hit.point;
					break;
				}
			}
		}
		ClientRPC(null, "CLIENT_FireGun", isCoax, targetPos);
		Facepunch.Pool.FreeList(ref obj);
	}

	public void ApplyDamage(BaseCombatEntity entity, Vector3 point, Vector3 normal)
	{
		float damageAmount = bulletDamage * UnityEngine.Random.Range(0.9f, 1.1f);
		HitInfo info = new HitInfo(this, entity, DamageType.Bullet, damageAmount, point);
		entity.OnAttacked(info);
		if (entity is BasePlayer || entity is BaseNpc)
		{
			Effect.server.ImpactEffect(new HitInfo
			{
				HitPositionWorld = point,
				HitNormalWorld = -normal,
				HitMaterial = StringPool.Get("Flesh")
			});
		}
	}

	public void AimWeaponAt(Transform weaponYaw, Transform weaponPitch, Vector3 direction, float minPitch = -360f, float maxPitch = 360f, float maxYaw = 360f, Transform parentOverride = null)
	{
		Vector3 direction2 = direction;
		direction2 = weaponYaw.parent.InverseTransformDirection(direction2);
		Quaternion localRotation = Quaternion.LookRotation(direction2);
		Vector3 eulerAngles = localRotation.eulerAngles;
		for (int i = 0; i < 3; i++)
		{
			eulerAngles[i] -= ((eulerAngles[i] > 180f) ? 360f : 0f);
		}
		Quaternion localRotation2 = Quaternion.Euler(0f, Mathf.Clamp(eulerAngles.y, 0f - maxYaw, maxYaw), 0f);
		Quaternion localRotation3 = Quaternion.Euler(Mathf.Clamp(eulerAngles.x, minPitch, maxPitch), 0f, 0f);
		if (weaponYaw == null && weaponPitch != null)
		{
			weaponPitch.transform.localRotation = localRotation3;
			return;
		}
		if (weaponPitch == null && weaponYaw != null)
		{
			weaponYaw.transform.localRotation = localRotation;
			return;
		}
		weaponYaw.transform.localRotation = localRotation2;
		weaponPitch.transform.localRotation = localRotation3;
	}

	public void LateUpdate()
	{
		float num = UnityEngine.Time.time - lastLateUpdate;
		lastLateUpdate = UnityEngine.Time.time;
		if (base.isServer)
		{
			float num2 = MathF.PI * 2f / 3f;
			turretAimVector = Vector3.RotateTowards(turretAimVector, desiredAimVector, num2 * num, 0f);
		}
		else
		{
			turretAimVector = Vector3.Lerp(turretAimVector, desiredAimVector, UnityEngine.Time.deltaTime * 10f);
		}
		AimWeaponAt(mainTurret, coaxPitch, turretAimVector, -90f, 90f);
		AimWeaponAt(mainTurret, CannonPitch, turretAimVector, -90f, 7f);
		topTurretAimVector = Vector3.Lerp(topTurretAimVector, desiredTopTurretAimVector, UnityEngine.Time.deltaTime * 5f);
		AimWeaponAt(topTurretYaw, topTurretPitch, topTurretAimVector, -360f, 360f, 360f, mainTurret);
	}
}
