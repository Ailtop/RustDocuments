using System;
using System.Collections.Generic;
using System.IO;
using ConVar;
using EasyAntiCheat.Server;
using EasyAntiCheat.Server.Cerberus;
using EasyAntiCheat.Server.Hydra;
using EasyAntiCheat.Server.Scout;
using Network;
using Oxide.Core;
using UnityEngine;

public static class EACServer
{
	public static ICerberus<EasyAntiCheat.Server.Hydra.Client> playerTracker;

	public static Scout eacScout;

	private static Dictionary<EasyAntiCheat.Server.Hydra.Client, Connection> client2connection = new Dictionary<EasyAntiCheat.Server.Hydra.Client, Connection>();

	private static Dictionary<Connection, EasyAntiCheat.Server.Hydra.Client> connection2client = new Dictionary<Connection, EasyAntiCheat.Server.Hydra.Client>();

	private static Dictionary<Connection, ClientStatus> connection2status = new Dictionary<Connection, ClientStatus>();

	private static EasyAntiCheatServer<EasyAntiCheat.Server.Hydra.Client> easyAntiCheat = null;

	public static void Encrypt(Connection connection, MemoryStream src, int srcOffset, MemoryStream dst, int dstOffset)
	{
		if (easyAntiCheat != null)
		{
			easyAntiCheat.NetProtect.ProtectMessage(GetClient(connection), src, srcOffset, dst, dstOffset);
		}
	}

	public static void Decrypt(Connection connection, MemoryStream src, int srcOffset, MemoryStream dst, int dstOffset)
	{
		if (easyAntiCheat != null)
		{
			easyAntiCheat.NetProtect.UnprotectMessage(GetClient(connection), src, srcOffset, dst, dstOffset);
		}
	}

	public static EasyAntiCheat.Server.Hydra.Client GetClient(Connection connection)
	{
		EasyAntiCheat.Server.Hydra.Client value;
		connection2client.TryGetValue(connection, out value);
		return value;
	}

	public static Connection GetConnection(EasyAntiCheat.Server.Hydra.Client client)
	{
		Connection value;
		client2connection.TryGetValue(client, out value);
		return value;
	}

	public static bool IsAuthenticated(Connection connection)
	{
		ClientStatus value;
		connection2status.TryGetValue(connection, out value);
		return value == ClientStatus.ClientAuthenticatedRemote;
	}

	private static void OnAuthenticatedLocal(Connection connection)
	{
		if (connection.authStatus == string.Empty)
		{
			connection.authStatus = "ok";
		}
		connection2status[connection] = ClientStatus.ClientAuthenticatedLocal;
	}

	private static void OnAuthenticatedRemote(Connection connection)
	{
		connection2status[connection] = ClientStatus.ClientAuthenticatedRemote;
	}

	public static bool ShouldIgnore(Connection connection)
	{
		if (connection.authLevel >= 3)
		{
			return true;
		}
		return false;
	}

	private static void HandleClientUpdate(ClientStatusUpdate<EasyAntiCheat.Server.Hydra.Client> clientStatus)
	{
		using (TimeWarning.New("AntiCheatKickPlayer", 10))
		{
			EasyAntiCheat.Server.Hydra.Client client = clientStatus.Client;
			Connection connection = GetConnection(client);
			if (connection == null)
			{
				Debug.LogError("EAC status update for invalid client: " + client.ClientID);
			}
			else
			{
				if (ShouldIgnore(connection))
				{
					return;
				}
				if (clientStatus.RequiresKick)
				{
					string text = clientStatus.Message;
					if (string.IsNullOrEmpty(text))
					{
						text = clientStatus.Status.ToString();
					}
					Debug.Log($"[EAC] Kicking {connection.userid} / {connection.username} ({text})");
					connection.authStatus = "eac";
					Network.Net.sv.Kick(connection, "EAC: " + text);
					Interface.CallHook("OnPlayerKicked", connection, text);
					DateTime? timeBanExpires;
					if (clientStatus.IsBanned(out timeBanExpires))
					{
						connection.authStatus = "eacbanned";
						object[] args = new object[3]
						{
							2,
							0,
							"<color=#fff>SERVER</color> Kicking " + connection.username + " (banned by anticheat)"
						};
						Interface.CallHook("OnPlayerBanned", connection, text);
						ConsoleNetwork.BroadcastToAllClients("chat.add", args);
						if (!timeBanExpires.HasValue)
						{
							Entity.DeleteBy(connection.userid);
						}
					}
					easyAntiCheat.UnregisterClient(client);
					client2connection.Remove(client);
					connection2client.Remove(connection);
					connection2status.Remove(connection);
				}
				else if (clientStatus.Status == ClientStatus.ClientAuthenticatedLocal)
				{
					OnAuthenticatedLocal(connection);
					easyAntiCheat.SetClientNetworkState(client, false);
				}
				else if (clientStatus.Status == ClientStatus.ClientAuthenticatedRemote)
				{
					OnAuthenticatedRemote(connection);
				}
			}
		}
	}

