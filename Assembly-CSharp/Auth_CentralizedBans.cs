using System;
using System.Collections;
using ConVar;
using Facepunch.Extend;
using Facepunch.Math;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Networking;

public static class Auth_CentralizedBans
{
	private class BanPayload
	{
		public ulong steamId;

		public string reason;

		public long expiryDate;
	}

	private static readonly BanPayload payloadData = new BanPayload();

	public static IEnumerator Run(Connection connection)
	{
		int num = default(int);
		UnityWebRequest ownerRequest = default(UnityWebRequest);
		UnityWebRequest userRequest = default(UnityWebRequest);
		while (true)
		{
			object obj = Interface.CallHook("OnCentralizedBanCheck", connection);
			if (obj != null)
			{
				break;
			}
			string uri2;
			switch (num)
			{
			default:
				yield break;
			case 0:
				if (!connection.active || connection.rejected || string.IsNullOrWhiteSpace(ConVar.Server.bansServerEndpoint) || !ConVar.Server.bansServerEndpoint.StartsWith("http"))
				{
					yield break;
				}
				connection.authStatus = "";
				if (!ConVar.Server.bansServerEndpoint.EndsWith("/") && !ConVar.Server.bansServerEndpoint.EndsWith("="))
				{
					ConVar.Server.bansServerEndpoint += "/";
				}
				if (connection.ownerid != 0L && connection.ownerid != connection.userid)
				{
					string uri = ConVar.Server.bansServerEndpoint + connection.ownerid;
					ownerRequest = UnityWebRequest.Get(uri);
					ownerRequest.timeout = ConVar.Server.bansServerTimeout;
					yield return ownerRequest.SendWebRequest();
					break;
				}
				goto IL_0163;
			case 1:
				if (CheckIfPlayerBanned(connection.ownerid, connection, ownerRequest))
				{
					yield break;
				}
				ownerRequest = null;
				goto IL_0163;
			case 2:
				{
					if (!CheckIfPlayerBanned(connection.userid, connection, userRequest))
					{
						connection.authStatus = "ok";
					}
					yield break;
				}
				IL_0163:
				uri2 = ConVar.Server.bansServerEndpoint + connection.userid;
				userRequest = UnityWebRequest.Get(uri2);
				userRequest.timeout = ConVar.Server.bansServerTimeout;
				yield return userRequest.SendWebRequest();
				break;
			}
		}
	}

	private static bool CheckIfPlayerBanned(ulong steamId, Connection connection, UnityWebRequest request)
	{
		if (request.isNetworkError)
		{
			Debug.LogError("Failed to check centralized bans due to a network error (" + request.error + ")");
			if (ConVar.Server.bansServerFailureMode == 1)
			{
				Reject("Centralized Ban Error: Network Error");
				return true;
			}
			return false;
		}
		if (request.responseCode == 404)
		{
			return false;
		}
		if (request.isHttpError)
		{
			Debug.LogError($"Failed to check centralized bans due to a server error ({request.responseCode}: {request.error})");
			if (ConVar.Server.bansServerFailureMode == 1)
			{
				Reject("Centralized Ban Error: Server Error");
				return true;
			}
			return false;
		}
		try
		{
			payloadData.steamId = 0uL;
			payloadData.reason = null;
			payloadData.expiryDate = 0L;
			JsonUtility.FromJsonOverwrite(request.downloadHandler.text, payloadData);
			if (payloadData.expiryDate > 0 && Epoch.Current >= payloadData.expiryDate)
			{
				return false;
			}
			if (payloadData.steamId != steamId)
			{
				Debug.LogError($"Failed to check centralized bans due to SteamID mismatch (expected {steamId}, got {payloadData.steamId})");
				if (ConVar.Server.bansServerFailureMode == 1)
				{
					Reject("Centralized Ban Error: SteamID Mismatch");
					return true;
				}
				return false;
			}
			string text = payloadData.reason ?? "no reason given";
			string text2 = ((payloadData.expiryDate > 0) ? (" for " + (payloadData.expiryDate - Epoch.Current).FormatSecondsLong()) : "");
			Reject("You are banned from this server" + text2 + " (" + text + ")");
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogError("Failed to check centralized bans due to a malformed response: " + request.downloadHandler.text);
			Debug.LogException(exception);
			if (ConVar.Server.bansServerFailureMode == 1)
			{
				Reject("Centralized Ban Error: Malformed Response");
				return true;
			}
			return false;
		}
		void Reject(string reason)
		{
			ConnectionAuth.Reject(connection, reason);
			PlatformService.Instance.EndPlayerSession(connection.userid);
		}
	}
}
