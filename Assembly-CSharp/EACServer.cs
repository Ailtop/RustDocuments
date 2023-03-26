using System;
using System.Collections.Concurrent;
using ConVar;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatCommon;
using Epic.OnlineServices.AntiCheatServer;
using Epic.OnlineServices.Reports;
using Network;
using Oxide.Core;
using UnityEngine;

public static class EACServer
{
	private static AntiCheatServerInterface Interface = null;

	private static ReportsInterface Reports = null;

	private static ConcurrentDictionary<uint, Connection> client2connection = new ConcurrentDictionary<uint, Connection>();

	private static ConcurrentDictionary<Connection, uint> connection2client = new ConcurrentDictionary<Connection, uint>();

	private static ConcurrentDictionary<Connection, AntiCheatCommonClientAuthStatus> connection2status = new ConcurrentDictionary<Connection, AntiCheatCommonClientAuthStatus>();

	private static uint clientHandleCounter = 0u;

	private static bool CanEnableGameplayData
	{
		get
		{
			if (ConVar.Server.official)
			{
				return ConVar.Server.stats;
			}
			return false;
		}
	}

	private static bool CanSendAnalytics
	{
		get
		{
			if (CanEnableGameplayData)
			{
				return Interface != null;
			}
			return false;
		}
	}

	private static bool CanSendReports => Reports != null;

	private static IntPtr GenerateCompatibilityClient()
	{
		return (IntPtr)(++clientHandleCounter);
	}

	public static void Encrypt(Connection connection, ArraySegment<byte> src, ref ArraySegment<byte> dst)
	{
		uint count = (uint)dst.Count;
		dst = new ArraySegment<byte>(dst.Array, dst.Offset, 0);
		if (!(Interface != null))
		{
			return;
		}
		IntPtr client = GetClient(connection);
		if (client != IntPtr.Zero)
		{
			ProtectMessageOptions protectMessageOptions = default(ProtectMessageOptions);
			protectMessageOptions.ClientHandle = client;
			protectMessageOptions.Data = src;
			protectMessageOptions.OutBufferSizeBytes = count;
			ProtectMessageOptions options = protectMessageOptions;
			uint outBytesWritten;
			Result result = Interface.ProtectMessage(ref options, dst, out outBytesWritten);
			if (result == Result.Success)
			{
				dst = new ArraySegment<byte>(dst.Array, dst.Offset, (int)outBytesWritten);
			}
			else
			{
				Debug.LogWarning("[EAC] ProtectMessage failed: " + result);
			}
		}
	}

	public static void Decrypt(Connection connection, ArraySegment<byte> src, ref ArraySegment<byte> dst)
	{
		uint count = (uint)dst.Count;
		dst = new ArraySegment<byte>(dst.Array, dst.Offset, 0);
		if (!(Interface != null))
		{
			return;
		}
		IntPtr client = GetClient(connection);
		if (client != IntPtr.Zero)
		{
			UnprotectMessageOptions unprotectMessageOptions = default(UnprotectMessageOptions);
			unprotectMessageOptions.ClientHandle = client;
			unprotectMessageOptions.Data = src;
			unprotectMessageOptions.OutBufferSizeBytes = count;
			UnprotectMessageOptions options = unprotectMessageOptions;
			uint outBytesWritten;
			Result result = Interface.UnprotectMessage(ref options, dst, out outBytesWritten);
			if (result == Result.Success)
			{
				dst = new ArraySegment<byte>(dst.Array, dst.Offset, (int)outBytesWritten);
			}
			else
			{
				Debug.LogWarning("[EAC] UnprotectMessage failed: " + result);
			}
		}
	}

	private static IntPtr GetClient(Connection connection)
	{
		connection2client.TryGetValue(connection, out var value);
		return (IntPtr)value;
	}

	private static Connection GetConnection(IntPtr client)
	{
		client2connection.TryGetValue((uint)(int)client, out var value);
		return value;
	}

