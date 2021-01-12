using ConVar;
using Facepunch.Extend;
using Facepunch.Math;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

	[CompilerGenerated]
	private sealed class _003CRun_003Ed__0 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Connection connection;

		private UnityWebRequest _003CuserRequest_003E5__2;

		private UnityWebRequest _003CownerRequest_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CRun_003Ed__0(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			string uri;
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!connection.active)
				{
					return false;
				}
				if (connection.rejected)
				{
					return false;
				}
				if (string.IsNullOrWhiteSpace(ConVar.Server.bansServerEndpoint) || !ConVar.Server.bansServerEndpoint.StartsWith("http"))
				{
					return false;
				}
				connection.authStatus = "";
				if (!ConVar.Server.bansServerEndpoint.EndsWith("/"))
				{
					ConVar.Server.bansServerEndpoint += "/";
				}
				if (connection.ownerid != 0L && connection.ownerid != connection.userid)
				{
					string uri2 = ConVar.Server.bansServerEndpoint + connection.ownerid;
					_003CownerRequest_003E5__3 = UnityWebRequest.Get(uri2);
					_003CownerRequest_003E5__3.timeout = ConVar.Server.bansServerTimeout;
					_003C_003E2__current = _003CownerRequest_003E5__3.SendWebRequest();
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_013b;
			case 1:
				_003C_003E1__state = -1;
				if (CheckIfPlayerBanned(connection.ownerid, connection, _003CownerRequest_003E5__3))
				{
					return false;
				}
				_003CownerRequest_003E5__3 = null;
				goto IL_013b;
			case 2:
				{
					_003C_003E1__state = -1;
					if (CheckIfPlayerBanned(connection.userid, connection, _003CuserRequest_003E5__2))
					{
						return false;
					}
					connection.authStatus = "ok";
					return false;
				}
				IL_013b:
				uri = ConVar.Server.bansServerEndpoint + connection.userid;
				_003CuserRequest_003E5__2 = UnityWebRequest.Get(uri);
				_003CuserRequest_003E5__2.timeout = ConVar.Server.bansServerTimeout;
				_003C_003E2__current = _003CuserRequest_003E5__2.SendWebRequest();
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass2_0
	{
		public Connection connection;
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
			UnityEngine.Debug.LogError("Failed to check centralized bans due to a network error (" + request.error + ")");
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
			UnityEngine.Debug.LogError($"Failed to check centralized bans due to a server error ({request.responseCode}: {request.error})");
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
				UnityEngine.Debug.LogError($"Failed to check centralized bans due to SteamID mismatch (expected {steamId}, got {payloadData.steamId})");
				if (ConVar.Server.bansServerFailureMode == 1)
				{
					_003CCheckIfPlayerBanned_003Eg__Reject_007C2_0("Centralized Ban Error: SteamID Mismatch", ref _003C_003Ec__DisplayClass2_);
					return true;
				}
				return false;
			}
			string text = payloadData.reason ?? "no reason given";
			string text2 = (payloadData.expiryDate > 0) ? (" for " + (payloadData.expiryDate - Epoch.Current).FormatSecondsLong()) : "";
			_003CCheckIfPlayerBanned_003Eg__Reject_007C2_0("You are banned from this server" + text2 + " (" + text + ")", ref _003C_003Ec__DisplayClass2_);
			return true;
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogError("Failed to check centralized bans due to a malformed response: " + request.downloadHandler.text);
			UnityEngine.Debug.LogException(exception);
			if (ConVar.Server.bansServerFailureMode == 1)
			{
				_003CCheckIfPlayerBanned_003Eg__Reject_007C2_0("Centralized Ban Error: Malformed Response", ref _003C_003Ec__DisplayClass2_);
				return true;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private static void _003CCheckIfPlayerBanned_003Eg__Reject_007C2_0(string reason, ref _003C_003Ec__DisplayClass2_0 P_1)
	{
		ConnectionAuth.Reject(P_1.connection, reason);
		PlatformService.Instance.EndPlayerSession(P_1.connection.userid);
	}
}
