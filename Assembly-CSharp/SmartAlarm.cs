#define UNITY_ASSERTIONS
using System;
using CompanionServer;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SmartAlarm : AppIOEntity, ISubscribable
{
	public const Flags Flag_HasCustomMessage = Flags.Reserved6;

	public static readonly Translate.Phrase DefaultNotificationTitle = new Translate.Phrase("app.alarm.title", "Alarm");

	public static readonly Translate.Phrase DefaultNotificationBody = new Translate.Phrase("app.alarm.body", "Your base is under attack!");

	[Header("Smart Alarm")]
	public GameObjectRef SetupNotificationDialog;

	public Animator Animator;

	public readonly NotificationList _subscriptions = new NotificationList();

	public string _notificationTitle = "";

	public string _notificationBody = "";

	public float _lastSentTime;

	public override AppEntityType Type => AppEntityType.Alarm;

	public override bool Value { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SmartAlarm.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 3292290572u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SetNotificationTextImpl "));
				}
				using (TimeWarning.New("SetNotificationTextImpl"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3292290572u, "SetNotificationTextImpl", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3292290572u, "SetNotificationTextImpl", this, player, 3f))
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
							RPCMessage notificationTextImpl = rPCMessage;
							SetNotificationTextImpl(notificationTextImpl);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetNotificationTextImpl");
					}
				}
				return true;
			}
			if (rpc == 4207149767u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - StartSetupNotification "));
				}
				using (TimeWarning.New("StartSetupNotification"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4207149767u, "StartSetupNotification", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4207149767u, "StartSetupNotification", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							StartSetupNotification(rpc2);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in StartSetupNotification");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool AddSubscription(ulong steamId)
	{
		return _subscriptions.AddSubscription(steamId);
	}

	public bool RemoveSubscription(ulong steamId)
	{
		return _subscriptions.RemoveSubscription(steamId);
	}

	public bool HasSubscription(ulong steamId)
	{
		return _subscriptions.HasSubscription(steamId);
	}

	public override void InitShared()
	{
		base.InitShared();
		_notificationTitle = DefaultNotificationTitle.translated;
		_notificationBody = DefaultNotificationBody.translated;
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		Value = inputAmount > 0;
		if (Value == IsOn())
		{
			return;
		}
		SetFlag(Flags.On, Value);
		BroadcastValueChange();
		float num = Mathf.Max(App.alarmcooldown, 15f);
		if (Value && UnityEngine.Time.realtimeSinceStartup - _lastSentTime >= num)
		{
			BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
			if (buildingPrivilege != null)
			{
				_subscriptions.IntersectWith(buildingPrivilege.authorizedPlayers);
			}
			_subscriptions.SendNotification(NotificationChannel.SmartAlarm, _notificationTitle, _notificationBody, "alarm");
			_lastSentTime = UnityEngine.Time.realtimeSinceStartup;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.smartAlarm = Facepunch.Pool.Get<ProtoBuf.SmartAlarm>();
			info.msg.smartAlarm.notificationTitle = _notificationTitle;
			info.msg.smartAlarm.notificationBody = _notificationBody;
			info.msg.smartAlarm.subscriptions = _subscriptions.ToList();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.smartAlarm != null)
		{
			_notificationTitle = info.msg.smartAlarm.notificationTitle;
			_notificationBody = info.msg.smartAlarm.notificationBody;
			_subscriptions.LoadFrom(info.msg.smartAlarm.subscriptions);
		}
	}

	protected override void OnPairedWithPlayer(BasePlayer player)
	{
		if (!(player == null) && !AddSubscription(player.userID))
		{
			player.ClientRPCPlayer(null, player, "HandleCompanionPairingResult", 7);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void StartSetupNotification(RPCMessage rpc)
	{
		if (rpc.player.CanInteract())
		{
			BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
			if (!(buildingPrivilege != null) || buildingPrivilege.CanAdministrate(rpc.player))
			{
				ClientRPCPlayer(null, rpc.player, "SetupNotification", _notificationTitle, _notificationBody);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void SetNotificationTextImpl(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract())
		{
			return;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
		if (!(buildingPrivilege != null) || buildingPrivilege.CanAdministrate(rpc.player))
		{
			string text = rpc.read.String(128);
			string text2 = rpc.read.String(512);
			if (!string.IsNullOrWhiteSpace(text))
			{
				_notificationTitle = text;
			}
			if (!string.IsNullOrWhiteSpace(text2))
			{
				_notificationBody = text2;
			}
			SetFlag(Flags.Reserved6, true);
		}
	}
}