	public static bool IsAuthenticated(Connection connection)
	{
		connection2status.TryGetValue(connection, out var value);
		return value == AntiCheatCommonClientAuthStatus.RemoteAuthComplete;
	}

	private static void OnAuthenticatedLocal(Connection connection)
	{
		if (connection.authStatus == string.Empty)
		{
			connection.authStatus = "ok";
		}
		connection2status[connection] = AntiCheatCommonClientAuthStatus.LocalAuthComplete;
	}

	private static void OnAuthenticatedRemote(Connection connection)
	{
		connection2status[connection] = AntiCheatCommonClientAuthStatus.RemoteAuthComplete;
	}

	private static void OnClientAuthStatusChanged(ref OnClientAuthStatusChangedCallbackInfo data)
	{
		using (TimeWarning.New("AntiCheatKickPlayer", 10))
		{
			IntPtr clientHandle = data.ClientHandle;
			Connection connection = GetConnection(clientHandle);
			if (connection == null)
			{
				Debug.LogError("[EAC] Status update for invalid client: " + clientHandle);
			}
			else if (data.ClientAuthStatus == AntiCheatCommonClientAuthStatus.LocalAuthComplete)
			{
				OnAuthenticatedLocal(connection);
				SetClientNetworkStateOptions setClientNetworkStateOptions = default(SetClientNetworkStateOptions);
				setClientNetworkStateOptions.ClientHandle = clientHandle;
				setClientNetworkStateOptions.IsNetworkActive = false;
				SetClientNetworkStateOptions options = setClientNetworkStateOptions;
				Interface.SetClientNetworkState(ref options);
			}
			else if (data.ClientAuthStatus == AntiCheatCommonClientAuthStatus.RemoteAuthComplete)
			{
				OnAuthenticatedRemote(connection);
			}
		}
	}

	private static void OnClientActionRequired(ref OnClientActionRequiredCallbackInfo data)
	{
		using (TimeWarning.New("OnClientActionRequired", 10))
		{
			IntPtr clientHandle = data.ClientHandle;
			Connection connection = GetConnection(clientHandle);
			if (connection == null)
			{
				Debug.LogError("[EAC] Status update for invalid client: " + clientHandle);
				return;
			}
			AntiCheatCommonClientAction clientAction = data.ClientAction;
			if (clientAction != AntiCheatCommonClientAction.RemovePlayer)
			{
				return;
			}
			Utf8String actionReasonDetailsString = data.ActionReasonDetailsString;
			Debug.Log($"[EAC] Kicking {connection.userid} / {connection.username} ({actionReasonDetailsString})");
			connection.authStatus = "eac";
			Network.Net.sv.Kick(connection, "EAC: " + actionReasonDetailsString);
			Oxide.Core.Interface.CallHook("OnPlayerKicked", connection, actionReasonDetailsString.ToString());
			if (data.ActionReasonCode == AntiCheatCommonClientActionReason.PermanentBanned || data.ActionReasonCode == AntiCheatCommonClientActionReason.TemporaryBanned)
			{
				connection.authStatus = "eacbanned";
				ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=#fff>SERVER</color> Kicking " + connection.username + " (banned by anticheat)");
				Oxide.Core.Interface.CallHook("OnPlayerBanned", connection, actionReasonDetailsString.ToString());
				if (data.ActionReasonCode == AntiCheatCommonClientActionReason.PermanentBanned)
				{
					Entity.DeleteBy(connection.userid);
				}
			}
			UnregisterClientOptions unregisterClientOptions = default(UnregisterClientOptions);
			unregisterClientOptions.ClientHandle = clientHandle;
			UnregisterClientOptions options = unregisterClientOptions;
			Interface.UnregisterClient(ref options);
			client2connection.TryRemove((uint)(int)clientHandle, out var _);
			connection2client.TryRemove(connection, out var _);
			connection2status.TryRemove(connection, out var _);
		}
	}

