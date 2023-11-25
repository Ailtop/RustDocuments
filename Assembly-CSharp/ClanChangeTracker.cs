using System.Collections.Generic;
using CompanionServer;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using ProtoBuf;

public class ClanChangeTracker : IClanChangeSink
{
	private struct ClanChangedEvent
	{
		public long ClanId;

		public ClanDataSource DataSources;
	}

	private struct ClanDisbandedEvent
	{
		public long ClanId;
	}

	private struct InvitationCreatedEvent
	{
		public ulong SteamId;

		public long ClanId;
	}

	private struct MembershipChangedEvent
	{
		public ulong SteamId;

		public long ClanId;
	}

	private struct ChatMessageEvent
	{
		public long ClanId;

		public ClanChatEntry Message;
	}

	private class ChatMessageEventComparer : IComparer<ChatMessageEvent>
	{
		public static readonly ChatMessageEventComparer Instance = new ChatMessageEventComparer();

		public int Compare(ChatMessageEvent x, ChatMessageEvent y)
		{
			return x.Message.Time.CompareTo(y.Message.Time);
		}
	}

	private readonly ClanManager _clanManager;

	private readonly List<ClanChangedEvent> _clanChangedEvents = new List<ClanChangedEvent>();

	private readonly List<ClanDisbandedEvent> _clanDisbandedEvents = new List<ClanDisbandedEvent>();

	private readonly List<InvitationCreatedEvent> _invitationCreatedEvents = new List<InvitationCreatedEvent>();

	private readonly List<MembershipChangedEvent> _membershipChangedEvents = new List<MembershipChangedEvent>();

	private readonly List<ChatMessageEvent> _chatMessageEvents = new List<ChatMessageEvent>();

	public ClanChangeTracker(ClanManager clanManager)
	{
		_clanManager = clanManager;
	}

	public void HandleEvents()
	{
		lock (_clanChangedEvents)
		{
			foreach (ClanChangedEvent clanChangedEvent in _clanChangedEvents)
			{
				ClanChangedEvent data = clanChangedEvent;
				HandleClanChanged(in data);
			}
			_clanChangedEvents.Clear();
		}
		lock (_clanDisbandedEvents)
		{
			foreach (ClanDisbandedEvent clanDisbandedEvent in _clanDisbandedEvents)
			{
				ClanDisbandedEvent data2 = clanDisbandedEvent;
				HandleClanDisbanded(in data2);
			}
			_clanDisbandedEvents.Clear();
		}
		lock (_invitationCreatedEvents)
		{
			foreach (InvitationCreatedEvent invitationCreatedEvent in _invitationCreatedEvents)
			{
				InvitationCreatedEvent data3 = invitationCreatedEvent;
				HandleInvitationCreated(in data3);
			}
			_invitationCreatedEvents.Clear();
		}
		lock (_membershipChangedEvents)
		{
			foreach (MembershipChangedEvent membershipChangedEvent in _membershipChangedEvents)
			{
				MembershipChangedEvent data4 = membershipChangedEvent;
				HandleMembershipChanged(in data4);
			}
			_membershipChangedEvents.Clear();
		}
		lock (_chatMessageEvents)
		{
			foreach (ChatMessageEvent chatMessageEvent in _chatMessageEvents)
			{
				ChatMessageEvent data5 = chatMessageEvent;
				HandleChatMessageEvent(in data5);
			}
			_chatMessageEvents.Clear();
		}
	}

	private void HandleClanChanged(in ClanChangedEvent data)
	{
		if (_clanManager.Backend.TryGet(data.ClanId, out var clan))
		{
			_clanManager.SendClanChanged(clan);
			AppBroadcast appBroadcast = Facepunch.Pool.Get<AppBroadcast>();
			appBroadcast.clanChanged = Facepunch.Pool.Get<AppClanChanged>();
			appBroadcast.clanChanged.clanInfo = ClanInfoExtensions.ToProto(clan);
			CompanionServer.Server.Broadcast(new ClanTarget(data.ClanId), appBroadcast);
		}
		if (data.DataSources.HasFlag(ClanDataSource.Members))
		{
			_clanManager.ClanMemberConnectionsChanged(data.ClanId);
		}
	}

