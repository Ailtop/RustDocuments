#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class LiquidWeapon : BaseLiquidVessel
{
	[Header("Liquid Weapon")]
	public float FireRate = 0.2f;

	public float MaxRange = 10f;

	public int FireAmountML = 100;

	public int MaxPressure = 100;

	public int PressureLossPerTick = 5;

	public int PressureGainedPerPump = 25;

	public float MinDmgRadius = 0.15f;

	public float MaxDmgRadius = 0.15f;

	public float SplashRadius = 2f;

	public GameObjectRef ImpactSplashEffect;

	public AnimationCurve PowerCurve;

	public List<DamageTypeEntry> Damage;

	public LiquidWeaponEffects EntityWeaponEffects;

	public bool RequiresPumping;

	public bool AutoPump;

	public bool WaitForFillAnim;

	public bool UseFalloffCurve;

	public AnimationCurve FalloffCurve;

	public float PumpingBlockDuration = 0.5f;

	public float StartFillingBlockDuration = 2f;

	public float StopFillingBlockDuration = 1f;

	private float cooldownTime;

	private int pressure;

	public const string RadiationFightAchievement = "SUMMER_RADICAL";

	public const string SoakedAchievement = "SUMMER_SOAKED";

	public const string LiquidatorAchievement = "SUMMER_LIQUIDATOR";

	public const string NoPressureAchievement = "SUMMER_NO_PRESSURE";

	public float PressureFraction => (float)pressure / (float)MaxPressure;

	public float MinimumPressureFraction => (float)PressureGainedPerPump / (float)MaxPressure;

	public float CurrentRange
	{
		get
		{
			if (!UseFalloffCurve)
			{
				return MaxRange;
			}
			return MaxRange * FalloffCurve.Evaluate((float)(MaxPressure - pressure) / (float)MaxPressure);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("LiquidWeapon.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 1600824953 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - PumpWater "));
				}
				using (TimeWarning.New("PumpWater"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1600824953u, "PumpWater", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							PumpWater(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in PumpWater");
					}
				}
				return true;
			}
			if (rpc == 3724096303u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - StartFiring "));
				}
				using (TimeWarning.New("StartFiring"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3724096303u, "StartFiring", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							StartFiring(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in StartFiring");
					}
				}
				return true;
			}
			if (rpc == 789289044 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - StopFiring "));
				}
				using (TimeWarning.New("StopFiring"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(789289044u, "StopFiring", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							StopFiring();
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in StopFiring");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void StartFiring(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (OnCooldown())
		{
			return;
		}
		if (!RequiresPumping)
		{
			pressure = MaxPressure;
		}
		if (CanFire(player))
		{
			CancelInvoke("FireTick");
			InvokeRepeating("FireTick", 0f, FireRate);
			SetFlag(Flags.On, true);
			StartCooldown(FireRate);
			if (base.isServer)
			{
				SendNetworkUpdateImmediate();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void StopFiring()
	{
		CancelInvoke("FireTick");
		if (!RequiresPumping)
		{
			pressure = MaxPressure;
		}
		SetFlag(Flags.On, false);
		if (base.isServer)
		{
			SendNetworkUpdateImmediate();
		}
	}

	private bool CanFire(BasePlayer player)
	{
		if (RequiresPumping && pressure < PressureLossPerTick)
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		if (HasFlag(Flags.Open))
		{
			return false;
		}
		if (AmountHeld() <= 0)
		{
			return false;
		}
		if (!player.CanInteract())
		{
			return false;
		}
		if (!player.CanAttack() || player.IsRunning())
		{
			return false;
		}
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return false;
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void PumpWater(RPCMessage msg)
	{
		PumpWater();
	}

	private void PumpWater()
	{
		if (!(GetOwnerPlayer() == null) && !OnCooldown() && !Firing())
		{
			pressure += PressureGainedPerPump;
			pressure = Mathf.Min(pressure, MaxPressure);
			StartCooldown(PumpingBlockDuration);
			GetOwnerPlayer().SignalBroadcast(Signal.Reload);
			SendNetworkUpdateImmediate();
		}
	}

	private void FireTick()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!CanFire(ownerPlayer))
		{
			StopFiring();
			return;
		}
		int num = Mathf.Min(FireAmountML, AmountHeld());
		if (num == 0)
		{
			StopFiring();
			return;
		}
		LoseWater(num);
		float currentRange = CurrentRange;
		pressure -= PressureLossPerTick;
		if (pressure <= 0)
		{
			StopFiring();
		}
		Ray ray = ownerPlayer.eyes.BodyRay();
		Debug.DrawLine(ray.origin, ray.origin + ray.direction * currentRange, Color.blue, 1f);
		RaycastHit hitInfo;
		if (UnityEngine.Physics.Raycast(ray, out hitInfo, currentRange, 1218652417))
		{
			DoSplash(ownerPlayer, hitInfo.point, ray.direction, num);
		}
		SendNetworkUpdate();
	}

	private void DoSplash(BasePlayer attacker, Vector3 position, Vector3 direction, int amount)
	{
		Item item = GetItem();
		if (item != null && item.contents != null)
		{
			Item slot = item.contents.GetSlot(0);
			if (slot != null && slot.amount > 0 && !(slot.info == null))
			{
				WaterBall.DoSplash(position, SplashRadius, slot.info, amount);
				DamageUtil.RadiusDamage(attacker, LookupPrefab(), position, MinDmgRadius, MaxDmgRadius, Damage, 131072, true);
			}
		}
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		StopFiring();
	}

	private void StartCooldown(float duration)
	{
		if (UnityEngine.Time.realtimeSinceStartup + duration > cooldownTime)
		{
			cooldownTime = UnityEngine.Time.realtimeSinceStartup + duration;
		}
	}

	private bool OnCooldown()
	{
		return UnityEngine.Time.realtimeSinceStartup < cooldownTime;
	}

	private bool Firing()
	{
		return HasFlag(Flags.On);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseProjectile = Facepunch.Pool.Get<ProtoBuf.BaseProjectile>();
		info.msg.baseProjectile.primaryMagazine = Facepunch.Pool.Get<Magazine>();
		info.msg.baseProjectile.primaryMagazine.contents = pressure;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseProjectile != null && info.msg.baseProjectile.primaryMagazine != null)
		{
			pressure = info.msg.baseProjectile.primaryMagazine.contents;
		}
	}
}
