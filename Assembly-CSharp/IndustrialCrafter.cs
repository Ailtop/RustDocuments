#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class IndustrialCrafter : IndustrialEntity, IItemContainerEntity, IIdealSlotEntity, ILootableEntity, LootPanel.IHasLootPanel, IContainerSounds, IIndustrialStorage
{
	public string LootPanelName = "generic";

	public bool NeedsBuildingPrivilegeToUse;

	public bool OnlyOneUser;

	public SoundDefinition ContainerOpenSound;

	public SoundDefinition ContainerCloseSound;

	public AnimationCurve MaterialOffsetCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public const Flags Crafting = Flags.Reserved1;

	public const Flags FullOutput = Flags.Reserved2;

	public Renderer[] MeshRenderers;

	public ParticleSystemContainer JobCompleteFx;

	public SoundDefinition JobCompleteSoundDef;

	public const int BlueprintSlotStart = 0;

	public const int BlueprintSlotEnd = 3;

	private ItemDefinition currentlyCrafting;

	private int currentlyCraftingAmount;

	private const int StorageSize = 12;

	private const int InputSlotStart = 4;

	private const int InputSlotEnd = 7;

	private const int OutputSlotStart = 8;

	private const int OutputSlotEnd = 11;

	public TimeUntilWithDuration jobFinishes { get; private set; }

	public ItemContainer inventory { get; set; }

	public Transform Transform => base.transform;

	public bool DropsLoot => true;

	public float DestroyLootPercent => 0f;

	public bool DropFloats { get; }

	public ulong LastLootedBy { get; set; }

	public ItemContainer Container => inventory;

	public BaseEntity IndustrialEntity => this;

	public Translate.Phrase LootPanelTitle => new Translate.Phrase("industrial.crafter.loot", "Industrial Crafter");

	public SoundDefinition OpenSound => ContainerOpenSound;

	public SoundDefinition CloseSound => ContainerCloseSound;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("IndustrialCrafter.OnRpcMessage"))
		{
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
							RPCMessage rpc2 = rPCMessage;
							RPC_OpenLoot(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_OpenLoot");
					}
				}
				return true;
			}
			if (rpc == 4167839872u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SvSwitch "));
				}
				using (TimeWarning.New("SvSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4167839872u, "SvSwitch", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4167839872u, "SvSwitch", this, player, 3f))
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
							SvSwitch(msg2);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SvSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = next.HasFlag(Flags.On);
		if (old.HasFlag(Flags.On) != flag && base.isServer)
		{
			float industrialCrafterFrequency = ConVar.Server.industrialCrafterFrequency;
			if (flag && industrialCrafterFrequency > 0f)
			{
				InvokeRandomized(CheckCraft, industrialCrafterFrequency, industrialCrafterFrequency, industrialCrafterFrequency * 0.5f);
			}
			else
			{
				CancelInvoke(CheckCraft);
			}
		}
	}

	public override void OnKilled(HitInfo info)
	{
		base.OnKilled(info);
		DropItems(info?.Initiator);
	}

	public void DropItems(BaseEntity initiator = null)
	{
		StorageContainer.DropItems(this, initiator);
	}

	public bool ShouldDropItemsIndividually()
	{
		return false;
	}

	public void DropBonusItems(BaseEntity initiator, ItemContainer container)
	{
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (inventory != null)
		{
			BasePlayer player = rpc.player;
			if ((bool)player && player.CanInteract())
			{
				PlayerOpenLoot(player);
			}
		}
	}

	public virtual bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (Interface.CallHook("CanLootEntity", player, this) != null)
		{
			return false;
		}
		if (NeedsBuildingPrivilegeToUse && !player.CanBuild())
		{
			return false;
		}
		if (OnlyOneUser && IsOpen())
		{
			player.ChatMessage("Already in use");
			return false;
		}
		if (player.inventory.loot.StartLootingEntity(this, doPositionChecks))
		{
			SetFlag(Flags.Open, b: true);
			player.inventory.loot.AddContainer(inventory);
			player.inventory.loot.SendImmediate();
			player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", LootPanelName);
			SendNetworkUpdate();
			return true;
		}
		return false;
	}

	public virtual void PlayerStoppedLooting(BasePlayer player)
	{
		SetFlag(Flags.Open, b: false);
		SendNetworkUpdate();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (inventory == null)
		{
			CreateInventory(giveUID: true);
		}
	}

	public void CreateInventory(bool giveUID)
	{
		inventory = new ItemContainer();
		inventory.entityOwner = this;
		inventory.canAcceptItem = CanAcceptItem;
		inventory.ServerInitialize(null, 12);
		if (giveUID)
		{
			inventory.GiveUID();
		}
	}

	private bool CanAcceptItem(Item item, int index)
	{
		if (index >= 0 && index <= 3 && !item.IsBlueprint())
		{
			return false;
		}
		return true;
	}

	private void CheckCraft()
	{
		global::IndustrialEntity.Queue.Add(this);
	}

	private Item GetTargetBlueprint(int index)
	{
		if (inventory == null)
		{
			return null;
		}
		if (index < 0 || index > 3)
		{
			return null;
		}
		Item slot = inventory.GetSlot(index);
		if (slot == null || !slot.IsBlueprint())
		{
			return null;
		}
		return slot;
	}

	protected override void RunJob()
	{
		base.RunJob();
		if (ConVar.Server.industrialCrafterFrequency <= 0f || HasFlag(Flags.Reserved1) || currentlyCrafting != null)
		{
			return;
		}
		for (int i = 0; i <= 3; i++)
		{
			Item targetBlueprint = GetTargetBlueprint(i);
			if (targetBlueprint == null || GetWorkbench() == null || GetWorkbench().Workbenchlevel < targetBlueprint.blueprintTargetDef.Blueprint.workbenchLevelRequired)
			{
				continue;
			}
			ItemBlueprint blueprint = targetBlueprint.blueprintTargetDef.Blueprint;
			if (Interface.CallHook("OnItemCraft", this, blueprint) != null)
			{
				break;
			}
			bool flag = true;
			foreach (ItemAmount ingredient in blueprint.ingredients)
			{
				if ((float)GetInputAmount(ingredient.itemDef) < ingredient.amount)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			flag = false;
			for (int j = 8; j <= 11; j++)
			{
				Item slot = inventory.GetSlot(j);
				if (slot == null || (slot.info == targetBlueprint.blueprintTargetDef && slot.amount + blueprint.amountToCreate <= slot.MaxStackable()))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				SetFlag(Flags.Reserved2, b: true);
				continue;
			}
			SetFlag(Flags.Reserved2, b: false);
			foreach (ItemAmount ingredient2 in blueprint.ingredients)
			{
				ConsumeInputIngredient(ingredient2);
			}
			currentlyCrafting = targetBlueprint.blueprintTargetDef;
			currentlyCraftingAmount = blueprint.amountToCreate;
			float time = blueprint.time;
			Invoke(CompleteCraft, time);
			jobFinishes = time;
			SetFlag(Flags.Reserved1, b: true);
			ClientRPC((Connection)null, "ClientUpdateCraftTimeRemaining", (float)jobFinishes, jobFinishes.Duration);
			break;
		}
	}

	private void CompleteCraft()
	{
		bool flag = false;
		for (int i = 8; i <= 11; i++)
		{
			Item slot = inventory.GetSlot(i);
			if (slot == null)
			{
				Item item = ItemManager.Create(currentlyCrafting, currentlyCraftingAmount, 0uL);
				item.position = i;
				inventory.Insert(item);
				flag = true;
				break;
			}
			if (slot.info == currentlyCrafting && slot.amount + currentlyCraftingAmount <= slot.MaxStackable())
			{
				slot.amount += currentlyCraftingAmount;
				slot.MarkDirty();
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ItemManager.Create(currentlyCrafting, currentlyCraftingAmount, 0uL).Drop(base.transform.position + base.transform.forward * 0.5f, Vector3.zero);
		}
		currentlyCrafting = null;
		currentlyCraftingAmount = 0;
		SetFlag(Flags.Reserved1, b: false);
	}

	private int GetInputAmount(ItemDefinition def)
	{
		if (def == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 4; i <= 7; i++)
		{
			Item slot = inventory.GetSlot(i);
			if (slot != null && def == slot.info)
			{
				num += slot.amount;
			}
		}
		return num;
	}

	private bool ConsumeInputIngredient(ItemAmount am)
	{
		if (am.itemDef == null)
		{
			return false;
		}
		float num = am.amount;
		for (int i = 4; i <= 7; i++)
		{
			Item slot = inventory.GetSlot(i);
			if (slot != null && am.itemDef == slot.info)
			{
				float num2 = Mathf.Min(num, slot.amount);
				slot.UseItem((int)num2);
				num -= num2;
				if (num2 <= 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (currentlyCrafting != null)
			{
				info.msg.industrialCrafter = Facepunch.Pool.Get<ProtoBuf.IndustrialCrafter>();
				info.msg.industrialCrafter.currentlyCrafting = currentlyCrafting.itemid;
				info.msg.industrialCrafter.currentlyCraftingAmount = currentlyCraftingAmount;
			}
			if (inventory != null)
			{
				info.msg.storageBox = Facepunch.Pool.Get<StorageBox>();
				info.msg.storageBox.contents = inventory.Save();
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.storageBox != null && inventory != null)
		{
			inventory.Load(info.msg.storageBox.contents);
			inventory.capacity = 12;
		}
		if (base.isServer && info.fromDisk && info.msg.industrialCrafter != null)
		{
			currentlyCrafting = ItemManager.FindItemDefinition(info.msg.industrialCrafter.currentlyCrafting);
			currentlyCraftingAmount = info.msg.industrialCrafter.currentlyCraftingAmount;
			CompleteCraft();
		}
	}

	public Vector2i InputSlotRange(int slotIndex)
	{
		if (slotIndex == 3)
		{
			return new Vector2i(0, 3);
		}
		return new Vector2i(4, 7);
	}

	public Vector2i OutputSlotRange(int slotIndex)
	{
		if (slotIndex == 1)
		{
			return new Vector2i(0, 3);
		}
		return new Vector2i(8, 11);
	}

	public void OnStorageItemTransferBegin()
	{
	}

	public void OnStorageItemTransferEnd()
	{
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (inputSlot == 1)
		{
			SetFlag(Flags.Reserved8, inputAmount >= ConsumptionAmount() && inputAmount > 0);
			currentEnergy = inputAmount;
			ensureOutputsUpdated = true;
			MarkDirty();
		}
		if (inputSlot == 1 && inputAmount <= 0 && IsOn())
		{
			SetSwitch(wantsOn: false);
		}
		if (inputSlot == 2)
		{
			if (IsOn() && inputAmount == 0)
			{
				SetSwitch(wantsOn: false);
			}
			else if (!IsOn() && inputAmount > 0 && HasFlag(Flags.Reserved8))
			{
				SetSwitch(wantsOn: true);
			}
		}
		if (inputSlot == 4 && inputAmount > 0 && HasFlag(Flags.Reserved8))
		{
			SetSwitch(wantsOn: true);
		}
		if (inputSlot == 5 && inputAmount > 0 && HasFlag(Flags.Reserved8))
		{
			SetSwitch(wantsOn: false);
		}
	}

	public virtual void SetSwitch(bool wantsOn)
	{
		if (wantsOn != IsOn())
		{
			SetFlag(Flags.On, wantsOn, recursive: false, networkupdate: false);
			SetFlag(Flags.Busy, b: true, recursive: false, networkupdate: false);
			if (!wantsOn)
			{
				SetFlag(Flags.Reserved2, b: false, recursive: false, networkupdate: false);
			}
			Invoke(Unbusy, 0.5f);
			SendNetworkUpdateImmediate();
			MarkDirty();
		}
	}

	public void Unbusy()
	{
		SetFlag(Flags.Busy, b: false);
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void SvSwitch(RPCMessage msg)
	{
		SetSwitch(!IsOn());
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.isServer)
		{
			if (inventory != null && inventory.IsEmpty())
			{
				return base.CanPickup(player);
			}
			return false;
		}
		return base.CanPickup(player);
	}

	public int GetIdealSlot(BasePlayer player, Item item)
	{
		return -1;
	}

	public ItemContainerId GetIdealContainer(BasePlayer player, Item item, bool altMove)
	{
		return default(ItemContainerId);
	}

	public Workbench GetWorkbench()
	{
		return GetParentEntity() as Workbench;
	}
}
