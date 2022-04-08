#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConVar;
using Facepunch;
using Network;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Registry;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class BaseNetworkable : BaseMonoBehaviour, IPrefabPostProcess, IEntity, NetworkHandler
{
	public struct SaveInfo
	{
		public ProtoBuf.Entity msg;

		public bool forDisk;

		public Connection forConnection;

		internal bool SendingTo(Connection ownerConnection)
		{
			if (ownerConnection == null)
			{
				return false;
			}
			if (forConnection == null)
			{
				return false;
			}
			return forConnection == ownerConnection;
		}
	}

	public struct LoadInfo
	{
		public ProtoBuf.Entity msg;

		public bool fromDisk;
	}

	public class EntityRealmServer : EntityRealm
	{
		protected override Manager visibilityManager
		{
			get
			{
				if (Network.Net.sv == null)
				{
					return null;
				}
				return Network.Net.sv.visibility;
			}
		}
	}

	public abstract class EntityRealm : IEnumerable<BaseNetworkable>, IEnumerable
	{
		public ListDictionary<uint, BaseNetworkable> entityList = new ListDictionary<uint, BaseNetworkable>();

		public int Count => entityList.Count;

		protected abstract Manager visibilityManager { get; }

		public bool Contains(uint uid)
		{
			return entityList.Contains(uid);
		}

		public BaseNetworkable Find(uint uid)
		{
			BaseNetworkable val = null;
			if (!entityList.TryGetValue(uid, out val))
			{
				return null;
			}
			return val;
		}

		public void RegisterID(BaseNetworkable ent)
		{
			if (ent.net != null)
			{
				if (entityList.Contains(ent.net.ID))
				{
					entityList[ent.net.ID] = ent;
				}
				else
				{
					entityList.Add(ent.net.ID, ent);
				}
			}
		}

		public void UnregisterID(BaseNetworkable ent)
		{
			if (ent.net != null)
			{
				entityList.Remove(ent.net.ID);
			}
		}

		public Group FindGroup(uint uid)
		{
			return visibilityManager?.Get(uid);
		}

		public Group TryFindGroup(uint uid)
		{
			return visibilityManager?.TryGet(uid);
		}

		public void FindInGroup(uint uid, List<BaseNetworkable> list)
		{
			Group group = TryFindGroup(uid);
			if (group == null)
			{
				return;
			}
			int count = group.networkables.Values.Count;
			Networkable[] buffer = group.networkables.Values.Buffer;
			for (int i = 0; i < count; i++)
			{
				Networkable networkable = buffer[i];
				BaseNetworkable baseNetworkable = Find(networkable.ID);
				if (!(baseNetworkable == null) && baseNetworkable.net != null && baseNetworkable.net.group != null)
				{
					if (baseNetworkable.net.group.ID != uid)
					{
						Debug.LogWarning("Group ID mismatch: " + baseNetworkable.ToString());
					}
					else
					{
						list.Add(baseNetworkable);
					}
				}
			}
		}

		public IEnumerator<BaseNetworkable> GetEnumerator()
		{
			return entityList.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Clear()
		{
			entityList.Clear();
		}
	}

	public enum DestroyMode : byte
	{
		None = 0,
		Gib = 1
	}

	public List<Component> postNetworkUpdateComponents = new List<Component>();

	public bool _limitedNetworking;

	[NonSerialized]
	public EntityRef parentEntity;

	[NonSerialized]
	public readonly List<BaseEntity> children = new List<BaseEntity>();

	public int creationFrame;

	public bool isSpawned;

	public MemoryStream _NetworkCache;

	public static Queue<MemoryStream> EntityMemoryStreamPool = new Queue<MemoryStream>();

	private MemoryStream _SaveCache;

	[Header("BaseNetworkable")]
	[ReadOnly]
	public uint prefabID;

	[Tooltip("If enabled the entity will send to everyone on the server - regardless of position")]
	public bool globalBroadcast;

	[NonSerialized]
	public Networkable net;

	private string _prefabName;

	private string _prefabNameWithoutExtension;

	public static EntityRealm serverEntities = new EntityRealmServer();

	private const bool isServersideEntity = true;

	public static List<Connection> connectionsInSphereList = new List<Connection>();

	public bool limitNetworking
	{
		get
		{
			return _limitedNetworking;
		}
		set
		{
			if (value != _limitedNetworking)
			{
				_limitedNetworking = value;
				if (_limitedNetworking)
				{
					OnNetworkLimitStart();
				}
				else
				{
					OnNetworkLimitEnd();
				}
				UpdateNetworkGroup();
			}
		}
	}

	public GameManager gameManager
	{
		get
		{
			if (isServer)
			{
				return GameManager.server;
			}
			throw new NotImplementedException("Missing gameManager path");
		}
	}

	public PrefabAttribute.Library prefabAttribute
	{
		get
		{
			if (isServer)
			{
				return PrefabAttribute.server;
			}
			throw new NotImplementedException("Missing prefabAttribute path");
		}
	}

	public static Group GlobalNetworkGroup => Network.Net.sv.visibility.Get(0u);

	public static Group LimboNetworkGroup => Network.Net.sv.visibility.Get(1u);

	public bool IsDestroyed { get; private set; }

	public string PrefabName
	{
		get
		{
			if (_prefabName == null)
			{
				_prefabName = StringPool.Get(prefabID);
			}
			return _prefabName;
		}
	}

	public string ShortPrefabName
	{
		get
		{
			if (_prefabNameWithoutExtension == null)
			{
				_prefabNameWithoutExtension = Path.GetFileNameWithoutExtension(PrefabName);
			}
			return _prefabNameWithoutExtension;
		}
	}

	public bool isServer => true;

	public bool isClient => false;

	public void BroadcastOnPostNetworkUpdate(BaseEntity entity)
	{
		foreach (Component postNetworkUpdateComponent in postNetworkUpdateComponents)
		{
			(postNetworkUpdateComponent as IOnPostNetworkUpdate)?.OnPostNetworkUpdate(entity);
		}
		foreach (BaseEntity child in children)
		{
			child.BroadcastOnPostNetworkUpdate(entity);
		}
	}

	public virtual void PostProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!serverside)
		{
			postNetworkUpdateComponents = GetComponentsInChildren<IOnPostNetworkUpdate>(true).Cast<Component>().ToList();
		}
	}

	private void OnNetworkLimitStart()
	{
		LogEntry(LogEntryType.Network, 2, "OnNetworkLimitStart");
		List<Connection> subscribers = GetSubscribers();
		if (subscribers == null)
		{
			return;
		}
		subscribers = subscribers.ToList();
		subscribers.RemoveAll((Connection x) => ShouldNetworkTo(x.player as BasePlayer));
		OnNetworkSubscribersLeave(subscribers);
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.OnNetworkLimitStart();
		}
	}

	private void OnNetworkLimitEnd()
	{
		LogEntry(LogEntryType.Network, 2, "OnNetworkLimitEnd");
		List<Connection> subscribers = GetSubscribers();
		if (subscribers == null)
		{
			return;
		}
		OnNetworkSubscribersEnter(subscribers);
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.OnNetworkLimitEnd();
		}
	}

	public BaseEntity GetParentEntity()
	{
		return parentEntity.Get(isServer);
	}

	public bool HasParent()
	{
		return parentEntity.IsValid(isServer);
	}

	public void AddChild(BaseEntity child)
	{
		if (!children.Contains(child))
		{
			children.Add(child);
			OnChildAdded(child);
		}
	}

	protected virtual void OnChildAdded(BaseEntity child)
	{
	}

	public void RemoveChild(BaseEntity child)
	{
		children.Remove(child);
		OnChildRemoved(child);
	}

	protected virtual void OnChildRemoved(BaseEntity child)
	{
	}

	public virtual float GetNetworkTime()
	{
		return UnityEngine.Time.time;
	}

	public virtual void Spawn()
	{
		SpawnShared();
		if (net == null)
		{
			net = Network.Net.sv.CreateNetworkable();
		}
		creationFrame = UnityEngine.Time.frameCount;
		PreInitShared();
		InitShared();
		ServerInit();
		PostInitShared();
		UpdateNetworkGroup();
		isSpawned = true;
		Interface.CallHook("OnEntitySpawned", this);
		SendNetworkUpdateImmediate(true);
		if (Rust.Application.isLoading && !Rust.Application.isLoadingSave)
		{
			OnSendNetworkUpdateEx.SendOnSendNetworkUpdate(base.gameObject, this as BaseEntity);
		}
	}

	public bool IsFullySpawned()
	{
		return isSpawned;
	}

	public virtual void ServerInit()
	{
		serverEntities.RegisterID(this);
		if (net != null)
		{
			net.handler = this;
		}
	}

	public List<Connection> GetSubscribers()
	{
		if (net == null)
		{
			return null;
		}
		if (net.group == null)
		{
			return null;
		}
		return net.group.subscribers;
	}

	public void KillMessage()
	{
		Kill();
	}

	public virtual void AdminKill()
	{
		Kill(DestroyMode.Gib);
	}

	public void Kill(DestroyMode mode = DestroyMode.None)
	{
		if (IsDestroyed)
		{
			Debug.LogWarning("Calling kill - but already IsDestroyed!? " + this);
		}
		else if (Interface.CallHook("OnEntityKill", this) == null)
		{
			OnParentDestroyingEx.BroadcastOnParentDestroying(base.gameObject);
			DoEntityDestroy();
			TerminateOnClient(mode);
			TerminateOnServer();
			EntityDestroy();
		}
	}

	public void TerminateOnClient(DestroyMode mode)
	{
		if (net != null && net.group != null && Network.Net.sv.IsConnected())
		{
			LogEntry(LogEntryType.Network, 2, "Term {0}", mode);
			if (Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Message.Type.EntityDestroy);
				Network.Net.sv.write.EntityID(net.ID);
				Network.Net.sv.write.UInt8((byte)mode);
				Network.Net.sv.write.Send(new SendInfo(net.group.subscribers));
			}
		}
	}

	private void TerminateOnServer()
	{
		if (net != null)
		{
			InvalidateNetworkCache();
			serverEntities.UnregisterID(this);
			Network.Net.sv.DestroyNetworkable(ref net);
			StopAllCoroutines();
			base.gameObject.SetActive(false);
		}
	}

	internal virtual void DoServerDestroy()
	{
		isSpawned = false;
	}

	public virtual bool ShouldNetworkTo(BasePlayer player)
	{
		object obj = Interface.CallHook("CanNetworkTo", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (net.group == null)
		{
			return true;
		}
		return player.net.subscriber.IsSubscribed(net.group);
	}

	public void SendNetworkGroupChange()
	{
		if (isSpawned && Network.Net.sv.IsConnected())
		{
			if (net.group == null)
			{
				Debug.LogWarning(ToString() + " changed its network group to null");
			}
			else if (Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Message.Type.GroupChange);
				Network.Net.sv.write.EntityID(net.ID);
				Network.Net.sv.write.GroupID(net.group.ID);
				Network.Net.sv.write.Send(new SendInfo(net.group.subscribers));
			}
		}
	}

	public void SendAsSnapshot(Connection connection, bool justCreated = false)
	{
		if (Interface.CallHook("OnEntitySnapshot", this, connection) == null && Network.Net.sv.write.Start())
		{
			connection.validate.entityUpdates++;
			SaveInfo saveInfo = default(SaveInfo);
			saveInfo.forConnection = connection;
			saveInfo.forDisk = false;
			SaveInfo saveInfo2 = saveInfo;
			Network.Net.sv.write.PacketID(Message.Type.Entities);
			Network.Net.sv.write.UInt32(connection.validate.entityUpdates);
			ToStreamForNetwork(Network.Net.sv.write, saveInfo2);
			Network.Net.sv.write.Send(new SendInfo(connection));
		}
	}

	public void SendNetworkUpdate(BasePlayer.NetworkQueue queue = BasePlayer.NetworkQueue.Update)
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate"))
		{
			LogEntry(LogEntryType.Network, 2, "SendNetworkUpdate");
			InvalidateNetworkCache();
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null && subscribers.Count > 0)
			{
				for (int i = 0; i < subscribers.Count; i++)
				{
					BasePlayer basePlayer = subscribers[i].player as BasePlayer;
					if (!(basePlayer == null) && ShouldNetworkTo(basePlayer))
					{
						basePlayer.QueueUpdate(queue, this);
					}
				}
			}
		}
		OnSendNetworkUpdateEx.SendOnSendNetworkUpdate(base.gameObject, this as BaseEntity);
	}

	public void SendNetworkUpdateImmediate(bool justCreated = false)
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdateImmediate"))
		{
			LogEntry(LogEntryType.Network, 2, "SendNetworkUpdateImmediate");
			InvalidateNetworkCache();
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null && subscribers.Count > 0)
			{
				for (int i = 0; i < subscribers.Count; i++)
				{
					Connection connection = subscribers[i];
					BasePlayer basePlayer = connection.player as BasePlayer;
					if (!(basePlayer == null) && ShouldNetworkTo(basePlayer))
					{
						SendAsSnapshot(connection, justCreated);
					}
				}
			}
		}
		OnSendNetworkUpdateEx.SendOnSendNetworkUpdate(base.gameObject, this as BaseEntity);
	}

	public void SendNetworkUpdate_Position()
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate_Position"))
		{
			LogEntry(LogEntryType.Network, 2, "SendNetworkUpdate_Position");
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null && subscribers.Count > 0 && Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Message.Type.EntityPosition);
				Network.Net.sv.write.EntityID(net.ID);
				NetWrite write = Network.Net.sv.write;
				Vector3 obj = GetNetworkPosition();
				write.Vector3(ref obj);
				NetWrite write2 = Network.Net.sv.write;
				obj = GetNetworkRotation().eulerAngles;
				write2.Vector3(ref obj);
				Network.Net.sv.write.Float(GetNetworkTime());
				uint uid = parentEntity.uid;
				if (uid != 0)
				{
					Network.Net.sv.write.EntityID(uid);
				}
				SendInfo sendInfo = new SendInfo(subscribers);
				sendInfo.method = SendMethod.ReliableUnordered;
				sendInfo.priority = Priority.Immediate;
				SendInfo info = sendInfo;
				Network.Net.sv.write.Send(info);
			}
		}
	}

	public void ToStream(Stream stream, SaveInfo saveInfo)
	{
		using (saveInfo.msg = Facepunch.Pool.Get<ProtoBuf.Entity>())
		{
			Save(saveInfo);
			if (saveInfo.msg.baseEntity == null)
			{
				Debug.LogError(string.Concat(this, ": ToStream - no BaseEntity!?"));
			}
			if (saveInfo.msg.baseNetworkable == null)
			{
				Debug.LogError(string.Concat(this, ": ToStream - no baseNetworkable!?"));
			}
			Interface.CallHook("IOnEntitySaved", this, saveInfo);
			saveInfo.msg.ToProto(stream);
			PostSave(saveInfo);
		}
	}

	public virtual bool CanUseNetworkCache(Connection connection)
	{
		return ConVar.Server.netcache;
	}

	public void ToStreamForNetwork(Stream stream, SaveInfo saveInfo)
	{
		if (!CanUseNetworkCache(saveInfo.forConnection))
		{
			ToStream(stream, saveInfo);
			return;
		}
		if (_NetworkCache == null)
		{
			_NetworkCache = ((EntityMemoryStreamPool.Count > 0) ? (_NetworkCache = EntityMemoryStreamPool.Dequeue()) : new MemoryStream(8));
			ToStream(_NetworkCache, saveInfo);
			ConVar.Server.netcachesize += (int)_NetworkCache.Length;
		}
		_NetworkCache.WriteTo(stream);
	}

	public void InvalidateNetworkCache()
	{
		using (TimeWarning.New("InvalidateNetworkCache"))
		{
			if (_SaveCache != null)
			{
				ConVar.Server.savecachesize -= (int)_SaveCache.Length;
				_SaveCache.SetLength(0L);
				_SaveCache.Position = 0L;
				EntityMemoryStreamPool.Enqueue(_SaveCache);
				_SaveCache = null;
			}
			if (_NetworkCache != null)
			{
				ConVar.Server.netcachesize -= (int)_NetworkCache.Length;
				_NetworkCache.SetLength(0L);
				_NetworkCache.Position = 0L;
				EntityMemoryStreamPool.Enqueue(_NetworkCache);
				_NetworkCache = null;
			}
			LogEntry(LogEntryType.Network, 3, "InvalidateNetworkCache");
		}
	}

	public MemoryStream GetSaveCache()
	{
		if (_SaveCache == null)
		{
			if (EntityMemoryStreamPool.Count > 0)
			{
				_SaveCache = EntityMemoryStreamPool.Dequeue();
			}
			else
			{
				_SaveCache = new MemoryStream(8);
			}
			SaveInfo saveInfo = default(SaveInfo);
			saveInfo.forDisk = true;
			SaveInfo saveInfo2 = saveInfo;
			ToStream(_SaveCache, saveInfo2);
			ConVar.Server.savecachesize += (int)_SaveCache.Length;
		}
		return _SaveCache;
	}

	public virtual void UpdateNetworkGroup()
	{
		Assert.IsTrue(isServer, "UpdateNetworkGroup called on clientside entity!");
		if (net == null)
		{
			return;
		}
		using (TimeWarning.New("UpdateGroups"))
		{
			if (net.UpdateGroups(base.transform.position))
			{
				SendNetworkGroupChange();
			}
		}
	}

	public virtual Vector3 GetNetworkPosition()
	{
		return base.transform.localPosition;
	}

	public virtual Quaternion GetNetworkRotation()
	{
		return base.transform.localRotation;
	}

	public string InvokeString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<InvokeAction> obj = Facepunch.Pool.GetList<InvokeAction>();
		InvokeHandler.FindInvokes(this, obj);
		foreach (InvokeAction item in obj)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(item.action.Method.Name);
		}
		Facepunch.Pool.FreeList(ref obj);
		return stringBuilder.ToString();
	}

	public BaseEntity LookupPrefab()
	{
		return GameObjectEx.ToBaseEntity(gameManager.FindPrefab(PrefabName));
	}

	public bool EqualNetID(BaseNetworkable other)
	{
		if (other != null && other.net != null && net != null)
		{
			return other.net.ID == net.ID;
		}
		return false;
	}

	public bool EqualNetID(uint otherID)
	{
		if (net != null)
		{
			return otherID == net.ID;
		}
		return false;
	}

	public virtual void ResetState()
	{
		if (children.Count > 0)
		{
			children.Clear();
		}
	}

	public virtual void InitShared()
	{
	}

	public virtual void PreInitShared()
	{
	}

	public virtual void PostInitShared()
	{
	}

	public virtual void DestroyShared()
	{
	}

	public virtual void OnNetworkGroupEnter(Group group)
	{
		Interface.CallHook("OnNetworkGroupEntered", this, group);
	}

	public virtual void OnNetworkGroupLeave(Group group)
	{
		Interface.CallHook("OnNetworkGroupLeft", this, group);
	}

	public void OnNetworkGroupChange()
	{
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			if (child.ShouldInheritNetworkGroup())
			{
				child.net.SwitchGroup(net.group);
			}
			else if (isServer)
			{
				child.UpdateNetworkGroup();
			}
		}
	}

	public void OnNetworkSubscribersEnter(List<Connection> connections)
	{
		if (!Network.Net.sv.IsConnected())
		{
			return;
		}
		foreach (Connection connection in connections)
		{
			BasePlayer basePlayer = connection.player as BasePlayer;
			if (!(basePlayer == null))
			{
				basePlayer.QueueUpdate(BasePlayer.NetworkQueue.Update, this as BaseEntity);
			}
		}
	}

	public void OnNetworkSubscribersLeave(List<Connection> connections)
	{
		if (Network.Net.sv.IsConnected())
		{
			LogEntry(LogEntryType.Network, 2, "LeaveVisibility");
			if (Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Message.Type.EntityDestroy);
				Network.Net.sv.write.EntityID(net.ID);
				Network.Net.sv.write.UInt8(0);
				Network.Net.sv.write.Send(new SendInfo(connections));
			}
		}
	}

	private void EntityDestroy()
	{
		if ((bool)base.gameObject)
		{
			ResetState();
			gameManager.Retire(base.gameObject);
		}
	}

	private void DoEntityDestroy()
	{
		if (IsDestroyed)
		{
			return;
		}
		IsDestroyed = true;
		if (Rust.Application.isQuitting)
		{
			return;
		}
		DestroyShared();
		if (isServer)
		{
			DoServerDestroy();
		}
		using (TimeWarning.New("Registry.Entity.Unregister"))
		{
			Rust.Registry.Entity.Unregister(base.gameObject);
		}
	}

	private void SpawnShared()
	{
		IsDestroyed = false;
		using (TimeWarning.New("Registry.Entity.Register"))
		{
			Rust.Registry.Entity.Register(base.gameObject, this);
		}
	}

	public virtual void Save(SaveInfo info)
	{
		if (prefabID == 0)
		{
			Debug.LogError("PrefabID is 0! " + TransformEx.GetRecursiveName(base.transform), base.gameObject);
		}
		info.msg.baseNetworkable = Facepunch.Pool.Get<ProtoBuf.BaseNetworkable>();
		info.msg.baseNetworkable.uid = net.ID;
		info.msg.baseNetworkable.prefabID = prefabID;
		if (net.group != null)
		{
			info.msg.baseNetworkable.group = net.group.ID;
		}
		if (!info.forDisk)
		{
			info.msg.createdThisFrame = creationFrame == UnityEngine.Time.frameCount;
		}
	}

	public virtual void PostSave(SaveInfo info)
	{
	}

	public void InitLoad(uint entityID)
	{
		net = Network.Net.sv.CreateNetworkable(entityID);
		serverEntities.RegisterID(this);
		PreServerLoad();
	}

	public virtual void PreServerLoad()
	{
	}

	public virtual void Load(LoadInfo info)
	{
		if (info.msg.baseNetworkable != null)
		{
			ProtoBuf.BaseNetworkable baseNetworkable = info.msg.baseNetworkable;
			if (prefabID != baseNetworkable.prefabID)
			{
				Debug.LogError("Prefab IDs don't match! " + prefabID + "/" + baseNetworkable.prefabID + " -> " + base.gameObject, base.gameObject);
			}
		}
	}

	public virtual void PostServerLoad()
	{
		OnSendNetworkUpdateEx.SendOnSendNetworkUpdate(base.gameObject, this as BaseEntity);
	}

	public T ToServer<T>() where T : BaseNetworkable
	{
		if (isServer)
		{
			return this as T;
		}
		return null;
	}

	public virtual bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		return false;
	}

	public static List<Connection> GetConnectionsWithin(Vector3 position, float distance)
	{
		connectionsInSphereList.Clear();
		float num = distance * distance;
		List<Connection> subscribers = GlobalNetworkGroup.subscribers;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!(basePlayer == null) && !(basePlayer.SqrDistance(position) > num))
				{
					connectionsInSphereList.Add(connection);
				}
			}
		}
		return connectionsInSphereList;
	}

	public static void GetCloseConnections(Vector3 position, float distance, List<BasePlayer> players)
	{
		if (Network.Net.sv == null || Network.Net.sv.visibility == null)
		{
			return;
		}
		float num = distance * distance;
		Group group = Network.Net.sv.visibility.GetGroup(position);
		if (group == null)
		{
			return;
		}
		List<Connection> subscribers = group.subscribers;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!(basePlayer == null) && !(basePlayer.SqrDistance(position) > num))
				{
					players.Add(basePlayer);
				}
			}
		}
	}

	public static bool HasCloseConnections(Vector3 position, float distance)
	{
		if (Network.Net.sv == null)
		{
			return false;
		}
		if (Network.Net.sv.visibility == null)
		{
			return false;
		}
		float num = distance * distance;
		Group group = Network.Net.sv.visibility.GetGroup(position);
		if (group == null)
		{
			return false;
		}
		List<Connection> subscribers = group.subscribers;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!(basePlayer == null) && !(basePlayer.SqrDistance(position) > num))
				{
					return true;
				}
			}
		}
		return false;
	}
}
