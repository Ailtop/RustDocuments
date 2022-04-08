#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai;
using Rust.Workshop;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class BaseEntity : BaseNetworkable, IOnParentSpawning, IPrefabPreProcess
{
	public class Menu : Attribute
	{
		[Serializable]
		public struct Option
		{
			public Translate.Phrase name;

			public Translate.Phrase description;

			public Sprite icon;

			public int order;

			public bool usableWhileWounded;
		}

		public class Description : Attribute
		{
			public string token;

			public string english;

			public Description(string t, string e)
			{
				token = t;
				english = e;
			}
		}

		public class Icon : Attribute
		{
			public string icon;

			public Icon(string i)
			{
				icon = i;
			}
		}

		public class ShowIf : Attribute
		{
			public string functionName;

			public ShowIf(string testFunc)
			{
				functionName = testFunc;
			}
		}

		public class UsableWhileWounded : Attribute
		{
		}

		public string TitleToken;

		public string TitleEnglish;

		public string UseVariable;

		public int Order;

		public string ProxyFunction;

		public float Time;

		public string OnStart;

		public string OnProgress;

		public bool LongUseOnly;

		public Menu()
		{
		}

		public Menu(string menuTitleToken, string menuTitleEnglish)
		{
			TitleToken = menuTitleToken;
			TitleEnglish = menuTitleEnglish;
		}
	}

	[Serializable]
	public struct MovementModify
	{
		public float drag;
	}

	[Flags]
	public enum Flags
	{
		Placeholder = 1,
		On = 2,
		OnFire = 4,
		Open = 8,
		Locked = 0x10,
		Debugging = 0x20,
		Disabled = 0x40,
		Reserved1 = 0x80,
		Reserved2 = 0x100,
		Reserved3 = 0x200,
		Reserved4 = 0x400,
		Reserved5 = 0x800,
		Broken = 0x1000,
		Busy = 0x2000,
		Reserved6 = 0x4000,
		Reserved7 = 0x8000,
		Reserved8 = 0x10000,
		Reserved9 = 0x20000,
		Reserved10 = 0x40000,
		Reserved11 = 0x80000
	}

	private readonly struct ServerFileRequest : IEquatable<ServerFileRequest>
	{
		public readonly FileStorage.Type Type;

		public readonly uint NumId;

		public readonly uint Crc;

		public readonly IServerFileReceiver Receiver;

		public readonly float Time;

		public ServerFileRequest(FileStorage.Type type, uint numId, uint crc, IServerFileReceiver receiver)
		{
			Type = type;
			NumId = numId;
			Crc = crc;
			Receiver = receiver;
			Time = UnityEngine.Time.realtimeSinceStartup;
		}

		public bool Equals(ServerFileRequest other)
		{
			if (Type == other.Type && NumId == other.NumId && Crc == other.Crc)
			{
				return object.Equals(Receiver, other.Receiver);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is ServerFileRequest other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(((((uint)((int)Type * 397) ^ NumId) * 397) ^ Crc) * 397) ^ ((Receiver != null) ? Receiver.GetHashCode() : 0);
		}

		public static bool operator ==(ServerFileRequest left, ServerFileRequest right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ServerFileRequest left, ServerFileRequest right)
		{
			return !left.Equals(right);
		}
	}

	public static class Query
	{
		public class EntityTree
		{
			public Grid<BaseEntity> Grid;

			public Grid<BasePlayer> PlayerGrid;

			public Grid<BaseEntity> BrainGrid;

			public EntityTree(float worldSize)
			{
				Grid = new Grid<BaseEntity>(32, worldSize);
				PlayerGrid = new Grid<BasePlayer>(32, worldSize);
				BrainGrid = new Grid<BaseEntity>(32, worldSize);
			}

			public void Add(BaseEntity ent)
			{
				Vector3 position = ent.transform.position;
				Grid.Add(ent, position.x, position.z);
			}

			public void AddPlayer(BasePlayer player)
			{
				Vector3 position = player.transform.position;
				PlayerGrid.Add(player, position.x, position.z);
			}

			public void AddBrain(BaseEntity entity)
			{
				Vector3 position = entity.transform.position;
				BrainGrid.Add(entity, position.x, position.z);
			}

			public void Remove(BaseEntity ent, bool isPlayer = false)
			{
				Grid.Remove(ent);
				if (isPlayer)
				{
					BasePlayer basePlayer = ent as BasePlayer;
					if (basePlayer != null)
					{
						PlayerGrid.Remove(basePlayer);
					}
				}
			}

			public void RemovePlayer(BasePlayer player)
			{
				PlayerGrid.Remove(player);
			}

			public void RemoveBrain(BaseEntity entity)
			{
				if (!(entity == null))
				{
					BrainGrid.Remove(entity);
				}
			}

			public void Move(BaseEntity ent)
			{
				Vector3 position = ent.transform.position;
				Grid.Move(ent, position.x, position.z);
				BasePlayer basePlayer = ent as BasePlayer;
				if (basePlayer != null)
				{
					MovePlayer(basePlayer);
				}
				if (ent.HasBrain)
				{
					MoveBrain(ent);
				}
			}

			public void MovePlayer(BasePlayer player)
			{
				Vector3 position = player.transform.position;
				PlayerGrid.Move(player, position.x, position.z);
			}

			public void MoveBrain(BaseEntity entity)
			{
				Vector3 position = entity.transform.position;
				BrainGrid.Move(entity, position.x, position.z);
			}

			public int GetInSphere(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				return Grid.Query(position.x, position.z, distance, results, filter);
			}

			public int GetPlayersInSphere(Vector3 position, float distance, BasePlayer[] results, Func<BasePlayer, bool> filter = null)
			{
				return PlayerGrid.Query(position.x, position.z, distance, results, filter);
			}

			public int GetBrainsInSphere(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				return BrainGrid.Query(position.x, position.z, distance, results, filter);
			}
		}

		public static EntityTree Server;
	}

	public class RPC_Shared : Attribute
	{
	}

	public struct RPCMessage
	{
		public Connection connection;

		public BasePlayer player;

		public NetRead read;
	}

	public class RPC_Server : RPC_Shared
	{
		public abstract class Conditional : Attribute
		{
			public virtual string GetArgs()
			{
				return null;
			}
		}

		public class MaxDistance : Conditional
		{
			private float maximumDistance;

			public MaxDistance(float maxDist)
			{
				maximumDistance = maxDist;
			}

			public override string GetArgs()
			{
				return maximumDistance.ToString("0.00f");
			}

			public static bool Test(string debugName, BaseEntity ent, BasePlayer player, float maximumDistance)
			{
				return Test(0u, debugName, ent, player, maximumDistance);
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, float maximumDistance)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityDistanceCheck", ent, player, id, debugName, maximumDistance);
				if (obj is bool)
				{
					return (bool)obj;
				}
				return ent.Distance(player.eyes.position) <= maximumDistance;
			}
		}

		public class IsVisible : Conditional
		{
			private float maximumDistance;

			public IsVisible(float maxDist)
			{
				maximumDistance = maxDist;
			}

			public override string GetArgs()
			{
				return maximumDistance.ToString("0.00f");
			}

			public static bool Test(string debugName, BaseEntity ent, BasePlayer player, float maximumDistance)
			{
				return Test(0u, debugName, ent, player, maximumDistance);
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, float maximumDistance)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityVisibilityCheck", ent, player, id, debugName, maximumDistance);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (GamePhysics.LineOfSight(player.eyes.center, player.eyes.position, 2162688))
				{
					if (!ent.IsVisible(player.eyes.HeadRay(), 1218519041, maximumDistance))
					{
						return ent.IsVisible(player.eyes.position, maximumDistance);
					}
					return true;
				}
				return false;
			}
		}

		public class FromOwner : Conditional
		{
			public static bool Test(string debugName, BaseEntity ent, BasePlayer player)
			{
				return Test(0u, debugName, ent, player);
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				if (ent.net == null || player.net == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityFromOwnerCheck", ent, player, id, debugName);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (ent.net.ID == player.net.ID)
				{
					return true;
				}
				if (ent.parentEntity.uid != player.net.ID)
				{
					return false;
				}
				return true;
			}
		}

		public class IsActiveItem : Conditional
		{
			public static bool Test(string debugName, BaseEntity ent, BasePlayer player)
			{
				return Test(0u, debugName, ent, player);
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				if (ent.net == null || player.net == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityActiveCheck", ent, player, id, debugName);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (ent.net.ID == player.net.ID)
				{
					return true;
				}
				if (ent.parentEntity.uid != player.net.ID)
				{
					return false;
				}
				Item activeItem = player.GetActiveItem();
				if (activeItem == null)
				{
					return false;
				}
				if (activeItem.GetHeldEntity() != ent)
				{
					return false;
				}
				return true;
			}
		}

		public class CallsPerSecond : Conditional
		{
			private ulong callsPerSecond;

			public CallsPerSecond(ulong limit)
			{
				callsPerSecond = limit;
			}

			public override string GetArgs()
			{
				return callsPerSecond.ToString();
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, ulong callsPerSecond)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				return player.rpcHistory.TryIncrement(id, callsPerSecond);
			}
		}
	}

	public enum Signal
	{
		Attack = 0,
		Alt_Attack = 1,
		DryFire = 2,
		Reload = 3,
		Deploy = 4,
		Flinch_Head = 5,
		Flinch_Chest = 6,
		Flinch_Stomach = 7,
		Flinch_RearHead = 8,
		Flinch_RearTorso = 9,
		Throw = 10,
		Relax = 11,
		Gesture = 12,
		PhysImpact = 13,
		Eat = 14,
		Startled = 15,
		Admire = 16
	}

	public enum Slot
	{
		Lock = 0,
		FireMod = 1,
		UpperModifier = 2,
		MiddleModifier = 3,
		LowerModifier = 4,
		CenterDecoration = 5,
		LowerCenterDecoration = 6,
		StorageMonitor = 7,
		Count = 8
	}

	[Flags]
	public enum TraitFlag
	{
		None = 0,
		Alive = 1,
		Animal = 2,
		Human = 4,
		Interesting = 8,
		Food = 0x10,
		Meat = 0x20,
		Water = 0x20
	}

	public static class Util
	{
		public static BaseEntity[] FindTargets(string strFilter, bool onlyPlayers)
		{
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BasePlayer)
					{
						BasePlayer basePlayer = x as BasePlayer;
						if (string.IsNullOrEmpty(strFilter))
						{
							return true;
						}
						if (strFilter == "!alive" && basePlayer.IsAlive())
						{
							return true;
						}
						if (strFilter == "!sleeping" && basePlayer.IsSleeping())
						{
							return true;
						}
						if (strFilter[0] != '!' && !basePlayer.displayName.Contains(strFilter, CompareOptions.IgnoreCase) && !basePlayer.UserIDString.Contains(strFilter))
						{
							return false;
						}
						return true;
					}
					if (onlyPlayers)
					{
						return false;
					}
					if (string.IsNullOrEmpty(strFilter))
					{
						return false;
					}
					return x.ShortPrefabName.Contains(strFilter) ? true : false;
				})
				select x as BaseEntity).ToArray();
		}

		public static BaseEntity[] FindTargetsOwnedBy(ulong ownedBy, string strFilter)
		{
			bool hasFilter = !string.IsNullOrEmpty(strFilter);
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BaseEntity baseEntity)
					{
						if (baseEntity.OwnerID != ownedBy)
						{
							return false;
						}
						if (!hasFilter || baseEntity.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					return false;
				})
				select x as BaseEntity).ToArray();
		}

		public static BaseEntity[] FindTargetsAuthedTo(ulong authId, string strFilter)
		{
			bool hasFilter = !string.IsNullOrEmpty(strFilter);
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BuildingPrivlidge buildingPrivlidge)
					{
						if (!buildingPrivlidge.IsAuthed(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is AutoTurret autoTurret)
					{
						if (!autoTurret.IsAuthed(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is CodeLock codeLock)
					{
						if (!codeLock.whitelistPlayers.Contains(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					return false;
				})
				select x as BaseEntity).ToArray();
		}

		public static ScientistNPC[] FindScientists()
		{
			return (from x in BaseNetworkable.serverEntities
				where x is ScientistNPC
				select x as ScientistNPC).ToArray();
		}
	}

	public enum GiveItemReason
	{
		Generic = 0,
		ResourceHarvested = 1,
		PickedUp = 2,
		Crafted = 3
	}

	private static Queue<BaseEntity> globalBroadcastQueue = new Queue<BaseEntity>();

	private static uint globalBroadcastProtocol = 0u;

	private uint broadcastProtocol;

	public List<EntityLink> links = new List<EntityLink>();

	private bool linkedToNeighbours;

	[NonSerialized]
	public BaseEntity creatorEntity;

	public int ticksSinceStopped;

	private int doneMovingWithoutARigidBodyCheck = 1;

	public bool isCallingUpdateNetworkGroup;

	private EntityRef[] entitySlots = new EntityRef[8];

	public List<TriggerBase> triggers;

	protected bool isVisible = true;

	protected bool isAnimatorVisible = true;

	protected bool isShadowVisible = true;

	protected OccludeeSphere localOccludee = new OccludeeSphere(-1);

	[Header("BaseEntity")]
	public Bounds bounds;

	public GameObjectRef impactEffect;

	public bool enableSaving = true;

	public bool syncPosition;

	public Model model;

	[InspectorFlags]
	public Flags flags;

	[NonSerialized]
	public uint parentBone;

	[NonSerialized]
	public ulong skinID;

	private EntityComponentBase[] _components;

	[HideInInspector]
	public bool HasBrain;

	[NonSerialized]
	public string _name;

	public Spawnable _spawnable;

	public static HashSet<BaseEntity> saveList = new HashSet<BaseEntity>();

	public virtual float RealisticMass => 100f;

	public float radiationLevel
	{
		get
		{
			if (triggers == null)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerRadiation triggerRadiation = triggers[i] as TriggerRadiation;
				if (!(triggerRadiation == null))
				{
					Vector3 position = GetNetworkPosition();
					BaseEntity baseEntity = GetParentEntity();
					if (baseEntity != null)
					{
						position = baseEntity.transform.TransformPoint(position);
					}
					num = Mathf.Max(num, triggerRadiation.GetRadiation(position, RadiationProtection()));
				}
			}
			return num;
		}
	}

	public float currentTemperature
	{
		get
		{
			float num = Climate.GetTemperature(base.transform.position);
			if (triggers == null)
			{
				return num;
			}
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerTemperature triggerTemperature = triggers[i] as TriggerTemperature;
				if (!(triggerTemperature == null))
				{
					num = triggerTemperature.WorkoutTemperature(GetNetworkPosition(), num);
				}
			}
			return num;
		}
	}

	public float currentEnvironmentalWetness
	{
		get
		{
			if (triggers == null)
			{
				return 0f;
			}
			float num = 0f;
			Vector3 networkPosition = GetNetworkPosition();
			foreach (TriggerBase trigger in triggers)
			{
				if (trigger is TriggerWetness triggerWetness)
				{
					num += triggerWetness.WorkoutWetness(networkPosition);
				}
			}
			return Mathf.Clamp01(num);
		}
	}

	public virtual float PositionTickRate => 0.1f;

	public virtual bool PositionTickFixedTime => false;

	public virtual Vector3 ServerPosition
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			if (!(base.transform.localPosition == value))
			{
				base.transform.localPosition = value;
				base.transform.hasChanged = true;
			}
		}
	}

	public virtual Quaternion ServerRotation
	{
		get
		{
			return base.transform.localRotation;
		}
		set
		{
			if (!(base.transform.localRotation == value))
			{
				base.transform.localRotation = value;
				base.transform.hasChanged = true;
			}
		}
	}

	public virtual TraitFlag Traits => TraitFlag.None;

	public float Weight { get; protected set; }

	public EntityComponentBase[] Components => _components ?? (_components = GetComponentsInChildren<EntityComponentBase>(includeInactive: true));

	public virtual bool IsNpc => false;

	public ulong OwnerID { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseEntity.OnRpcMessage"))
		{
			if (rpc == 1552640099 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - BroadcastSignalFromClient "));
				}
				using (TimeWarning.New("BroadcastSignalFromClient"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1552640099u, "BroadcastSignalFromClient", this, player))
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
							BroadcastSignalFromClient(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in BroadcastSignalFromClient");
					}
				}
				return true;
			}
			if (rpc == 3645147041u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SV_RequestFile "));
				}
				using (TimeWarning.New("SV_RequestFile"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							SV_RequestFile(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SV_RequestFile");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		throw new NotImplementedException();
	}

	protected void ReceiveCollisionMessages(bool b)
	{
		if (b)
		{
			base.gameObject.transform.GetOrAddComponent<EntityCollisionMessage>();
		}
		else
		{
			UnityEngine.TransformEx.RemoveComponent<EntityCollisionMessage>(base.gameObject.transform);
		}
	}

	public virtual void DebugServer(int rep, float time)
	{
		DebugText(base.transform.position + Vector3.up * 1f, $"{((net != null) ? net.ID : 0u)}: {base.name}\n{DebugText()}", Color.white, time);
	}

	public virtual string DebugText()
	{
		return "";
	}

	public void OnDebugStart()
	{
		EntityDebug entityDebug = base.gameObject.GetComponent<EntityDebug>();
		if (entityDebug == null)
		{
			entityDebug = base.gameObject.AddComponent<EntityDebug>();
		}
		entityDebug.enabled = true;
	}

	protected void DebugText(Vector3 pos, string str, Color color, float time)
	{
		if (base.isServer)
		{
			ConsoleNetwork.BroadcastToAllClients("ddraw.text", time, color, pos, str);
		}
	}

	public bool HasFlag(Flags f)
	{
		return (flags & f) == f;
	}

	public bool ParentHasFlag(Flags f)
	{
		BaseEntity baseEntity = GetParentEntity();
		if (baseEntity == null)
		{
			return false;
		}
		return baseEntity.HasFlag(f);
	}

	public void SetFlag(Flags f, bool b, bool recursive = false, bool networkupdate = true)
	{
		Flags old = flags;
		if (b)
		{
			if (HasFlag(f))
			{
				return;
			}
			flags |= f;
		}
		else
		{
			if (!HasFlag(f))
			{
				return;
			}
			flags &= ~f;
		}
		OnFlagsChanged(old, flags);
		if (networkupdate)
		{
			SendNetworkUpdate();
		}
		else
		{
			InvalidateNetworkCache();
		}
		if (recursive && children != null)
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].SetFlag(f, b, recursive: true);
			}
		}
	}

	public bool IsOn()
	{
		return HasFlag(Flags.On);
	}

	public bool IsOpen()
	{
		return HasFlag(Flags.Open);
	}

	public bool IsOnFire()
	{
		return HasFlag(Flags.OnFire);
	}

	public bool IsLocked()
	{
		return HasFlag(Flags.Locked);
	}

	public override bool IsDebugging()
	{
		return HasFlag(Flags.Debugging);
	}

	public bool IsDisabled()
	{
		if (!HasFlag(Flags.Disabled))
		{
			return ParentHasFlag(Flags.Disabled);
		}
		return true;
	}

	public bool IsBroken()
	{
		return HasFlag(Flags.Broken);
	}

	public bool IsBusy()
	{
		return HasFlag(Flags.Busy);
	}

	public override string GetLogColor()
	{
		if (base.isServer)
		{
			return "cyan";
		}
		return "yellow";
	}

	public virtual void OnFlagsChanged(Flags old, Flags next)
	{
		if (IsDebugging() && (old & Flags.Debugging) != (next & Flags.Debugging))
		{
			OnDebugStart();
		}
	}

	public void SendNetworkUpdate_Flags()
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave || base.IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate_Flags"))
		{
			LogEntry(LogEntryType.Network, 2, "SendNetworkUpdate_Flags");
			if (Interface.CallHook("OnEntityFlagsNetworkUpdate", this) == null)
			{
				List<Connection> subscribers = GetSubscribers();
				if (subscribers != null && subscribers.Count > 0 && Network.Net.sv.write.Start())
				{
					Network.Net.sv.write.PacketID(Message.Type.EntityFlags);
					Network.Net.sv.write.EntityID(net.ID);
					Network.Net.sv.write.Int32((int)flags);
					SendInfo info = new SendInfo(subscribers);
					Network.Net.sv.write.Send(info);
				}
				OnSendNetworkUpdateEx.SendOnSendNetworkUpdate(base.gameObject, this);
			}
		}
	}

	public bool IsOccupied(Socket_Base socket)
	{
		return FindLink(socket)?.IsOccupied() ?? false;
	}

	public bool IsOccupied(string socketName)
	{
		return FindLink(socketName)?.IsOccupied() ?? false;
	}

	public EntityLink FindLink(Socket_Base socket)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			if (entityLinks[i].socket == socket)
			{
				return entityLinks[i];
			}
		}
		return null;
	}

	public EntityLink FindLink(string socketName)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			if (entityLinks[i].socket.socketName == socketName)
			{
				return entityLinks[i];
			}
		}
		return null;
	}

	public EntityLink FindLink(string[] socketNames)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			for (int j = 0; j < socketNames.Length; j++)
			{
				if (entityLinks[i].socket.socketName == socketNames[j])
				{
					return entityLinks[i];
				}
			}
		}
		return null;
	}

	public T FindLinkedEntity<T>() where T : BaseEntity
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				EntityLink entityLink2 = entityLink.connections[j];
				if (entityLink2.owner is T)
				{
					return entityLink2.owner as T;
				}
			}
		}
		return null;
	}

	public void EntityLinkMessage<T>(Action<T> action) where T : BaseEntity
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				EntityLink entityLink2 = entityLink.connections[j];
				if (entityLink2.owner is T)
				{
					action(entityLink2.owner as T);
				}
			}
		}
	}

	public void EntityLinkBroadcast<T, S>(Action<T> action, Func<S, bool> canTraverseSocket) where T : BaseEntity where S : Socket_Base
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		if (this is T)
		{
			action(this as T);
		}
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				if (!(entityLink.socket is S) || !canTraverseSocket(entityLink.socket as S))
				{
					continue;
				}
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
						if (owner is T)
						{
							action(owner as T);
						}
					}
				}
			}
		}
	}

	public void EntityLinkBroadcast<T>(Action<T> action) where T : BaseEntity
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		if (this is T)
		{
			action(this as T);
		}
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
						if (owner is T)
						{
							action(owner as T);
						}
					}
				}
			}
		}
	}

	public void EntityLinkBroadcast()
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
					}
				}
			}
		}
	}

	public bool ReceivedEntityLinkBroadcast()
	{
		return broadcastProtocol == globalBroadcastProtocol;
	}

	public List<EntityLink> GetEntityLinks(bool linkToNeighbours = true)
	{
		if (Rust.Application.isLoadingSave)
		{
			return links;
		}
		if (!linkedToNeighbours && linkToNeighbours)
		{
			LinkToNeighbours();
		}
		return links;
	}

	private void LinkToEntity(BaseEntity other)
	{
		if (this == other || links.Count == 0 || other.links.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("LinkToEntity"))
		{
			for (int i = 0; i < links.Count; i++)
			{
				EntityLink entityLink = links[i];
				for (int j = 0; j < other.links.Count; j++)
				{
					EntityLink entityLink2 = other.links[j];
					if (entityLink.CanConnect(entityLink2))
					{
						if (!entityLink.Contains(entityLink2))
						{
							entityLink.Add(entityLink2);
						}
						if (!entityLink2.Contains(entityLink))
						{
							entityLink2.Add(entityLink);
						}
					}
				}
			}
		}
	}

	private void LinkToNeighbours()
	{
		if (links.Count == 0)
		{
			return;
		}
		linkedToNeighbours = true;
		using (TimeWarning.New("LinkToNeighbours"))
		{
			List<BaseEntity> obj = Facepunch.Pool.GetList<BaseEntity>();
			OBB oBB = WorldSpaceBounds();
			Vis.Entities(oBB.position, oBB.extents.magnitude + 1f, obj);
			for (int i = 0; i < obj.Count; i++)
			{
				BaseEntity baseEntity = obj[i];
				if (baseEntity.isServer == base.isServer)
				{
					LinkToEntity(baseEntity);
				}
			}
			Facepunch.Pool.FreeList(ref obj);
		}
	}

	private void InitEntityLinks()
	{
		using (TimeWarning.New("InitEntityLinks"))
		{
			if (base.isServer)
			{
				EntityLinkEx.AddLinks(links, this, PrefabAttribute.server.FindAll<Socket_Base>(prefabID));
			}
		}
	}

	private void FreeEntityLinks()
	{
		using (TimeWarning.New("FreeEntityLinks"))
		{
			EntityLinkEx.FreeLinks(links);
			linkedToNeighbours = false;
		}
	}

	public void RefreshEntityLinks()
	{
		using (TimeWarning.New("RefreshEntityLinks"))
		{
			EntityLinkEx.ClearLinks(links);
			LinkToNeighbours();
		}
	}

	[RPC_Server]
	public void SV_RequestFile(RPCMessage msg)
	{
		uint num = msg.read.UInt32();
		FileStorage.Type type = (FileStorage.Type)msg.read.UInt8();
		string funcName = StringPool.Get(msg.read.UInt32());
		uint num2 = ((msg.read.Unread > 0) ? msg.read.UInt32() : 0u);
		byte[] array = FileStorage.server.Get(num, type, net.ID, num2);
		SendInfo sendInfo = new SendInfo(msg.connection);
		sendInfo.channel = 2;
		sendInfo.method = SendMethod.Reliable;
		SendInfo sendInfo2 = sendInfo;
		ClientRPCEx(sendInfo2, null, funcName, num, (array != null) ? ((uint)array.Length) : 0u, array ?? Array.Empty<byte>(), num2, (byte)type);
	}

	public void SetParent(BaseEntity entity, bool worldPositionStays = false, bool sendImmediate = false)
	{
		SetParent(entity, 0u, worldPositionStays, sendImmediate);
	}

	public void SetParent(BaseEntity entity, string strBone, bool worldPositionStays = false, bool sendImmediate = false)
	{
		SetParent(entity, (!string.IsNullOrEmpty(strBone)) ? StringPool.Get(strBone) : 0u, worldPositionStays, sendImmediate);
	}

	public bool HasChild(BaseEntity c)
	{
		if (c == this)
		{
			return true;
		}
		BaseEntity baseEntity = c.GetParentEntity();
		if (baseEntity != null)
		{
			return HasChild(baseEntity);
		}
		return false;
	}

	public void SetParent(BaseEntity entity, uint boneID, bool worldPositionStays = false, bool sendImmediate = false)
	{
		if (entity != null)
		{
			if (entity == this)
			{
				Debug.LogError("Trying to parent to self " + this, base.gameObject);
				return;
			}
			if (HasChild(entity))
			{
				Debug.LogError("Trying to parent to child " + this, base.gameObject);
				return;
			}
		}
		LogEntry(LogEntryType.Hierarchy, 2, "SetParent {0} {1}", entity, boneID);
		BaseEntity baseEntity = GetParentEntity();
		if ((bool)baseEntity)
		{
			baseEntity.RemoveChild(this);
		}
		if (base.limitNetworking && baseEntity != null && baseEntity != entity)
		{
			BasePlayer basePlayer = baseEntity as BasePlayer;
			if (BaseEntityEx.IsValid(basePlayer))
			{
				DestroyOnClient(basePlayer.net.connection);
			}
		}
		if (entity == null)
		{
			OnParentChanging(baseEntity, null);
			parentEntity.Set(null);
			base.transform.SetParent(null, worldPositionStays);
			parentBone = 0u;
			UpdateNetworkGroup();
			if (sendImmediate)
			{
				SendNetworkUpdateImmediate();
				SendChildrenNetworkUpdateImmediate();
			}
			else
			{
				SendNetworkUpdate();
				SendChildrenNetworkUpdate();
			}
			return;
		}
		Debug.Assert(entity.isServer, "SetParent - child should be a SERVER entity");
		Debug.Assert(entity.net != null, "Setting parent to entity that hasn't spawned yet! (net is null)");
		Debug.Assert(entity.net.ID != 0, "Setting parent to entity that hasn't spawned yet! (id = 0)");
		entity.AddChild(this);
		OnParentChanging(baseEntity, entity);
		parentEntity.Set(entity);
		if (boneID != 0 && boneID != StringPool.closest)
		{
			base.transform.SetParent(entity.FindBone(StringPool.Get(boneID)), worldPositionStays);
		}
		else
		{
			base.transform.SetParent(entity.transform, worldPositionStays);
		}
		parentBone = boneID;
		UpdateNetworkGroup();
		if (sendImmediate)
		{
			SendNetworkUpdateImmediate();
			SendChildrenNetworkUpdateImmediate();
		}
		else
		{
			SendNetworkUpdate();
			SendChildrenNetworkUpdate();
		}
	}

	public void DestroyOnClient(Connection connection)
	{
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				child.DestroyOnClient(connection);
			}
		}
		if (Network.Net.sv.IsConnected() && Network.Net.sv.write.Start())
		{
			Network.Net.sv.write.PacketID(Message.Type.EntityDestroy);
			Network.Net.sv.write.EntityID(net.ID);
			Network.Net.sv.write.UInt8(0);
			Network.Net.sv.write.Send(new SendInfo(connection));
			LogEntry(LogEntryType.Network, 2, "EntityDestroy");
		}
	}

	public void SendChildrenNetworkUpdate()
	{
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.UpdateNetworkGroup();
			child.SendNetworkUpdate();
		}
	}

	public void SendChildrenNetworkUpdateImmediate()
	{
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.UpdateNetworkGroup();
			child.SendNetworkUpdateImmediate();
		}
	}

	public virtual void SwitchParent(BaseEntity ent)
	{
		Log("SwitchParent Missed " + ent);
	}

	public virtual void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			if (oldParent != null)
			{
				component.velocity += oldParent.GetWorldVelocity();
			}
			if (newParent != null)
			{
				component.velocity -= newParent.GetWorldVelocity();
			}
		}
	}

	public virtual BuildingPrivlidge GetBuildingPrivilege()
	{
		return GetBuildingPrivilege(WorldSpaceBounds());
	}

	public BuildingPrivlidge GetBuildingPrivilege(OBB obb)
	{
		object obj = Interface.CallHook("OnBuildingPrivilege", this, obb);
		if (obj is BuildingPrivlidge)
		{
			return (BuildingPrivlidge)obj;
		}
		BuildingBlock other = null;
		BuildingPrivlidge result = null;
		List<BuildingBlock> obj2 = Facepunch.Pool.GetList<BuildingBlock>();
		Vis.Entities(obb.position, 16f + obb.extents.magnitude, obj2, 2097152);
		for (int i = 0; i < obj2.Count; i++)
		{
			BuildingBlock buildingBlock = obj2[i];
			if (buildingBlock.isServer != base.isServer || !buildingBlock.IsOlderThan(other) || obb.Distance(buildingBlock.WorldSpaceBounds()) > 16f)
			{
				continue;
			}
			BuildingManager.Building building = buildingBlock.GetBuilding();
			if (building != null)
			{
				BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
				if (!(dominatingBuildingPrivilege == null))
				{
					other = buildingBlock;
					result = dominatingBuildingPrivilege;
				}
			}
		}
		Facepunch.Pool.FreeList(ref obj2);
		return result;
	}

	public void SV_RPCMessage(uint nameID, Message message)
	{
		Assert.IsTrue(base.isServer, "Should be server!");
		BasePlayer basePlayer = NetworkPacketEx.Player(message);
		if (!BaseEntityEx.IsValid(basePlayer))
		{
			if (ConVar.Global.developer > 0)
			{
				Debug.Log("SV_RPCMessage: From invalid player " + basePlayer);
			}
		}
		else if (basePlayer.isStalled)
		{
			if (ConVar.Global.developer > 0)
			{
				Debug.Log("SV_RPCMessage: player is stalled " + basePlayer);
			}
		}
		else if (!OnRpcMessage(basePlayer, nameID, message))
		{
			for (int i = 0; i < Components.Length && !Components[i].OnRpcMessage(basePlayer, nameID, message); i++)
			{
			}
		}
	}

	public void ClientRPCPlayer<T1, T2, T3, T4, T5>(Connection sourceConnection, BasePlayer player, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null && player.net.connection != null)
		{
			ClientRPCEx(new SendInfo(player.net.connection), sourceConnection, funcName, arg1, arg2, arg3, arg4, arg5);
		}
	}

	public void ClientRPCPlayer<T1, T2, T3, T4>(Connection sourceConnection, BasePlayer player, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null && player.net.connection != null)
		{
			ClientRPCEx(new SendInfo(player.net.connection), sourceConnection, funcName, arg1, arg2, arg3, arg4);
		}
	}

	public void ClientRPCPlayer<T1, T2, T3>(Connection sourceConnection, BasePlayer player, string funcName, T1 arg1, T2 arg2, T3 arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null && player.net.connection != null)
		{
			ClientRPCEx(new SendInfo(player.net.connection), sourceConnection, funcName, arg1, arg2, arg3);
		}
	}

	public void ClientRPCPlayer<T1, T2>(Connection sourceConnection, BasePlayer player, string funcName, T1 arg1, T2 arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null && player.net.connection != null)
		{
			ClientRPCEx(new SendInfo(player.net.connection), sourceConnection, funcName, arg1, arg2);
		}
	}

	public void ClientRPCPlayer<T1>(Connection sourceConnection, BasePlayer player, string funcName, T1 arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null && player.net.connection != null)
		{
			ClientRPCEx(new SendInfo(player.net.connection), sourceConnection, funcName, arg1);
		}
	}

	public void ClientRPCPlayer(Connection sourceConnection, BasePlayer player, string funcName)
	{
		if (Network.Net.sv.IsConnected() && net != null && player.net.connection != null)
		{
			ClientRPCEx(new SendInfo(player.net.connection), sourceConnection, funcName);
		}
	}

	public void ClientRPC<T1, T2, T3, T4, T5>(Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null && net.group != null)
		{
			ClientRPCEx(new SendInfo(net.group.subscribers), sourceConnection, funcName, arg1, arg2, arg3, arg4, arg5);
		}
	}

	public void ClientRPC<T1, T2, T3, T4>(Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null && net.group != null)
		{
			ClientRPCEx(new SendInfo(net.group.subscribers), sourceConnection, funcName, arg1, arg2, arg3, arg4);
		}
	}

	public void ClientRPC<T1, T2, T3>(Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null && net.group != null)
		{
			ClientRPCEx(new SendInfo(net.group.subscribers), sourceConnection, funcName, arg1, arg2, arg3);
		}
	}

	public void ClientRPC<T1, T2>(Connection sourceConnection, string funcName, T1 arg1, T2 arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null && net.group != null)
		{
			ClientRPCEx(new SendInfo(net.group.subscribers), sourceConnection, funcName, arg1, arg2);
		}
	}

	public void ClientRPC<T1>(Connection sourceConnection, string funcName, T1 arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null && net.group != null)
		{
			ClientRPCEx(new SendInfo(net.group.subscribers), sourceConnection, funcName, arg1);
		}
	}

	public void ClientRPC(Connection sourceConnection, string funcName)
	{
		if (Network.Net.sv.IsConnected() && net != null && net.group != null)
		{
			ClientRPCEx(new SendInfo(net.group.subscribers), sourceConnection, funcName);
		}
	}

	public void ClientRPCEx<T1, T2, T3, T4, T5>(SendInfo sendInfo, Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null && ClientRPCStart(sourceConnection, funcName))
		{
			ClientRPCWrite(arg1);
			ClientRPCWrite(arg2);
			ClientRPCWrite(arg3);
			ClientRPCWrite(arg4);
			ClientRPCWrite(arg5);
			ClientRPCSend(sendInfo);
		}
	}

	public void ClientRPCEx<T1, T2, T3, T4>(SendInfo sendInfo, Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null && ClientRPCStart(sourceConnection, funcName))
		{
			ClientRPCWrite(arg1);
			ClientRPCWrite(arg2);
			ClientRPCWrite(arg3);
			ClientRPCWrite(arg4);
			ClientRPCSend(sendInfo);
		}
	}

	public void ClientRPCEx<T1, T2, T3>(SendInfo sendInfo, Connection sourceConnection, string funcName, T1 arg1, T2 arg2, T3 arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null && ClientRPCStart(sourceConnection, funcName))
		{
			ClientRPCWrite(arg1);
			ClientRPCWrite(arg2);
			ClientRPCWrite(arg3);
			ClientRPCSend(sendInfo);
		}
	}

	public void ClientRPCEx<T1, T2>(SendInfo sendInfo, Connection sourceConnection, string funcName, T1 arg1, T2 arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null && ClientRPCStart(sourceConnection, funcName))
		{
			ClientRPCWrite(arg1);
			ClientRPCWrite(arg2);
			ClientRPCSend(sendInfo);
		}
	}

	public void ClientRPCEx<T1>(SendInfo sendInfo, Connection sourceConnection, string funcName, T1 arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null && ClientRPCStart(sourceConnection, funcName))
		{
			ClientRPCWrite(arg1);
			ClientRPCSend(sendInfo);
		}
	}

	public void ClientRPCEx(SendInfo sendInfo, Connection sourceConnection, string funcName)
	{
		if (Network.Net.sv.IsConnected() && net != null && ClientRPCStart(sourceConnection, funcName))
		{
			ClientRPCSend(sendInfo);
		}
	}

	private bool ClientRPCStart(Connection sourceConnection, string funcName)
	{
		if (Network.Net.sv.write.Start())
		{
			Network.Net.sv.write.PacketID(Message.Type.RPCMessage);
			Network.Net.sv.write.UInt32(net.ID);
			Network.Net.sv.write.UInt32(StringPool.Get(funcName));
			Network.Net.sv.write.UInt64(sourceConnection?.userid ?? 0);
			return true;
		}
		return false;
	}

	private void ClientRPCWrite<T>(T arg)
	{
		NetworkWriteEx.WriteObject(Network.Net.sv.write, arg);
	}

	private void ClientRPCSend(SendInfo sendInfo)
	{
		Network.Net.sv.write.Send(sendInfo);
	}

	public virtual float RadiationProtection()
	{
		return 0f;
	}

	public virtual float RadiationExposureFraction()
	{
		return 1f;
	}

	public virtual Vector3 GetLocalVelocityServer()
	{
		return Vector3.zero;
	}

	public virtual Quaternion GetAngularVelocityServer()
	{
		return Quaternion.identity;
	}

	public void EnableGlobalBroadcast(bool wants)
	{
		if (globalBroadcast != wants)
		{
			globalBroadcast = wants;
			UpdateNetworkGroup();
		}
	}

	public void EnableSaving(bool wants)
	{
		if (enableSaving == wants)
		{
			return;
		}
		enableSaving = wants;
		if (enableSaving)
		{
			if (!saveList.Contains(this))
			{
				saveList.Add(this);
			}
		}
		else
		{
			saveList.Remove(this);
		}
	}

	public override void ServerInit()
	{
		_spawnable = GetComponent<Spawnable>();
		base.ServerInit();
		if (enableSaving && !saveList.Contains(this))
		{
			saveList.Add(this);
		}
		if (flags != 0)
		{
			OnFlagsChanged((Flags)0, flags);
		}
		if (syncPosition && PositionTickRate >= 0f)
		{
			if (PositionTickFixedTime)
			{
				InvokeRepeatingFixedTime(NetworkPositionTick);
			}
			else
			{
				InvokeRandomized(NetworkPositionTick, PositionTickRate, PositionTickRate - PositionTickRate * 0.05f, PositionTickRate * 0.05f);
			}
		}
		Query.Server.Add(this);
	}

	public virtual void OnSensation(Sensation sensation)
	{
	}

	public void NetworkPositionTick()
	{
		if (!base.transform.hasChanged)
		{
			if (ticksSinceStopped >= 3)
			{
				return;
			}
			ticksSinceStopped++;
		}
		else
		{
			ticksSinceStopped = 0;
		}
		TransformChanged();
		base.transform.hasChanged = false;
	}

	public void TransformChanged()
	{
		if (Query.Server != null)
		{
			Query.Server.Move(this);
		}
		if (net == null)
		{
			return;
		}
		InvalidateNetworkCache();
		if (!globalBroadcast && !ValidBounds.Test(base.transform.position))
		{
			OnInvalidPosition();
		}
		else if (syncPosition)
		{
			if (!isCallingUpdateNetworkGroup)
			{
				Invoke(UpdateNetworkGroup, 5f);
				isCallingUpdateNetworkGroup = true;
			}
			SendNetworkUpdate_Position();
			OnPositionalNetworkUpdate();
		}
	}

	public virtual void OnPositionalNetworkUpdate()
	{
	}

	public void DoMovingWithoutARigidBodyCheck()
	{
		if (doneMovingWithoutARigidBodyCheck <= 10)
		{
			doneMovingWithoutARigidBodyCheck++;
			if (doneMovingWithoutARigidBodyCheck >= 10 && !(GetComponent<Collider>() == null) && GetComponent<Rigidbody>() == null)
			{
				Debug.LogWarning(string.Concat("Entity moving without a rigid body! (", base.gameObject, ")"), this);
			}
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		if (base.isServer)
		{
			OnParentSpawningEx.BroadcastOnParentSpawning(base.gameObject);
		}
	}

	public void OnParentSpawning()
	{
		if (net != null || base.IsDestroyed)
		{
			return;
		}
		if (Rust.Application.isLoadingSave)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (GameManager.server.preProcessed.NeedsProcessing(base.gameObject))
		{
			GameManager.server.preProcessed.ProcessObject(null, base.gameObject, resetLocalTransform: false);
		}
		BaseEntity baseEntity = ((base.transform.parent != null) ? base.transform.parent.GetComponentInParent<BaseEntity>() : null);
		Spawn();
		if (baseEntity != null)
		{
			SetParent(baseEntity, worldPositionStays: true);
		}
	}

	public void SpawnAsMapEntity()
	{
		if (net == null && !base.IsDestroyed && ((base.transform.parent != null) ? base.transform.parent.GetComponentInParent<BaseEntity>() : null) == null)
		{
			if (GameManager.server.preProcessed.NeedsProcessing(base.gameObject))
			{
				GameManager.server.preProcessed.ProcessObject(null, base.gameObject, resetLocalTransform: false);
			}
			base.transform.parent = null;
			SceneManager.MoveGameObjectToScene(base.gameObject, Rust.Server.EntityScene);
			base.gameObject.SetActive(value: true);
			Spawn();
		}
	}

	public virtual void PostMapEntitySpawn()
	{
	}

	internal override void DoServerDestroy()
	{
		CancelInvoke(NetworkPositionTick);
		saveList.Remove(this);
		RemoveFromTriggers();
		if (children != null)
		{
			BaseEntity[] array = children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnParentRemoved();
			}
		}
		SetParent(null, worldPositionStays: true);
		Query.Server.Remove(this);
		base.DoServerDestroy();
	}

	internal virtual void OnParentRemoved()
	{
		Kill();
	}

	public virtual void OnInvalidPosition()
	{
		Debug.Log(string.Concat("Invalid Position: ", this, " ", base.transform.position, " (destroying)"));
		Kill();
	}

	public BaseCorpse DropCorpse(string strCorpsePrefab)
	{
		Assert.IsTrue(base.isServer, "DropCorpse called on client!");
		if (!ConVar.Server.corpses)
		{
			return null;
		}
		if (string.IsNullOrEmpty(strCorpsePrefab))
		{
			return null;
		}
		BaseCorpse baseCorpse = GameManager.server.CreateEntity(strCorpsePrefab) as BaseCorpse;
		if (baseCorpse == null)
		{
			Debug.LogWarning(string.Concat("Error creating corpse: ", base.gameObject, " - ", strCorpsePrefab));
			return null;
		}
		baseCorpse.InitCorpse(this);
		return baseCorpse;
	}

	public override void UpdateNetworkGroup()
	{
		Assert.IsTrue(base.isServer, "UpdateNetworkGroup called on clientside entity!");
		isCallingUpdateNetworkGroup = false;
		if (net == null || Network.Net.sv == null || Network.Net.sv.visibility == null)
		{
			return;
		}
		using (TimeWarning.New("UpdateNetworkGroup"))
		{
			if (globalBroadcast)
			{
				if (net.SwitchGroup(BaseNetworkable.GlobalNetworkGroup))
				{
					SendNetworkGroupChange();
				}
			}
			else if (ShouldInheritNetworkGroup() && parentEntity.IsSet())
			{
				BaseEntity baseEntity = GetParentEntity();
				if (!BaseEntityEx.IsValid(baseEntity))
				{
					Debug.LogWarning("UpdateNetworkGroup: Missing parent entity " + parentEntity.uid);
					Invoke(UpdateNetworkGroup, 2f);
					isCallingUpdateNetworkGroup = true;
				}
				else if (baseEntity != null)
				{
					if (net.SwitchGroup(baseEntity.net.group))
					{
						SendNetworkGroupChange();
					}
				}
				else
				{
					Debug.LogWarning(string.Concat(base.gameObject, ": has parent id - but couldn't find parent! ", parentEntity));
				}
			}
			else if (base.limitNetworking && !(this is BasePlayer))
			{
				if (net.SwitchGroup(BaseNetworkable.LimboNetworkGroup))
				{
					SendNetworkGroupChange();
				}
			}
			else
			{
				base.UpdateNetworkGroup();
			}
		}
	}

	public virtual void Eat(BaseNpc baseNpc, float timeSpent)
	{
		baseNpc.AddCalories(100f);
	}

	public virtual void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
	}

	public override bool ShouldNetworkTo(BasePlayer player)
	{
		if (player == this)
		{
			return true;
		}
		BaseEntity baseEntity = GetParentEntity();
		if (base.limitNetworking)
		{
			if (baseEntity == null)
			{
				return false;
			}
			if (baseEntity != player)
			{
				return false;
			}
		}
		if (baseEntity != null)
		{
			object obj = Interface.CallHook("CanNetworkTo", this, player);
			if (obj is bool)
			{
				return (bool)obj;
			}
			return baseEntity.ShouldNetworkTo(player);
		}
		return base.ShouldNetworkTo(player);
	}

	public virtual void AttackerInfo(PlayerLifeStory.DeathInfo info)
	{
		info.attackerName = base.ShortPrefabName;
		info.attackerSteamID = 0uL;
		info.inflictorName = "";
	}

	public virtual void Push(Vector3 velocity)
	{
		SetVelocity(velocity);
	}

	public virtual void ApplyInheritedVelocity(Vector3 velocity)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.velocity = Vector3.Lerp(component.velocity, velocity, 10f * UnityEngine.Time.fixedDeltaTime);
			component.angularVelocity *= Mathf.Clamp01(1f - 10f * UnityEngine.Time.fixedDeltaTime);
			component.AddForce(-UnityEngine.Physics.gravity * Mathf.Clamp01(0.9f), ForceMode.Acceleration);
		}
	}

	public virtual void SetVelocity(Vector3 velocity)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.velocity = velocity;
		}
	}

	public virtual void SetAngularVelocity(Vector3 velocity)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.angularVelocity = velocity;
		}
	}

	public virtual Vector3 GetDropPosition()
	{
		return base.transform.position;
	}

	public virtual Vector3 GetDropVelocity()
	{
		return GetInheritedDropVelocity() + Vector3.up;
	}

	public virtual bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		return true;
	}

	public virtual string Admin_Who()
	{
		return $"Owner ID: {OwnerID}";
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void BroadcastSignalFromClient(RPCMessage msg)
	{
		uint num = StringPool.Get("BroadcastSignalFromClient");
		if (num != 0)
		{
			BasePlayer player = msg.player;
			if (!(player == null) && player.rpcHistory.TryIncrement(num, (ulong)ConVar.Server.maxpacketspersecond_rpc_signal))
			{
				Signal signal = (Signal)msg.read.Int32();
				string arg = msg.read.String();
				SignalBroadcast(signal, arg, msg.connection);
			}
		}
	}

	public void SignalBroadcast(Signal signal, string arg, Connection sourceConnection = null)
	{
		if (net != null && net.group != null && !base.limitNetworking && Interface.CallHook("OnSignalBroadcast", this) == null)
		{
			ClientRPCEx(new SendInfo(net.group.subscribers)
			{
				method = SendMethod.Unreliable,
				priority = Priority.Immediate
			}, sourceConnection, "SignalFromServerEx", (int)signal, arg);
		}
	}

	public void SignalBroadcast(Signal signal, Connection sourceConnection = null)
	{
		if (net != null && net.group != null && !base.limitNetworking && Interface.CallHook("OnSignalBroadcast", this) == null)
		{
			ClientRPCEx(new SendInfo(net.group.subscribers)
			{
				method = SendMethod.Unreliable,
				priority = Priority.Immediate
			}, sourceConnection, "SignalFromServer", (int)signal);
		}
	}

	private void OnSkinChanged(ulong oldSkinID, ulong newSkinID)
	{
		if (oldSkinID != newSkinID)
		{
			skinID = newSkinID;
		}
	}

	public virtual void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (clientside && Skinnable.All != null && Skinnable.FindForEntity(name) != null)
		{
			Rust.Workshop.WorkshopSkin.Prepare(rootObj);
			MaterialReplacement.Prepare(rootObj);
		}
	}

	public bool HasAnySlot()
	{
		for (int i = 0; i < entitySlots.Length; i++)
		{
			if (entitySlots[i].IsValid(base.isServer))
			{
				return true;
			}
		}
		return false;
	}

	public BaseEntity GetSlot(Slot slot)
	{
		return entitySlots[(int)slot].Get(base.isServer);
	}

	public string GetSlotAnchorName(Slot slot)
	{
		return slot.ToString().ToLower();
	}

	public void SetSlot(Slot slot, BaseEntity ent)
	{
		entitySlots[(int)slot].Set(ent);
		SendNetworkUpdate();
	}

	public EntityRef[] GetSlots()
	{
		return entitySlots;
	}

	public void SetSlots(EntityRef[] newSlots)
	{
		entitySlots = newSlots;
	}

	public virtual bool HasSlot(Slot slot)
	{
		return false;
	}

	public bool HasTrait(TraitFlag f)
	{
		return (Traits & f) == f;
	}

	public bool HasAnyTrait(TraitFlag f)
	{
		return (Traits & f) != 0;
	}

	public virtual bool EnterTrigger(TriggerBase trigger)
	{
		if (triggers == null)
		{
			triggers = Facepunch.Pool.Get<List<TriggerBase>>();
		}
		triggers.Add(trigger);
		return true;
	}

	public virtual void LeaveTrigger(TriggerBase trigger)
	{
		if (triggers != null)
		{
			triggers.Remove(trigger);
			if (triggers.Count == 0)
			{
				Facepunch.Pool.FreeList(ref triggers);
			}
		}
	}

	public void RemoveFromTriggers()
	{
		if (triggers == null)
		{
			return;
		}
		using (TimeWarning.New("RemoveFromTriggers"))
		{
			TriggerBase[] array = triggers.ToArray();
			foreach (TriggerBase triggerBase in array)
			{
				if ((bool)triggerBase)
				{
					triggerBase.RemoveEntity(this);
				}
			}
			if (triggers != null && triggers.Count == 0)
			{
				Facepunch.Pool.FreeList(ref triggers);
			}
		}
	}

	public T FindTrigger<T>() where T : TriggerBase
	{
		if (triggers == null)
		{
			return null;
		}
		foreach (TriggerBase trigger in triggers)
		{
			if (!((UnityEngine.Object)(trigger as T) == (UnityEngine.Object)null))
			{
				return trigger as T;
			}
		}
		return null;
	}

	public bool FindTrigger<T>(out T result) where T : TriggerBase
	{
		result = FindTrigger<T>();
		return (UnityEngine.Object)result != (UnityEngine.Object)null;
	}

	private void ForceUpdateTriggersAction()
	{
		if (!base.IsDestroyed)
		{
			ForceUpdateTriggers(enter: false, exit: true, invoke: false);
		}
	}

	public void ForceUpdateTriggers(bool enter = true, bool exit = true, bool invoke = true)
	{
		List<TriggerBase> obj = Facepunch.Pool.GetList<TriggerBase>();
		List<TriggerBase> obj2 = Facepunch.Pool.GetList<TriggerBase>();
		if (triggers != null)
		{
			obj.AddRange(triggers);
		}
		Collider componentInChildren = GetComponentInChildren<Collider>();
		if (componentInChildren is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = componentInChildren as CapsuleCollider;
			Vector3 point = base.transform.position + new Vector3(0f, capsuleCollider.radius, 0f);
			Vector3 point2 = base.transform.position + new Vector3(0f, capsuleCollider.height - capsuleCollider.radius, 0f);
			GamePhysics.OverlapCapsule(point, point2, capsuleCollider.radius, obj2, 262144, QueryTriggerInteraction.Collide);
		}
		else if (componentInChildren is BoxCollider)
		{
			BoxCollider boxCollider = componentInChildren as BoxCollider;
			GamePhysics.OverlapOBB(new OBB(base.transform.position, base.transform.lossyScale, base.transform.rotation, new Bounds(boxCollider.center, boxCollider.size)), obj2, 262144, QueryTriggerInteraction.Collide);
		}
		else if (componentInChildren is SphereCollider)
		{
			SphereCollider sphereCollider = componentInChildren as SphereCollider;
			GamePhysics.OverlapSphere(base.transform.TransformPoint(sphereCollider.center), sphereCollider.radius, obj2, 262144, QueryTriggerInteraction.Collide);
		}
		else
		{
			obj2.AddRange(obj);
		}
		if (exit)
		{
			foreach (TriggerBase item in obj)
			{
				if (!obj2.Contains(item))
				{
					item.OnTriggerExit(componentInChildren);
				}
			}
		}
		if (enter)
		{
			foreach (TriggerBase item2 in obj2)
			{
				if (!obj.Contains(item2))
				{
					item2.OnTriggerEnter(componentInChildren);
				}
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		Facepunch.Pool.FreeList(ref obj2);
		if (invoke)
		{
			Invoke(ForceUpdateTriggersAction, UnityEngine.Time.time - UnityEngine.Time.fixedTime + UnityEngine.Time.fixedDeltaTime * 1.5f);
		}
	}

	public virtual BasePlayer ToPlayer()
	{
		return null;
	}

	public override void InitShared()
	{
		base.InitShared();
		InitEntityLinks();
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		FreeEntityLinks();
	}

	public override void ResetState()
	{
		base.ResetState();
		parentBone = 0u;
		OwnerID = 0uL;
		flags = (Flags)0;
		parentEntity = default(EntityRef);
		if (base.isServer)
		{
			_spawnable = null;
		}
	}

	public virtual float InheritedVelocityScale()
	{
		return 0f;
	}

	public virtual Vector3 GetInheritedProjectileVelocity()
	{
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!(baseEntity != null))
		{
			return Vector3.zero;
		}
		return GetParentVelocity() * baseEntity.InheritedVelocityScale();
	}

	public virtual Vector3 GetInheritedThrowVelocity()
	{
		return GetParentVelocity();
	}

	public virtual Vector3 GetInheritedDropVelocity()
	{
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!(baseEntity != null))
		{
			return Vector3.zero;
		}
		return baseEntity.GetWorldVelocity();
	}

	public Vector3 GetParentVelocity()
	{
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!(baseEntity != null))
		{
			return Vector3.zero;
		}
		return baseEntity.GetWorldVelocity() + (baseEntity.GetAngularVelocity() * base.transform.localPosition - base.transform.localPosition);
	}

	public Vector3 GetWorldVelocity()
	{
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!(baseEntity != null))
		{
			return GetLocalVelocity();
		}
		return baseEntity.GetWorldVelocity() + (baseEntity.GetAngularVelocity() * base.transform.localPosition - base.transform.localPosition) + baseEntity.transform.TransformDirection(GetLocalVelocity());
	}

	public Vector3 GetLocalVelocity()
	{
		if (base.isServer)
		{
			return GetLocalVelocityServer();
		}
		return Vector3.zero;
	}

	public Quaternion GetAngularVelocity()
	{
		if (base.isServer)
		{
			return GetAngularVelocityServer();
		}
		return Quaternion.identity;
	}

	public virtual OBB WorldSpaceBounds()
	{
		return new OBB(base.transform.position, base.transform.lossyScale, base.transform.rotation, bounds);
	}

	public Vector3 PivotPoint()
	{
		return base.transform.position;
	}

	public Vector3 CenterPoint()
	{
		return WorldSpaceBounds().position;
	}

	public Vector3 ClosestPoint(Vector3 position)
	{
		return WorldSpaceBounds().ClosestPoint(position);
	}

	public virtual Vector3 TriggerPoint()
	{
		return CenterPoint();
	}

	public float Distance(Vector3 position)
	{
		return (ClosestPoint(position) - position).magnitude;
	}

	public float SqrDistance(Vector3 position)
	{
		return (ClosestPoint(position) - position).sqrMagnitude;
	}

	public float Distance(BaseEntity other)
	{
		return Distance(other.transform.position);
	}

	public float SqrDistance(BaseEntity other)
	{
		return SqrDistance(other.transform.position);
	}

	public float Distance2D(Vector3 position)
	{
		return (ClosestPoint(position) - position).Magnitude2D();
	}

	public float SqrDistance2D(Vector3 position)
	{
		return (ClosestPoint(position) - position).SqrMagnitude2D();
	}

	public float Distance2D(BaseEntity other)
	{
		return Distance(other.transform.position);
	}

	public float SqrDistance2D(BaseEntity other)
	{
		return SqrDistance(other.transform.position);
	}

	public bool IsVisible(Ray ray, int layerMask, float maxDistance)
	{
		if (ray.origin.IsNaNOrInfinity())
		{
			return false;
		}
		if (ray.direction.IsNaNOrInfinity())
		{
			return false;
		}
		if (ray.direction == Vector3.zero)
		{
			return false;
		}
		if (!WorldSpaceBounds().Trace(ray, out var hit, maxDistance))
		{
			return false;
		}
		if (GamePhysics.Trace(ray, 0f, out var hitInfo, maxDistance, layerMask))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if (entity == this)
			{
				return true;
			}
			if (entity != null && (bool)GetParentEntity() && GetParentEntity().EqualNetID(entity) && RaycastHitEx.IsOnLayer(hitInfo, Rust.Layer.Vehicle_Detailed))
			{
				return true;
			}
			if (hitInfo.distance <= hit.distance)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsVisibleSpecificLayers(Vector3 position, Vector3 target, int layerMask, float maxDistance = float.PositiveInfinity)
	{
		Vector3 vector = target - position;
		float magnitude = vector.magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return true;
		}
		Vector3 vector2 = vector / magnitude;
		Vector3 vector3 = vector2 * Mathf.Min(magnitude, 0.01f);
		return IsVisible(new Ray(position + vector3, vector2), layerMask, maxDistance);
	}

	public bool IsVisible(Vector3 position, Vector3 target, float maxDistance = float.PositiveInfinity)
	{
		Vector3 vector = target - position;
		float magnitude = vector.magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return true;
		}
		Vector3 vector2 = vector / magnitude;
		Vector3 vector3 = vector2 * Mathf.Min(magnitude, 0.01f);
		return IsVisible(new Ray(position + vector3, vector2), 1218519041, maxDistance);
	}

	public bool IsVisible(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		Vector3 target = CenterPoint();
		if (IsVisible(position, target, maxDistance))
		{
			return true;
		}
		Vector3 target2 = ClosestPoint(position);
		if (IsVisible(position, target2, maxDistance))
		{
			return true;
		}
		return false;
	}

	public bool IsVisibleAndCanSee(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		Vector3 vector = CenterPoint();
		if (IsVisible(position, vector, maxDistance) && IsVisible(vector, position, maxDistance))
		{
			return true;
		}
		Vector3 vector2 = ClosestPoint(position);
		if (IsVisible(position, vector2, maxDistance) && IsVisible(vector2, position, maxDistance))
		{
			return true;
		}
		return false;
	}

	public bool IsOlderThan(BaseEntity other)
	{
		if (other == null)
		{
			return true;
		}
		uint num = ((net != null) ? net.ID : 0);
		uint num2 = ((other.net != null) ? other.net.ID : 0u);
		return num < num2;
	}

	public virtual bool IsOutside()
	{
		return IsOutside(WorldSpaceBounds().position);
	}

	public bool IsOutside(Vector3 position)
	{
		bool result = true;
		bool flag;
		do
		{
			flag = false;
			if (!UnityEngine.Physics.Raycast(position, Vector3.up, out var hitInfo, 100f, 161546513))
			{
				continue;
			}
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(hitInfo.collider);
			if (baseEntity != null && BaseEntityEx.HasEntityInParents(baseEntity, this))
			{
				if (hitInfo.point.y > position.y + 0.2f)
				{
					position = hitInfo.point + Vector3.up * 0.05f;
				}
				else
				{
					position.y += 0.2f;
				}
				flag = true;
			}
			else
			{
				result = false;
			}
		}
		while (flag);
		return result;
	}

	public virtual float WaterFactor()
	{
		return WaterLevel.Factor(WorldSpaceBounds().ToBounds(), this);
	}

	public virtual float AirFactor()
	{
		if (!(WaterFactor() > 0.85f))
		{
			return 1f;
		}
		return 0f;
	}

	public bool WaterTestFromVolumes(Vector3 pos, out WaterLevel.WaterInfo info)
	{
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(pos, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public bool IsInWaterVolume(Vector3 pos)
	{
		if (triggers == null)
		{
			return false;
		}
		WaterLevel.WaterInfo info = default(WaterLevel.WaterInfo);
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(pos, out info))
			{
				return true;
			}
		}
		return false;
	}

	public bool WaterTestFromVolumes(Bounds bounds, out WaterLevel.WaterInfo info)
	{
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(bounds, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public bool WaterTestFromVolumes(Vector3 start, Vector3 end, float radius, out WaterLevel.WaterInfo info)
	{
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(start, end, radius, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public virtual bool BlocksWaterFor(BasePlayer player)
	{
		return false;
	}

	public virtual float Health()
	{
		return 0f;
	}

	public virtual float MaxHealth()
	{
		return 0f;
	}

	public virtual float MaxVelocity()
	{
		return 0f;
	}

	public virtual float BoundsPadding()
	{
		return 0.1f;
	}

	public virtual float PenetrationResistance(HitInfo info)
	{
		return 100f;
	}

	public virtual GameObjectRef GetImpactEffect(HitInfo info)
	{
		return impactEffect;
	}

	public virtual void OnAttacked(HitInfo info)
	{
	}

	public virtual Item GetItem()
	{
		return null;
	}

	public virtual Item GetItem(uint itemId)
	{
		return null;
	}

	public virtual void GiveItem(Item item, GiveItemReason reason = GiveItemReason.Generic)
	{
		item.Remove();
	}

	public virtual bool CanBeLooted(BasePlayer player)
	{
		return true;
	}

	public virtual BaseEntity GetEntity()
	{
		return this;
	}

	public override string ToString()
	{
		if (_name == null)
		{
			if (base.isServer)
			{
				_name = string.Format("{1}[{0}]", (net != null) ? net.ID : 0u, base.ShortPrefabName);
			}
			else
			{
				_name = base.ShortPrefabName;
			}
		}
		return _name;
	}

	public virtual string Categorize()
	{
		return "entity";
	}

	public void Log(string str)
	{
		if (base.isClient)
		{
			Debug.Log("<color=#ffa>[" + ToString() + "] " + str + "</color>", base.gameObject);
		}
		else
		{
			Debug.Log("<color=#aff>[" + ToString() + "] " + str + "</color>", base.gameObject);
		}
	}

	public void SetModel(Model mdl)
	{
		if (!(model == mdl))
		{
			model = mdl;
		}
	}

	public Model GetModel()
	{
		return model;
	}

	public virtual Transform[] GetBones()
	{
		if ((bool)model)
		{
			return model.GetBones();
		}
		return null;
	}

	public virtual Transform FindBone(string strName)
	{
		if ((bool)model)
		{
			return model.FindBone(strName);
		}
		return base.transform;
	}

	public virtual uint FindBoneID(Transform boneTransform)
	{
		if ((bool)model)
		{
			return model.FindBoneID(boneTransform);
		}
		return StringPool.closest;
	}

	public virtual Transform FindClosestBone(Vector3 worldPos)
	{
		if ((bool)model)
		{
			return model.FindClosestBone(worldPos);
		}
		return base.transform;
	}

	public virtual bool ShouldBlockProjectiles()
	{
		return true;
	}

	public virtual bool ShouldInheritNetworkGroup()
	{
		return true;
	}

	public virtual bool SupportsChildDeployables()
	{
		return true;
	}

	public void BroadcastEntityMessage(string msg, float radius = 20f, int layerMask = 1218652417)
	{
		if (base.isClient)
		{
			return;
		}
		List<BaseEntity> obj = Facepunch.Pool.GetList<BaseEntity>();
		Vis.Entities(base.transform.position, radius, obj, layerMask);
		foreach (BaseEntity item in obj)
		{
			if (item.isServer)
			{
				item.OnEntityMessage(this, msg);
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public virtual void OnEntityMessage(BaseEntity from, string msg)
	{
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		info.msg.baseEntity = Facepunch.Pool.Get<ProtoBuf.BaseEntity>();
		if (info.forDisk)
		{
			if (this is BasePlayer)
			{
				if (baseEntity == null || baseEntity.enableSaving)
				{
					info.msg.baseEntity.pos = base.transform.localPosition;
					info.msg.baseEntity.rot = base.transform.localRotation.eulerAngles;
				}
				else
				{
					info.msg.baseEntity.pos = base.transform.position;
					info.msg.baseEntity.rot = base.transform.rotation.eulerAngles;
				}
			}
			else
			{
				info.msg.baseEntity.pos = base.transform.localPosition;
				info.msg.baseEntity.rot = base.transform.localRotation.eulerAngles;
			}
		}
		else
		{
			info.msg.baseEntity.pos = GetNetworkPosition();
			info.msg.baseEntity.rot = GetNetworkRotation().eulerAngles;
			info.msg.baseEntity.time = GetNetworkTime();
		}
		info.msg.baseEntity.flags = (int)flags;
		info.msg.baseEntity.skinid = skinID;
		if (info.forDisk && this is BasePlayer)
		{
			if (baseEntity != null && baseEntity.enableSaving)
			{
				info.msg.parent = Facepunch.Pool.Get<ParentInfo>();
				info.msg.parent.uid = parentEntity.uid;
				info.msg.parent.bone = parentBone;
			}
		}
		else if (baseEntity != null)
		{
			info.msg.parent = Facepunch.Pool.Get<ParentInfo>();
			info.msg.parent.uid = parentEntity.uid;
			info.msg.parent.bone = parentBone;
		}
		if (HasAnySlot())
		{
			info.msg.entitySlots = Facepunch.Pool.Get<EntitySlots>();
			info.msg.entitySlots.slotLock = entitySlots[0].uid;
			info.msg.entitySlots.slotFireMod = entitySlots[1].uid;
			info.msg.entitySlots.slotUpperModification = entitySlots[2].uid;
			info.msg.entitySlots.centerDecoration = entitySlots[5].uid;
			info.msg.entitySlots.lowerCenterDecoration = entitySlots[6].uid;
			info.msg.entitySlots.storageMonitor = entitySlots[7].uid;
		}
		if (info.forDisk && (bool)_spawnable)
		{
			_spawnable.Save(info);
		}
		if (OwnerID != 0L && (info.forDisk || ShouldNetworkOwnerInfo()))
		{
			info.msg.ownerInfo = Facepunch.Pool.Get<OwnerInfo>();
			info.msg.ownerInfo.steamid = OwnerID;
		}
	}

	public virtual bool ShouldNetworkOwnerInfo()
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseEntity != null)
		{
			ProtoBuf.BaseEntity baseEntity = info.msg.baseEntity;
			Flags old = flags;
			flags = (Flags)baseEntity.flags;
			OnFlagsChanged(old, flags);
			OnSkinChanged(skinID, info.msg.baseEntity.skinid);
			if (info.fromDisk)
			{
				if (baseEntity.pos.IsNaNOrInfinity())
				{
					Debug.LogWarning(ToString() + " has broken position - " + baseEntity.pos);
					baseEntity.pos = Vector3.zero;
				}
				base.transform.localPosition = baseEntity.pos;
				base.transform.localRotation = Quaternion.Euler(baseEntity.rot);
			}
		}
		if (info.msg.entitySlots != null)
		{
			entitySlots[0].uid = info.msg.entitySlots.slotLock;
			entitySlots[1].uid = info.msg.entitySlots.slotFireMod;
			entitySlots[2].uid = info.msg.entitySlots.slotUpperModification;
			entitySlots[5].uid = info.msg.entitySlots.centerDecoration;
			entitySlots[6].uid = info.msg.entitySlots.lowerCenterDecoration;
			entitySlots[7].uid = info.msg.entitySlots.storageMonitor;
		}
		if (info.msg.parent != null)
		{
			if (base.isServer)
			{
				BaseEntity entity = BaseNetworkable.serverEntities.Find(info.msg.parent.uid) as BaseEntity;
				SetParent(entity, info.msg.parent.bone);
			}
			parentEntity.uid = info.msg.parent.uid;
			parentBone = info.msg.parent.bone;
		}
		else
		{
			parentEntity.uid = 0u;
			parentBone = 0u;
		}
		if (info.msg.ownerInfo != null)
		{
			OwnerID = info.msg.ownerInfo.steamid;
		}
		if ((bool)_spawnable)
		{
			_spawnable.Load(info);
		}
	}
}
