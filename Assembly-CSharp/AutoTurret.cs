#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class AutoTurret : ContainerIOEntity, IRemoteControllable
{
	public static class TurretFlags
	{
		public const Flags Peacekeeper = Flags.Reserved1;
	}

	public class UpdateAutoTurretScanQueue : ObjectWorkQueue<AutoTurret>
	{
		protected override void RunJob(AutoTurret entity)
		{
			if (ShouldAdd(entity))
			{
				entity.TargetScan();
			}
		}

		protected override bool ShouldAdd(AutoTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return BaseNetworkableEx.IsValid(entity);
			}
			return false;
		}
	}

	public GameObjectRef gun_fire_effect;

	public GameObjectRef bulletEffect;

	public float bulletSpeed = 200f;

	public AmbienceEmitter ambienceEmitter;

	public GameObject assignDialog;

	public static UpdateAutoTurretScanQueue updateAutoTurretScanQueue = new UpdateAutoTurretScanQueue();

	private BasePlayer playerController;

	public string rcIdentifier = "TURRET";

	public Vector3 initialAimDir;

	public float rcTurnSensitivity = 4f;

	public Transform RCEyes;

	public TargetTrigger targetTrigger;

	public Transform socketTransform;

	public float nextShotTime;

	public float lastShotTime;

	public float nextVisCheck;

	public float lastTargetSeenTime;

	public bool targetVisible = true;

	public bool booting;

	public float nextIdleAimTime;

	public Vector3 targetAimDir = Vector3.forward;

	public const float bulletDamage = 15f;

	public float nextForcedAimTime;

	public Vector3 lastSentAimDir = Vector3.zero;

	private static float[] visibilityOffsets = new float[3] { 0f, 0.15f, -0.15f };

	public int peekIndex;

	[NonSerialized]
	public int numConsecutiveMisses;

	[NonSerialized]
	public int totalAmmo;

	public float nextAmmoCheckTime;

	public bool totalAmmoDirty = true;

	public float currentAmmoGravity;

	public float currentAmmoVelocity;

	public HeldEntity AttachedWeapon;

	public float attachedWeaponZOffsetScale = -0.5f;

	public BaseCombatEntity target;

	public Transform eyePos;

	public Transform muzzlePos;

	public Vector3 aimDir;

	public Transform gun_yaw;

	public Transform gun_pitch;

	public float sightRange = 30f;

	public SoundDefinition turnLoopDef;

	public SoundDefinition movementChangeDef;

	public SoundDefinition ambientLoopDef;

	public SoundDefinition focusCameraDef;

	public float focusSoundFreqMin = 2.5f;

	public float focusSoundFreqMax = 7f;

	public GameObjectRef peacekeeperToggleSound;

	public GameObjectRef onlineSound;

	public GameObjectRef offlineSound;

	public GameObjectRef targetAcquiredEffect;

	public GameObjectRef targetLostEffect;

	public GameObjectRef reloadEffect;

	public float aimCone;

	public const Flags Flag_Equipped = Flags.Reserved3;

	public const Flags Flag_MaxAuths = Flags.Reserved4;

	[NonSerialized]
	public List<PlayerNameID> authorizedPlayers = new List<PlayerNameID>();

	[NonSerialized]
	public int consumptionAmount = 10;

	public virtual bool RequiresMouse => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AutoTurret.OnRpcMessage"))
		{
			if (rpc == 1092560690 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - AddSelfAuthorize "));
				}
				using (TimeWarning.New("AddSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1092560690u, "AddSelfAuthorize", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							AddSelfAuthorize(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AddSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 3057055788u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - AssignToFriend "));
				}
				using (TimeWarning.New("AssignToFriend"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3057055788u, "AssignToFriend", this, player, 3f))
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
							AssignToFriend(msg2);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in AssignToFriend");
					}
				}
				return true;
			}
			if (rpc == 253307592 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ClearList "));
				}
				using (TimeWarning.New("ClearList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(253307592u, "ClearList", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							ClearList(rpc3);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in ClearList");
					}
				}
				return true;
			}
			if (rpc == 1500257773 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - FlipAim "));
				}
				using (TimeWarning.New("FlipAim"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1500257773u, "FlipAim", this, player, 3f))
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
							RPCMessage rpc4 = rPCMessage;
							FlipAim(rpc4);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in FlipAim");
					}
				}
				return true;
			}
			if (rpc == 3617985969u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RemoveSelfAuthorize "));
				}
				using (TimeWarning.New("RemoveSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3617985969u, "RemoveSelfAuthorize", this, player, 3f))
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
							RPCMessage rpc5 = rPCMessage;
							RemoveSelfAuthorize(rpc5);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 1770263114 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SERVER_AttackAll "));
				}
				using (TimeWarning.New("SERVER_AttackAll"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1770263114u, "SERVER_AttackAll", this, player, 3f))
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
							RPCMessage rpc6 = rPCMessage;
							SERVER_AttackAll(rpc6);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in SERVER_AttackAll");
					}
				}
				return true;
			}
			if (rpc == 3265538831u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SERVER_Peacekeeper "));
				}
				using (TimeWarning.New("SERVER_Peacekeeper"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3265538831u, "SERVER_Peacekeeper", this, player, 3f))
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
							RPCMessage rpc7 = rPCMessage;
							SERVER_Peacekeeper(rpc7);
						}
					}
					catch (Exception exception7)
					{
						Debug.LogException(exception7);
						player.Kick("RPC Error in SERVER_Peacekeeper");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool PeacekeeperMode()
	{
		return HasFlag(Flags.Reserved1);
	}

	public bool IsBeingRemoteControlled()
	{
		return playerController != null;
	}

	public Transform GetEyes()
	{
		return RCEyes;
	}

	public bool Occupied()
	{
		return false;
	}

	public BaseEntity GetEnt()
	{
		return this;
	}

	public virtual bool CanControl()
	{
		object obj = Interface.CallHook("OnEntityControl", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}

	public void UserInput(InputState inputState, BasePlayer player)
	{
		float x = Mathf.Clamp(0f - inputState.current.mouseDelta.y, -1f, 1f) * rcTurnSensitivity;
		float y = Mathf.Clamp(inputState.current.mouseDelta.x, -1f, 1f) * rcTurnSensitivity;
		Quaternion quaternion = Quaternion.LookRotation(aimDir, base.transform.up);
		Quaternion quaternion2 = Quaternion.Euler(x, y, 0f);
		Quaternion quaternion3 = quaternion * quaternion2;
		aimDir = quaternion3 * Vector3.forward;
		if (inputState.IsDown(BUTTON.RELOAD))
		{
			Reload();
		}
		bool flag = inputState.IsDown(BUTTON.FIRE_PRIMARY);
		EnsureReloaded();
		if (!(UnityEngine.Time.time >= nextShotTime && flag))
		{
			return;
		}
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if ((bool)attachedWeapon)
		{
			if (attachedWeapon.primaryMagazine.contents > 0)
			{
				FireAttachedGun(Vector3.zero, aimCone);
				float delay = (attachedWeapon.isSemiAuto ? (attachedWeapon.repeatDelay * 1.5f) : attachedWeapon.repeatDelay);
				delay = attachedWeapon.ScaleRepeatDelay(delay);
				nextShotTime = UnityEngine.Time.time + delay;
			}
			else
			{
				nextShotTime = UnityEngine.Time.time + 5f;
			}
		}
		else if (HasGenericFireable())
		{
			AttachedWeapon.ServerUse();
			nextShotTime = UnityEngine.Time.time + 0.115f;
		}
		else
		{
			nextShotTime = UnityEngine.Time.time + 1f;
		}
	}

	public void InitializeControl(BasePlayer controller)
	{
		playerController = controller;
		SetTarget(null);
		initialAimDir = aimDir;
	}

	public void StopControl()
	{
		playerController = null;
	}

	public void RCSetup()
	{
	}

	public void RCShutdown()
	{
		if (base.isServer)
		{
			RemoteControlEntity.RemoveControllable(this);
		}
	}

	public void UpdateIdentifier(string newID, bool clientSend = false)
	{
		rcIdentifier = newID;
	}

	public string GetIdentifier()
	{
		return rcIdentifier;
	}

	public override int ConsumptionAmount()
	{
		return consumptionAmount;
	}

	public void SetOnline()
	{
		SetIsOnline(online: true);
	}

	public void SetIsOnline(bool online)
	{
		if (online != HasFlag(Flags.On) && Interface.CallHook("OnTurretToggle", this) == null)
		{
			SetFlag(Flags.On, online);
			booting = false;
			SendNetworkUpdate();
			if (IsOffline())
			{
				SetTarget(null);
				isLootable = true;
			}
			else
			{
				isLootable = false;
			}
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		int result = Mathf.Min(1, GetCurrentEnergy());
		switch (outputSlot)
		{
		case 0:
			if (!HasTarget())
			{
				return 0;
			}
			return result;
		case 1:
			if (totalAmmo > 50)
			{
				return 0;
			}
			return result;
		case 2:
			if (totalAmmo != 0)
			{
				return 0;
			}
			return result;
		default:
			return 0;
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (IsPowered() && !IsOn())
		{
			InitiateStartup();
		}
		else if ((!IsPowered() && IsOn()) || booting)
		{
			InitiateShutdown();
		}
	}

	public void InitiateShutdown()
	{
		if ((!IsOffline() || booting) && Interface.CallHook("OnTurretShutdown", this) == null)
		{
			CancelInvoke(SetOnline);
			booting = false;
			Effect.server.Run(offlineSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			SetIsOnline(online: false);
		}
	}

	public void InitiateStartup()
	{
		if (!IsOnline() && !booting && Interface.CallHook("OnTurretStartup", this) == null)
		{
			Effect.server.Run(onlineSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			Invoke(SetOnline, 2f);
			booting = true;
		}
	}

	public void SetPeacekeepermode(bool isOn)
	{
		if (PeacekeeperMode() != isOn)
		{
			SetFlag(Flags.Reserved1, isOn);
			Effect.server.Run(peacekeeperToggleSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			Interface.CallHook("OnTurretModeToggle", this);
		}
	}

	public bool IsValidWeapon(Item item)
	{
		ItemDefinition info = item.info;
		if (item.isBroken)
		{
			return false;
		}
		ItemModEntity component = info.GetComponent<ItemModEntity>();
		if (component == null)
		{
			return false;
		}
		HeldEntity component2 = component.entityPrefab.Get().GetComponent<HeldEntity>();
		if (component2 == null)
		{
			return false;
		}
		if (!component2.IsUsableByTurret)
		{
			return false;
		}
		return true;
	}

	public bool CanAcceptItem(Item item, int targetSlot)
	{
		Item slot = base.inventory.GetSlot(0);
		if (IsValidWeapon(item) && targetSlot == 0)
		{
			return true;
		}
		if (item.info.category == ItemCategory.Ammunition)
		{
			if (slot == null || !GetAttachedWeapon())
			{
				return false;
			}
			if (targetSlot == 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool AtMaxAuthCapacity()
	{
		return HasFlag(Flags.Reserved4);
	}

	public void UpdateMaxAuthCapacity()
	{
		if (authorizedPlayers.Count >= 200)
		{
			SetFlag(Flags.Reserved4, b: true);
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		bool b = activeGameMode != null && activeGameMode.limitTeamAuths && authorizedPlayers.Count >= activeGameMode.GetMaxRelationshipTeamSize();
		SetFlag(Flags.Reserved4, b);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void FlipAim(RPCMessage rpc)
	{
		if (!IsOnline() && IsAuthed(rpc.player) && !booting && Interface.CallHook("OnTurretRotate", this, rpc.player) == null)
		{
			base.transform.rotation = Quaternion.LookRotation(-base.transform.forward, base.transform.up);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void AddSelfAuthorize(RPCMessage rpc)
	{
		AddSelfAuthorize(rpc.player);
	}

	private void AddSelfAuthorize(BasePlayer player)
	{
		BasePlayer player2 = player;
		if (!IsOnline() && player2.CanBuild() && !AtMaxAuthCapacity() && Interface.CallHook("OnTurretAuthorize", this, player) == null)
		{
			authorizedPlayers.RemoveAll((PlayerNameID x) => x.userid == player2.userID);
			PlayerNameID playerNameID = new PlayerNameID();
			playerNameID.userid = player2.userID;
			playerNameID.username = player2.displayName;
			authorizedPlayers.Add(playerNameID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RemoveSelfAuthorize(RPCMessage rpc)
	{
		RPCMessage rpc2 = rpc;
		if (!booting && !IsOnline() && IsAuthed(rpc2.player) && Interface.CallHook("OnTurretDeauthorize", this, rpc.player) == null)
		{
			authorizedPlayers.RemoveAll((PlayerNameID x) => x.userid == rpc2.player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void ClearList(RPCMessage rpc)
	{
		if (!booting && !IsOnline() && IsAuthed(rpc.player) && Interface.CallHook("OnTurretClearList", this, rpc.player) == null)
		{
			authorizedPlayers.Clear();
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void AssignToFriend(RPCMessage msg)
	{
		if (!AtMaxAuthCapacity() && !(msg.player == null) && msg.player.CanInteract() && CanChangeSettings(msg.player))
		{
			ulong num = msg.read.UInt64();
			if (num != 0L && !IsAuthed(num) && Interface.CallHook("OnTurretAssign", this, num, msg.player) == null)
			{
				string username = BasePlayer.SanitizePlayerNameString(msg.read.String(), num);
				PlayerNameID playerNameID = new PlayerNameID();
				playerNameID.userid = num;
				playerNameID.username = username;
				authorizedPlayers.Add(playerNameID);
				UpdateMaxAuthCapacity();
				SendNetworkUpdate();
				Interface.CallHook("OnTurretAssigned", this, num, msg.player);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void SERVER_Peacekeeper(RPCMessage rpc)
	{
		if (IsAuthed(rpc.player))
		{
			SetPeacekeepermode(isOn: true);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void SERVER_AttackAll(RPCMessage rpc)
	{
		if (IsAuthed(rpc.player))
		{
			SetPeacekeepermode(isOn: false);
		}
	}

	public virtual float TargetScanRate()
	{
		return 1f;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
		InvokeRepeating(ServerTick, UnityEngine.Random.Range(0f, 1f), 0.015f);
		InvokeRandomized(SendAimDir, UnityEngine.Random.Range(0f, 1f), 0.2f, 0.05f);
		InvokeRandomized(ScheduleForTargetScan, UnityEngine.Random.Range(0f, 1f), TargetScanRate(), 0.2f);
		targetTrigger.GetComponent<SphereCollider>().radius = sightRange;
	}

	public void SendAimDir()
	{
		if (UnityEngine.Time.realtimeSinceStartup > nextForcedAimTime || HasTarget() || Vector3.Angle(lastSentAimDir, aimDir) > 0.03f)
		{
			lastSentAimDir = aimDir;
			ClientRPC(null, "CLIENT_ReceiveAimDir", aimDir);
			nextForcedAimTime = UnityEngine.Time.realtimeSinceStartup + 2f;
		}
	}

	public void SetTarget(BaseCombatEntity targ)
	{
		if (Interface.CallHook("OnTurretTarget", this, targ) == null)
		{
			if (targ != target)
			{
				Effect.server.Run((targ == null) ? targetLostEffect.resourcePath : targetAcquiredEffect.resourcePath, base.transform.position, Vector3.up);
				MarkDirtyForceUpdateOutputs();
				nextShotTime += 0.1f;
			}
			target = targ;
		}
	}

	public virtual bool CheckPeekers()
	{
		return true;
	}

	public bool ObjectVisible(BaseCombatEntity obj)
	{
		object obj2 = Interface.CallHook("CanBeTargeted", obj, this);
		if (obj2 is bool)
		{
			return (bool)obj2;
		}
		List<RaycastHit> obj3 = Facepunch.Pool.GetList<RaycastHit>();
		Vector3 position = eyePos.transform.position;
		if (GamePhysics.CheckSphere(position, 0.1f, 2097152))
		{
			return false;
		}
		Vector3 vector = AimOffset(obj);
		float num = Vector3.Distance(vector, position);
		Vector3 vector2 = Vector3.Cross((vector - position).normalized, Vector3.up);
		for (int i = 0; (float)i < (CheckPeekers() ? 3f : 1f); i++)
		{
			Vector3 normalized = (vector + vector2 * visibilityOffsets[i] - position).normalized;
			obj3.Clear();
			GamePhysics.TraceAll(new Ray(position, normalized), 0f, obj3, num * 1.1f, 1218652417);
			for (int j = 0; j < obj3.Count; j++)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(obj3[j]);
				if ((!(entity != null) || !entity.isClient) && (!(entity != null) || !(entity.ToPlayer() != null) || entity.EqualNetID(obj)) && (!(entity != null) || !entity.EqualNetID(this)))
				{
					if (entity != null && (entity == obj || entity.EqualNetID(obj)))
					{
						Facepunch.Pool.FreeList(ref obj3);
						peekIndex = i;
						return true;
					}
					if (!(entity != null) || entity.ShouldBlockProjectiles())
					{
						break;
					}
				}
			}
		}
		Facepunch.Pool.FreeList(ref obj3);
		return false;
	}

	public virtual void FireAttachedGun(Vector3 targetPos, float aimCone, Transform muzzleToUse = null, BaseCombatEntity target = null)
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (!(attachedWeapon == null) && !IsOffline())
		{
			attachedWeapon.ServerUse(1f, gun_pitch);
		}
	}

	public virtual void FireGun(Vector3 targetPos, float aimCone, Transform muzzleToUse = null, BaseCombatEntity target = null)
	{
		if (IsOffline())
		{
			return;
		}
		if (muzzleToUse == null)
		{
			muzzleToUse = muzzlePos;
		}
		Vector3 vector = GetCenterMuzzle().transform.position - GetCenterMuzzle().forward * 0.25f;
		Vector3 vector2 = GetCenterMuzzle().transform.forward;
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(aimCone, vector2);
		targetPos = vector + modifiedAimConeDirection * 300f;
		List<RaycastHit> obj = Facepunch.Pool.GetList<RaycastHit>();
		GamePhysics.TraceAll(new Ray(vector, modifiedAimConeDirection), 0f, obj, 300f, 1219701521);
		bool flag = false;
		for (int i = 0; i < obj.Count; i++)
		{
			RaycastHit hit = obj[i];
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if ((entity != null && (entity == this || entity.EqualNetID(this))) || (PeacekeeperMode() && target != null && entity != null && entity.GetComponent<BasePlayer>() != null && !entity.EqualNetID(target)))
			{
				continue;
			}
			BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
			if (baseCombatEntity != null)
			{
				ApplyDamage(baseCombatEntity, hit.point, modifiedAimConeDirection);
				if (baseCombatEntity.EqualNetID(target))
				{
					flag = true;
				}
			}
			if (!(entity != null) || entity.ShouldBlockProjectiles())
			{
				targetPos = hit.point;
				vector2 = (targetPos - vector).normalized;
				break;
			}
		}
		int num = 2;
		if (!flag)
		{
			numConsecutiveMisses++;
		}
		else
		{
			numConsecutiveMisses = 0;
		}
		if (target != null && targetVisible && numConsecutiveMisses > num)
		{
			ApplyDamage(target, target.transform.position - vector2 * 0.25f, vector2);
			numConsecutiveMisses = 0;
		}
		ClientRPC(null, "CLIENT_FireGun", StringPool.Get(muzzleToUse.gameObject.name), targetPos);
		Facepunch.Pool.FreeList(ref obj);
	}

	public void ApplyDamage(BaseCombatEntity entity, Vector3 point, Vector3 normal)
	{
		float num = 15f * UnityEngine.Random.Range(0.9f, 1.1f);
		if (entity is BasePlayer && entity != target)
		{
			num *= 0.5f;
		}
		if (PeacekeeperMode() && entity == target)
		{
			target.MarkHostileFor(300f);
		}
		HitInfo info = new HitInfo(this, entity, DamageType.Bullet, num, point);
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

	public void IdleTick()
	{
		if (UnityEngine.Time.realtimeSinceStartup > nextIdleAimTime)
		{
			nextIdleAimTime = UnityEngine.Time.realtimeSinceStartup + UnityEngine.Random.Range(4f, 5f);
			Quaternion quaternion = Quaternion.LookRotation(base.transform.forward, Vector3.up);
			quaternion *= Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.up);
			targetAimDir = quaternion * Vector3.forward;
		}
		if (!HasTarget())
		{
			aimDir = Lerp(aimDir, targetAimDir, 2f);
		}
	}

	public virtual bool HasClipAmmo()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return false;
		}
		return attachedWeapon.primaryMagazine.contents > 0;
	}

	public virtual bool HasReserveAmmo()
	{
		return totalAmmo > 0;
	}

	public int GetTotalAmmo()
	{
		int num = 0;
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return num;
		}
		List<Item> obj = Facepunch.Pool.GetList<Item>();
		base.inventory.FindAmmo(obj, attachedWeapon.primaryMagazine.definition.ammoTypes);
		for (int i = 0; i < obj.Count; i++)
		{
			num += obj[i].amount;
		}
		Facepunch.Pool.FreeList(ref obj);
		return num;
	}

	public AmmoTypes GetValidAmmoTypes()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return AmmoTypes.RIFLE_556MM;
		}
		return attachedWeapon.primaryMagazine.definition.ammoTypes;
	}

	public ItemDefinition GetDesiredAmmo()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return null;
		}
		return attachedWeapon.primaryMagazine.ammoType;
	}

	public void Reload()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return;
		}
		nextShotTime = Mathf.Max(nextShotTime, UnityEngine.Time.time + Mathf.Min(attachedWeapon.GetReloadDuration() * 0.5f, 2f));
		AmmoTypes ammoTypes = attachedWeapon.primaryMagazine.definition.ammoTypes;
		if (attachedWeapon.primaryMagazine.contents > 0)
		{
			bool flag = false;
			if (base.inventory.capacity > base.inventory.itemList.Count)
			{
				flag = true;
			}
			else
			{
				int num = 0;
				foreach (Item item in base.inventory.itemList)
				{
					if (item.info == attachedWeapon.primaryMagazine.ammoType)
					{
						num += item.MaxStackable() - item.amount;
					}
				}
				flag = num >= attachedWeapon.primaryMagazine.contents;
			}
			if (!flag)
			{
				return;
			}
			base.inventory.AddItem(attachedWeapon.primaryMagazine.ammoType, attachedWeapon.primaryMagazine.contents, 0uL);
			attachedWeapon.primaryMagazine.contents = 0;
		}
		List<Item> obj = Facepunch.Pool.GetList<Item>();
		base.inventory.FindAmmo(obj, ammoTypes);
		if (obj.Count > 0)
		{
			Effect.server.Run(reloadEffect.resourcePath, this, StringPool.Get("WeaponAttachmentPoint"), Vector3.zero, Vector3.zero);
			totalAmmoDirty = true;
			attachedWeapon.primaryMagazine.ammoType = obj[0].info;
			int num2 = 0;
			while (attachedWeapon.primaryMagazine.contents < attachedWeapon.primaryMagazine.capacity && num2 < obj.Count)
			{
				if (obj[num2].info == attachedWeapon.primaryMagazine.ammoType)
				{
					int b = attachedWeapon.primaryMagazine.capacity - attachedWeapon.primaryMagazine.contents;
					b = Mathf.Min(obj[num2].amount, b);
					obj[num2].UseItem(b);
					attachedWeapon.primaryMagazine.contents += b;
				}
				num2++;
			}
		}
		ItemDefinition ammoType = attachedWeapon.primaryMagazine.ammoType;
		if ((bool)ammoType)
		{
			ItemModProjectile component = ammoType.GetComponent<ItemModProjectile>();
			GameObject gameObject = component.projectileObject.Get();
			if ((bool)gameObject)
			{
				if ((bool)gameObject.GetComponent<Projectile>())
				{
					currentAmmoGravity = 0f;
					currentAmmoVelocity = component.GetMaxVelocity();
				}
				else
				{
					ServerProjectile component2 = gameObject.GetComponent<ServerProjectile>();
					if ((bool)component2)
					{
						currentAmmoGravity = component2.gravityModifier;
						currentAmmoVelocity = component2.speed;
					}
				}
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		attachedWeapon.SendNetworkUpdate();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		totalAmmoDirty = true;
		Reload();
	}

	public void UpdateTotalAmmo()
	{
		int num = totalAmmo;
		totalAmmo = GetTotalAmmo();
		if (num != totalAmmo)
		{
			MarkDirtyForceUpdateOutputs();
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if ((bool)item.info.GetComponent<ItemModEntity>())
		{
			if (IsInvoking(UpdateAttachedWeapon))
			{
				UpdateAttachedWeapon();
			}
			Invoke(UpdateAttachedWeapon, 0.5f);
		}
	}

	public void EnsureReloaded(bool onlyReloadIfEmpty = true)
	{
		bool flag = HasReserveAmmo();
		if (onlyReloadIfEmpty)
		{
			if (flag && !HasClipAmmo())
			{
				Reload();
			}
		}
		else if (flag)
		{
			Reload();
		}
	}

	public BaseProjectile GetAttachedWeapon()
	{
		return AttachedWeapon as BaseProjectile;
	}

	public virtual bool HasFallbackWeapon()
	{
		return false;
	}

	public bool HasGenericFireable()
	{
		if (AttachedWeapon != null)
		{
			return AttachedWeapon.IsInstrument();
		}
		return false;
	}

	public void UpdateAttachedWeapon()
	{
		Item slot = base.inventory.GetSlot(0);
		HeldEntity heldEntity = null;
		if (slot != null && (slot.info.category == ItemCategory.Weapon || slot.info.category == ItemCategory.Fun))
		{
			BaseEntity heldEntity2 = slot.GetHeldEntity();
			if (heldEntity2 != null)
			{
				HeldEntity component = heldEntity2.GetComponent<HeldEntity>();
				if (component != null && component.IsUsableByTurret)
				{
					heldEntity = component;
				}
			}
		}
		SetFlag(Flags.Reserved3, heldEntity != null);
		if (heldEntity == null)
		{
			if ((bool)GetAttachedWeapon())
			{
				GetAttachedWeapon().SetGenericVisible(wantsVis: false);
				GetAttachedWeapon().SetLightsOn(isOn: false);
			}
			AttachedWeapon = null;
			return;
		}
		heldEntity.SetLightsOn(isOn: true);
		Transform transform = heldEntity.transform;
		Transform muzzleTransform = heldEntity.MuzzleTransform;
		heldEntity.SetParent(null);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		Quaternion quaternion = transform.rotation * Quaternion.Inverse(muzzleTransform.rotation);
		heldEntity.limitNetworking = false;
		heldEntity.SetFlag(Flags.Disabled, b: false);
		heldEntity.SetParent(this, StringPool.Get(socketTransform.name));
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.rotation *= quaternion;
		Vector3 vector = socketTransform.InverseTransformPoint(muzzleTransform.position);
		transform.localPosition = Vector3.left * vector.x;
		float num = Vector3.Distance(muzzleTransform.position, transform.position);
		transform.localPosition += Vector3.forward * num * attachedWeaponZOffsetScale;
		heldEntity.SetGenericVisible(wantsVis: true);
		AttachedWeapon = heldEntity;
		totalAmmoDirty = true;
		Reload();
		UpdateTotalAmmo();
	}

	public override void OnKilled(HitInfo info)
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon != null)
		{
			attachedWeapon.SetGenericVisible(wantsVis: false);
			attachedWeapon.SetLightsOn(isOn: false);
		}
		AttachedWeapon = null;
		base.OnKilled(info);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		UpdateTotalAmmo();
		EnsureReloaded(onlyReloadIfEmpty: false);
		UpdateTotalAmmo();
		nextShotTime = UnityEngine.Time.time;
	}

	public virtual float GetMaxAngleForEngagement()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		float result = ((attachedWeapon == null) ? 1f : ((1f - Mathf.InverseLerp(0.2f, 1f, attachedWeapon.repeatDelay)) * 7f));
		if (UnityEngine.Time.time - lastShotTime > 1f)
		{
			result = 1f;
		}
		return result;
	}

	public void TargetTick()
	{
		if (UnityEngine.Time.realtimeSinceStartup >= nextVisCheck)
		{
			nextVisCheck = UnityEngine.Time.realtimeSinceStartup + UnityEngine.Random.Range(0.2f, 0.3f);
			targetVisible = ObjectVisible(target);
			if (targetVisible)
			{
				lastTargetSeenTime = UnityEngine.Time.realtimeSinceStartup;
			}
		}
		EnsureReloaded();
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (UnityEngine.Time.time >= nextShotTime && targetVisible && Mathf.Abs(AngleToTarget(target, currentAmmoGravity != 0f)) < GetMaxAngleForEngagement())
		{
			if ((bool)attachedWeapon)
			{
				if (attachedWeapon.primaryMagazine.contents > 0)
				{
					FireAttachedGun(AimOffset(target), aimCone, null, PeacekeeperMode() ? target : null);
					float delay = (attachedWeapon.isSemiAuto ? (attachedWeapon.repeatDelay * 1.5f) : attachedWeapon.repeatDelay);
					delay = attachedWeapon.ScaleRepeatDelay(delay);
					nextShotTime = UnityEngine.Time.time + delay;
				}
				else
				{
					nextShotTime = UnityEngine.Time.time + 5f;
				}
			}
			else if (HasFallbackWeapon())
			{
				FireGun(AimOffset(target), aimCone, null, target);
				nextShotTime = UnityEngine.Time.time + 0.115f;
			}
			else if (HasGenericFireable())
			{
				AttachedWeapon.ServerUse();
				nextShotTime = UnityEngine.Time.time + 0.115f;
			}
			else
			{
				nextShotTime = UnityEngine.Time.time + 1f;
			}
		}
		if (target == null || target.IsDead() || UnityEngine.Time.realtimeSinceStartup - lastTargetSeenTime > 3f || Vector3.Distance(base.transform.position, target.transform.position) > sightRange || (PeacekeeperMode() && !IsEntityHostile(target)))
		{
			SetTarget(null);
		}
	}

	public bool HasTarget()
	{
		if (target != null)
		{
			return target.IsAlive();
		}
		return false;
	}

	public void OfflineTick()
	{
		aimDir = Vector3.up;
	}

	public virtual bool IsEntityHostile(BaseCombatEntity ent)
	{
		if (ent is BasePet basePet && basePet.Brain.OwningPlayer != null)
		{
			if (!basePet.Brain.OwningPlayer.IsHostile())
			{
				return ent.IsHostile();
			}
			return true;
		}
		return ent.IsHostile();
	}

	public bool ShouldTarget(BaseCombatEntity targ)
	{
		if (targ is AutoTurret)
		{
			return false;
		}
		if (targ is RidableHorse)
		{
			return false;
		}
		if (targ is BasePet basePet && basePet.Brain.OwningPlayer != null && IsAuthed(basePet.Brain.OwningPlayer))
		{
			return false;
		}
		return true;
	}

	public void ScheduleForTargetScan()
	{
		updateAutoTurretScanQueue.Add(this);
	}

	public void TargetScan()
	{
		if (HasTarget() || IsOffline() || IsBeingRemoteControlled() || targetTrigger.entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in targetTrigger.entityContents)
		{
			if (entityContent == null)
			{
				continue;
			}
			BaseCombatEntity component = entityContent.GetComponent<BaseCombatEntity>();
			if (component == null || !component.IsAlive() || !InFiringArc(component) || !ObjectVisible(component))
			{
				continue;
			}
			if (!Sentry.targetall)
			{
				BasePlayer basePlayer = component as BasePlayer;
				if ((bool)basePlayer && (IsAuthed(basePlayer) || Ignore(basePlayer)))
				{
					continue;
				}
			}
			if (!ShouldTarget(component))
			{
				continue;
			}
			if (PeacekeeperMode())
			{
				if (!IsEntityHostile(component))
				{
					continue;
				}
				if (target == null)
				{
					nextShotTime = UnityEngine.Time.time + 1f;
				}
			}
			SetTarget(component);
			if ((object)target != null)
			{
				break;
			}
		}
	}

	protected virtual bool Ignore(BasePlayer player)
	{
		return false;
	}

	public void ServerTick()
	{
		if (base.isClient || base.IsDestroyed)
		{
			return;
		}
		if (!IsOnline())
		{
			OfflineTick();
		}
		else if (!IsBeingRemoteControlled())
		{
			if (HasTarget())
			{
				TargetTick();
			}
			else
			{
				IdleTick();
			}
		}
		UpdateFacingToTarget();
		if (totalAmmoDirty && UnityEngine.Time.time > nextAmmoCheckTime)
		{
			UpdateTotalAmmo();
			totalAmmoDirty = false;
			nextAmmoCheckTime = UnityEngine.Time.time + 0.5f;
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (((IsOnline() && !HasTarget()) || !targetVisible) && !(info.Initiator as AutoTurret != null) && !(info.Initiator as SamSite != null) && !(info.Initiator as GunTrap != null))
		{
			BasePlayer basePlayer = info.Initiator as BasePlayer;
			if (!basePlayer || !IsAuthed(basePlayer))
			{
				SetTarget(info.Initiator as BaseCombatEntity);
			}
		}
	}

	public void UpdateFacingToTarget()
	{
		if (target != null && targetVisible)
		{
			Vector3 vector = AimOffset(target);
			if (peekIndex != 0)
			{
				Vector3 position = eyePos.transform.position;
				Vector3.Distance(vector, position);
				Vector3 vector2 = Vector3.Cross((vector - position).normalized, Vector3.up);
				vector += vector2 * visibilityOffsets[peekIndex];
			}
			Vector3 vector3 = (vector - eyePos.transform.position).normalized;
			if (currentAmmoGravity != 0f)
			{
				float num = 0.2f;
				if (target is BasePlayer)
				{
					float num2 = Mathf.Clamp01(target.WaterFactor()) * 1.8f;
					if (num2 > num)
					{
						num = num2;
					}
				}
				vector = target.transform.position + Vector3.up * num;
				float angle = GetAngle(eyePos.transform.position, vector, currentAmmoVelocity, currentAmmoGravity);
				Vector3 normalized = (vector.XZ3D() - eyePos.transform.position.XZ3D()).normalized;
				vector3 = Quaternion.LookRotation(normalized) * Quaternion.Euler(angle, 0f, 0f) * Vector3.forward;
			}
			aimDir = vector3;
		}
		UpdateAiming();
	}

	public float GetAngle(Vector3 launchPosition, Vector3 targetPosition, float launchVelocity, float gravityScale)
	{
		float num = UnityEngine.Physics.gravity.y * gravityScale;
		float num2 = Vector3.Distance(launchPosition.XZ3D(), targetPosition.XZ3D());
		float num3 = launchPosition.y - targetPosition.y;
		float num4 = Mathf.Pow(launchVelocity, 2f);
		float num5 = Mathf.Pow(launchVelocity, 4f);
		float num6 = Mathf.Atan((num4 + Mathf.Sqrt(num5 - num * (num * Mathf.Pow(num2, 2f) + 2f * num3 * num4))) / (num * num2)) * 57.29578f;
		float num7 = Mathf.Atan((num4 - Mathf.Sqrt(num5 - num * (num * Mathf.Pow(num2, 2f) + 2f * num3 * num4))) / (num * num2)) * 57.29578f;
		if (float.IsNaN(num6) && float.IsNaN(num7))
		{
			return -45f;
		}
		if (float.IsNaN(num6))
		{
			return num7;
		}
		if (!(num6 > num7))
		{
			return num7;
		}
		return num6;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		AddSelfAuthorize(deployedBy);
	}

	public override uint GetIdealContainer(BasePlayer player, Item item)
	{
		return 0u;
	}

	public override int GetIdealSlot(BasePlayer player, Item item)
	{
		bool num = item.info.category == ItemCategory.Weapon;
		bool flag = item.info.category == ItemCategory.Ammunition;
		if (num)
		{
			return 0;
		}
		if (flag)
		{
			for (int i = 1; i < base.inventory.capacity; i++)
			{
				if (!base.inventory.SlotTaken(item, i))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool IsOnline()
	{
		return IsOn();
	}

	public bool IsOffline()
	{
		return !IsOnline();
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public virtual Transform GetCenterMuzzle()
	{
		return gun_pitch;
	}

	public float AngleToTarget(BaseCombatEntity potentialtarget, bool use2D = false)
	{
		use2D = true;
		Transform centerMuzzle = GetCenterMuzzle();
		Vector3 position = centerMuzzle.position;
		Vector3 vector = AimOffset(potentialtarget);
		Vector3 zero = Vector3.zero;
		return Vector3.Angle(to: (!use2D) ? (vector - position).normalized : Vector3Ex.Direction2D(vector, position), from: use2D ? centerMuzzle.forward.XZ3D().normalized : centerMuzzle.forward);
	}

	public virtual bool InFiringArc(BaseCombatEntity potentialtarget)
	{
		return Mathf.Abs(AngleToTarget(potentialtarget)) <= 90f;
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.CanPickup(player) && IsOffline())
		{
			return IsAuthed(player);
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.autoturret = Facepunch.Pool.Get<ProtoBuf.AutoTurret>();
		info.msg.autoturret.users = authorizedPlayers;
		info.msg.rcEntity = Facepunch.Pool.Get<RCEntity>();
		info.msg.rcEntity.identifier = GetIdentifier();
	}

	public override void PostSave(SaveInfo info)
	{
		base.PostSave(info);
		info.msg.autoturret.users = null;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.autoturret != null)
		{
			authorizedPlayers = info.msg.autoturret.users;
			info.msg.autoturret.users = null;
		}
		if (info.msg.rcEntity != null)
		{
			UpdateIdentifier(info.msg.rcEntity.identifier);
		}
	}

	public Vector3 AimOffset(BaseCombatEntity aimat)
	{
		BasePlayer basePlayer = aimat as BasePlayer;
		if (basePlayer != null)
		{
			if (basePlayer.IsSleeping())
			{
				return basePlayer.transform.position + Vector3.up * 0.1f;
			}
			if (basePlayer.IsWounded())
			{
				return basePlayer.transform.position + Vector3.up * 0.25f;
			}
			return basePlayer.eyes.position;
		}
		return aimat.CenterPoint();
	}

	public float GetAimSpeed()
	{
		if (HasTarget())
		{
			return 5f;
		}
		return 1f;
	}

	public void UpdateAiming()
	{
		if (!(aimDir == Vector3.zero))
		{
			float speed = 5f;
			if (base.isServer)
			{
				speed = ((!HasTarget()) ? 15f : 35f);
			}
			Quaternion quaternion = Quaternion.LookRotation(aimDir);
			Quaternion quaternion2 = Quaternion.Euler(0f, quaternion.eulerAngles.y, 0f);
			Quaternion quaternion3 = Quaternion.Euler(quaternion.eulerAngles.x, 0f, 0f);
			if (gun_yaw.transform.rotation != quaternion2)
			{
				gun_yaw.transform.rotation = Lerp(gun_yaw.transform.rotation, quaternion2, speed);
			}
			if (gun_pitch.transform.localRotation != quaternion3)
			{
				gun_pitch.transform.localRotation = Lerp(gun_pitch.transform.localRotation, quaternion3, speed);
			}
		}
	}

	private static Quaternion Lerp(Quaternion from, Quaternion to, float speed)
	{
		return Quaternion.Lerp(to, from, Mathf.Pow(2f, (0f - speed) * UnityEngine.Time.deltaTime));
	}

	private static Vector3 Lerp(Vector3 from, Vector3 to, float speed)
	{
		return Vector3.Lerp(to, from, Mathf.Pow(2f, (0f - speed) * UnityEngine.Time.deltaTime));
	}

	public bool IsAuthed(ulong id)
	{
		return authorizedPlayers.Any((PlayerNameID x) => x.userid == id);
	}

	public bool IsAuthed(BasePlayer player)
	{
		return authorizedPlayers.Any((PlayerNameID x) => x.userid == player.userID);
	}

	public bool AnyAuthed()
	{
		return authorizedPlayers.Count > 0;
	}

	public virtual bool CanChangeSettings(BasePlayer player)
	{
		if (IsAuthed(player) && IsOffline())
		{
			return player.CanBuild();
		}
		return false;
	}
}
