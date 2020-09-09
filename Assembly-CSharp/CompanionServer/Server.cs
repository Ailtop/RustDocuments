using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using UnityEngine;

namespace CompanionServer
{
	public static class Server
	{
		private class RegisterResponse
		{
			public string ServerId;

			public string ServerToken;
		}

		private const string ApiEndpoint = "https://companion-rust.facepunch.com/api/server";

		private static readonly HttpClient Http = new HttpClient();

		public static readonly ChatLog TeamChat = new ChatLog();

		internal static string Token;

		public static Listener Listener
		{
			get;
			private set;
		}

		public static void Initialize()
		{
			if (App.port >= 0)
			{
				SetupServerRegistration();
				Map.PopulateCache();
				if (App.port == 0)
				{
					App.port = Math.Max(ConVar.Server.port, RCon.Port) + 67;
				}
				try
				{
					Listener = new Listener(App.GetListenIP(), App.port);
				}
				catch (Exception arg)
				{
					Debug.LogError($"Companion server failed to start: {arg}");
				}
			}
		}

		public static void Shutdown()
		{
			Listener?.Dispose();
			Listener = null;
		}

		public static void Update()
		{
			Listener?.Update();
		}

		public static void Broadcast(PlayerTarget target, AppBroadcast broadcast)
		{
			Listener?.PlayerSubscribers?.Send(target, broadcast);
		}

		public static void Broadcast(EntityTarget target, AppBroadcast broadcast)
		{
			Listener?.EntitySubscribers?.Send(target, broadcast);
		}

		public static void ClearSubscribers(EntityTarget target)
		{
			Listener?.EntitySubscribers?.Clear(target);
		}

		public static bool CanSendPairingNotification(ulong playerId)
		{
			return Listener?.CanSendPairingNotification(playerId) ?? false;
		}

		private static async void SetupServerRegistration()
		{
			int num = 2;
			try
			{
				string serverId;
				string serverToken;
				if (!TryLoadServerRegistration(out serverId, out serverToken))
				{
					goto IL_0185;
				}
				StringContent content = new StringContent(serverToken, Encoding.UTF8, "text/plain");
				HttpResponseMessage httpResponseMessage = await Http.PostAsync("https://companion-rust.facepunch.com/api/server/refresh", content);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					Debug.LogWarning("Failed to refresh server ID - registering a new one");
					goto IL_0185;
				}
				SetServerRegistration(await httpResponseMessage.Content.ReadAsStringAsync());
				goto end_IL_001e;
				IL_0185:
				SetServerRegistration(await Http.GetStringAsync("https://companion-rust.facepunch.com/api/server/register"));
				end_IL_001e:;
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed to setup companion server registration: {arg}");
			}
		}

		private static bool TryLoadServerRegistration(out string serverId, out string serverToken)
		{
			serverId = null;
			serverToken = null;
			string serverIdPath = GetServerIdPath();
			if (!File.Exists(serverIdPath))
			{
				return false;
			}
			try
			{
				RegisterResponse registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(File.ReadAllText(serverIdPath));
				serverId = registerResponse.ServerId;
				serverToken = registerResponse.ServerToken;
				return true;
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed to load companion server registration: {arg}");
				return false;
			}
		}

		private static void SetServerRegistration(string responseJson)
		{
			RegisterResponse registerResponse = null;
			try
			{
				registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(responseJson);
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed to parse registration response JSON: {responseJson}\n\n{arg}");
			}
			ConsoleSystem.Index.Server.Find("app.serverid")?.Set(registerResponse?.ServerId);
			Token = registerResponse?.ServerToken;
			if (registerResponse != null)
			{
				try
				{
					File.WriteAllText(GetServerIdPath(), responseJson);
				}
				catch (Exception arg2)
				{
					Debug.LogError($"Unable to save companion app server registration - server ID may be different after restart: {arg2}");
				}
			}
		}

		private static string GetServerIdPath()
		{
			return Path.Combine(ConVar.Server.rootFolder, "companion.id");
		}
	}
}
