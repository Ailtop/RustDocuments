#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.CardGames;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class BaseCardGameEntity : BaseVehicle
{
	[Serializable]
	public class PlayerStorageInfo
	{
		public Transform storagePos;

		public EntityRef storageInstance;

		public CardGamePlayerStorage GetStorage()
		{
			BaseEntity baseEntity = storageInstance.Get(serverside: true);
			if (baseEntity != null && BaseNetworkableEx.IsValid(baseEntity))
			{
				return baseEntity as CardGamePlayerStorage;
			}
			return null;
		}
	}

	public enum CardGameOption
	{
		TexasHoldEm = 0,
		Blackjack = 1
	}

	[SerializeField]
	[Header("Card Game")]
	private GameObjectRef uiPrefab;

	public ItemDefinition scrapItemDef;

	[SerializeField]
	private GameObjectRef potPrefab;

	public PlayerStorageInfo[] playerStoragePoints;

	[SerializeField]
	private GameObjectRef playerStoragePrefab;

	private CardGameController _gameCont;

	public CardGameOption gameOption;

	public EntityRef PotInstance;

	private bool storageLinked;

	public int ScrapItemID => scrapItemDef.itemid;

	public CardGameController GameController
	{
		get
		{
			if (_gameCont == null)
			{
				_gameCont = GetGameController();
			}
			return _gameCont;
		}
	}

	protected abstract float MaxStorageInteractionDist { get; }

	protected override bool CanSwapSeats
	{
		public get
		{
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseCardGameEntity.OnRpcMessage"))
		{
			if (rpc == 2395020190u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Editor_MakeRandomMove "));
				}
				using (TimeWarning.New("RPC_Editor_MakeRandomMove"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2395020190u, "RPC_Editor_MakeRandomMove", this, player, 3f))
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
							RPC_Editor_MakeRandomMove(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Editor_MakeRandomMove");
					}
				}
				return true;
			}
			if (rpc == 1608700874 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Editor_SpawnTestPlayer "));
				}
				using (TimeWarning.New("RPC_Editor_SpawnTestPlayer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1608700874u, "RPC_Editor_SpawnTestPlayer", this, player, 3f))
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
							RPC_Editor_SpawnTestPlayer(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_Editor_SpawnTestPlayer");
					}
				}
				return true;
			}
			if (rpc == 1499640189 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_LeaveTable "));
				}
				using (TimeWarning.New("RPC_LeaveTable"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1499640189u, "RPC_LeaveTable", this, player, 3f))
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
							RPCMessage msg4 = rPCMessage;
							RPC_LeaveTable(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_LeaveTable");
					}
				}
				return true;
			}
			if (rpc == 331989034 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenLoot "));
				}
				using (TimeWarning.New("RPC_OpenLoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(331989034u, "RPC_OpenLoot", this, player, 3f))
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
							RPCMessage msg5 = rPCMessage;
							RPC_OpenLoot(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_OpenLoot");
					}
				}
				return true;
			}
			if (rpc == 2847205856u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Play "));
				}
				using (TimeWarning.New("RPC_Play"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2847205856u, "RPC_Play", this, player, 3f))
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
							RPCMessage msg6 = rPCMessage;
							RPC_Play(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RPC_Play");
					}
				}
				return true;
			}
			if (rpc == 2495306863u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_PlayerInput "));
				}
				using (TimeWarning.New("RPC_PlayerInput"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2495306863u, "RPC_PlayerInput", this, player, 3f))
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
							RPCMessage msg7 = rPCMessage;
							RPC_PlayerInput(msg7);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in RPC_PlayerInput");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer)
		{
			PotInstance.uid = info.msg.cardGame.potRef;
		}
	}

	private CardGameController GetGameController()
	{
		return gameOption switch
		{
			CardGameOption.TexasHoldEm => new TexasHoldEmController(this), 
			CardGameOption.Blackjack => new BlackjackController(this), 
			_ => new TexasHoldEmController(this), 
		};
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		GameController.Dispose();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.cardGame = Facepunch.Pool.Get<CardGame>();
		info.msg.cardGame.potRef = PotInstance.uid;
		if (!info.forDisk && storageLinked)
		{
			GameController.Save(info.msg.cardGame);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		int num = 0;
		int num2 = 0;
		foreach (BaseEntity child in children)
		{
			if (child is CardGamePlayerStorage cardGamePlayerStorage)
			{
				playerStoragePoints[num].storageInstance.Set(cardGamePlayerStorage);
				if (!cardGamePlayerStorage.inventory.IsEmpty())
				{
					num2++;
				}
				num++;
			}
		}
		storageLinked = true;
		bool flag = true;
		StorageContainer pot = GetPot();
		if (pot == null)
		{
			flag = false;
		}
		else
		{
			int num3 = ((num2 > 0) ? num2 : playerStoragePoints.Length);
			int iAmount = Mathf.CeilToInt(pot.inventory.GetAmount(ScrapItemID, onlyUsableAmounts: true) / num3);
			PlayerStorageInfo[] array = playerStoragePoints;
			for (int i = 0; i < array.Length; i++)
			{
				CardGamePlayerStorage cardGamePlayerStorage2 = array[i].storageInstance.Get(base.isServer) as CardGamePlayerStorage;
				if (!BaseNetworkableEx.IsValid(cardGamePlayerStorage2) || (cardGamePlayerStorage2.inventory.IsEmpty() && num2 != 0))
				{
					continue;
				}
				List<Item> obj = Facepunch.Pool.GetList<Item>();
				if (pot.inventory.Take(obj, ScrapItemID, iAmount) > 0)
				{
					foreach (Item item in obj)
					{
						if (!item.MoveToContainer(cardGamePlayerStorage2.inventory, -1, allowStack: true, ignoreStackLimit: true))
						{
							item.Remove();
						}
					}
				}
				Facepunch.Pool.FreeList(ref obj);
			}
		}
		if (flag)
		{
			PlayerStorageInfo[] array = playerStoragePoints;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].storageInstance.IsValid(base.isServer))
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			Debug.LogWarning(GetType().Name + ": Card game storage didn't load in. Destroying the card game (and parent entity if there is one).");
			BaseEntity baseEntity = GetParentEntity();
			if (baseEntity != null)
			{
				baseEntity.Invoke(baseEntity.KillMessage, 0f);
			}
			else
			{
				Invoke(base.KillMessage, 0f);
			}
		}
	}

	internal override void DoServerDestroy()
	{
		GameController?.OnTableDestroyed();
		PlayerStorageInfo[] array = playerStoragePoints;
		for (int i = 0; i < array.Length; i++)
		{
			CardGamePlayerStorage storage = array[i].GetStorage();
			if (storage != null)
			{
				storage.DropItems();
			}
		}
		StorageContainer pot = GetPot();
		if (pot != null)
		{
			pot.DropItems();
		}
		base.DoServerDestroy();
	}

	public override void PrePlayerDismount(BasePlayer player, BaseMountable seat)
	{
		base.PrePlayerDismount(player, seat);
		if (!Rust.Application.isLoadingSave)
		{
			CardGamePlayerStorage playerStorage = GetPlayerStorage(player.userID);
			if (playerStorage != null)
			{
				playerStorage.inventory.GetSlot(0)?.MoveToContainer(player.inventory.containerMain);
			}
		}
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		GameController.LeaveTable(player.userID);
	}

	public StorageContainer GetPot()
	{
		BaseEntity baseEntity = PotInstance.Get(serverside: true);
		if (baseEntity != null && BaseNetworkableEx.IsValid(baseEntity))
		{
			return baseEntity as StorageContainer;
		}
		return null;
	}

	public BasePlayer IDToPlayer(ulong id)
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (mountPoint.mountable != null && mountPoint.mountable.GetMounted() != null && mountPoint.mountable.GetMounted().userID == id)
			{
				return mountPoint.mountable.GetMounted();
			}
		}
		return null;
	}

	public virtual void PlayerStorageChanged()
	{
		GameController.PlayerStorageChanged();
	}

	public CardGamePlayerStorage GetPlayerStorage(int storageIndex)
	{
		return playerStoragePoints[storageIndex].GetStorage();
	}

	public CardGamePlayerStorage GetPlayerStorage(ulong playerID)
	{
		int mountPointIndex = GetMountPointIndex(playerID);
		if (mountPointIndex < 0)
		{
			return null;
		}
		return playerStoragePoints[mountPointIndex].GetStorage();
	}

	public int GetMountPointIndex(ulong playerID)
	{
		int num = -1;
		for (int i = 0; i < mountPoints.Count; i++)
		{
			BaseMountable mountable = mountPoints[i].mountable;
			if (mountable != null)
			{
				BasePlayer mounted = mountable.GetMounted();
				if (mounted != null && mounted.userID == playerID)
				{
					num = i;
				}
			}
		}
		if (num < 0)
		{
			Debug.LogError(GetType().Name + ": Couldn't find mount point for this player.");
		}
		return num;
	}

	public override void SpawnSubEntities()
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity(potPrefab.resourcePath, Vector3.zero, Quaternion.identity);
		StorageContainer storageContainer = baseEntity as StorageContainer;
		if (storageContainer != null)
		{
			storageContainer.SetParent(this);
			storageContainer.Spawn();
			PotInstance.Set(baseEntity);
		}
		else
		{
			Debug.LogError(GetType().Name + ": Spawned prefab is not a StorageContainer as expected.");
		}
		PlayerStorageInfo[] array = playerStoragePoints;
		foreach (PlayerStorageInfo playerStorageInfo in array)
		{
			baseEntity = GameManager.server.CreateEntity(playerStoragePrefab.resourcePath, playerStorageInfo.storagePos.localPosition, playerStorageInfo.storagePos.localRotation);
			CardGamePlayerStorage cardGamePlayerStorage = baseEntity as CardGamePlayerStorage;
			if (cardGamePlayerStorage != null)
			{
				cardGamePlayerStorage.SetCardTable(this);
				cardGamePlayerStorage.SetParent(this);
				cardGamePlayerStorage.Spawn();
				playerStorageInfo.storageInstance.Set(baseEntity);
				storageLinked = true;
			}
			else
			{
				Debug.LogError(GetType().Name + ": Spawned prefab is not a CardTablePlayerStorage as expected.");
			}
		}
		base.SpawnSubEntities();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_PlayerInput(RPCMessage msg)
	{
		GameController.ReceivedInputFromPlayer(msg.player, msg.read.Int32(), countAsAction: true, msg.read.Int32());
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_LeaveTable(RPCMessage msg)
	{
		GameController.LeaveTable(msg.player.userID);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_OpenLoot(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player != null && PlayerIsMounted(player))
		{
			GetPlayerStorage(player.userID).PlayerOpenLoot(player);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Editor_SpawnTestPlayer(RPCMessage msg)
	{
		if (!UnityEngine.Application.isEditor)
		{
			return;
		}
		int num = GameController.MaxPlayersAtTable();
		if (GameController.NumPlayersAllowedToPlay() >= num || NumMounted() >= num)
		{
			return;
		}
		Debug.Log("Adding test NPC for card game");
		BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", base.transform.position, Quaternion.identity);
		baseEntity.Spawn();
		BasePlayer basePlayer = (BasePlayer)baseEntity;
		AttemptMount(basePlayer, doMountChecks: false);
		GameController.JoinTable(basePlayer);
		if (GameController.TryGetCardPlayerData(basePlayer, out var cardPlayer))
		{
			int scrapAmount = cardPlayer.GetScrapAmount();
			if (scrapAmount < 400)
			{
				StorageContainer storage = cardPlayer.GetStorage();
				if (storage != null)
				{
					storage.inventory.AddItem(scrapItemDef, 400 - scrapAmount, 0uL);
				}
				else
				{
					Debug.LogError("Couldn't get storage for NPC.");
				}
			}
		}
		else
		{
			Debug.Log("Couldn't find player data for NPC. No scrap given.");
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_Editor_MakeRandomMove(RPCMessage msg)
	{
		if (UnityEngine.Application.isEditor)
		{
			GameController.EditorMakeRandomMove();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_Play(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player != null && PlayerIsMounted(player))
		{
			GameController.JoinTable(player);
		}
	}
}
