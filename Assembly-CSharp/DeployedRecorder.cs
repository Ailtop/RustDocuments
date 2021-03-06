#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class DeployedRecorder : StorageContainer, ICassettePlayer
{
	public AudioSource SoundSource;

	public ItemDefinition[] ValidCassettes;

	public SoundDefinition PlaySfx;

	public SoundDefinition StopSfx;

	public SwapKeycard TapeSwapper;

	private CollisionDetectionMode? initialCollisionDetectionMode;

	public BaseEntity ToBaseEntity => this;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DeployedRecorder.OnRpcMessage"))
		{
			if (rpc == 1785864031 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ServerTogglePlay "));
				}
				using (TimeWarning.New("ServerTogglePlay"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1785864031u, "ServerTogglePlay", this, player, 3f))
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
							ServerTogglePlay(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ServerTogglePlay");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ServerTogglePlay(RPCMessage msg)
	{
		bool play = msg.read.ReadByte() == 1;
		ServerTogglePlay(play);
	}

	private void ServerTogglePlay(bool play)
	{
		SetFlag(Flags.On, play);
	}

	public void OnCassetteInserted(Cassette c)
	{
		ClientRPC(null, "Client_OnCassetteInserted", c.net.ID);
		SendNetworkUpdate();
	}

	public void OnCassetteRemoved(Cassette c)
	{
		ClientRPC(null, "Client_OnCassetteRemoved");
		ServerTogglePlay(false);
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		ItemDefinition[] validCassettes = ValidCassettes;
		for (int i = 0; i < validCassettes.Length; i++)
		{
			if (validCassettes[i] == item.info)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (base.isServer)
		{
			DoCollisionStick(collision, hitEntity);
		}
	}

	private void DoCollisionStick(Collision collision, BaseEntity ent)
	{
		ContactPoint contact = collision.GetContact(0);
		DoStick(contact.point, contact.normal, ent);
	}

	public virtual void SetMotionEnabled(bool wantsMotion)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			if (!initialCollisionDetectionMode.HasValue)
			{
				initialCollisionDetectionMode = component.collisionDetectionMode;
			}
			component.useGravity = wantsMotion;
			if (!wantsMotion)
			{
				component.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
			component.isKinematic = !wantsMotion;
			if (wantsMotion)
			{
				component.collisionDetectionMode = initialCollisionDetectionMode.Value;
			}
		}
	}

	public void DoStick(Vector3 position, Vector3 normal, BaseEntity ent)
	{
		if (ent != null && ent is TimedExplosive)
		{
			if (!ent.HasParent())
			{
				return;
			}
			position = ent.transform.position;
			ent = ent.parentEntity.Get(true);
		}
		SetMotionEnabled(false);
		SetCollisionEnabled(false);
		if (!(ent != null) || !HasChild(ent))
		{
			base.transform.position = position;
			base.transform.rotation = Quaternion.LookRotation(normal, base.transform.up);
			if (ent != null)
			{
				SetParent(ent, StringPool.closest, true);
			}
			ReceiveCollisionMessages(false);
		}
	}

	private void UnStick()
	{
		if ((bool)GetParentEntity())
		{
			SetParent(null, true, true);
			SetMotionEnabled(true);
			SetCollisionEnabled(true);
			ReceiveCollisionMessages(true);
		}
	}

	internal override void OnParentRemoved()
	{
		UnStick();
	}

	public virtual void SetCollisionEnabled(bool wantsCollision)
	{
		Collider component = GetComponent<Collider>();
		if ((bool)component && component.enabled != wantsCollision)
		{
			component.enabled = wantsCollision;
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		if (base.isServer)
		{
			initialCollisionDetectionMode = null;
		}
	}
}
