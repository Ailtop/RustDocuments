#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Chainsaw : BaseMelee
{
	public float attackFadeInTime = 0.1f;

	public float attackFadeInDelay = 0.1f;

	public float attackFadeOutTime = 0.1f;

	public float idleFadeInTimeFromOff = 0.1f;

	public float idleFadeInTimeFromAttack = 0.3f;

	public float idleFadeInDelay = 0.1f;

	public float idleFadeOutTime = 0.1f;

	public Renderer chainRenderer;

	private MaterialPropertyBlock block;

	private Vector2 saveST;

	[Header("Chainsaw")]
	public float fuelPerSec = 1f;

	public int maxAmmo = 100;

	public int ammo = 100;

	public ItemDefinition fuelType;

	public float reloadDuration = 2.5f;

	[Header("Sounds")]
	public SoundPlayer idleLoop;

	public SoundPlayer attackLoopAir;

	public SoundPlayer revUp;

	public SoundPlayer revDown;

	public SoundPlayer offSound;

	private int failedAttempts;

	public float engineStartChance = 0.33f;

	private float ammoRemainder;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Chainsaw.OnRpcMessage"))
		{
			if (rpc == 3381353917u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - DoReload "));
				}
				using (TimeWarning.New("DoReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3381353917u, "DoReload", this, player))
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
							DoReload(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in DoReload");
					}
				}
				return true;
			}
			if (rpc == 706698034 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_SetAttacking "));
				}
				using (TimeWarning.New("Server_SetAttacking"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(706698034u, "Server_SetAttacking", this, player))
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
							Server_SetAttacking(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Server_SetAttacking");
					}
				}
				return true;
			}
			if (rpc == 3881794867u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_StartEngine "));
				}
				using (TimeWarning.New("Server_StartEngine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3881794867u, "Server_StartEngine", this, player))
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
							Server_StartEngine(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in Server_StartEngine");
					}
				}
				return true;
			}
			if (rpc == 841093980 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_StopEngine "));
				}
				using (TimeWarning.New("Server_StopEngine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(841093980u, "Server_StopEngine", this, player))
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
							Server_StopEngine(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in Server_StopEngine");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool EngineOn()
	{
		return HasFlag(Flags.On);
	}

	public bool IsAttacking()
	{
		return HasFlag(Flags.Busy);
	}

	public void ServerNPCStart()
	{
		if (!HasFlag(Flags.On))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (ownerPlayer != null && ownerPlayer.IsNpc)
			{
				DoReload(default(RPCMessage));
				SetEngineStatus(status: true);
				SendNetworkUpdateImmediate();
			}
		}
	}

	public override void ServerUse()
	{
		base.ServerUse();
		SetAttackStatus(status: true);
		Invoke(DelayedStopAttack, attackSpacing + 0.5f);
	}

	public override void ServerUse_OnHit(HitInfo info)
	{
		EnableHitEffect(info.HitMaterial);
	}

	private void DelayedStopAttack()
	{
		SetAttackStatus(status: false);
	}

	protected override bool VerifyClientAttack(BasePlayer player)
	{
		if (!EngineOn() || !IsAttacking())
		{
			return false;
		}
		return base.VerifyClientAttack(player);
	}

	public override void CollectedForCrafting(Item item, BasePlayer crafter)
	{
		ServerCommand(item, "unload_ammo", crafter);
	}

	public override void SetHeld(bool bHeld)
	{
		if (!bHeld)
		{
			SetEngineStatus(status: false);
		}
		base.SetHeld(bHeld);
	}

	public void ReduceAmmo(float firingTime)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer != null && ownerPlayer.IsNpc)
		{
			return;
		}
		ammoRemainder += firingTime;
		if (ammoRemainder >= 1f)
		{
			int num = Mathf.FloorToInt(ammoRemainder);
			ammoRemainder -= num;
			if (ammoRemainder >= 1f)
			{
				num++;
				ammoRemainder -= 1f;
			}
			ammo -= num;
			if (ammo <= 0)
			{
				ammo = 0;
			}
		}
		if ((float)ammo <= 0f)
		{
			SetEngineStatus(status: false);
		}
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void DoReload(RPCMessage msg)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!(ownerPlayer == null) && !IsAttacking())
		{
			Item item;
			while (ammo < maxAmmo && (item = GetAmmo()) != null && item.amount > 0)
			{
				int num = Mathf.Min(maxAmmo - ammo, item.amount);
				ammo += num;
				item.UseItem(num);
			}
			SendNetworkUpdateImmediate();
			ItemManager.DoRemoves();
			ownerPlayer.inventory.ServerUpdate(0f);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseProjectile = Facepunch.Pool.Get<ProtoBuf.BaseProjectile>();
		info.msg.baseProjectile.primaryMagazine = Facepunch.Pool.Get<Magazine>();
		info.msg.baseProjectile.primaryMagazine.contents = ammo;
	}

	public void SetEngineStatus(bool status)
	{
		SetFlag(Flags.On, status);
		if (!status)
		{
			SetAttackStatus(status: false);
		}
		CancelInvoke(EngineTick);
		if (status)
		{
			InvokeRepeating(EngineTick, 0f, 1f);
		}
	}

	public void SetAttackStatus(bool status)
	{
		if (!EngineOn())
		{
			status = false;
		}
		SetFlag(Flags.Busy, status);
		CancelInvoke(AttackTick);
		if (status)
		{
			InvokeRepeating(AttackTick, 0f, 1f);
		}
	}

	public void EngineTick()
	{
		ReduceAmmo(0.05f);
	}

	public void AttackTick()
	{
		ReduceAmmo(fuelPerSec);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void Server_StartEngine(RPCMessage msg)
	{
		if (ammo > 0 && !EngineOn())
		{
			ReduceAmmo(0.25f);
			bool num = UnityEngine.Random.Range(0f, 1f) <= engineStartChance;
			if (!num)
			{
				failedAttempts++;
			}
			if (num || failedAttempts >= 3)
			{
				failedAttempts = 0;
				SetEngineStatus(status: true);
				SendNetworkUpdateImmediate();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void Server_StopEngine(RPCMessage msg)
	{
		SetEngineStatus(status: false);
		SendNetworkUpdateImmediate();
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void Server_SetAttacking(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (IsAttacking() != flag && EngineOn())
		{
			SetAttackStatus(flag);
			SendNetworkUpdateImmediate();
		}
	}

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		if (item == null || !(command == "unload_ammo"))
		{
			return;
		}
		int num = ammo;
		if (num > 0)
		{
			ammo = 0;
			SendNetworkUpdateImmediate();
			Item item2 = ItemManager.Create(fuelType, num, 0uL);
			if (!item2.MoveToContainer(player.inventory.containerMain))
			{
				item2.Drop(player.GetDropPosition(), player.GetDropVelocity());
			}
		}
	}

	public void DisableHitEffects()
	{
		SetFlag(Flags.Reserved6, b: false);
		SetFlag(Flags.Reserved7, b: false);
		SetFlag(Flags.Reserved8, b: false);
		SendNetworkUpdateImmediate();
	}

	public void EnableHitEffect(uint hitMaterial)
	{
		SetFlag(Flags.Reserved6, b: false);
		SetFlag(Flags.Reserved7, b: false);
		SetFlag(Flags.Reserved8, b: false);
		if (hitMaterial == StringPool.Get("Flesh"))
		{
			SetFlag(Flags.Reserved8, b: true);
		}
		else if (hitMaterial == StringPool.Get("Wood"))
		{
			SetFlag(Flags.Reserved7, b: true);
		}
		else
		{
			SetFlag(Flags.Reserved6, b: true);
		}
		SendNetworkUpdateImmediate();
		CancelInvoke(DisableHitEffects);
		Invoke(DisableHitEffects, 0.5f);
	}

	public override void DoAttackShared(HitInfo info)
	{
		base.DoAttackShared(info);
		if (base.isServer)
		{
			EnableHitEffect(info.HitMaterial);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseProjectile != null && info.msg.baseProjectile.primaryMagazine != null)
		{
			ammo = info.msg.baseProjectile.primaryMagazine.contents;
		}
	}

	public bool HasAmmo()
	{
		return GetAmmo() != null;
	}

	public Item GetAmmo()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return null;
		}
		Item item = ownerPlayer.inventory.containerMain.FindItemsByItemName(fuelType.shortname);
		if (item == null)
		{
			item = ownerPlayer.inventory.containerBelt.FindItemsByItemName(fuelType.shortname);
		}
		return item;
	}
}
