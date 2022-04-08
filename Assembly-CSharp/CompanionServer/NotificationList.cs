using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using Network;
using Newtonsoft.Json;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public class NotificationList
{
	private const string ApiEndpoint = "https://companion-rust.facepunch.com/api/push/send";

	private static readonly HttpClient Http = new HttpClient();

	private readonly HashSet<ulong> _subscriptions = new HashSet<ulong>();

	private double _lastSend;

	public bool AddSubscription(ulong steamId)
	{
		if (steamId == 0L)
		{
			return false;
		}
		if (_subscriptions.Count >= 50)
		{
			return false;
		}
		return _subscriptions.Add(steamId);
	}

	public bool RemoveSubscription(ulong steamId)
	{
		return _subscriptions.Remove(steamId);
	}

	public bool HasSubscription(ulong steamId)
	{
		return _subscriptions.Contains(steamId);
	}

	public List<ulong> ToList()
	{
		List<ulong> list = Facepunch.Pool.GetList<ulong>();
		foreach (ulong subscription in _subscriptions)
		{
			list.Add(subscription);
		}
		return list;
	}

	public void LoadFrom(List<ulong> steamIds)
	{
		_subscriptions.Clear();
		if (steamIds == null)
		{
			return;
		}
		foreach (ulong steamId in steamIds)
		{
			_subscriptions.Add(steamId);
		}
	}

	public void IntersectWith(List<PlayerNameID> players)
	{
		List<ulong> obj = Facepunch.Pool.GetList<ulong>();
		foreach (PlayerNameID player in players)
		{
			obj.Add(player.userid);
		}
		_subscriptions.IntersectWith(obj);
		Facepunch.Pool.FreeList(ref obj);
	}

	public Task<NotificationSendResult> SendNotification(NotificationChannel channel, string title, string body, string type)
	{
		double realtimeSinceStartup = TimeEx.realtimeSinceStartup;
		if (realtimeSinceStartup - _lastSend < 15.0)
		{
			return Task.FromResult(NotificationSendResult.RateLimited);
		}
		Dictionary<string, string> serverPairingData = Util.GetServerPairingData();
		if (!string.IsNullOrWhiteSpace(type))
		{
			serverPairingData["type"] = type;
		}
		_lastSend = realtimeSinceStartup;
		return SendNotificationImpl(_subscriptions, channel, title, body, serverPairingData);
	}

	public static async Task<NotificationSendResult> SendNotificationTo(ICollection<ulong> steamIds, NotificationChannel channel, string title, string body, Dictionary<string, string> data)
	{
		NotificationSendResult notificationSendResult = await SendNotificationImpl(steamIds, channel, title, body, data);
		if (notificationSendResult == NotificationSendResult.NoTargetsFound)
		{
			notificationSendResult = NotificationSendResult.Sent;
		}
		return notificationSendResult;
	}

	public static async Task<NotificationSendResult> SendNotificationTo(ulong steamId, NotificationChannel channel, string title, string body, Dictionary<string, string> data)
	{
		HashSet<ulong> set = Facepunch.Pool.Get<HashSet<ulong>>();
		set.Clear();
		set.Add(steamId);
		NotificationSendResult result = await SendNotificationImpl(set, channel, title, body, data);
		set.Clear();
		Facepunch.Pool.Free(ref set);
		return result;
	}

	private static async Task<NotificationSendResult> SendNotificationImpl(ICollection<ulong> steamIds, NotificationChannel channel, string title, string body, Dictionary<string, string> data)
	{
		if (!Server.IsEnabled || !App.notifications)
		{
			return NotificationSendResult.Disabled;
		}
		if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
		{
			return NotificationSendResult.Empty;
		}
		if (steamIds.Count == 0)
		{
			return NotificationSendResult.Sent;
		}
		PushRequest obj = Facepunch.Pool.Get<PushRequest>();
		obj.ServerToken = Server.Token;
		obj.Channel = channel;
		obj.Title = title;
		obj.Body = body;
		obj.Data = data;
		obj.SteamIds = Facepunch.Pool.GetList<ulong>();
		foreach (ulong steamId in steamIds)
		{
			obj.SteamIds.Add(steamId);
		}
		string content = JsonConvert.SerializeObject(obj);
		Facepunch.Pool.Free(ref obj);
		try
		{
			StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json");
			HttpResponseMessage httpResponseMessage = await Http.PostAsync("https://companion-rust.facepunch.com/api/push/send", content2);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				DebugEx.LogWarning($"Failed to send notification: {httpResponseMessage.StatusCode}");
				return NotificationSendResult.ServerError;
			}
			if (httpResponseMessage.StatusCode == HttpStatusCode.Accepted)
			{
				return NotificationSendResult.NoTargetsFound;
			}
			return NotificationSendResult.Sent;
		}
		catch (Exception arg)
		{
			DebugEx.LogWarning($"Exception thrown when sending notification: {arg}");
			return NotificationSendResult.Failed;
		}
	}
}
