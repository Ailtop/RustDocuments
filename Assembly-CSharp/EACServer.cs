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
	public static AntiCheatServerInterface Interface = null;

	public static ReportsInterface Reports = null;

	private static ConcurrentDictionary<uint, Connection> client2connection = new ConcurrentDictionary<uint, Connection>();

	private static ConcurrentDictionary<Connection, uint> connection2client = new ConcurrentDictionary<Connection, uint>();

	private static ConcurrentDictionary<Connection, AntiCheatCommonClientAuthStatus> connection2status = new ConcurrentDictionary<Connection, AntiCheatCommonClientAuthStatus>();

	private static uint clientHandleCounter = 0u;

	public static bool CanSendAnalytics
	{
		get
		{
			if (ConVar.Server.official && ConVar.Server.stats)
			{
				return Interface != null;
			}
			return false;
		}
	}

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

	public static IntPtr GetClient(Connection connection)
	{
		connection2client.TryGetValue(connection, out var value);
		return (IntPtr)value;
	}

	public static Connection GetConnection(IntPtr client)
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
			EOS.Initialize(isServer: true, ConVar.Server.anticheatid, ConVar.Server.anticheatkey, ConVar.Server.rootFolder + "/Log.EAC.txt");
			Interface = EOS.Interface.GetAntiCheatServerInterface();
			AddNotifyClientActionRequiredOptions options = default(AddNotifyClientActionRequiredOptions);
			Interface.AddNotifyClientActionRequired(ref options, null, OnClientActionRequired);
			AddNotifyClientAuthStatusChangedOptions options2 = default(AddNotifyClientAuthStatusChangedOptions);
			Interface.AddNotifyClientAuthStatusChanged(ref options2, null, OnClientAuthStatusChanged);
			AddNotifyMessageToClientOptions options3 = default(AddNotifyMessageToClientOptions);
			Interface.AddNotifyMessageToClient(ref options3, null, SendToClient);
			BeginSessionOptions beginSessionOptions = default(BeginSessionOptions);
			beginSessionOptions.LocalUserId = null;
			beginSessionOptions.EnableGameplayData = CanSendAnalytics;
			beginSessionOptions.RegisterTimeoutSeconds = 20u;
			beginSessionOptions.ServerName = ConVar.Server.hostname;
			BeginSessionOptions options4 = beginSessionOptions;
			Interface.BeginSession(ref options4);
			LogGameRoundStartOptions logGameRoundStartOptions = default(LogGameRoundStartOptions);
			logGameRoundStartOptions.LevelName = World.Name;
			LogGameRoundStartOptions options5 = logGameRoundStartOptions;
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
			}
			EOS.Shutdown();
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
				Interface.RegisterClient(ref options);
				SetClientDetailsOptions setClientDetailsOptions = default(SetClientDetailsOptions);
				setClientDetailsOptions.ClientHandle = intPtr;
				setClientDetailsOptions.ClientFlags = ((connection.authLevel != 0) ? AntiCheatCommonClientFlags.Admin : AntiCheatCommonClientFlags.None);
				SetClientDetailsOptions options2 = setClientDetailsOptions;
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
}
