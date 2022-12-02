#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.Modular;
using UnityEngine;
using UnityEngine.Assertions;

public class VehicleModuleStorage : VehicleModuleSeating
{
	[Serializable]
	public class Storage
	{
		public GameObjectRef storageUnitPrefab;

		public Transform storageUnitPoint;
	}

	[SerializeField]
	private Storage storage;

	private EntityRef storageUnitInstance;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehicleModuleStorage.OnRpcMessage"))
		{
			if (rpc == 4254195175u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Open "));
				}
				using (TimeWarning.New("RPC_Open"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4254195175u, "RPC_Open", this, player, 3f))
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
							RPC_Open(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Open");
					}
				}
				return true;
			}
			if (rpc == 425471188 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_TryOpenWithKeycode "));
				}
				using (TimeWarning.New("RPC_TryOpenWithKeycode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(425471188u, "RPC_TryOpenWithKeycode", this, player, 3f))
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
							RPCMessage msg3 = rPCMessage;
							RPC_TryOpenWithKeycode(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_TryOpenWithKeycode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public IItemContainerEntity GetContainer()
	{
		BaseEntity baseEntity = storageUnitInstance.Get(base.isServer);
		if (baseEntity != null && BaseNetworkableEx.IsValid(baseEntity))
		{
			return baseEntity as IItemContainerEntity;
		}
		return null;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		storageUnitInstance.uid = info.msg.simpleUID.uid;
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Rust.Application.isLoadingSave && storage.storageUnitPoint.gameObject.activeSelf)
		{
			CreateStorageEntity();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container))
		{
			ItemContainer inventory = container.inventory;
			inventory.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(inventory.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
		}
	}

	private void OnItemAddedRemoved(Item item, bool add)
	{
		AssociatedItemInstance?.LockUnlock(!CanBeMovedNowOnVehicle());
	}

	public override void NonUserSpawn()
	{
		Rust.Modular.EngineStorage engineStorage = GetContainer() as Rust.Modular.EngineStorage;
		if (engineStorage != null)
		{
			engineStorage.NonUserSpawn();
		}
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			IItemContainerEntity container = GetContainer();
			if (!ObjectEx.IsUnityNull(container))
			{
				container.DropItems();
			}
		}
		base.DoServerDestroy();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.simpleUID = Facepunch.Pool.Get<SimpleUID>();
		info.msg.simpleUID.uid = storageUnitInstance.uid;
	}

	public void CreateStorageEntity()
	{
		if (IsFullySpawned() && base.isServer && !storageUnitInstance.IsValid(base.isServer))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(storage.storageUnitPrefab.resourcePath, storage.storageUnitPoint.localPosition, storage.storageUnitPoint.localRotation);
			storageUnitInstance.Set(baseEntity);
			baseEntity.SetParent(this);
			baseEntity.Spawn();
			ItemContainer inventory = GetContainer().inventory;
			inventory.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(inventory.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
		}
	}

	public void DestroyStorageEntity()
	{
		if (!IsFullySpawned() || !base.isServer)
		{
			return;
		}
		BaseEntity baseEntity = storageUnitInstance.Get(base.isServer);
		if (BaseNetworkableEx.IsValid(baseEntity))
		{
			if (baseEntity is BaseCombatEntity baseCombatEntity)
			{
				baseCombatEntity.Die();
			}
			else
			{
				baseEntity.Kill();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_Open(RPCMessage msg)
	{
		TryOpen(msg.player);
	}

	private bool TryOpen(BasePlayer player)
	{
		if (!BaseNetworkableEx.IsValid(player) || !CanBeLooted(player))
		{
			return false;
		}
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container))
		{
			container.PlayerOpenLoot(player);
		}
		else
		{
			Debug.LogError(GetType().Name + ": No container component found.");
		}
		return true;
	}

	protected override bool CanBeMovedNowOnVehicle()
	{
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container) && !container.inventory.IsEmpty())
		{
			return false;
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_TryOpenWithKeycode(RPCMessage msg)
	{
		if (!base.IsOnACar)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			string codeEntered = msg.read.String();
			if (base.Car.CarLock.TryOpenWithCode(player, codeEntered))
			{
				TryOpen(player);
			}
			else
			{
				base.Car.ClientRPC(null, "CodeEntryFailed");
			}
		}
	}
}
