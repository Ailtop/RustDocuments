#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseLiquidVessel : AttackEntity
{
	[Header("Liquid Vessel")]
	public GameObjectRef thrownWaterObject;

	public GameObjectRef ThrowEffect3P;

	public SoundDefinition throwSound3P;

	public GameObjectRef fillFromContainer;

	public GameObjectRef fillFromWorld;

	public SoundDefinition fillFromContainerStartSoundDef;

	public SoundDefinition fillFromContainerSoundDef;

	public SoundDefinition fillFromWorldStartSoundDef;

	public SoundDefinition fillFromWorldSoundDef;

	public bool hasLid;

	public float throwScale = 10f;

	public bool canDrinkFrom;

	public bool updateVMWater;

	public float minThrowFrac;

	public bool useThrowAnim;

	public float fillMlPerSec = 500f;

	private float lastFillTime;

	private float nextFreeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseLiquidVessel.OnRpcMessage"))
		{
			if (rpc == 4013436649u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - DoDrink "));
				}
				using (TimeWarning.New("DoDrink"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4013436649u, "DoDrink", this, player))
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
							DoDrink(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in DoDrink");
					}
				}
				return true;
			}
			if (rpc == 2781345828u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SendFilling "));
				}
				using (TimeWarning.New("SendFilling"))
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
							SendFilling(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SendFilling");
					}
				}
				return true;
			}
			if (rpc == 3038767821u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ThrowContents "));
				}
				using (TimeWarning.New("ThrowContents"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg4 = rPCMessage;
							ThrowContents(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in ThrowContents");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(FillCheck, 1f, 1f);
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		if (IsDisabled())
		{
			StopFilling();
		}
		if (!hasLid)
		{
			DoThrow(base.transform.position, Vector3.zero);
			Item item = GetItem();
			if (item != null)
			{
				item.contents.SetLocked(IsDisabled());
				SendNetworkUpdateImmediate();
			}
		}
	}

	public void SetFilling(bool isFilling)
	{
		SetFlag(Flags.Open, isFilling);
		if (isFilling)
		{
			StartFilling();
		}
		else
		{
			StopFilling();
		}
		OnSetFilling(isFilling);
	}

	public virtual void OnSetFilling(bool flag)
	{
	}

	public void StartFilling()
	{
		float num = UnityEngine.Time.realtimeSinceStartup - lastFillTime;
		StopFilling();
		InvokeRepeating(FillCheck, 0f, 0.3f);
		if (num > 1f)
		{
			LiquidContainer facingLiquidContainer = GetFacingLiquidContainer();
			if (facingLiquidContainer != null && facingLiquidContainer.GetLiquidItem() != null)
			{
				if (fillFromContainer.isValid)
				{
					Effect.server.Run(fillFromContainer.resourcePath, facingLiquidContainer.transform.position, Vector3.up);
				}
				ClientRPC(null, "CLIENT_StartFillingSoundsContainer");
			}
			else if (CanFillFromWorld())
			{
				if (fillFromWorld.isValid)
				{
					Effect.server.Run(fillFromWorld.resourcePath, GetOwnerPlayer(), 0u, Vector3.zero, Vector3.up);
				}
				ClientRPC(null, "CLIENT_StartFillingSoundsWorld");
			}
		}
		lastFillTime = UnityEngine.Time.realtimeSinceStartup;
	}

	public void StopFilling()
	{
		ClientRPC(null, "CLIENT_StopFillingSounds");
		CancelInvoke(FillCheck);
	}

	public void FillCheck()
	{
		if (base.isClient)
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return;
		}
		float f = (UnityEngine.Time.realtimeSinceStartup - lastFillTime) * fillMlPerSec;
		Vector3 pos = ownerPlayer.transform.position - new Vector3(0f, 1f, 0f);
		LiquidContainer facingLiquidContainer = GetFacingLiquidContainer();
		if (Interface.CallHook("OnLiquidVesselFill", this, ownerPlayer, facingLiquidContainer) != null)
		{
			return;
		}
		if (facingLiquidContainer == null && CanFillFromWorld())
		{
			AddLiquid(WaterResource.GetAtPoint(pos), Mathf.FloorToInt(f));
		}
		else if (facingLiquidContainer != null && facingLiquidContainer.HasLiquidItem())
		{
			int num = Mathf.CeilToInt((1f - HeldFraction()) * (float)MaxHoldable());
			if (num > 0)
			{
				Item liquidItem = facingLiquidContainer.GetLiquidItem();
				int num2 = Mathf.Min(Mathf.CeilToInt(f), Mathf.Min(liquidItem.amount, num));
				AddLiquid(liquidItem.info, num2);
				liquidItem.UseItem(num2);
				facingLiquidContainer.OpenTap(2f);
			}
		}
		lastFillTime = UnityEngine.Time.realtimeSinceStartup;
	}

	public void LoseWater(int amount)
	{
		Item slot = GetItem().contents.GetSlot(0);
		if (slot != null)
		{
			slot.UseItem(amount);
			slot.MarkDirty();
			SendNetworkUpdateImmediate();
		}
	}

	public void AddLiquid(ItemDefinition liquidType, int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		Item item = GetItem();
		Item item2 = item.contents.GetSlot(0);
		ItemModContainer component = item.info.GetComponent<ItemModContainer>();
		if (item2 == null)
		{
			ItemManager.Create(liquidType, amount, 0uL)?.MoveToContainer(item.contents);
			return;
		}
		int num = Mathf.Clamp(item2.amount + amount, 0, component.maxStackSize);
		ItemDefinition itemDefinition = WaterResource.Merge(item2.info, liquidType);
		if (itemDefinition != item2.info)
		{
			item2.Remove();
			item2 = ItemManager.Create(itemDefinition, num, 0uL);
			item2.MoveToContainer(item.contents);
		}
		else
		{
			item2.amount = num;
		}
		item2.MarkDirty();
		SendNetworkUpdateImmediate();
	}

	public int AmountHeld()
	{
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return 0;
		}
		return item.contents.GetSlot(0)?.amount ?? 0;
	}

	public float HeldFraction()
	{
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return 0f;
		}
		return (float)AmountHeld() / (float)MaxHoldable();
	}

	public int MaxHoldable()
	{
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return 1;
		}
		return GetItem().info.GetComponent<ItemModContainer>().maxStackSize;
	}

	public bool CanDrink()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return false;
		}
		if (!ownerPlayer.metabolism.CanConsume())
		{
			return false;
		}
		if (!canDrinkFrom)
		{
			return false;
		}
		Item item = GetItem();
		if (item == null)
		{
			return false;
		}
		if (item.contents == null)
		{
			return false;
		}
		if (item.contents.itemList == null)
		{
			return false;
		}
		if (item.contents.itemList.Count == 0)
		{
			return false;
		}
		return true;
	}

	private bool IsWeaponBusy()
	{
		return UnityEngine.Time.realtimeSinceStartup < nextFreeTime;
	}

	private void SetBusyFor(float dur)
	{
		nextFreeTime = UnityEngine.Time.realtimeSinceStartup + dur;
	}

	private void ClearBusy()
	{
		nextFreeTime = UnityEngine.Time.realtimeSinceStartup - 1f;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void DoDrink(RPCMessage msg)
	{
		if (!msg.player.CanInteract())
		{
			return;
		}
		Item item = GetItem();
		if (item == null || item.contents == null || !msg.player.metabolism.CanConsume())
		{
			return;
		}
		foreach (Item item2 in item.contents.itemList)
		{
			ItemModConsume component = item2.info.GetComponent<ItemModConsume>();
			if (!(component == null) && component.CanDoAction(item2, msg.player))
			{
				component.DoAction(item2, msg.player);
				break;
			}
		}
	}

	[RPC_Server]
	private void ThrowContents(RPCMessage msg)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!(ownerPlayer == null))
		{
			DoThrow(ownerPlayer.eyes.position + ownerPlayer.eyes.BodyForward() * 1f, ownerPlayer.estimatedVelocity + ownerPlayer.eyes.BodyForward() * throwScale);
			Effect.server.Run(ThrowEffect3P.resourcePath, ownerPlayer.transform.position, ownerPlayer.eyes.BodyForward(), ownerPlayer.net.connection);
		}
	}

	public void DoThrow(Vector3 pos, Vector3 velocity)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return;
		}
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return;
		}
		Item slot = item.contents.GetSlot(0);
		if (slot != null && slot.amount > 0)
		{
			Vector3 vector = ownerPlayer.eyes.position + ownerPlayer.eyes.BodyForward() * 1f;
			WaterBall waterBall = GameManager.server.CreateEntity(thrownWaterObject.resourcePath, vector, Quaternion.identity) as WaterBall;
			if ((bool)waterBall)
			{
				waterBall.liquidType = slot.info;
				waterBall.waterAmount = slot.amount;
				waterBall.transform.position = vector;
				waterBall.SetVelocity(velocity);
				waterBall.Spawn();
			}
			slot.UseItem(slot.amount);
			slot.MarkDirty();
			SendNetworkUpdateImmediate();
		}
	}

	[RPC_Server]
	private void SendFilling(RPCMessage msg)
	{
		bool filling = msg.read.Bit();
		SetFilling(filling);
	}

	public bool CanFillFromWorld()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return false;
		}
		if (ownerPlayer.IsInWaterVolume(base.transform.position))
		{
			return false;
		}
		return ownerPlayer.WaterFactor() >= 0.05f;
	}

	public bool CanThrow()
	{
		return HeldFraction() > minThrowFrac;
	}

	public LiquidContainer GetFacingLiquidContainer()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return null;
		}
		if (UnityEngine.Physics.Raycast(ownerPlayer.eyes.HeadRay(), out var hitInfo, 2f, 1236478737))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if ((bool)entity && !hitInfo.collider.gameObject.CompareTag("Not Player Usable") && !hitInfo.collider.gameObject.CompareTag("Usable Primary"))
			{
				entity = entity.ToServer<BaseEntity>();
				return entity.GetComponent<LiquidContainer>();
			}
		}
		return null;
	}
}