	private void HandleClanDisbanded(in ClanDisbandedEvent data)
	{
	}

	private void HandleInvitationCreated(in InvitationCreatedEvent data)
	{
		_clanManager.SendClanInvitation(data.SteamId, data.ClanId);
	}

	private void HandleMembershipChanged(in MembershipChangedEvent data)
	{
		BasePlayer basePlayer = BasePlayer.FindByID(data.SteamId);
		if (basePlayer == null)
		{
			basePlayer = BasePlayer.FindSleeping(data.SteamId);
		}
		if (basePlayer != null)
		{
			basePlayer.clanId = data.ClanId;
			basePlayer.SendNetworkUpdateImmediate();
			if (basePlayer.IsConnected)
			{
				_clanManager.ClientRPCPlayer(null, basePlayer, "Client_CurrentClanChanged");
			}
		}
	}

	private void HandleChatMessageEvent(in ChatMessageEvent data)
	{
		if (_clanManager.TryGetClanMemberConnections(data.ClanId, out var connections) && connections.Count > 0)
		{
			string nameColor = Chat.GetNameColor(data.Message.SteamId);
			ConsoleNetwork.SendClientCommand(connections, "chat.add2", 5, data.Message.SteamId, data.Message.Message, data.Message.Name, nameColor, 1f);
		}
		AppBroadcast appBroadcast = Facepunch.Pool.Get<AppBroadcast>();
		appBroadcast.clanMessage = Facepunch.Pool.Get<AppNewClanMessage>();
		appBroadcast.clanMessage.clanId = data.ClanId;
		appBroadcast.clanMessage.message = Facepunch.Pool.Get<AppClanMessage>();
		appBroadcast.clanMessage.message.steamId = data.Message.SteamId;
		appBroadcast.clanMessage.message.name = data.Message.Name;
		appBroadcast.clanMessage.message.message = data.Message.Message;
		appBroadcast.clanMessage.message.time = data.Message.Time;
		CompanionServer.Server.Broadcast(new ClanTarget(data.ClanId), appBroadcast);
	}

	public void ClanChanged(long clanId, ClanDataSource dataSources)
	{
		lock (_clanChangedEvents)
		{
			int num = _clanChangedEvents.FindIndexWith((ClanChangedEvent e) => e.ClanId, clanId);
			if (num < 0)
			{
				_clanChangedEvents.Add(new ClanChangedEvent
				{
					ClanId = clanId,
					DataSources = dataSources
				});
			}
			else
			{
				ClanChangedEvent value = _clanChangedEvents[num];
				value.DataSources |= dataSources;
				_clanChangedEvents[num] = value;
			}
		}
	}

	public void ClanDisbanded(long clanId)
	{
		lock (_clanDisbandedEvents)
		{
			_clanDisbandedEvents.Add(new ClanDisbandedEvent
			{
				ClanId = clanId
			});
		}
	}

	public void InvitationCreated(ulong steamId, long clanId)
	{
		lock (_invitationCreatedEvents)
		{
			_invitationCreatedEvents.Add(new InvitationCreatedEvent
			{
				SteamId = steamId,
				ClanId = clanId
			});
		}
	}

	public void MembershipChanged(ulong steamId, long? clanId)
	{
		lock (_membershipChangedEvents)
		{
			_membershipChangedEvents.Add(new MembershipChangedEvent
			{
				SteamId = steamId,
				ClanId = clanId.GetValueOrDefault()
			});
		}
	}

	public void ClanChatMessage(long clanId, ClanChatEntry entry)
	{
		lock (_chatMessageEvents)
		{
			ChatMessageEvent chatMessageEvent = default(ChatMessageEvent);
			chatMessageEvent.ClanId = clanId;
			chatMessageEvent.Message = entry;
			ChatMessageEvent item = chatMessageEvent;
			int num = _chatMessageEvents.BinarySearch(item, ChatMessageEventComparer.Instance);
			_chatMessageEvents.Insert((num >= 0) ? num : (~num), item);
		}
	}
}
