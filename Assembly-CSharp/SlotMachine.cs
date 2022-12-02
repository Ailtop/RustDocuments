#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SlotMachine : BaseMountable
{
	public enum SlotFaces
	{
		Scrap = 0,
		Rope = 1,
		Apple = 2,
		LowGrade = 3,
		Wood = 4,
		Bandage = 5,
		Charcoal = 6,
		Gunpowder = 7,
		Rust = 8,
		Meat = 9,
		Hammer = 10,
		Sulfur = 11,
		TechScrap = 12,
		Frags = 13,
		Cloth = 14,
		LuckySeven = 15
	}

	[ServerVar]
	public static int ForcePayoutIndex = -1;

	[Header("Slot Machine")]
	public Transform Reel1;

	public Transform Reel2;

	public Transform Reel3;

	public Transform Arm;

	public AnimationCurve Curve;

	public int Reel1Spins = 16;

	public int Reel2Spins = 48;

	public int Reel3Spins = 80;

	public int MaxReelSpins = 96;

	public float SpinDuration = 2f;

	private int SpinResult1;

	private int SpinResult2;

	private int SpinResult3;

	private int SpinResultPrevious1;

	private int SpinResultPrevious2;

	private int SpinResultPrevious3;

	private float SpinTime;

	public GameObjectRef StoragePrefab;

	public EntityRef StorageInstance;

	public SoundDefinition SpinSound;

	public SlotMachinePayoutDisplay PayoutDisplay;

	public SlotMachinePayoutSettings PayoutSettings;

	public Transform HandIkTarget;

	private const Flags HasScrapForSpin = Flags.Reserved1;

	private const Flags IsSpinningFlag = Flags.Reserved2;

	public Material PayoutIconMaterial;

	public bool UseTimeOfDayAdjustedSprite = true;

	public MeshRenderer[] PulseRenderers;

	public float PulseSpeed = 5f;

	[ColorUsage(true, true)]
	public Color PulseFrom;

	[ColorUsage(true, true)]
	public Color PulseTo;

	private BasePlayer CurrentSpinPlayer;

	private bool IsSpinning => HasFlag(Flags.Reserved2);

	public int CurrentMultiplier { get; private set; } = 1;


	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SlotMachine.OnRpcMessage"))
		{
			if (rpc == 1251063754 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Deposit "));
				}
				using (TimeWarning.New("RPC_Deposit"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1251063754u, "RPC_Deposit", this, player, 3f))
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
							RPC_Deposit(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Deposit");
					}
				}
				return true;
			}
			if (rpc == 1455840454 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Spin "));
				}
				using (TimeWarning.New("RPC_Spin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1455840454u, "RPC_Spin", this, player, 3f))
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
							RPC_Spin(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_Spin");
					}
				}
				return true;
			}
			if (rpc == 3942337446u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_RequestMultiplierChange "));
				}
				using (TimeWarning.New("Server_RequestMultiplierChange"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3942337446u, "Server_RequestMultiplierChange", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3942337446u, "Server_RequestMultiplierChange", this, player, 3f))
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
							Server_RequestMultiplierChange(msg2);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in Server_RequestMultiplierChange");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.slotMachine = Facepunch.Pool.Get<ProtoBuf.SlotMachine>();
		info.msg.slotMachine.oldResult1 = SpinResultPrevious1;
		info.msg.slotMachine.oldResult2 = SpinResultPrevious2;
		info.msg.slotMachine.oldResult3 = SpinResultPrevious3;
		info.msg.slotMachine.newResult1 = SpinResult1;
		info.msg.slotMachine.newResult2 = SpinResult2;
		info.msg.slotMachine.newResult3 = SpinResult3;
		info.msg.slotMachine.isSpinning = IsSpinning;
		info.msg.slotMachine.spinTime = SpinTime;
		info.msg.slotMachine.storageID = StorageInstance.uid;
		info.msg.slotMachine.multiplier = CurrentMultiplier;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.slotMachine != null)
		{
			SpinResultPrevious1 = info.msg.slotMachine.oldResult1;
			SpinResultPrevious2 = info.msg.slotMachine.oldResult2;
			SpinResultPrevious3 = info.msg.slotMachine.oldResult3;
			SpinResult1 = info.msg.slotMachine.newResult1;
			SpinResult2 = info.msg.slotMachine.newResult2;
			SpinResult3 = info.msg.slotMachine.newResult3;
			CurrentMultiplier = info.msg.slotMachine.multiplier;
			if (base.isServer)
			{
				SpinTime = info.msg.slotMachine.spinTime;
			}
			StorageInstance.uid = info.msg.slotMachine.storageID;
			if (info.fromDisk && base.isServer)
			{
				SetFlag(Flags.Reserved2, b: false);
			}
		}
	}

	public override float GetComfort()
	{
		return 1f;
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Rust.Application.isLoadingSave)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(StoragePrefab.resourcePath);
			baseEntity.Spawn();
			baseEntity.SetParent(this);
			StorageInstance.Set(baseEntity);
		}
	}

	internal override void DoServerDestroy()
	{
		SlotMachineStorage slotMachineStorage = StorageInstance.Get(base.isServer) as SlotMachineStorage;
		if (BaseNetworkableEx.IsValid(slotMachineStorage))
		{
			slotMachineStorage.DropItems();
		}
		base.DoServerDestroy();
	}

	private int GetBettingAmount()
	{
		SlotMachineStorage component = StorageInstance.Get(base.isServer).GetComponent<SlotMachineStorage>();
		if (component == null)
		{
			return 0;
		}
		return component.inventory.GetSlot(0)?.amount ?? 0;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_Spin(RPCMessage rpc)
	{
		if (IsSpinning || rpc.player != GetMounted())
		{
			return;
		}
		SlotMachineStorage component = StorageInstance.Get(base.isServer).GetComponent<SlotMachineStorage>();
		int num = (int)PayoutSettings.SpinCost.amount * CurrentMultiplier;
		if (GetBettingAmount() < num || rpc.player == null)
		{
			return;
		}
		BasePlayer basePlayer = (CurrentSpinPlayer = rpc.player);
		Item slot = component.inventory.GetSlot(0);
		int amount = 0;
		if (slot != null)
		{
			if (slot.amount > num)
			{
				slot.MarkDirty();
				slot.amount -= num;
				amount = slot.amount;
			}
			else
			{
				slot.amount -= num;
				slot.RemoveFromContainer();
			}
		}
		component.UpdateAmount(amount);
		SetFlag(Flags.Reserved2, b: true);
		SpinResultPrevious1 = SpinResult1;
		SpinResultPrevious2 = SpinResult2;
		SpinResultPrevious3 = SpinResult3;
		CalculateSpinResults();
		SpinTime = UnityEngine.Time.time;
		ClientRPC(null, "RPC_OnSpin", (sbyte)SpinResult1, (sbyte)SpinResult2, (sbyte)SpinResult3);
		Invoke(CheckPayout, SpinDuration);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_Deposit(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!(player == null) && StorageInstance.IsValid(base.isServer))
		{
			StorageInstance.Get(base.isServer).GetComponent<StorageContainer>().PlayerOpenLoot(player, "", doPositionChecks: false);
		}
	}

	private void CheckPayout()
	{
		bool flag = false;
		if (PayoutSettings != null)
		{
			if (CalculatePayout(out var info, out var bonus))
			{
				int num = ((int)info.Item.amount + bonus) * CurrentMultiplier;
				BaseEntity baseEntity = StorageInstance.Get(serverside: true);
				if (baseEntity != null && baseEntity is SlotMachineStorage slotMachineStorage)
				{
					Item slot = slotMachineStorage.inventory.GetSlot(1);
					if (slot != null)
					{
						slot.amount += num;
						slot.MarkDirty();
					}
					else
					{
						ItemManager.Create(info.Item.itemDef, num, 0uL).MoveToContainer(slotMachineStorage.inventory, 1);
					}
				}
				if (BaseNetworkableEx.IsValid(CurrentSpinPlayer) && CurrentSpinPlayer == _mounted)
				{
					CurrentSpinPlayer.ChatMessage($"You received {num}x {info.Item.itemDef.displayName.english} for slots payout!");
				}
				Analytics.Server.SlotMachineTransaction((int)PayoutSettings.SpinCost.amount * CurrentMultiplier, num);
				if (info.OverrideWinEffect != null && info.OverrideWinEffect.isValid)
				{
					Effect.server.Run(info.OverrideWinEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
				}
				else if (PayoutSettings.DefaultWinEffect != null && PayoutSettings.DefaultWinEffect.isValid)
				{
					Effect.server.Run(PayoutSettings.DefaultWinEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
				}
				if (info.OverrideWinEffect != null && info.OverrideWinEffect.isValid)
				{
					flag = true;
				}
			}
			else
			{
				Analytics.Server.SlotMachineTransaction((int)PayoutSettings.SpinCost.amount * CurrentMultiplier, 0);
			}
		}
		else
		{
			Debug.LogError($"Failed to process spin results: PayoutSettings != null {PayoutSettings != null} CurrentSpinPlayer.IsValid {BaseNetworkableEx.IsValid(CurrentSpinPlayer)} CurrentSpinPlayer == mounted {CurrentSpinPlayer == _mounted}");
		}
		if (!flag)
		{
			SetFlag(Flags.Reserved2, b: false);
		}
		else
		{
			Invoke(DelayedSpinningReset, 4f);
		}
		CurrentSpinPlayer = null;
	}

	private void DelayedSpinningReset()
	{
		SetFlag(Flags.Reserved2, b: false);
	}

	private void CalculateSpinResults()
	{
		if (ForcePayoutIndex != -1)
		{
			SpinResult1 = PayoutSettings.Payouts[ForcePayoutIndex].Result1;
			SpinResult2 = PayoutSettings.Payouts[ForcePayoutIndex].Result2;
			SpinResult3 = PayoutSettings.Payouts[ForcePayoutIndex].Result3;
		}
		else
		{
			SpinResult1 = RandomSpinResult();
			SpinResult2 = RandomSpinResult();
			SpinResult3 = RandomSpinResult();
		}
	}

	private int RandomSpinResult()
	{
		int num = new System.Random(UnityEngine.Random.Range(0, 1000)).Next(0, PayoutSettings.TotalStops);
		int num2 = 0;
		int num3 = 0;
		int[] virtualFaces = PayoutSettings.VirtualFaces;
		foreach (int num4 in virtualFaces)
		{
			if (num < num4 + num2)
			{
				return num3;
			}
			num2 += num4;
			num3++;
		}
		return 15;
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		BaseEntity baseEntity = StorageInstance.Get(serverside: true);
		if (baseEntity != null && baseEntity is SlotMachineStorage slotMachineStorage)
		{
			slotMachineStorage.inventory.GetSlot(1)?.MoveToContainer(player.inventory.containerMain);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void Server_RequestMultiplierChange(RPCMessage msg)
	{
		if (!(msg.player != _mounted))
		{
			CurrentMultiplier = Mathf.Clamp(msg.read.Int32(), 1, 5);
			OnBettingScrapUpdated(GetBettingAmount());
			SendNetworkUpdate();
		}
	}

	public void OnBettingScrapUpdated(int amount)
	{
		SetFlag(Flags.Reserved1, (float)amount >= PayoutSettings.SpinCost.amount * (float)CurrentMultiplier);
	}

	private bool CalculatePayout(out SlotMachinePayoutSettings.PayoutInfo info, out int bonus)
	{
		info = default(SlotMachinePayoutSettings.PayoutInfo);
		bonus = 0;
		SlotMachinePayoutSettings.IndividualPayouts[] facePayouts = PayoutSettings.FacePayouts;
		for (int i = 0; i < facePayouts.Length; i++)
		{
			SlotMachinePayoutSettings.IndividualPayouts individualPayouts = facePayouts[i];
			if (individualPayouts.Result == SpinResult1)
			{
				bonus += (int)individualPayouts.Item.amount;
			}
			if (individualPayouts.Result == SpinResult2)
			{
				bonus += (int)individualPayouts.Item.amount;
			}
			if (individualPayouts.Result == SpinResult3)
			{
				bonus += (int)individualPayouts.Item.amount;
			}
			if (bonus > 0)
			{
				info.Item = new ItemAmount(individualPayouts.Item.itemDef);
			}
		}
		SlotMachinePayoutSettings.PayoutInfo[] payouts = PayoutSettings.Payouts;
		for (int i = 0; i < payouts.Length; i++)
		{
			SlotMachinePayoutSettings.PayoutInfo payoutInfo = payouts[i];
			if (payoutInfo.Result1 == SpinResult1 && payoutInfo.Result2 == SpinResult2 && payoutInfo.Result3 == SpinResult3)
			{
				info = payoutInfo;
				return true;
			}
		}
		return bonus > 0;
	}
}
