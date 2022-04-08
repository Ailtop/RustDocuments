#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class EasterBasket : AttackEntity
{
	public GameObjectRef eggProjectile;

	public ItemDefinition ammoType;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("EasterBasket.OnRpcMessage"))
		{
			if (rpc == 3763591455u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ThrowEgg "));
				}
				using (TimeWarning.New("ThrowEgg"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3763591455u, "ThrowEgg", this, player))
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
							ThrowEgg(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ThrowEgg");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override Vector3 GetInheritedVelocity(BasePlayer player)
	{
		return player.GetInheritedProjectileVelocity();
	}

	public Item GetAmmo()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!ownerPlayer)
		{
			return null;
		}
		Item item = ownerPlayer.inventory.containerMain.FindItemByItemID(ammoType.itemid);
		if (item == null)
		{
			item = ownerPlayer.inventory.containerBelt.FindItemByItemID(ammoType.itemid);
		}
		return item;
	}

	public bool HasAmmo()
	{
		return GetAmmo() != null;
	}

	public void UseAmmo()
	{
		GetAmmo()?.UseItem();
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	public void ThrowEgg(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else
		{
			if (!HasAmmo())
			{
				return;
			}
			UseAmmo();
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
			float num2 = 2f;
			if (num2 > 0f)
			{
				vector2 = AimConeUtil.GetModifiedAimConeDirection(num2, vector2);
			}
			float num3 = 1f;
			if (UnityEngine.Physics.Raycast(vector, vector2, out var hitInfo, num3, 1236478737))
			{
				num3 = hitInfo.distance - 0.1f;
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(eggProjectile.resourcePath, vector + vector2 * num3);
			if (!(baseEntity == null))
			{
				baseEntity.creatorEntity = player;
				ServerProjectile component = baseEntity.GetComponent<ServerProjectile>();
				if ((bool)component)
				{
					component.InitializeVelocity(GetInheritedVelocity(player) + vector2 * component.speed);
				}
				baseEntity.Spawn();
				GetOwnerItem()?.LoseCondition(UnityEngine.Random.Range(1f, 2f));
			}
		}
	}
}
