#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Text;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.Modular;
using UnityEngine;
using UnityEngine.Assertions;

public class VehicleModuleCamper : VehicleModuleSeating
{
	public GameObjectRef SleepingBagEntity;

	public Transform[] SleepingBagPoints;

	public GameObjectRef LockerEntity;

	public Transform LockerPoint;

	public GameObjectRef BbqEntity;

	public Transform BbqPoint;

	public GameObjectRef StorageEntity;

	public Transform StoragePoint;

	public EntityRef<BaseOven> activeBbq;

	public EntityRef<Locker> activeLocker;

	public EntityRef<StorageContainer> activeStorage;

	private bool wasLoaded;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehicleModuleCamper.OnRpcMessage"))
		{
			if (rpc == 2501069650u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenLocker "));
				}
				using (TimeWarning.New("RPC_OpenLocker"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2501069650u, "RPC_OpenLocker", this, player, 3f))
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
							RPC_OpenLocker(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_OpenLocker");
					}
				}
				return true;
			}
			if (rpc == 4185921214u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenStorage "));
				}
				using (TimeWarning.New("RPC_OpenStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4185921214u, "RPC_OpenStorage", this, player, 3f))
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
							RPC_OpenStorage(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_OpenStorage");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
		activeBbq.Set(null);
		activeLocker.Set(null);
		activeStorage.Set(null);
		wasLoaded = false;
	}

	public override void ModuleAdded(BaseModularVehicle vehicle, int firstSocketIndex)
	{
		base.ModuleAdded(vehicle, firstSocketIndex);
		if (!base.isServer)
		{
			return;
		}
		if (!Rust.Application.isLoadingSave && !wasLoaded)
		{
			for (int i = 0; i < SleepingBagPoints.Length; i++)
			{
				SleepingBagCamper sleepingBagCamper = base.gameManager.CreateEntity(SleepingBagEntity.resourcePath, SleepingBagPoints[i].localPosition, SleepingBagPoints[i].localRotation) as SleepingBagCamper;
				if (sleepingBagCamper != null)
				{
					sleepingBagCamper.SetParent(this);
					sleepingBagCamper.SetSeat(GetSeatAtIndex(i));
					sleepingBagCamper.Spawn();
				}
			}
			PostConditionalRefresh();
			return;
		}
		int num = 0;
		foreach (BaseEntity child in children)
		{
			if (child is SleepingBagCamper sleepingBagCamper2)
			{
				sleepingBagCamper2.SetSeat(GetSeatAtIndex(num++), sendNetworkUpdate: true);
			}
			else if (child is IItemContainerEntity itemContainerEntity)
			{
				ItemContainer inventory = itemContainerEntity.inventory;
				inventory.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(inventory.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
			}
		}
	}

	protected override Vector3 ModifySeatPositionLocalSpace(int index, Vector3 desiredPos)
	{
		CamperSeatConfig seatConfig = GetSeatConfig();
		if (seatConfig != null && seatConfig.SeatPositions.Length > index)
		{
			return seatConfig.SeatPositions[index].localPosition;
		}
		return base.ModifySeatPositionLocalSpace(index, desiredPos);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		wasLoaded = true;
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Rust.Application.isLoadingSave)
		{
			Locker locker = base.gameManager.CreateEntity(LockerEntity.resourcePath, LockerPoint.localPosition, LockerPoint.localRotation) as Locker;
			locker.SetParent(this);
			locker.Spawn();
			ItemContainer inventory = locker.inventory;
			inventory.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(inventory.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
			activeLocker.Set(locker);
			BaseOven baseOven = base.gameManager.CreateEntity(BbqEntity.resourcePath, BbqPoint.localPosition, BbqPoint.localRotation) as BaseOven;
			baseOven.SetParent(this);
			baseOven.Spawn();
			ItemContainer inventory2 = baseOven.inventory;
			inventory2.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(inventory2.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
			activeBbq.Set(baseOven);
			StorageContainer storageContainer = base.gameManager.CreateEntity(StorageEntity.resourcePath, StoragePoint.localPosition, StoragePoint.localRotation) as StorageContainer;
			storageContainer.SetParent(this);
			storageContainer.Spawn();
			ItemContainer inventory3 = storageContainer.inventory;
			inventory3.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(inventory3.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
			activeStorage.Set(storageContainer);
			PostConditionalRefresh();
		}
	}

	private void OnItemAddedRemoved(Item item, bool add)
	{
		AssociatedItemInstance?.LockUnlock(!CanBeMovedNowOnVehicle());
	}

	protected override bool CanBeMovedNowOnVehicle()
	{
		foreach (BaseEntity child in children)
		{
			if (child is IItemContainerEntity itemContainerEntity && !ObjectEx.IsUnityNull(itemContainerEntity) && !itemContainerEntity.inventory.IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	protected override void PostConditionalRefresh()
	{
		base.PostConditionalRefresh();
		if (base.isClient)
		{
			return;
		}
		CamperSeatConfig seatConfig = GetSeatConfig();
		if (seatConfig != null && mountPoints != null)
		{
			for (int i = 0; i < mountPoints.Count; i++)
			{
				if (mountPoints[i].mountable != null)
				{
					mountPoints[i].mountable.transform.position = seatConfig.SeatPositions[i].position;
					mountPoints[i].mountable.SendNetworkUpdate();
				}
			}
		}
		if (activeBbq.IsValid(base.isServer) && seatConfig != null)
		{
			BaseOven baseOven = activeBbq.Get(serverside: true);
			baseOven.transform.position = seatConfig.StovePosition.position;
			baseOven.transform.rotation = seatConfig.StovePosition.rotation;
			baseOven.SendNetworkUpdate();
		}
		if (activeStorage.IsValid(base.isServer) && seatConfig != null)
		{
			StorageContainer storageContainer = activeStorage.Get(base.isServer);
			storageContainer.transform.position = seatConfig.StoragePosition.position;
			storageContainer.transform.rotation = seatConfig.StoragePosition.rotation;
			storageContainer.SendNetworkUpdate();
		}
	}

	private CamperSeatConfig GetSeatConfig()
	{
		List<ConditionalObject> list = GetConditionals();
		CamperSeatConfig result = null;
		foreach (ConditionalObject item in list)
		{
			if (item.gameObject.activeSelf && item.gameObject.TryGetComponent<CamperSeatConfig>(out var component))
			{
				result = component;
			}
		}
		return result;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.camperModule == null)
		{
			info.msg.camperModule = Facepunch.Pool.Get<CamperModule>();
		}
		info.msg.camperModule.bbqId = activeBbq.uid;
		info.msg.camperModule.lockerId = activeLocker.uid;
		info.msg.camperModule.storageID = activeStorage.uid;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_OpenLocker(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanBeLooted(player))
		{
			IItemContainerEntity itemContainerEntity = activeLocker.Get(base.isServer);
			if (!ObjectEx.IsUnityNull(itemContainerEntity))
			{
				itemContainerEntity.PlayerOpenLoot(player);
			}
			else
			{
				Debug.LogError(GetType().Name + ": No container component found.");
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_OpenStorage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanBeLooted(player))
		{
			IItemContainerEntity itemContainerEntity = activeStorage.Get(base.isServer);
			if (!ObjectEx.IsUnityNull(itemContainerEntity))
			{
				itemContainerEntity.PlayerOpenLoot(player);
			}
			else
			{
				Debug.LogError(GetType().Name + ": No container component found.");
			}
		}
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			if (activeStorage.IsValid(base.isServer))
			{
				activeStorage.Get(base.isServer).DropItems();
			}
			if (activeBbq.IsValid(base.isServer))
			{
				activeBbq.Get(base.isServer).DropItems();
			}
			if (activeLocker.IsValid(base.isServer))
			{
				activeLocker.Get(base.isServer).DropItems();
			}
		}
		base.DoServerDestroy();
	}

	public IItemContainerEntity GetContainer()
	{
		Locker locker = activeLocker.Get(base.isServer);
		if (locker != null && BaseNetworkableEx.IsValid(locker) && !locker.inventory.IsEmpty())
		{
			return locker;
		}
		BaseOven baseOven = activeBbq.Get(base.isServer);
		if (baseOven != null && BaseNetworkableEx.IsValid(baseOven) && !baseOven.inventory.IsEmpty())
		{
			return baseOven;
		}
		StorageContainer storageContainer = activeStorage.Get(base.isServer);
		if (storageContainer != null && BaseNetworkableEx.IsValid(storageContainer) && !storageContainer.inventory.IsEmpty())
		{
			return storageContainer;
		}
		return null;
	}

	public override string Admin_Who()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (BaseEntity child in children)
		{
			if (child is SleepingBagCamper sleepingBagCamper)
			{
				stringBuilder.AppendLine($"Bag {num++}:");
				stringBuilder.AppendLine(sleepingBagCamper.Admin_Who());
			}
		}
		return stringBuilder.ToString();
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (base.IsOnAVehicle && base.Vehicle.IsDead())
		{
			return base.CanBeLooted(player);
		}
		if (base.CanBeLooted(player))
		{
			return IsOnThisModule(player);
		}
		return false;
	}

	public override bool IsOnThisModule(BasePlayer player)
	{
		if (base.IsOnThisModule(player))
		{
			return true;
		}
		if (!player.isMounted)
		{
			return false;
		}
		return new OBB(base.transform, bounds).Contains(player.CenterPoint());
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.camperModule != null)
		{
			activeBbq.uid = info.msg.camperModule.bbqId;
			activeLocker.uid = info.msg.camperModule.lockerId;
			activeStorage.uid = info.msg.camperModule.storageID;
		}
	}
}
