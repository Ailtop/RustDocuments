#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Facepunch.CardGames;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class CardTable : BaseVehicle
{
	[Serializable]
	public class ChipStack : IComparable<ChipStack>
	{
		public int chipValue;

		public GameObject[] chips;

		public int CompareTo(ChipStack other)
		{
			if (other == null)
			{
				return 1;
			}
			return chipValue.CompareTo(other.chipValue);
		}
	}

	public enum CardGameOption
	{
		TexasHoldEm = 0
	}

	[Serializable]
	public class PlayerStorageInfo
	{
		public Transform storagePos;

		public EntityRef storageInstance;

		public CardTablePlayerStorage GetStorage()
		{
			BaseEntity baseEntity = storageInstance.Get(serverside: true);
			if (baseEntity != null && BaseNetworkableEx.IsValid(baseEntity))
			{
				return baseEntity as CardTablePlayerStorage;
			}
			return null;
		}
	}

	public EntityRef PotInstance;

	[Header("Card Table")]
	[SerializeField]
	private GameObjectRef uiPrefab;

	[SerializeField]
	private GameObjectRef playerStoragePrefab;

	[SerializeField]
	private GameObjectRef potPrefab;

	[SerializeField]
	private ViewModel viewModel;

	[SerializeField]
	private CardTableUI.PlayingCardImage[] tableCards;

	[SerializeField]
	private Renderer[] tableCardBackings;

	[SerializeField]
	private Canvas cardUICanvas;

	[SerializeField]
	private Image[] tableCardImages;

	[SerializeField]
	private Sprite blankCard;

	[SerializeField]
	private Transform chipStacksParent;

	[SerializeField]
	private ChipStack[] chipStacks;

	[SerializeField]
	private ChipStack[] fillerStacks;

	public ItemDefinition scrapItemDef;

	public PlayerStorageInfo[] playerStoragePoints;

	public CardGameOption gameOption;

	private CardGameController _gameCont;

	private const float MAX_STORAGE_INTERACTION_DIST = 1f;

	protected override bool CanSwapSeats
	{
		public get
		{
			return false;
		}
	}

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

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CardTable.OnRpcMessage"))
		{
			if (rpc == 2395020190u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
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
				if (Global.developer > 2)
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
				if (Global.developer > 2)
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
				if (Global.developer > 2)
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
				if (Global.developer > 2)
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
				if (Global.developer > 2)
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

	public StorageContainer GetPot()
	{
		BaseEntity baseEntity = PotInstance.Get(serverside: true);
		if (baseEntity != null && BaseNetworkableEx.IsValid(baseEntity))
		{
			return baseEntity as StorageContainer;
		}
		return null;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.cardTable = Facepunch.Pool.Get<ProtoBuf.CardTable>();
		info.msg.cardTable.potRef = PotInstance.uid;
		if (!info.forDisk)
		{
			GameController.Save(info.msg.cardTable);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		int num = 0;
		foreach (BaseEntity child in children)
		{
			if (child is CardTablePlayerStorage ent)
			{
				playerStoragePoints[num].storageInstance.Set(ent);
				num++;
			}
		}
		StorageContainer pot = GetPot();
		if (pot != null)
		{
			pot.inventory.Clear();
		}
	}

	public override void SpawnSubEntities()
	{
		base.SpawnSubEntities();
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
			CardTablePlayerStorage cardTablePlayerStorage = baseEntity as CardTablePlayerStorage;
			if (cardTablePlayerStorage != null)
			{
				cardTablePlayerStorage.SetCardTable(this);
				cardTablePlayerStorage.SetParent(this);
				cardTablePlayerStorage.Spawn();
				playerStorageInfo.storageInstance.Set(baseEntity);
			}
			else
			{
				Debug.LogError(GetType().Name + ": Spawned prefab is not a CardTablePlayerStorage as expected.");
			}
		}
	}

	internal override void DoServerDestroy()
	{
		GameController?.OnTableDestroyed();
		PlayerStorageInfo[] array = playerStoragePoints;
		for (int i = 0; i < array.Length; i++)
		{
			CardTablePlayerStorage storage = array[i].GetStorage();
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

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		GameController.LeaveTable(player.userID);
	}

	public override void PrePlayerDismount(BasePlayer player, BaseMountable seat)
	{
		base.PrePlayerDismount(player, seat);
		CardTablePlayerStorage playerStorage = GetPlayerStorage(player.userID);
		if (playerStorage != null)
		{
			playerStorage.inventory.GetSlot(0)?.MoveToContainer(player.inventory.containerMain, -1, allowStack: true, ignoreStackLimit: true);
		}
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

	public CardTablePlayerStorage GetPlayerStorage(ulong playerID)
	{
		int mountPointIndex = GetMountPointIndex(playerID);
		if (mountPointIndex < 0)
		{
			return null;
		}
		return playerStoragePoints[mountPointIndex].GetStorage();
	}

	public CardTablePlayerStorage GetPlayerStorage(int storageIndex)
	{
		return playerStoragePoints[storageIndex].GetStorage();
	}

	public void PlayerStorageChanged()
	{
		GameController.PlayerStorageChanged();
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
		Debug.Log("Adding test NPC for card table");
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

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
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
	private void RPC_LeaveTable(RPCMessage msg)
	{
		GameController.LeaveTable(msg.player.userID);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_PlayerInput(RPCMessage msg)
	{
		GameController.ReceivedInputFromPlayer(msg.player, msg.read.Int32(), countAsAction: true, msg.read.Int32());
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		GameController.Dispose();
	}

	private CardGameController GetGameController()
	{
		CardGameOption cardGameOption = gameOption;
		return new TexasHoldEmController(this);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer)
		{
			PotInstance.uid = info.msg.cardTable.potRef;
		}
	}
}
