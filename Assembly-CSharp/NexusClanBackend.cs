using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;
using ProtoBuf.Nexus;
using Rust;
using UnityEngine;

public class NexusClanBackend : IClanBackend, IDisposable
{
	private readonly Dictionary<long, NexusClanWrapper> _clanWrappers;

	private IClanChangeSink _changeSink;

	private NexusClanChatCollector _chatCollector;

	private NexusClanEventHandler _eventHandler;

	private NexusZoneClient _client;

	public NexusClanBackend()
	{
		_clanWrappers = new Dictionary<long, NexusClanWrapper>();
	}

	public ValueTask Initialize(IClanChangeSink changeSink)
	{
		if (!NexusServer.Started)
		{
			throw new InvalidOperationException("Cannot use the Nexus clan backend when nexus is not enabled on this server!");
		}
		_clanWrappers.Clear();
		_changeSink = changeSink;
		_chatCollector = new NexusClanChatCollector(changeSink);
		_eventHandler = new NexusClanEventHandler(this, changeSink);
		_client = NexusServer.ZoneClient;
		_client.ClanEventListener = _eventHandler;
		Rust.Global.Runner.StartCoroutine(BroadcastClanChatBatches());
		return default(ValueTask);
	}

	public void Dispose()
	{
		_clanWrappers.Clear();
		_changeSink = null;
		_chatCollector = null;
		_eventHandler = null;
		if (_client?.ClanEventListener != null)
		{
			_client.ClanEventListener = null;
		}
		_client = null;
	}

	public async ValueTask<ClanValueResult<IClan>> Get(long clanId)
	{
		NexusClanResult<NexusClan> nexusClanResult = await _client.GetClan(clanId);
		if (nexusClanResult.IsSuccess && nexusClanResult.TryGetResponse(out var response))
		{
			return (ClanValueResult<IClan>)(IClan)Wrap(response);
		}
		return NexusClanUtil.ToClanResult(nexusClanResult.ResultCode);
	}

	public bool TryGet(long clanId, out IClan clan)
	{
		if (!_client.TryGetClan(clanId, out var clan2))
		{
			clan = null;
			return false;
		}
		clan = Wrap(clan2);
		return true;
	}

	public async ValueTask<ClanValueResult<IClan>> GetByMember(ulong steamId)
	{
		NexusClanResult<NexusClan> nexusClanResult = await _client.GetClanByMember(NexusClanUtil.GetPlayerId(steamId));
		if (nexusClanResult.IsSuccess && nexusClanResult.TryGetResponse(out var response))
		{
			return (ClanValueResult<IClan>)(IClan)Wrap(response);
		}
		return NexusClanUtil.ToClanResult(nexusClanResult.ResultCode);
	}

	public async ValueTask<ClanValueResult<IClan>> Create(ulong leaderSteamId, string name)
	{
		ClanCreateParameters clanCreateParameters = default(ClanCreateParameters);
		clanCreateParameters.ClanName = name;
		clanCreateParameters.ClanNameNormalized = name.ToLowerInvariant().Normalize(NormalizationForm.FormKC);
		clanCreateParameters.LeaderPlayerId = NexusClanUtil.GetPlayerId(leaderSteamId);
		clanCreateParameters.LeaderRoleName = "Leader";
		clanCreateParameters.LeaderRoleVariables = NexusClanUtil.DefaultLeaderVariables;
		clanCreateParameters.MemberRoleName = "Member";
		ClanCreateParameters parameters = clanCreateParameters;
		NexusClanResult<NexusClan> nexusClanResult = await _client.CreateClan(parameters);
		if (nexusClanResult.IsSuccess && nexusClanResult.TryGetResponse(out var response))
		{
			return (ClanValueResult<IClan>)(IClan)Wrap(response);
		}
		return NexusClanUtil.ToClanResult(nexusClanResult.ResultCode);
	}

	public async ValueTask<ClanValueResult<List<ClanInvitation>>> ListInvitations(ulong steamId)
	{
		NexusClanResult<List<Facepunch.Nexus.Models.ClanInvitation>> nexusClanResult = await _client.ListClanInvitations(NexusClanUtil.GetPlayerId(steamId));
		if (nexusClanResult.IsSuccess && nexusClanResult.TryGetResponse(out var response))
		{
			List<ClanInvitation> value = response.Select(delegate(Facepunch.Nexus.Models.ClanInvitation i)
			{
				ClanInvitation result = default(ClanInvitation);
				result.ClanId = i.ClanId;
				result.Recruiter = NexusClanUtil.GetSteamId(i.RecruiterPlayerId);
				result.Timestamp = i.Timestamp;
				return result;
			}).ToList();
			return new ClanValueResult<List<ClanInvitation>>(value);
		}
		return NexusClanUtil.ToClanResult(nexusClanResult.ResultCode);
	}

	public void HandleClanChatBatch(ClanChatBatchRequest request)
	{
		if (_changeSink == null)
		{
			return;
		}
		foreach (ClanChatBatchRequest.Message message in request.messages)
		{
			_changeSink.ClanChatMessage(message.clanId, new ClanChatEntry
			{
				SteamId = message.userId,
				Message = message.text,
				Name = message.name,
				Time = message.timestamp
			});
		}
	}

	private IEnumerator BroadcastClanChatBatches()
	{
		while (_chatCollector != null && NexusServer.Started)
		{
			List<ClanChatBatchRequest.Message> obj = Facepunch.Pool.GetList<ClanChatBatchRequest.Message>();
			_chatCollector.TakeMessages(obj);
			if (obj.Count > 0)
			{
				SendClanChatBatch(obj);
			}
			else
			{
				Facepunch.Pool.FreeList(ref obj);
			}
			yield return CoroutineEx.waitForSecondsRealtime(ConVar.Nexus.clanClatBatchDuration);
		}
		static async void SendClanChatBatch(List<ClanChatBatchRequest.Message> messages)
		{
			Request request = Facepunch.Pool.Get<Request>();
			request.isFireAndForget = true;
			request.clanChatBatch = Facepunch.Pool.Get<ClanChatBatchRequest>();
			request.clanChatBatch.messages = messages;
			try
			{
				(await NexusServer.BroadcastRpc(request))?.Dispose();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void UpdateWrapper(long clanId)
	{
		NexusClanWrapper value;
		lock (_clanWrappers)
		{
			if (!_clanWrappers.TryGetValue(clanId, out value))
			{
				return;
			}
		}
		value.UpdateValuesInternal();
	}

	public void RemoveWrapper(long clanId)
	{
		lock (_clanWrappers)
		{
			_clanWrappers.Remove(clanId);
		}
	}

	private NexusClanWrapper Wrap(NexusClan clan)
	{
		lock (_clanWrappers)
		{
			if (_clanWrappers.TryGetValue(clan.ClanId, out var value) && value.Internal == clan)
			{
				return value;
			}
			value = new NexusClanWrapper(clan, _chatCollector);
			_clanWrappers[clan.ClanId] = value;
			return value;
		}
	}
}
