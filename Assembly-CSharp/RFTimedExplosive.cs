#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class RFTimedExplosive : TimedExplosive, IRFObject
{
	public SoundPlayer beepLoop;

	private ulong creatorPlayerID;

	public ItemDefinition pickupDefinition;

	public float minutesUntilDecayed = 1440f;

	private int RFFrequency = -1;

	private float decayTickDuration = 3600f;

	private float minutesDecayed;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RFTimedExplosive.OnRpcMessage"))
		{
			if (rpc == 2778075470u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Pickup "));
				}
				using (TimeWarning.New("Pickup"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2778075470u, "Pickup", this, player, 3f))
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
							Pickup(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Pickup");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public float GetMaxRange()
	{
		return float.PositiveInfinity;
	}

	public void RFSignalUpdate(bool on)
	{
		if (IsArmed() && on && !IsInvoking(Explode))
		{
			Invoke(Explode, UnityEngine.Random.Range(0f, 0.2f));
		}
	}

	public void SetFrequency(int newFreq)
	{
		RFManager.RemoveListener(RFFrequency, this);
		RFFrequency = newFreq;
		if (RFFrequency > 0)
		{
			RFManager.AddListener(RFFrequency, this);
		}
	}

	public int GetFrequency()
	{
		return RFFrequency;
	}

	public override void SetFuse(float fuseLength)
	{
		if (!base.isServer)
		{
			return;
		}
		if (GetFrequency() > 0)
		{
			if (IsInvoking(Explode))
			{
				CancelInvoke(Explode);
			}
			Invoke(ArmRF, fuseLength);
			SetFlag(Flags.Reserved1, b: true, recursive: false, networkupdate: false);
			SetFlag(Flags.Reserved2, b: true);
		}
		else
		{
			base.SetFuse(fuseLength);
		}
	}

	public void ArmRF()
	{
		SetFlag(Flags.On, b: true, recursive: false, networkupdate: false);
		SetFlag(Flags.Reserved2, b: false);
		SendNetworkUpdate();
	}

	public void DisarmRF()
	{
		SetFlag(Flags.On, b: false);
		SendNetworkUpdate();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.explosive == null)
		{
			info.msg.explosive = Facepunch.Pool.Get<ProtoBuf.TimedExplosive>();
		}
		if (info.forDisk)
		{
			info.msg.explosive.freq = GetFrequency();
		}
		info.msg.explosive.creatorID = creatorPlayerID;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetFrequency(RFFrequency);
		InvokeRandomized(DecayCheck, decayTickDuration, decayTickDuration, 10f);
	}

	public void DecayCheck()
	{
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
		BasePlayer basePlayer = BasePlayer.FindByID(creatorPlayerID);
		if (basePlayer != null && (buildingPrivilege == null || !buildingPrivilege.IsAuthed(basePlayer)))
		{
			minutesDecayed += decayTickDuration / 60f;
		}
		if (minutesDecayed >= minutesUntilDecayed)
		{
			Kill();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (RFFrequency > 0)
		{
			if (IsInvoking(Explode))
			{
				CancelInvoke(Explode);
			}
			SetFrequency(RFFrequency);
			ArmRF();
		}
	}

	internal override void DoServerDestroy()
	{
		if (RFFrequency > 0)
		{
			RFManager.RemoveListener(RFFrequency, this);
		}
		base.DoServerDestroy();
	}

	public void ChangeFrequency(int newFreq)
	{
		RFManager.ChangeFrequency(RFFrequency, newFreq, this, isListener: true);
		RFFrequency = newFreq;
	}

	public override void SetCreatorEntity(BaseEntity newCreatorEntity)
	{
		base.SetCreatorEntity(newCreatorEntity);
		BasePlayer component = newCreatorEntity.GetComponent<BasePlayer>();
		if ((bool)component)
		{
			creatorPlayerID = component.userID;
			if (GetFrequency() > 0)
			{
				component.ConsoleMessage("Frequency is:" + GetFrequency());
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void Pickup(RPCMessage msg)
	{
		if (msg.player.CanInteract() && IsArmed())
		{
			Item item = ItemManager.Create(pickupDefinition, 1, 0uL);
			if (item != null)
			{
				item.instanceData.dataInt = GetFrequency();
				item.SetFlag(Item.Flag.IsOn, IsArmed());
				msg.player.GiveItem(item, GiveItemReason.PickedUp);
				Kill();
			}
		}
	}

	public bool IsArmed()
	{
		return HasFlag(Flags.On);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.explosive == null)
		{
			return;
		}
		creatorPlayerID = info.msg.explosive.creatorID;
		if (base.isServer)
		{
			if (info.fromDisk)
			{
				RFFrequency = info.msg.explosive.freq;
			}
			creatorEntity = BasePlayer.FindByID(creatorPlayerID);
		}
	}

	public bool CanPickup(BasePlayer player)
	{
		return IsArmed();
	}
}