	private static void SendToClient(EasyAntiCheat.Server.Hydra.Client client, byte[] message, int messageLength)
	{
		Connection connection = GetConnection(client);
		if (connection == null)
		{
			Debug.LogError("EAC network packet for invalid client: " + client.ClientID);
		}
		else if (Network.Net.sv.write.Start())
		{
			Network.Net.sv.write.PacketID(Message.Type.EAC);
			Network.Net.sv.write.UInt32((uint)messageLength);
			Network.Net.sv.write.Write(message, 0, messageLength);
			Network.Net.sv.write.Send(new SendInfo(connection));
		}
	}

	public static void DoStartup()
	{
		if (ConVar.Server.secure)
		{
			client2connection.Clear();
			connection2client.Clear();
			connection2status.Clear();
			Log.SetOut(new StreamWriter(ConVar.Server.rootFolder + "/Log.EAC.txt", false)
			{
				AutoFlush = true
			});
			Log.Prefix = "";
			Log.Level = LogLevel.Info;
			easyAntiCheat = new EasyAntiCheatServer<EasyAntiCheat.Server.Hydra.Client>(HandleClientUpdate, 20, ConVar.Server.hostname);
			playerTracker = easyAntiCheat.Cerberus;
			playerTracker.LogGameRoundStart(World.Name, string.Empty, 0);
			eacScout = new Scout();
		}
	}

	public static void DoUpdate()
	{
		if (easyAntiCheat == null)
		{
			return;
		}
		easyAntiCheat.HandleClientUpdates();
		if (Network.Net.sv != null && Network.Net.sv.IsConnected())
		{
			EasyAntiCheat.Server.Hydra.Client client;
			byte[] messageBuffer;
			int messageLength;
			while (easyAntiCheat.PopNetworkMessage(out client, out messageBuffer, out messageLength))
			{
				SendToClient(client, messageBuffer, messageLength);
			}
		}
	}

	public static void DoShutdown()
	{
		client2connection.Clear();
		connection2client.Clear();
		connection2status.Clear();
		if (eacScout != null)
		{
			Debug.Log("EasyAntiCheat Scout Shutting Down");
			eacScout.Dispose();
			eacScout = null;
		}
		if (easyAntiCheat != null)
		{
			Debug.Log("EasyAntiCheat Server Shutting Down");
			easyAntiCheat.Dispose();
			easyAntiCheat = null;
		}
	}

	public static void OnLeaveGame(Connection connection)
	{
		if (easyAntiCheat != null)
		{
			EasyAntiCheat.Server.Hydra.Client client = GetClient(connection);
			easyAntiCheat.UnregisterClient(client);
			client2connection.Remove(client);
			connection2client.Remove(connection);
			connection2status.Remove(connection);
		}
	}

	public static void OnJoinGame(Connection connection)
	{
		if (easyAntiCheat != null)
		{
			EasyAntiCheat.Server.Hydra.Client client = easyAntiCheat.GenerateCompatibilityClient();
			easyAntiCheat.RegisterClient(client, connection.userid.ToString(), connection.ipaddress, connection.ownerid.ToString(), connection.username, (connection.authLevel != 0) ? PlayerRegisterFlags.PlayerRegisterFlagAdmin : PlayerRegisterFlags.PlayerRegisterFlagNone);
			client2connection.Add(client, connection);
			connection2client.Add(connection, client);
			connection2status.Add(connection, ClientStatus.Reserved);
			if (ShouldIgnore(connection))
			{
				OnAuthenticatedLocal(connection);
				OnAuthenticatedRemote(connection);
			}
		}
		else
		{
			OnAuthenticatedLocal(connection);
			OnAuthenticatedRemote(connection);
		}
	}

	public static void OnStartLoading(Connection connection)
	{
		if (easyAntiCheat != null)
		{
			EasyAntiCheat.Server.Hydra.Client client = GetClient(connection);
			easyAntiCheat.SetClientNetworkState(client, false);
		}
	}

	public static void OnFinishLoading(Connection connection)
	{
		if (easyAntiCheat != null)
		{
			EasyAntiCheat.Server.Hydra.Client client = GetClient(connection);
			easyAntiCheat.SetClientNetworkState(client, true);
		}
	}

	public static void OnMessageReceived(Message message)
	{
		if (!connection2client.ContainsKey(message.connection))
		{
			Debug.LogError("EAC network packet from invalid connection: " + message.connection.userid);
			return;
		}
		EasyAntiCheat.Server.Hydra.Client client = GetClient(message.connection);
		byte[] buffer;
		int size;
		if (message.read.TemporaryBytesWithSize(out buffer, out size))
		{
			easyAntiCheat.PushNetworkMessage(client, buffer, size);
		}
	}
}
