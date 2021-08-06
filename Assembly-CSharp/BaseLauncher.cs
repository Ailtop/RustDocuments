#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseLauncher : BaseProjectile
{
	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseLauncher.OnRpcMessage"))
		{
			if (rpc == 853319324 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SV_Launch "));
				}
				using (TimeWarning.New("SV_Launch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(853319324u, "SV_Launch", this, player))
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
							SV_Launch(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SV_Launch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool ForceSendMagazine()
	{
		return true;
	}

	public override void ServerUse()
	{
		ServerUse(1f);
	}

	public override void ServerUse(float damageModifier, Transform originOverride = null)
	{
		ItemModProjectile component = primaryMagazine.ammoType.GetComponent<ItemModProjectile>();
		if (!component)
		{
			return;
		}
		if (primaryMagazine.contents <= 0)
		{
			SignalBroadcast(Signal.DryFire);
			StartAttackCooldown(1f);
			return;
		}
		if (!component.projectileObject.Get().GetComponent<ServerProjectile>())
		{
			base.ServerUse(damageModifier, originOverride);
			return;
		}
		primaryMagazine.contents--;
		if (primaryMagazine.contents < 0)
		{
			primaryMagazine.contents = 0;
		}
		Vector3 vector = MuzzlePoint.transform.forward;
		Vector3 position = MuzzlePoint.transform.position;
		float num = GetAimCone() + component.projectileSpread;
		if (num > 0f)
		{
			vector = AimConeUtil.GetModifiedAimConeDirection(num, vector);
		}
		float num2 = 1f;
		RaycastHit hitInfo;
		if (UnityEngine.Physics.Raycast(position, vector, out hitInfo, num2, 1236478737))
		{
			num2 = hitInfo.distance - 0.1f;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(component.projectileObject.resourcePath, position + vector * num2);
		if (!(baseEntity == null))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			bool flag = ownerPlayer != null && ownerPlayer.IsNpc;
			ServerProjectile component2 = baseEntity.GetComponent<ServerProjectile>();
			if ((bool)component2)
			{
				component2.InitializeVelocity(vector * component2.speed);
			}
			baseEntity.SendMessage("SetDamageScale", flag ? npcDamageScale : turretDamageScale);
			baseEntity.Spawn();
			StartAttackCooldown(ScaleRepeatDelay(repeatDelay));
			SignalBroadcast(Signal.Attack, string.Empty);
			GetOwnerItem()?.LoseCondition(UnityEngine.Random.Range(1f, 2f));
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void SV_Launch(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
			return;
		}
		if (reloadFinished && HasReloadCooldown())
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Reloading (" + base.ShortPrefabName + ")");
			player.stats.combat.Log(this, "reload_cooldown");
			return;
		}
		reloadStarted = false;
		reloadFinished = false;
		if (primaryMagazine.contents <= 0)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Magazine empty (" + base.ShortPrefabName + ")");
			player.stats.combat.Log(this, "magazine_empty");
			return;
		}
		primaryMagazine.contents--;
		SignalBroadcast(Signal.Attack, string.Empty, player.net.connection);
		Vector3 vector = msg.read.Vector3();
		Vector3 vector2 = msg.read.Vector3().normalized;
		bool num = msg.read.Bit();
		BaseEntity mounted = player.GetParentEntity();
		if (mounted == null)
		{
			mounted = player.GetMounted();
		}
		if (num)
		{
			if (mounted != null)
			{
				vector = mounted.transform.TransformPoint(vector);
				vector2 = mounted.transform.TransformDirection(vector2);
			}
			else
			{
				vector = player.eyes.position;
				vector2 = player.eyes.BodyForward();
			}
		}
		if (!ValidateEyePos(player, vector))
		{
			return;
		}
		ItemModProjectile component = primaryMagazine.ammoType.GetComponent<ItemModProjectile>();
		if (!component)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
			player.stats.combat.Log(this, "mod_missing");
			return;
		}
		float num2 = GetAimCone() + component.projectileSpread;
		if (num2 > 0f)
		{
			vector2 = AimConeUtil.GetModifiedAimConeDirection(num2, vector2);
		}
		float num3 = 1f;
		RaycastHit hitInfo;
		if (UnityEngine.Physics.Raycast(vector, vector2, out hitInfo, num3, 1236478737))
		{
			num3 = hitInfo.distance - 0.1f;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(component.projectileObject.resourcePath, vector + vector2 * num3);
		if (!(baseEntity == null))
		{
			baseEntity.creatorEntity = player;
			ServerProjectile component2 = baseEntity.GetComponent<ServerProjectile>();
			if ((bool)component2)
			{
				component2.InitializeVelocity(GetInheritedVelocity(player) + vector2 * component2.speed);
			}
			baseEntity.Spawn();
			StartAttackCooldown(ScaleRepeatDelay(repeatDelay));
			Interface.CallHook("OnRocketLaunched", player, baseEntity);
			GetOwnerItem()?.LoseCondition(UnityEngine.Random.Range(1f, 2f));
		}
	}
}
