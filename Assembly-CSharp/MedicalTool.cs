#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class MedicalTool : AttackEntity
{
	public float healDurationSelf = 4f;

	public float healDurationOther = 4f;

	public float healDurationOtherWounded = 7f;

	public float maxDistanceOther = 2f;

	public bool canUseOnOther = true;

	public bool canRevive = true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MedicalTool.OnRpcMessage"))
		{
			if (rpc == 789049461 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - UseOther "));
				}
				using (TimeWarning.New("UseOther"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(789049461u, "UseOther", this, player))
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
							UseOther(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in UseOther");
					}
				}
				return true;
			}
			if (rpc == 2918424470u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - UseSelf "));
				}
				using (TimeWarning.New("UseSelf"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(2918424470u, "UseSelf", this, player))
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
							UseSelf(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in UseSelf");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void UseOther(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else if (player.CanInteract() && HasItemAmount() && canUseOnOther)
		{
			BasePlayer basePlayer = BaseNetworkable.serverEntities.Find(msg.read.UInt32()) as BasePlayer;
			if (basePlayer != null && Vector3.Distance(basePlayer.transform.position, player.transform.position) < 4f)
			{
				ClientRPCPlayer(null, player, "Reset");
				GiveEffectsTo(basePlayer);
				UseItemAmount(1);
				StartAttackCooldown(repeatDelay);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void UseSelf(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else if (player.CanInteract() && HasItemAmount())
		{
			ClientRPCPlayer(null, player, "Reset");
			GiveEffectsTo(player);
			UseItemAmount(1);
			StartAttackCooldown(repeatDelay);
		}
	}

	public override void ServerUse()
	{
		if (base.isClient)
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!(ownerPlayer == null) && ownerPlayer.CanInteract() && HasItemAmount())
		{
			GiveEffectsTo(ownerPlayer);
			UseItemAmount(1);
			StartAttackCooldown(repeatDelay);
			SignalBroadcast(Signal.Attack, string.Empty);
			if (ownerPlayer.IsNpc)
			{
				ownerPlayer.SignalBroadcast(Signal.Attack);
			}
		}
	}

	private void GiveEffectsTo(BasePlayer player)
	{
		if (!player)
		{
			return;
		}
		ItemModConsumable component = GetOwnerItemDefinition().GetComponent<ItemModConsumable>();
		if (!component)
		{
			Debug.LogWarning("No consumable for medicaltool :" + base.name);
		}
		else
		{
			if (Interface.CallHook("OnHealingItemUse", this, player) != null)
			{
				return;
			}
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (player != ownerPlayer && player.IsWounded() && canRevive)
			{
				if (Interface.CallHook("OnPlayerRevive", GetOwnerPlayer(), player) != null)
				{
					return;
				}
				player.StopWounded(ownerPlayer);
			}
			foreach (ItemModConsumable.ConsumableEffect effect in component.effects)
			{
				if (effect.type == MetabolismAttribute.Type.Health)
				{
					player.health += effect.amount;
				}
				else
				{
					player.metabolism.ApplyChange(effect.type, effect.amount, effect.time);
				}
			}
			if (player is BasePet)
			{
				player.SendNetworkUpdateImmediate();
			}
		}
	}
}
