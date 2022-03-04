#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class BaseVehicle : BaseMountable
{
	public enum ClippingCheckMode
	{
		OnMountOnly = 0,
		Always = 1,
		AlwaysHeadOnly = 2
	}

	[Serializable]
	public class MountPointInfo
	{
		public bool isDriver;

		public Vector3 pos;

		public Vector3 rot;

		public string bone = "";

		public GameObjectRef prefab;

		[HideInInspector]
		public BaseMountable mountable;
	}

	[IsReadOnly]
	public struct Enumerable : IEnumerable<MountPointInfo>, IEnumerable
	{
		private readonly BaseVehicle _vehicle;

		public Enumerable(BaseVehicle vehicle)
		{
			if (vehicle == null)
			{
				throw new ArgumentNullException("vehicle");
			}
			_vehicle = vehicle;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_vehicle);
		}

		IEnumerator<MountPointInfo> IEnumerable<MountPointInfo>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public struct Enumerator : IEnumerator<MountPointInfo>, IEnumerator, IDisposable
	{
		private enum State
		{
			Direct = 0,
			EnterChild = 1,
			EnumerateChild = 2,
			Finished = 3
		}

		private class Box : Facepunch.Pool.IPooled
		{
			public Enumerator Value;

			public void EnterPool()
			{
				Value = default(Enumerator);
			}

			public void LeavePool()
			{
				Value = default(Enumerator);
			}
		}

		private readonly BaseVehicle _vehicle;

		private State _state;

		private int _index;

		private int _childIndex;

		private Box _enumerator;

		public MountPointInfo Current { get; private set; }

		object IEnumerator.Current => Current;

		public Enumerator(BaseVehicle vehicle)
		{
			if (vehicle == null)
			{
				throw new ArgumentNullException("vehicle");
			}
			_vehicle = vehicle;
			_state = State.Direct;
			_index = -1;
			_childIndex = -1;
			_enumerator = null;
			Current = null;
		}

		public bool MoveNext()
		{
			Current = null;
			switch (_state)
			{
			case State.Direct:
				_index++;
				if (_index >= _vehicle.mountPoints.Count)
				{
					_state = State.EnterChild;
					goto case State.EnterChild;
				}
				Current = _vehicle.mountPoints[_index];
				return true;
			case State.EnterChild:
				do
				{
					_childIndex++;
				}
				while (_childIndex < _vehicle.childVehicles.Count && _vehicle.childVehicles[_childIndex] == null);
				if (_childIndex >= _vehicle.childVehicles.Count)
				{
					_state = State.Finished;
					return false;
				}
				_enumerator = Facepunch.Pool.Get<Box>();
				_enumerator.Value = _vehicle.childVehicles[_childIndex].allMountPoints.GetEnumerator();
				_state = State.EnumerateChild;
				goto case State.EnumerateChild;
			case State.EnumerateChild:
				if (_enumerator.Value.MoveNext())
				{
					Current = _enumerator.Value.Current;
					return true;
				}
				_enumerator.Value.Dispose();
				Facepunch.Pool.Free(ref _enumerator);
				_state = State.EnterChild;
				goto case State.EnterChild;
			case State.Finished:
				return false;
			default:
				throw new NotSupportedException();
			}
		}

		public void Dispose()
		{
			if (_enumerator != null)
			{
				_enumerator.Value.Dispose();
				Facepunch.Pool.Free(ref _enumerator);
			}
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const float MIN_TIME_BETWEEN_PUSHES = 1f;

	public TimeSince timeSinceLastPush;

	private bool prevSleeping;

	private ProtoBuf.BaseVehicle pendingLoad;

	public Queue<BasePlayer> recentDrivers = new Queue<BasePlayer>();

	public Action clearRecentDriverAction;

	public float safeAreaRadius;

	public Vector3 safeAreaOrigin;

	public float spawnTime = -1f;

	[Tooltip("Allow players to mount other mountables/ladders from this vehicle")]
	public bool mountChaining = true;

	public ClippingCheckMode clippingChecks;

	public bool shouldShowHudHealth;

	public bool ignoreDamageFromOutside;

	[Header("Rigidbody (Optional)")]
	public Rigidbody rigidBody;

	[Header("Mount Points")]
	public List<MountPointInfo> mountPoints;

	public bool doClippingAndVisChecks = true;

	[Header("Damage")]
	public DamageRenderer damageRenderer;

	[FormerlySerializedAs("explosionDamageMultiplier")]
	public float explosionForceMultiplier = 400f;

	public float explosionForceMax = 75000f;

	public const Flags Flag_OnlyOwnerEntry = Flags.Locked;

	public const Flags Flag_Headlights = Flags.Reserved5;

	public const Flags Flag_Stationary = Flags.Reserved7;

	public const Flags Flag_SeatsFull = Flags.Reserved11;

	private readonly List<BaseVehicle> childVehicles = new List<BaseVehicle>(0);

	public virtual bool AlwaysAllowBradleyTargeting => false;

	protected bool RecentlyPushed => (float)timeSinceLastPush < 1f;

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	protected virtual bool CanSwapSeats => true;

	public bool IsMovingOrOn
	{
		get
		{
			if (!IsMoving())
			{
				return IsOn();
			}
			return true;
		}
	}

	public override float RealisticMass
	{
		get
		{
			if (rigidBody != null)
			{
				return rigidBody.mass;
			}
			return base.RealisticMass;
		}
	}

	public Enumerable allMountPoints => new Enumerable(this);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseVehicle.OnRpcMessage"))
		{
			if (rpc == 2115395408 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_WantsPush "));
				}
				using (TimeWarning.New("RPC_WantsPush"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2115395408u, "RPC_WantsPush", this, player, 5f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_WantsPush(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_WantsPush");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnAttacked(HitInfo info)
	{
		if (IsSafe() && !info.damageTypes.Has(DamageType.Decay))
		{
			info.damageTypes.ScaleAll(0f);
		}
		base.OnAttacked(info);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		ClearOwnerEntry();
		CheckAndSpawnMountPoints();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!base.isServer || !info.forDisk)
		{
			return;
		}
		info.msg.baseVehicle = Facepunch.Pool.Get<ProtoBuf.BaseVehicle>();
		info.msg.baseVehicle.mountPoints = Facepunch.Pool.GetList<ProtoBuf.BaseVehicle.MountPoint>();
		for (int i = 0; i < mountPoints.Count; i++)
		{
			MountPointInfo mountPointInfo = mountPoints[i];
			if (!(mountPointInfo.mountable == null))
			{
				ProtoBuf.BaseVehicle.MountPoint mountPoint = Facepunch.Pool.Get<ProtoBuf.BaseVehicle.MountPoint>();
				mountPoint.index = i;
				mountPoint.mountableId = mountPointInfo.mountable.net.ID;
				info.msg.baseVehicle.mountPoints.Add(mountPoint);
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk && info.msg.baseVehicle != null)
		{
			pendingLoad?.Dispose();
			pendingLoad = info.msg.baseVehicle;
			info.msg.baseVehicle = null;
		}
	}

	public override float GetNetworkTime()
	{
		return UnityEngine.Time.fixedTime;
	}

	public bool AnyMounted()
	{
		return (float)NumMounted() > 0f;
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (clippingChecks != 0 && AnyMounted())
		{
			Vector3 center = base.transform.TransformPoint(bounds.center);
			int num = (IsFlipped() ? 1218511105 : 1210122497);
			if (UnityEngine.Physics.OverlapBox(center, bounds.extents, base.transform.rotation, num).Length != 0)
			{
				bool fullBodyCheck = clippingChecks != ClippingCheckMode.AlwaysHeadOnly;
				CheckSeatsForClipping(fullBodyCheck, num);
			}
		}
		if (rigidBody != null)
		{
			bool flag = rigidBody.IsSleeping();
			SetFlag(Flags.Reserved7, flag && !AnyMounted());
			if (prevSleeping && !flag)
			{
				OnServerWake();
			}
			else if (!prevSleeping && flag)
			{
				OnServerSleep();
			}
			prevSleeping = flag;
		}
		if (OnlyOwnerAccessible() && safeAreaRadius != -1f && Vector3.Distance(base.transform.position, safeAreaOrigin) > safeAreaRadius)
		{
			ClearOwnerEntry();
		}
	}

	public override Vector3 GetLocalVelocityServer()
	{
		if (rigidBody == null)
		{
			return Vector3.zero;
		}
		return rigidBody.velocity;
	}

	public override Quaternion GetAngularVelocityServer()
	{
		if (rigidBody == null)
		{
			return Quaternion.identity;
		}
		if (rigidBody.angularVelocity.sqrMagnitude < 0.1f)
		{
			return Quaternion.identity;
		}
		return Quaternion.LookRotation(rigidBody.angularVelocity, base.transform.up);
	}

	public virtual int StartingFuelUnits()
	{
		return -1;
	}

	public bool InSafeZone()
	{
		return InSafeZone(triggers, base.transform.position);
	}

	public static bool InSafeZone(List<TriggerBase> triggers, Vector3 position)
	{
		float num = 0f;
		if (triggers != null)
		{
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerSafeZone triggerSafeZone = triggers[i] as TriggerSafeZone;
				if (!(triggerSafeZone == null))
				{
					float safeLevel = triggerSafeZone.GetSafeLevel(position);
					if (safeLevel > num)
					{
						num = safeLevel;
					}
				}
			}
		}
		return num > 0f;
	}

	public virtual bool IsSeatVisible(BaseMountable mountable, Vector3 eyePos, int mask = 1218511105)
	{
		if (!doClippingAndVisChecks)
		{
			return true;
		}
		if (mountable == null)
		{
			return false;
		}
		Vector3 p = mountable.transform.position + base.transform.up * 0.15f;
		return GamePhysics.LineOfSight(eyePos, p, mask);
	}

	public virtual bool IsSeatClipping(BaseMountable mountable, bool fullBodyCheck, int mask = 1218511105)
	{
		if (!doClippingAndVisChecks)
		{
			return false;
		}
		if (mountable == null)
		{
			return false;
		}
		Vector3 position = mountable.eyePositionOverride.transform.position;
		Vector3 position2 = mountable.transform.position;
		Vector3 vector = position - position2;
		float num = 0.4f;
		if (mountable.modifiesPlayerCollider)
		{
			num = Mathf.Min(num, mountable.customPlayerCollider.radius);
		}
		Vector3 vector2 = position - vector * (num - 0.15f);
		if (fullBodyCheck)
		{
			Vector3 end = position2 + vector * (num + 0.05f);
			return GamePhysics.CheckCapsule(vector2, end, num, mask, QueryTriggerInteraction.Ignore);
		}
		return GamePhysics.CheckSphere(vector2, num, mask, QueryTriggerInteraction.Ignore);
	}

	public virtual void CheckSeatsForClipping(bool fullBodyCheck, int mask)
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			BaseMountable mountable = mountPoint.mountable;
			if (!(mountable == null) && mountable.IsMounted() && IsSeatClipping(mountable, fullBodyCheck, mask))
			{
				SeatClippedWorld(mountable);
			}
		}
	}

	public virtual void SeatClippedWorld(BaseMountable mountable)
	{
		mountable.DismountPlayer(mountable.GetMounted());
	}

	public override void MounteeTookDamage(BasePlayer mountee, HitInfo info)
	{
	}

	public override void DismountAllPlayers()
	{
		foreach (MountPointInfo allMountPoint in allMountPoints)
		{
			if (allMountPoint.mountable != null)
			{
				allMountPoint.mountable.DismountAllPlayers();
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		clearRecentDriverAction = ClearRecentDriver;
		prevSleeping = false;
	}

	public virtual void SpawnSubEntities()
	{
		CheckAndSpawnMountPoints();
	}

	public virtual bool AdminFixUp(int tier)
	{
		if (IsDead())
		{
			return false;
		}
		GetFuelSystem()?.AdminAddFuel();
		SetHealth(MaxHealth());
		SendNetworkUpdate();
		return true;
	}

	private void OnPhysicsNeighbourChanged()
	{
		if (rigidBody != null)
		{
			rigidBody.WakeUp();
		}
	}

	private void CheckAndSpawnMountPoints()
	{
		if (pendingLoad?.mountPoints != null)
		{
			foreach (ProtoBuf.BaseVehicle.MountPoint mountPoint in pendingLoad.mountPoints)
			{
				EntityRef<BaseMountable> entityRef = new EntityRef<BaseMountable>(mountPoint.mountableId);
				if (!entityRef.IsValid(true))
				{
					Debug.LogError($"Loaded a mountpoint which doesn't exist: {mountPoint.index}", this);
					continue;
				}
				if (mountPoint.index < 0 || mountPoint.index >= mountPoints.Count)
				{
					Debug.LogError($"Loaded a mountpoint which has no info: {mountPoint.index}", this);
					entityRef.Get(true).Kill();
					continue;
				}
				MountPointInfo mountPointInfo = mountPoints[mountPoint.index];
				if (mountPointInfo.mountable != null)
				{
					Debug.LogError($"Loading a mountpoint after one was already set: {mountPoint.index}", this);
					mountPointInfo.mountable.Kill();
				}
				mountPointInfo.mountable = entityRef.Get(true);
				if (!mountPointInfo.mountable.enableSaving)
				{
					mountPointInfo.mountable.EnableSaving(true);
				}
			}
		}
		pendingLoad?.Dispose();
		pendingLoad = null;
		for (int i = 0; i < mountPoints.Count; i++)
		{
			SpawnMountPoint(mountPoints[i], model);
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		if (base.isServer && !Rust.Application.isLoadingSave)
		{
			SpawnSubEntities();
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (!IsDead() && rigidBody != null && !rigidBody.isKinematic)
		{
			float num = info.damageTypes.Get(DamageType.Explosion) + info.damageTypes.Get(DamageType.AntiVehicle);
			if (num > 3f)
			{
				float explosionForce = Mathf.Min(num * explosionForceMultiplier, explosionForceMax);
				rigidBody.AddExplosionForce(explosionForce, info.HitPositionWorld, 1f, 2.5f);
			}
		}
		base.Hurt(info);
	}

	public int NumMounted()
	{
		if (!HasMountPoints())
		{
			if (!IsMounted())
			{
				return 0;
			}
			return 1;
		}
		int num = 0;
		foreach (MountPointInfo allMountPoint in allMountPoints)
		{
			if (allMountPoint.mountable != null && allMountPoint.mountable.GetMounted() != null)
			{
				num++;
			}
		}
		return num;
	}

	public int MaxMounted()
	{
		if (!HasMountPoints())
		{
			return 1;
		}
		int num = 0;
		foreach (MountPointInfo allMountPoint in allMountPoints)
		{
			if (allMountPoint.mountable != null)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasDriver()
	{
		if (HasMountPoints())
		{
			foreach (MountPointInfo allMountPoint in allMountPoints)
			{
				if (allMountPoint != null && allMountPoint.mountable != null && allMountPoint.isDriver && allMountPoint.mountable.IsMounted())
				{
					return true;
				}
			}
			return false;
		}
		return base.IsMounted();
	}

	public bool IsDriver(BasePlayer player)
	{
		if (HasMountPoints())
		{
			foreach (MountPointInfo allMountPoint in allMountPoints)
			{
				if (allMountPoint != null && allMountPoint.mountable != null && allMountPoint.isDriver)
				{
					BasePlayer mounted = allMountPoint.mountable.GetMounted();
					if (mounted != null && mounted == player)
					{
						return true;
					}
				}
			}
		}
		else if (_mounted != null)
		{
			return _mounted == player;
		}
		return false;
	}

	public BasePlayer GetDriver()
	{
		if (HasMountPoints())
		{
			foreach (MountPointInfo allMountPoint in allMountPoints)
			{
				if (allMountPoint != null && allMountPoint.mountable != null && allMountPoint.isDriver)
				{
					BasePlayer mounted = allMountPoint.mountable.GetMounted();
					if (mounted != null)
					{
						return mounted;
					}
				}
			}
		}
		else if (_mounted != null)
		{
			return _mounted;
		}
		return null;
	}

	public void GetDrivers(List<BasePlayer> drivers)
	{
		if (HasMountPoints())
		{
			foreach (MountPointInfo allMountPoint in allMountPoints)
			{
				if (allMountPoint != null && allMountPoint.mountable != null && allMountPoint.isDriver)
				{
					BasePlayer mounted = allMountPoint.mountable.GetMounted();
					if (mounted != null)
					{
						drivers.Add(mounted);
					}
				}
			}
		}
		else if (_mounted != null)
		{
			drivers.Add(_mounted);
		}
	}

	public BasePlayer GetPlayerDamageInitiator()
	{
		if (HasDriver())
		{
			return GetDriver();
		}
		if (recentDrivers.Count <= 0)
		{
			return null;
		}
		return recentDrivers.Peek();
	}

	public int GetPlayerSeat(BasePlayer player)
	{
		if (!HasMountPoints() && GetMounted() == player)
		{
			return 0;
		}
		int num = 0;
		foreach (MountPointInfo allMountPoint in allMountPoints)
		{
			if (allMountPoint.mountable != null && allMountPoint.mountable.GetMounted() == player)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public MountPointInfo GetPlayerSeatInfo(BasePlayer player)
	{
		if (!HasMountPoints())
		{
			return null;
		}
		foreach (MountPointInfo allMountPoint in allMountPoints)
		{
			if (allMountPoint.mountable != null && allMountPoint.mountable.GetMounted() == player)
			{
				return allMountPoint;
			}
		}
		return null;
	}

	public void SwapSeats(BasePlayer player, int targetSeat = 0)
	{
		if (!HasMountPoints() || !CanSwapSeats)
		{
			return;
		}
		int playerSeat = GetPlayerSeat(player);
		if (playerSeat == -1)
		{
			return;
		}
		BaseMountable mountable = GetMountPoint(playerSeat).mountable;
		int num = playerSeat;
		BaseMountable baseMountable = null;
		if (baseMountable == null)
		{
			int num2 = MaxMounted();
			for (int i = 0; i < num2; i++)
			{
				num++;
				if (num >= num2)
				{
					num = 0;
				}
				MountPointInfo mountPoint = GetMountPoint(num);
				if (mountPoint?.mountable != null && !mountPoint.mountable.IsMounted() && mountPoint.mountable.CanSwapToThis(player) && !IsSeatClipping(mountPoint.mountable, true) && IsSeatVisible(mountPoint.mountable, player.eyes.position))
				{
					baseMountable = mountPoint.mountable;
					break;
				}
			}
		}
		if (baseMountable != null && baseMountable != mountable)
		{
			mountable.DismountPlayer(player, true);
			baseMountable.MountPlayer(player);
			player.MarkSwapSeat();
		}
	}

	public bool HasDriverMountPoints()
	{
		foreach (MountPointInfo allMountPoint in allMountPoints)
		{
			if (allMountPoint.isDriver)
			{
				return true;
			}
		}
		return false;
	}

	public bool OnlyOwnerAccessible()
	{
		return HasFlag(Flags.Locked);
	}

	public bool IsDespawnEligable()
	{
		if (spawnTime != -1f)
		{
			return spawnTime + 300f < UnityEngine.Time.realtimeSinceStartup;
		}
		return true;
	}

	public void SetupOwner(BasePlayer owner, Vector3 newSafeAreaOrigin, float newSafeAreaRadius)
	{
		if (owner != null)
		{
			creatorEntity = owner;
			SetFlag(Flags.Locked, true);
			safeAreaRadius = newSafeAreaRadius;
			safeAreaOrigin = newSafeAreaOrigin;
			spawnTime = UnityEngine.Time.realtimeSinceStartup;
		}
	}

	public void ClearOwnerEntry()
	{
		creatorEntity = null;
		SetFlag(Flags.Locked, false);
		safeAreaRadius = -1f;
		safeAreaOrigin = Vector3.zero;
	}

	public virtual EntityFuelSystem GetFuelSystem()
	{
		return null;
	}

	public bool IsSafe()
	{
		if (OnlyOwnerAccessible())
		{
			return Vector3.Distance(safeAreaOrigin, base.transform.position) <= safeAreaRadius;
		}
		return false;
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		if (IsSafe())
		{
			info.damageTypes.ScaleAll(0f);
		}
		base.ScaleDamageForPlayer(player, info);
	}

	public BaseMountable GetIdealMountPoint(Vector3 eyePos, Vector3 pos, BasePlayer playerFor = null)
	{
		if (playerFor == null)
		{
			return null;
		}
		if (!HasMountPoints())
		{
			return this;
		}
		BasePlayer basePlayer = creatorEntity as BasePlayer;
		bool flag = basePlayer != null;
		bool flag2 = flag && basePlayer.Team != null;
		bool flag3 = flag && playerFor == basePlayer;
		if (!flag3 && flag && OnlyOwnerAccessible() && playerFor != null && (playerFor.Team == null || !playerFor.Team.members.Contains(basePlayer.userID)))
		{
			return null;
		}
		BaseMountable result = null;
		float num = float.PositiveInfinity;
		foreach (MountPointInfo allMountPoint in allMountPoints)
		{
			if (allMountPoint.mountable.IsMounted())
			{
				continue;
			}
			float num2 = Vector3.Distance(allMountPoint.mountable.mountAnchor.position, pos);
			if (num2 > num)
			{
				continue;
			}
			if (IsSeatClipping(allMountPoint.mountable, true))
			{
				if (UnityEngine.Application.isEditor)
				{
					Debug.Log($"Skipping seat {allMountPoint.mountable} - it's clipping");
				}
			}
			else if (!IsSeatVisible(allMountPoint.mountable, eyePos))
			{
				if (UnityEngine.Application.isEditor)
				{
					Debug.Log($"Skipping seat {allMountPoint.mountable} - it's not visible");
				}
			}
			else if (!(OnlyOwnerAccessible() && flag3) || flag2 || allMountPoint.isDriver)
			{
				result = allMountPoint.mountable;
				num = num2;
			}
		}
		return result;
	}

	public virtual bool MountEligable(BasePlayer player)
	{
		if (creatorEntity != null && OnlyOwnerAccessible() && player != creatorEntity)
		{
			BasePlayer basePlayer = creatorEntity as BasePlayer;
			if (basePlayer != null && basePlayer.Team != null && !basePlayer.Team.members.Contains(player.userID))
			{
				return false;
			}
		}
		return true;
	}

	public int GetIndexFromSeat(BaseMountable seat)
	{
		int num = 0;
		foreach (MountPointInfo allMountPoint in allMountPoints)
		{
			if (allMountPoint.mountable == seat)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public virtual void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		UpdateFullFlag();
	}

	public virtual void PrePlayerDismount(BasePlayer player, BaseMountable seat)
	{
	}

	public virtual void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		recentDrivers.Enqueue(player);
		if (!IsInvoking(clearRecentDriverAction))
		{
			Invoke(clearRecentDriverAction, 3f);
		}
		UpdateFullFlag();
	}

	private void UpdateFullFlag()
	{
		bool b = NumMounted() == MaxMounted();
		SetFlag(Flags.Reserved11, b);
	}

	public void ClearRecentDriver()
	{
		if (recentDrivers.Count > 0)
		{
			recentDrivers.Dequeue();
		}
		if (recentDrivers.Count > 0)
		{
			Invoke(clearRecentDriverAction, 3f);
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (_mounted != null || !MountEligable(player))
		{
			return;
		}
		BaseMountable idealMountPointFor = GetIdealMountPointFor(player);
		if (!(idealMountPointFor == null))
		{
			if (idealMountPointFor == this)
			{
				base.AttemptMount(player, doMountChecks);
			}
			else
			{
				idealMountPointFor.AttemptMount(player, doMountChecks);
			}
			if (player.GetMountedVehicle() == this)
			{
				PlayerMounted(player, idealMountPointFor);
			}
		}
	}

	public BaseMountable GetIdealMountPointFor(BasePlayer player)
	{
		return GetIdealMountPoint(player.eyes.position, player.eyes.position + player.eyes.HeadForward() * 1f, player);
	}

	public override bool GetDismountPosition(BasePlayer player, out Vector3 res)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null)
		{
			return baseVehicle.GetDismountPosition(player, out res);
		}
		List<Vector3> obj = Facepunch.Pool.GetList<Vector3>();
		Vector3 visualCheckOrigin = player.TriggerPoint();
		Transform[] array = dismountPositions;
		foreach (Transform transform in array)
		{
			if (ValidDismountPosition(transform.transform.position, visualCheckOrigin))
			{
				obj.Add(transform.transform.position);
			}
		}
		if (obj.Count == 0)
		{
			Debug.LogWarning("Failed to find dismount position for player :" + player.displayName + " / " + player.userID + " on obj : " + base.gameObject.name);
			Facepunch.Pool.FreeList(ref obj);
			res = player.transform.position;
			return false;
		}
		Vector3 pos = player.transform.position;
		obj.Sort((Vector3 a, Vector3 b) => Vector3.Distance(a, pos).CompareTo(Vector3.Distance(b, pos)));
		res = obj[0];
		Facepunch.Pool.FreeList(ref obj);
		return true;
	}

	public BaseMountable SpawnMountPoint(MountPointInfo mountToSpawn, Model model)
	{
		if (mountToSpawn.mountable != null)
		{
			return mountToSpawn.mountable;
		}
		Vector3 vector = Quaternion.Euler(mountToSpawn.rot) * Vector3.forward;
		Vector3 pos = mountToSpawn.pos;
		Vector3 up = Vector3.up;
		if (mountToSpawn.bone != "")
		{
			pos = model.FindBone(mountToSpawn.bone).transform.position + base.transform.TransformDirection(mountToSpawn.pos);
			vector = base.transform.TransformDirection(vector);
			up = base.transform.up;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(mountToSpawn.prefab.resourcePath, pos, Quaternion.LookRotation(vector, up));
		BaseMountable baseMountable = baseEntity as BaseMountable;
		if (baseMountable != null)
		{
			if (!baseMountable.enableSaving)
			{
				baseMountable.EnableSaving(true);
			}
			if (mountToSpawn.bone != "")
			{
				baseMountable.SetParent(this, mountToSpawn.bone, true, true);
			}
			else
			{
				baseMountable.SetParent(this);
			}
			baseMountable.Spawn();
			mountToSpawn.mountable = baseMountable;
		}
		else
		{
			Debug.LogError("MountPointInfo prefab is not a BaseMountable. Cannot spawn mount point.");
			if (baseEntity != null)
			{
				baseEntity.Kill();
			}
		}
		return baseMountable;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(5f)]
	public void RPC_WantsPush(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!player.isMounted && !RecentlyPushed && CanPushNow(player) && !(rigidBody == null) && (!OnlyOwnerAccessible() || !(player != creatorEntity)) && Interface.CallHook("OnVehiclePush", this, msg.player) == null)
		{
			player.metabolism.calories.Subtract(3f);
			player.metabolism.SendChangesToClient();
			if (rigidBody.IsSleeping())
			{
				rigidBody.WakeUp();
			}
			DoPushAction(player);
			timeSinceLastPush = 0f;
		}
	}

	public virtual void DoPushAction(BasePlayer player)
	{
		if (rigidBody == null)
		{
			return;
		}
		if (IsFlipped())
		{
			float num = rigidBody.mass * 8f;
			Vector3 torque = Vector3.forward * num;
			if (Vector3.Dot(base.transform.InverseTransformVector(base.transform.position - player.transform.position), Vector3.right) > 0f)
			{
				torque *= -1f;
			}
			if (base.transform.up.y < 0f)
			{
				torque *= -1f;
			}
			rigidBody.AddRelativeTorque(torque, ForceMode.Impulse);
		}
		else
		{
			Vector3 normalized = Vector3.ProjectOnPlane(base.transform.position - player.eyes.position, base.transform.up).normalized;
			float num2 = rigidBody.mass * 4f;
			rigidBody.AddForce(normalized * num2, ForceMode.Impulse);
		}
	}

	protected virtual void OnServerWake()
	{
	}

	protected virtual void OnServerSleep()
	{
	}

	public bool IsStationary()
	{
		return HasFlag(Flags.Reserved7);
	}

	public bool IsMoving()
	{
		return !HasFlag(Flags.Reserved7);
	}

	public override bool PlayerIsMounted(BasePlayer player)
	{
		if (BaseEntityEx.IsValid(player))
		{
			return player.GetMountedVehicle() == this;
		}
		return false;
	}

	public virtual bool CanPushNow(BasePlayer pusher)
	{
		return !IsOn();
	}

	public bool HasMountPoints()
	{
		if (mountPoints.Count > 0)
		{
			return true;
		}
		using (Enumerator enumerator = allMountPoints.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				MountPointInfo current = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	public override bool IsMounted()
	{
		if (base.isServer)
		{
			return HasDriver();
		}
		throw new NotImplementedException("Please don't call BaseVehicle IsMounted on the client.");
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (IsAlive() && !base.IsDestroyed)
		{
			return player != null;
		}
		return false;
	}

	public override bool SupportsChildDeployables()
	{
		return false;
	}

	public bool IsFlipped()
	{
		return Vector3.Dot(Vector3.up, base.transform.up) <= 0f;
	}

	public virtual bool IsVehicleRoot()
	{
		return true;
	}

	public override bool DirectlyMountable()
	{
		return IsVehicleRoot();
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		BaseVehicle baseVehicle;
		if ((object)(baseVehicle = child as BaseVehicle) != null && !baseVehicle.IsVehicleRoot() && !childVehicles.Contains(baseVehicle))
		{
			childVehicles.Add(baseVehicle);
		}
	}

	protected override void OnChildRemoved(BaseEntity child)
	{
		base.OnChildRemoved(child);
		BaseVehicle baseVehicle;
		if ((object)(baseVehicle = child as BaseVehicle) != null && !baseVehicle.IsVehicleRoot())
		{
			childVehicles.Remove(baseVehicle);
		}
	}

	public MountPointInfo GetMountPoint(int index)
	{
		if (index < 0)
		{
			return null;
		}
		if (index < mountPoints.Count)
		{
			return mountPoints[index];
		}
		index -= mountPoints.Count;
		int num = 0;
		foreach (BaseVehicle childVehicle in childVehicles)
		{
			if (childVehicle == null)
			{
				continue;
			}
			foreach (MountPointInfo allMountPoint in childVehicle.allMountPoints)
			{
				if (num == index)
				{
					return allMountPoint;
				}
				num++;
			}
		}
		return null;
	}
}
