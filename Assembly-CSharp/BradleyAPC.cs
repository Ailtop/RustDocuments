using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.AI;
using UnityEngine;
using UnityEngine.AI;

public class BradleyAPC : BaseCombatEntity
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

	public BaseCombatEntity mainGunTarget;

	public List<TargetInfo> targetList = new List<TargetInfo>();

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

	public BasePath patrolPath;

	public bool DoAI = true;

	public GameObjectRef mainCannonMuzzleFlash;

	public GameObjectRef mainCannonProjectile;

	public float recoilScale = 200f;

	public NavMeshPath navMeshPath;

	public int navMeshPathIndex;

	public float nextFireTime = 10f;

	public int numBursted;

	public float nextPatrolTime;

	public float nextEngagementPathTime;

	public float currentSpeedZoneLimit;

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
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (pathLooping)
		{
			int loopedIndex = GetLoopedIndex(index - 1);
			zero = currentPath[loopedIndex];
			zero2 = currentPath[GetLoopedIndex(index)];
		}
		else
		{
			zero = ((index - 1 >= 0) ? currentPath[index - 1] : base.transform.position);
			zero2 = currentPath[index];
		}
		return (zero2 - zero).normalized;
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

	public void AdvancePathMovement()
	{
		if (HasPath())
		{
			if (AtCurrentPathNode() || currentPathIndex == -1)
			{
				currentPathIndex = GetLoopedIndex(currentPathIndex + 1);
			}
			if (PathComplete())
			{
				ClearPath();
				return;
			}
			Vector3 vector = IdealPathPosition();
			float a = Vector3.Distance(vector, currentPath[currentPathIndex]);
			float value = Vector3.Distance(base.transform.position, vector);
			float num = Mathf.InverseLerp(8f, 0f, value);
			vector += Direction2D(currentPath[currentPathIndex], vector) * Mathf.Min(a, num * 20f);
			SetDestination(vector);
		}
	}

	public bool GetPathToClosestTurnableNode(BasePathNode start, Vector3 forward, ref List<BasePathNode> nodes)
	{
		float num = float.NegativeInfinity;
		BasePathNode basePathNode = null;
		foreach (BasePathNode item in start.linked)
		{
			float num2 = Vector3.Dot(forward, (item.transform.position - start.transform.position).normalized);
			if (num2 > num)
			{
				num = num2;
				basePathNode = item;
			}
		}
		if (basePathNode != null)
		{
			nodes.Add(basePathNode);
			if (!basePathNode.straightaway)
			{
				return true;
			}
			return GetPathToClosestTurnableNode(basePathNode, (basePathNode.transform.position - start.transform.position).normalized, ref nodes);
		}
		return false;
	}

	public bool GetEngagementPath(ref List<BasePathNode> nodes)
	{
		BasePathNode closestToPoint = patrolPath.GetClosestToPoint(base.transform.position);
		Vector3 normalized = (closestToPoint.transform.position - base.transform.position).normalized;
		if (Vector3.Dot(base.transform.forward, normalized) > 0f)
		{
			nodes.Add(closestToPoint);
			if (!closestToPoint.straightaway)
			{
				return true;
			}
		}
		return GetPathToClosestTurnableNode(closestToPoint, base.transform.forward, ref nodes);
	}

	public void AddOrUpdateTarget(BaseEntity ent, Vector3 pos, float damageFrom = 0f)
	{
		if (!(ent is BasePlayer))
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
			if (!(item is BasePlayer))
			{
				continue;
			}
			BasePlayer basePlayer = item as BasePlayer;
			if (basePlayer.IsDead() || basePlayer is Scientist || basePlayer is HumanNPCNew || !VisibilityTest(item))
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
			if (obj2.entity == null || UnityEngine.Time.time - obj2.lastSeenTime > memoryDuration || basePlayer2.IsDead())
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
				FireGun(GetAimPoint(mainGunTarget), 3f, true);
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
				FireGun(GetAimPoint(targetList[1].entity), 3f, false);
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
		GamePhysics.TraceAll(new Ray(vector, modifiedAimConeDirection), 0f, obj, 300f, 1219701521);
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
			float num2 = (float)Math.PI * 2f / 3f;
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

	public void InstallPatrolPath(BasePath path)
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
		PathInterestNode randomInterestNodeAwayFrom = patrolPath.GetRandomInterestNodeAwayFrom(base.transform.position);
		BasePathNode closestToPoint = patrolPath.GetClosestToPoint(randomInterestNodeAwayFrom.transform.position);
		BasePathNode basePathNode = null;
		bool flag = false;
		List<BasePathNode> nodes = Facepunch.Pool.GetList<BasePathNode>();
		if (GetEngagementPath(ref nodes))
		{
			flag = true;
			basePathNode = nodes[nodes.Count - 1];
		}
		else
		{
			basePathNode = patrolPath.GetClosestToPoint(base.transform.position);
		}
		if (!(Vector3.Distance(finalDestination, closestToPoint.transform.position) > 2f))
		{
			return;
		}
		if (closestToPoint == basePathNode)
		{
			currentPath.Clear();
			currentPath.Add(closestToPoint.transform.position);
			currentPathIndex = -1;
			pathLooping = false;
			finalDestination = closestToPoint.transform.position;
		}
		else
		{
			Stack<BasePathNode> path;
			float pathCost;
			if (!AStarPath.FindPath(basePathNode, closestToPoint, out path, out pathCost))
			{
				return;
			}
			currentPath.Clear();
			if (flag)
			{
				for (int i = 0; i < nodes.Count - 1; i++)
				{
					currentPath.Add(nodes[i].transform.position);
				}
			}
			foreach (BasePathNode item in path)
			{
				currentPath.Add(item.transform.position);
			}
			currentPathIndex = -1;
			pathLooping = false;
			finalDestination = closestToPoint.transform.position;
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
			BasePathNode start = patrolPath.GetClosestToPoint(base.transform.position);
			List<BasePathNode> nodes = Facepunch.Pool.GetList<BasePathNode>();
			if (GetEngagementPath(ref nodes))
			{
				flag = true;
				start = nodes[nodes.Count - 1];
			}
			BasePathNode basePathNode = null;
			List<BasePathNode> nearNodes = Facepunch.Pool.GetList<BasePathNode>();
			patrolPath.GetNodesNear(targetInfo.lastSeenPosition, ref nearNodes, 30f);
			Stack<BasePathNode> stack = null;
			float num = float.PositiveInfinity;
			float y = mainTurretEyePos.localPosition.y;
			foreach (BasePathNode item2 in nearNodes)
			{
				Stack<BasePathNode> path = new Stack<BasePathNode>();
				float pathCost;
				if (targetInfo.entity.IsVisible(item2.transform.position + new Vector3(0f, y, 0f)) && AStarPath.FindPath(start, item2, out path, out pathCost) && pathCost < num)
				{
					stack = path;
					num = pathCost;
					basePathNode = item2;
				}
			}
			if (stack == null && nearNodes.Count > 0)
			{
				Stack<BasePathNode> path2 = new Stack<BasePathNode>();
				BasePathNode basePathNode2 = nearNodes[UnityEngine.Random.Range(0, nearNodes.Count)];
				float pathCost2;
				if (AStarPath.FindPath(start, basePathNode2, out path2, out pathCost2) && pathCost2 < num)
				{
					stack = path2;
					basePathNode = basePathNode2;
				}
			}
			if (stack != null)
			{
				currentPath.Clear();
				if (flag)
				{
					for (int i = 0; i < nodes.Count - 1; i++)
					{
						currentPath.Add(nodes[i].transform.position);
					}
				}
				foreach (BasePathNode item3 in stack)
				{
					currentPath.Add(item3.transform.position);
				}
				currentPathIndex = -1;
				pathLooping = false;
				finalDestination = basePathNode.transform.position;
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
		if (!DoAI)
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
		AdvancePathMovement();
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
			float num5 = 1f - Mathf.InverseLerp(0f, 0.3f, Mathf.Abs(turning));
			float num6 = Mathf.InverseLerp(0.1f, 0.4f, Vector3.Dot(base.transform.forward, Vector3.up));
			throttle = (0.1f + Mathf.InverseLerp(0f, 20f, value) * 1f) * num5 + num6;
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
		float num8 = 20f;
		if (patrolPath != null)
		{
			float num5 = num2;
			foreach (PathSpeedZone speedZone in patrolPath.speedZones)
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
		SetMotorTorque(leftThrottle, false, torqueAmount);
		SetMotorTorque(rightThrottle, true, torqueAmount);
		impactDamager.damageEnabled = myRigidBody.velocity.magnitude > 2f;
	}

	public void ApplyBrakes(float amount)
	{
		ApplyBrakeTorque(amount, true);
		ApplyBrakeTorque(amount, false);
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
			WheelHit hit;
			if (array[i].GetGroundHit(out hit))
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
			WheelHit hit2;
			if (wheelCollider.GetGroundHit(out hit2))
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
		Effect.server.Run(explosionEffect.resourcePath, mainTurretEyePos.transform.position, Vector3.up, null, true);
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
			float min = 3f;
			float max = 10f;
			Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
			baseEntity.transform.position = base.transform.position + new Vector3(0f, 1.5f, 0f) + onUnitSphere * UnityEngine.Random.Range(-4f, 4f);
			Collider component = baseEntity.GetComponent<Collider>();
			baseEntity.Spawn();
			baseEntity.SetVelocity(zero + onUnitSphere * UnityEngine.Random.Range(min, max));
			foreach (ServerGib item in list)
			{
				UnityEngine.Physics.IgnoreCollision(component, item.GetCollider(), true);
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
				UnityEngine.Physics.IgnoreCollision(component2, item2.GetCollider(), true);
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
}