	private static void SendToClient(ref OnMessageToClientCallbackInfo data)
	{
		IntPtr clientHandle = data.ClientHandle;
		Connection connection = GetConnection(clientHandle);
		if (connection == null)
		{
			Debug.LogError("[EAC] Network packet for invalid client: " + clientHandle);
			return;
		}
		NetWrite netWrite = Network.Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.EAC);
		netWrite.UInt32((uint)data.MessageData.Count);
		netWrite.Write(data.MessageData.Array, data.MessageData.Offset, data.MessageData.Count);
		netWrite.Send(new SendInfo(connection));
	}

	public static void DoStartup()
	{
		if (ConVar.Server.secure && !Application.isEditor)
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
			AddNotifyClientActionRequiredOptions options = default(AddNotifyClientActionRequiredOptions);
			AddNotifyClientAuthStatusChangedOptions options2 = default(AddNotifyClientAuthStatusChangedOptions);
			AddNotifyMessageToClientOptions options3 = default(AddNotifyMessageToClientOptions);
			BeginSessionOptions beginSessionOptions = default(BeginSessionOptions);
			beginSessionOptions.LocalUserId = null;
			beginSessionOptions.EnableGameplayData = CanEnableGameplayData;
			beginSessionOptions.RegisterTimeoutSeconds = 20u;
			beginSessionOptions.ServerName = ConVar.Server.hostname;
			BeginSessionOptions options4 = beginSessionOptions;
			LogGameRoundStartOptions logGameRoundStartOptions = default(LogGameRoundStartOptions);
			logGameRoundStartOptions.LevelName = World.Name;
			LogGameRoundStartOptions options5 = logGameRoundStartOptions;
			EOS.Initialize(isServer: true, ConVar.Server.anticheatid, ConVar.Server.anticheatkey, ConVar.Server.rootFolder + "/Log.EAC.txt");
			Interface = EOS.Interface.GetAntiCheatServerInterface();
			Interface.AddNotifyClientActionRequired(ref options, null, OnClientActionRequired);
			Interface.AddNotifyClientAuthStatusChanged(ref options2, null, OnClientAuthStatusChanged);
			Interface.AddNotifyMessageToClient(ref options3, null, SendToClient);
			Interface.BeginSession(ref options4);
			Interface.LogGameRoundStart(ref options5);
		}
		else
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
		}
	}

	public static void DoUpdate()
	{
		if (ConVar.Server.secure && !Application.isEditor)
		{
			EOS.Tick();
		}
	}

	public static void DoShutdown()
	{
		if (ConVar.Server.secure && !Application.isEditor)
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
			if (Interface != null)
			{
				Debug.Log("EasyAntiCheat Server Shutting Down");
				EndSessionOptions options = default(EndSessionOptions);
				Interface.EndSession(ref options);
				Interface = null;
				EOS.Shutdown();
			}
		}
		else
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
		}
	}

	public static void OnLeaveGame(Connection connection)
	{
		AntiCheatCommonClientAuthStatus value3;
		if (ConVar.Server.secure && !Application.isEditor)
		{
			if (Interface != null)
			{
				IntPtr client = GetClient(connection);
				if (client != IntPtr.Zero)
				{
					UnregisterClientOptions unregisterClientOptions = default(UnregisterClientOptions);
					unregisterClientOptions.ClientHandle = client;
					UnregisterClientOptions options = unregisterClientOptions;
					Interface.UnregisterClient(ref options);
					client2connection.TryRemove((uint)(int)client, out var _);
				}
				connection2client.TryRemove(connection, out var _);
				connection2status.TryRemove(connection, out value3);
			}
		}
		else
		{
			connection2status.TryRemove(connection, out value3);
		}
	}

	public static void OnJoinGame(Connection connection)
	{
		if (ConVar.Server.secure && !Application.isEditor)
		{
			if (Interface != null)
			{
				IntPtr intPtr = GenerateCompatibilityClient();
				if (intPtr == IntPtr.Zero)
				{
					Debug.LogError("[EAC] GenerateCompatibilityClient returned invalid client: " + intPtr);
					return;
				}
				RegisterClientOptions registerClientOptions = default(RegisterClientOptions);
				registerClientOptions.ClientHandle = intPtr;
				registerClientOptions.AccountId = connection.userid.ToString();
				registerClientOptions.IpAddress = connection.IPAddressWithoutPort();
				registerClientOptions.ClientType = ((connection.authLevel >= 3 && connection.os == "editor") ? AntiCheatCommonClientType.UnprotectedClient : AntiCheatCommonClientType.ProtectedClient);
				registerClientOptions.ClientPlatform = ((connection.os == "windows") ? AntiCheatCommonClientPlatform.Windows : ((connection.os == "linux") ? AntiCheatCommonClientPlatform.Linux : ((connection.os == "mac") ? AntiCheatCommonClientPlatform.Mac : AntiCheatCommonClientPlatform.Unknown)));
				RegisterClientOptions options = registerClientOptions;
				SetClientDetailsOptions setClientDetailsOptions = default(SetClientDetailsOptions);
				setClientDetailsOptions.ClientHandle = intPtr;
				setClientDetailsOptions.ClientFlags = ((connection.authLevel != 0) ? AntiCheatCommonClientFlags.Admin : AntiCheatCommonClientFlags.None);
				SetClientDetailsOptions options2 = setClientDetailsOptions;
				Interface.RegisterClient(ref options);
				Interface.SetClientDetails(ref options2);
				client2connection.TryAdd((uint)(int)intPtr, connection);
				connection2client.TryAdd(connection, (uint)(int)intPtr);
				connection2status.TryAdd(connection, AntiCheatCommonClientAuthStatus.Invalid);
			}
		}
		else
		{
			connection2status.TryAdd(connection, AntiCheatCommonClientAuthStatus.Invalid);
			OnAuthenticatedLocal(connection);
			OnAuthenticatedRemote(connection);
		}
	}

	public static void OnStartLoading(Connection connection)
	{
		if (Interface != null)
		{
			IntPtr client = GetClient(connection);
			if (client != IntPtr.Zero)
			{
				SetClientNetworkStateOptions setClientNetworkStateOptions = default(SetClientNetworkStateOptions);
				setClientNetworkStateOptions.ClientHandle = client;
				setClientNetworkStateOptions.IsNetworkActive = false;
				SetClientNetworkStateOptions options = setClientNetworkStateOptions;
				Interface.SetClientNetworkState(ref options);
			}
		}
	}

	public static void OnFinishLoading(Connection connection)
	{
		if (Interface != null)
		{
			IntPtr client = GetClient(connection);
			if (client != IntPtr.Zero)
			{
				SetClientNetworkStateOptions setClientNetworkStateOptions = default(SetClientNetworkStateOptions);
				setClientNetworkStateOptions.ClientHandle = client;
				setClientNetworkStateOptions.IsNetworkActive = true;
				SetClientNetworkStateOptions options = setClientNetworkStateOptions;
				Interface.SetClientNetworkState(ref options);
			}
		}
	}

	public static void OnMessageReceived(Message message)
	{
		IntPtr client = GetClient(message.connection);
		byte[] buffer;
		int size;
		if (client == IntPtr.Zero)
		{
			Debug.LogError("EAC network packet from invalid connection: " + message.connection.userid);
		}
		else if (message.read.TemporaryBytesWithSize(out buffer, out size))
		{
			ReceiveMessageFromClientOptions receiveMessageFromClientOptions = default(ReceiveMessageFromClientOptions);
			receiveMessageFromClientOptions.ClientHandle = client;
			receiveMessageFromClientOptions.Data = new ArraySegment<byte>(buffer, 0, size);
			ReceiveMessageFromClientOptions options = receiveMessageFromClientOptions;
			Interface.ReceiveMessageFromClient(ref options);
		}
	}

	public static void LogPlayerUseWeapon(BasePlayer player, BaseProjectile weapon)
	{
		if (CanSendAnalytics && player.net.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerShooting"))
			{
				Vector3 networkPosition = player.GetNetworkPosition();
				Quaternion networkRotation = player.GetNetworkRotation();
				Item item = weapon.GetItem();
				string text = ((item != null) ? item.info.shortname : "unknown");
				LogPlayerUseWeaponOptions options = default(LogPlayerUseWeaponOptions);
				LogPlayerUseWeaponData value = default(LogPlayerUseWeaponData);
				value.PlayerHandle = GetClient(player.net.connection);
				value.PlayerPosition = new Vec3f
				{
					x = networkPosition.x,
					y = networkPosition.y,
					z = networkPosition.z
				};
				value.PlayerViewRotation = new Quat
				{
					w = networkRotation.w,
					x = networkRotation.x,
					y = networkRotation.y,
					z = networkRotation.z
				};
				value.WeaponName = text;
				options.UseWeaponData = value;
				Interface.LogPlayerUseWeapon(ref options);
			}
		}
	}

	public static void LogPlayerSpawn(BasePlayer player)
	{
		if (CanSendAnalytics && player.net.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerSpawn"))
			{
				LogPlayerSpawnOptions options = default(LogPlayerSpawnOptions);
				options.SpawnedPlayerHandle = GetClient(player.net.connection);
				Interface.LogPlayerSpawn(ref options);
			}
		}
	}

	public static void LogPlayerDespawn(BasePlayer player)
	{
		if (CanSendAnalytics && player.net.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerDespawn"))
			{
				LogPlayerDespawnOptions options = default(LogPlayerDespawnOptions);
				options.DespawnedPlayerHandle = GetClient(player.net.connection);
				Interface.LogPlayerDespawn(ref options);
			}
		}
	}

	public static void LogPlayerTakeDamage(BasePlayer player, HitInfo info)
	{
		if (!CanSendAnalytics || !(info.Initiator != null) || !(info.Initiator is BasePlayer))
		{
			return;
		}
		BasePlayer basePlayer = info.Initiator.ToPlayer();
		if (player.net.connection == null || basePlayer.net.connection == null)
		{
			return;
		}
		using (TimeWarning.New("EAC.LogPlayerTakeDamage"))
		{
			LogPlayerTakeDamageOptions options = default(LogPlayerTakeDamageOptions);
			LogPlayerUseWeaponData value = default(LogPlayerUseWeaponData);
			options.AttackerPlayerHandle = GetClient(basePlayer.net.connection);
			options.VictimPlayerHandle = GetClient(player.net.connection);
			options.DamageTaken = info.damageTypes.Total();
			options.DamagePosition = new Vec3f
			{
				x = info.HitPositionWorld.x,
				y = info.HitPositionWorld.y,
				z = info.HitPositionWorld.z
			};
			options.IsCriticalHit = info.isHeadshot;
			if (player.IsDead())
			{
				options.DamageResult = AntiCheatCommonPlayerTakeDamageResult.Eliminated;
			}
			else if (player.IsWounded())
			{
				options.DamageResult = AntiCheatCommonPlayerTakeDamageResult.Downed;
			}
			if (info.Weapon != null)
			{
				Item item = info.Weapon.GetItem();
				if (item != null)
				{
					value.WeaponName = item.info.shortname;
				}
				else
				{
					value.WeaponName = "unknown";
				}
			}
			else
			{
				value.WeaponName = "unknown";
			}
			Vector3 position = basePlayer.eyes.position;
			Quaternion rotation = basePlayer.eyes.rotation;
			Vector3 position2 = player.eyes.position;
			Quaternion rotation2 = player.eyes.rotation;
			options.AttackerPlayerPosition = new Vec3f
			{
				x = position.x,
				y = position.y,
				z = position.z
			};
			options.AttackerPlayerViewRotation = new Quat
			{
				w = rotation.w,
				x = rotation.x,
				y = rotation.y,
				z = rotation.z
			};
			options.VictimPlayerPosition = new Vec3f
			{
				x = position2.x,
				y = position2.y,
				z = position2.z
			};
			options.VictimPlayerViewRotation = new Quat
			{
				w = rotation2.w,
				x = rotation2.x,
				y = rotation2.y,
				z = rotation2.z
			};
			options.PlayerUseWeaponData = value;
			Interface.LogPlayerTakeDamage(ref options);
		}
	}

	public static void LogPlayerTick(BasePlayer player)
	{
		if (!CanSendAnalytics || player.net == null || player.net.connection == null)
		{
			return;
		}
		using (TimeWarning.New("EAC.LogPlayerTick"))
		{
			Vector3 position = player.eyes.position;
			Quaternion rotation = player.eyes.rotation;
			LogPlayerTickOptions options = default(LogPlayerTickOptions);
			options.PlayerHandle = GetClient(player.net.connection);
			options.PlayerPosition = new Vec3f
			{
				x = position.x,
				y = position.y,
				z = position.z
			};
			options.PlayerViewRotation = new Quat
			{
				w = rotation.w,
				x = rotation.x,
				y = rotation.y,
				z = rotation.z
			};
			options.PlayerHealth = player.Health();
			if (player.IsDucked())
			{
				options.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Crouching;
			}
			if (player.isMounted)
			{
				options.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Mounted;
			}
			if (player.IsCrawling())
			{
				options.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Prone;
			}
			if (player.IsSwimming())
			{
				options.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Swimming;
			}
			if (!player.IsOnGround())
			{
				options.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Falling;
			}
			if (player.OnLadder())
			{
				options.PlayerMovementState |= AntiCheatCommonPlayerMovementState.OnLadder;
			}
			if (player.IsFlying)
			{
				options.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Flying;
			}
			Interface.LogPlayerTick(ref options);
		}
	}

	public static void LogPlayerRevive(BasePlayer source, BasePlayer target)
	{
		if (CanSendAnalytics && target.net.connection != null && source != null && source.net.connection != null)
		{
			using (TimeWarning.New("EAC.LogPlayerRevive"))
			{
				LogPlayerReviveOptions options = default(LogPlayerReviveOptions);
				options.RevivedPlayerHandle = GetClient(target.net.connection);
				options.ReviverPlayerHandle = GetClient(source.net.connection);
				Interface.LogPlayerRevive(ref options);
			}
		}
	}

	public static void SendPlayerBehaviorReport(BasePlayer reporter, PlayerReportsCategory reportCategory, string reportedID, string reportText)
	{
		if (CanSendReports)
		{
			SendPlayerBehaviorReportOptions sendPlayerBehaviorReportOptions = default(SendPlayerBehaviorReportOptions);
			sendPlayerBehaviorReportOptions.ReportedUserId = ProductUserId.FromString(reportedID);
			sendPlayerBehaviorReportOptions.ReporterUserId = ProductUserId.FromString(reporter.UserIDString);
			sendPlayerBehaviorReportOptions.Category = reportCategory;
			sendPlayerBehaviorReportOptions.Message = reportText;
			SendPlayerBehaviorReportOptions options = sendPlayerBehaviorReportOptions;
			Reports.SendPlayerBehaviorReport(ref options, null, null);
		}
	}

	public static void SendPlayerBehaviorReport(PlayerReportsCategory reportCategory, string reportedID, string reportText)
	{
		if (CanSendReports)
		{
			SendPlayerBehaviorReportOptions sendPlayerBehaviorReportOptions = default(SendPlayerBehaviorReportOptions);
			sendPlayerBehaviorReportOptions.ReportedUserId = ProductUserId.FromString(reportedID);
			sendPlayerBehaviorReportOptions.Category = reportCategory;
			sendPlayerBehaviorReportOptions.Message = reportText;
			SendPlayerBehaviorReportOptions options = sendPlayerBehaviorReportOptions;
			Reports.SendPlayerBehaviorReport(ref options, null, null);
		}
	}
}
