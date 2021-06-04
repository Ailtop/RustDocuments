using System;
using System.Collections;
using ConVar;
using Facepunch.Extend;
using Facepunch.Math;
using Network;
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
		if (!connection.active || connection.rejected || string.IsNullOrWhiteSpace(ConVar.Server.bansServerEndpoint) || !ConVar.Server.bansServerEndpoint.StartsWith("http"))
		{
			yield break;
		}
		connection.authStatus = "";
		if (!ConVar.Server.bansServerEndpoint.EndsWith("/"))
		{
			ConVar.Server.bansServerEndpoint += "/";
		}
		if (connection.ownerid != 0L && connection.ownerid != connection.userid)
		{
			string uri = ConVar.Server.bansServerEndpoint + connection.ownerid;
			UnityWebRequest ownerRequest = UnityWebRequest.Get(uri);
			ownerRequest.timeout = ConVar.Server.bansServerTimeout;
			yield return ownerRequest.SendWebRequest();
			if (CheckIfPlayerBanned(connection.ownerid, connection, ownerRequest))
			{
				yield break;
			}
		}
		string uri2 = ConVar.Server.bansServerEndpoint + connection.userid;
		UnityWebRequest userRequest = UnityWebRequest.Get(uri2);
		userRequest.timeout = ConVar.Server.bansServerTimeout;
		yield return userRequest.SendWebRequest();
		if (!CheckIfPlayerBanned(connection.userid, connection, userRequest))
		{
			connection.authStatus = "ok";
		}
	}

	private static bool CheckIfPlayerBanned(ulong steamId, Connection connection, UnityWebRequest request)
	{
		_003C_003Ec__DisplayClass2_0 _003C_003Ec__DisplayClass2_ = default(_003C_003Ec__DisplayClass2_0);
		_003C_003Ec__DisplayClass2_.connection = connection;
		if (request.isNetworkError)
		{
			Debug.LogError("Failed to check centralized bans due to a network error (" + request.error + ")");
			if (ConVar.Server.bansServerFailureMode == 1)
			{
				_003CCheckIfPlayerBanned_003Eg__Reject_007C2_0("Centralized Ban Error: Network Error", ref _003C_003Ec__DisplayClass2_);
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
				_003CCheckIfPlayerBanned_003Eg__Reject_007C2_0("Centralized Ban Error: Server Error", ref _003C_003Ec__DisplayClass2_);
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
					_003CCheckIfPlayerBanned_003Eg__Reject_007C2_0("Centralized Ban Error: SteamID Mismatch", ref _003C_003Ec__DisplayClass2_);
					return true;
				}
				return false;
			}
			string text = payloadData.reason ?? "no reason given";
			string text2 = ((payloadData.expiryDate > 0) ? (" for " + (payloadData.expiryDate - Epoch.Current).FormatSecondsLong()) : "");
			_003CCheckIfPlayerBanned_003Eg__Reject_007C2_0("You are banned from this server" + text2 + " (" + text + ")", ref _003C_003Ec__DisplayClass2_);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to check centralized bans due to a malformed response: " + request.downloadHandler.text);
			Debug.LogException(ex);
			if (ConVar.Server.bansServerFailureMode == 1)
			{
				_003CCheckIfPlayerBanned_003Eg__Reject_007C2_0("Centralized Ban Error: Malformed Response", ref _003C_003Ec__DisplayClass2_);
				return true;
			}
			return false;
		}
	}
}
