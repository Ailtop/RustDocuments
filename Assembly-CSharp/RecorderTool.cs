#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class RecorderTool : ThrownWeapon, ICassettePlayer
{
	[ClientVar(Saved = true)]
	public static bool debugRecording;

	public AudioSource RecorderAudioSource;

	public SoundDefinition RecordStartSfx;

	public SoundDefinition RewindSfx;

	public SoundDefinition RecordFinishedSfx;

	public SoundDefinition PlayTapeSfx;

	public SoundDefinition StopTapeSfx;

	public float ThrowScale = 3f;

	public Cassette cachedCassette { get; set; }

	public Sprite LoadedCassetteIcon
	{
		get
		{
			if (!(cachedCassette != null))
			{
				return null;
			}
			return cachedCassette.HudSprite;
		}
	}

	public BaseEntity ToBaseEntity => this;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RecorderTool.OnRpcMessage"))
		{
			if (rpc == 3075830603u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_TogglePlaying "));
				}
				using (TimeWarning.New("Server_TogglePlaying"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3075830603u, "Server_TogglePlaying", this, player))
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
							Server_TogglePlaying(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_TogglePlaying");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool HasCassette()
	{
		return cachedCassette != null;
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	public void Server_TogglePlaying(RPCMessage msg)
	{
		bool b = msg.read.ReadByte() == 1;
		SetFlag(Flags.On, b);
	}

	public void OnCassetteInserted(Cassette c)
	{
		cachedCassette = c;
		ClientRPC(null, "Client_OnCassetteInserted", c.net.ID);
	}

	public void OnCassetteRemoved(Cassette c)
	{
		cachedCassette = null;
		ClientRPC(null, "Client_OnCassetteRemoved");
	}

	protected override void SetUpThrownWeapon(BaseEntity ent)
	{
		base.SetUpThrownWeapon(ent);
		if (GetOwnerPlayer() != null)
		{
			ent.OwnerID = GetOwnerPlayer().userID;
		}
		DeployedRecorder deployedRecorder;
		if (cachedCassette != null && (object)(deployedRecorder = ent as DeployedRecorder) != null)
		{
			GetItem().contents.itemList[0].SetParent(deployedRecorder.inventory);
		}
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		if (IsDisabled())
		{
			SetFlag(Flags.On, false);
		}
	}
}
