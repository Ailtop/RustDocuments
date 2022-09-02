using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Newtonsoft.Json;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public static class Server
{
	private class RegisterResponse
	{
		public string ServerId;

		public string ServerToken;
	}

	private class TestConnectionResponse
	{
		public List<string> Messages;
	}

	private const string ApiEndpoint = "https://companion-rust.facepunch.com/api/server";

	private static readonly HttpClient Http = new HttpClient();

	public static readonly ChatLog TeamChat = new ChatLog();

	internal static string Token;

	public static Listener Listener { get; private set; }

	public static bool IsEnabled
	{
		get
		{
			if (App.port >= 0 && !string.IsNullOrWhiteSpace(App.serverid))
			{
				return Listener != null;
			}
			return false;
		}
	}

	public static void Initialize()
	{
		if (App.port < 0)
		{
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (!(activeGameMode != null) || activeGameMode.rustPlus)
		{
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
				UnityEngine.Debug.LogError($"Companion server failed to start: {arg}");
			}
			PostInitializeServer();
		}
	}

	public static void Shutdown()
	{
		SetServerId(null);
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

	private static async void PostInitializeServer()
	{
		await SetupServerRegistration();
		await CheckConnectivity();
	}

	private static async Task SetupServerRegistration()
	{
		_ = 2;
		try
		{
			if (TryLoadServerRegistration(out var _, out var serverToken))
			{
				StringContent content = new StringContent(serverToken, Encoding.UTF8, "text/plain");
				HttpResponseMessage httpResponseMessage = await Http.PostAsync("https://companion-rust.facepunch.com/api/server/refresh", content);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					SetServerRegistration(await httpResponseMessage.Content.ReadAsStringAsync());
					return;
				}
				UnityEngine.Debug.LogWarning("Failed to refresh server ID - registering a new one");
			}
			SetServerRegistration(await Http.GetStringAsync("https://companion-rust.facepunch.com/api/server/register"));
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Failed to setup companion server registration: {arg}");
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
			UnityEngine.Debug.LogError($"Failed to load companion server registration: {arg}");
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
			UnityEngine.Debug.LogError($"Failed to parse registration response JSON: {responseJson}\n\n{arg}");
		}
		SetServerId(registerResponse?.ServerId);
		Token = registerResponse?.ServerToken;
		if (registerResponse == null)
		{
			return;
		}
		try
		{
			File.WriteAllText(GetServerIdPath(), responseJson);
		}
		catch (Exception arg2)
		{
			UnityEngine.Debug.LogError($"Unable to save companion app server registration - server ID may be different after restart: {arg2}");
		}
	}

	private static async Task CheckConnectivity()
	{
		if (!IsEnabled)
		{
			SetServerId(null);
			return;
		}
		try
		{
			string arg = await GetPublicIPAsync();
			StringContent content = new StringContent("", Encoding.UTF8, "text/plain");
			HttpResponseMessage testResponse = await Http.PostAsync("https://companion-rust.facepunch.com/api/server" + $"/test_connection?address={arg}&port={App.port}", content);
			string text = await testResponse.Content.ReadAsStringAsync();
			TestConnectionResponse testConnectionResponse = null;
			try
			{
				testConnectionResponse = JsonConvert.DeserializeObject<TestConnectionResponse>(text);
			}
			catch (Exception arg2)
			{
				UnityEngine.Debug.LogError($"Failed to parse connectivity test response JSON: {text}\n\n{arg2}");
			}
			if (testConnectionResponse == null)
			{
				return;
			}
			IEnumerable<string> messages = testConnectionResponse.Messages;
			string text2 = string.Join("\n", messages ?? Enumerable.Empty<string>());
			if (testResponse.StatusCode == (HttpStatusCode)555)
			{
				UnityEngine.Debug.LogError("Rust+ companion server connectivity test failed! Disabling Rust+ features.\n\n" + text2);
				SetServerId(null);
				return;
			}
			testResponse.EnsureSuccessStatusCode();
			if (!string.IsNullOrWhiteSpace(text2))
			{
				UnityEngine.Debug.LogWarning("Rust+ companion server connectivity test has warnings:\n" + text2);
			}
		}
		catch (Exception arg3)
		{
			UnityEngine.Debug.LogError($"Failed to check connectivity to the companion server: {arg3}");
		}
	}

	private static async Task<string> GetPublicIPAsync()
	{
		Stopwatch timer = Stopwatch.StartNew();
		string publicIP;
		while (true)
		{
			bool num = timer.Elapsed.TotalMinutes > 2.0;
			publicIP = App.GetPublicIP();
			if (num || (!string.IsNullOrWhiteSpace(publicIP) && publicIP != "0.0.0.0"))
			{
				break;
			}
			await Task.Delay(10000);
		}
		return publicIP;
	}

	private static void SetServerId(string serverId)
	{
		ConsoleSystem.Index.Server.Find("app.serverid")?.Set(serverId ?? "");
	}

	private static string GetServerIdPath()
	{
		return Path.Combine(ConVar.Server.rootFolder, "companion.id");
	}
}
