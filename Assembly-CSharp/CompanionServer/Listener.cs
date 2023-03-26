using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Fleck;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public class Listener : IDisposable, IBroadcastSender<Connection, AppBroadcast>
{
	private struct Message
	{
		public readonly Connection Connection;

		public readonly MemoryBuffer Buffer;

		public Message(Connection connection, MemoryBuffer buffer)
		{
			Connection = connection;
			Buffer = buffer;
		}
	}

	private static readonly ByteArrayStream Stream = new ByteArrayStream();

	private readonly TokenBucketList<IPAddress> _ipTokenBuckets;

	private readonly BanList<IPAddress> _ipBans;

	private readonly TokenBucketList<ulong> _playerTokenBuckets;

	private readonly TokenBucketList<ulong> _pairingTokenBuckets;

	private readonly Queue<Message> _messageQueue;

	private readonly WebSocketServer _server;

	private readonly Stopwatch _stopwatch;

	private RealTimeSince _lastCleanup;

	private long _nextConnectionId;

	public readonly IPAddress Address;

	public readonly int Port;

	public readonly ConnectionLimiter Limiter;

	public readonly SubscriberList<PlayerTarget, Connection, AppBroadcast> PlayerSubscribers;

	public readonly SubscriberList<EntityTarget, Connection, AppBroadcast> EntitySubscribers;

	public readonly SubscriberList<CameraTarget, Connection, AppBroadcast> CameraSubscribers;

	public Listener(IPAddress ipAddress, int port)
	{
		Address = ipAddress;
		Port = port;
		Limiter = new ConnectionLimiter();
		_ipTokenBuckets = new TokenBucketList<IPAddress>(50.0, 15.0);
		_ipBans = new BanList<IPAddress>();
		_playerTokenBuckets = new TokenBucketList<ulong>(25.0, 3.0);
		_pairingTokenBuckets = new TokenBucketList<ulong>(5.0, 0.1);
		_messageQueue = new Queue<Message>();
		SynchronizationContext syncContext = SynchronizationContext.Current;
		_server = new WebSocketServer($"ws://{Address}:{Port}/");
		_server.Start(delegate(IWebSocketConnection socket)
		{
			IPAddress address = socket.ConnectionInfo.ClientIpAddress;
			if (!Limiter.TryAdd(address) || _ipBans.IsBanned(address))
			{
				socket.Close();
			}
			else
			{
				long connectionId = Interlocked.Increment(ref _nextConnectionId);
				Connection conn = new Connection(connectionId, this, socket);
				socket.OnClose = delegate
				{
					Limiter.Remove(address);
					syncContext.Post(delegate(object c)
					{
						((Connection)c).OnClose();
					}, conn);
				};
				socket.OnBinary = conn.OnMessage;
				socket.OnError = UnityEngine.Debug.LogError;
			}
		});
		_stopwatch = new Stopwatch();
		PlayerSubscribers = new SubscriberList<PlayerTarget, Connection, AppBroadcast>(this);
		EntitySubscribers = new SubscriberList<EntityTarget, Connection, AppBroadcast>(this);
		CameraSubscribers = new SubscriberList<CameraTarget, Connection, AppBroadcast>(this, 30.0);
	}

	public void Dispose()
	{
		_server?.Dispose();
	}

	internal void Enqueue(Connection connection, MemoryBuffer data)
	{
		lock (_messageQueue)
		{
			if (!App.update || _messageQueue.Count >= App.queuelimit)
			{
				data.Dispose();
				return;
			}
			Message item = new Message(connection, data);
			_messageQueue.Enqueue(item);
		}
	}

	public void Update()
	{
		if (!App.update)
		{
			return;
		}
		using (TimeWarning.New("CompanionServer.MessageQueue"))
		{
			lock (_messageQueue)
			{
				_stopwatch.Restart();
				while (_messageQueue.Count > 0 && _stopwatch.Elapsed.TotalMilliseconds < 5.0)
				{
					Message message = _messageQueue.Dequeue();
					Dispatch(message);
				}
			}
		}
		if ((float)_lastCleanup >= 3f)
		{
			_lastCleanup = 0f;
			_ipTokenBuckets.Cleanup();
			_ipBans.Cleanup();
			_playerTokenBuckets.Cleanup();
			_pairingTokenBuckets.Cleanup();
		}
	}

	private void Dispatch(Message message)
	{
		MemoryBuffer buffer = message.Buffer;
		AppRequest request;
		try
		{
			Stream.SetData(message.Buffer.Data, 0, message.Buffer.Length);
			request = AppRequest.Deserialize(Stream);
		}
		catch
		{
			DebugEx.LogWarning($"Malformed companion packet from {message.Connection.Address}");
			message.Connection.Close();
			throw;
		}
		finally
		{
			buffer.Dispose();
		}
		if (Handle<AppEmpty, Info>((AppRequest r) => r.getInfo, out var requestHandler2) || Handle<AppEmpty, CompanionServer.Handlers.Time>((AppRequest r) => r.getTime, out requestHandler2) || Handle<AppEmpty, Map>((AppRequest r) => r.getMap, out requestHandler2) || Handle<AppEmpty, TeamInfo>((AppRequest r) => r.getTeamInfo, out requestHandler2) || Handle<AppEmpty, TeamChat>((AppRequest r) => r.getTeamChat, out requestHandler2) || Handle<AppSendMessage, SendTeamChat>((AppRequest r) => r.sendTeamMessage, out requestHandler2) || Handle<AppEmpty, EntityInfo>((AppRequest r) => r.getEntityInfo, out requestHandler2) || Handle<AppSetEntityValue, SetEntityValue>((AppRequest r) => r.setEntityValue, out requestHandler2) || Handle<AppEmpty, CheckSubscription>((AppRequest r) => r.checkSubscription, out requestHandler2) || Handle<AppFlag, SetSubscription>((AppRequest r) => r.setSubscription, out requestHandler2) || Handle<AppEmpty, MapMarkers>((AppRequest r) => r.getMapMarkers, out requestHandler2) || Handle<AppPromoteToLeader, PromoteToLeader>((AppRequest r) => r.promoteToLeader, out requestHandler2) || Handle<AppCameraSubscribe, CameraSubscribe>((AppRequest r) => r.cameraSubscribe, out requestHandler2) || Handle<AppEmpty, CameraUnsubscribe>((AppRequest r) => r.cameraUnsubscribe, out requestHandler2) || Handle<AppCameraInput, CameraInput>((AppRequest r) => r.cameraInput, out requestHandler2))
		{
			try
			{
				ValidationResult validationResult = requestHandler2.Validate();
				switch (validationResult)
				{
				case ValidationResult.Rejected:
					message.Connection.Close();
					break;
				default:
					requestHandler2.SendError(Util.ToErrorCode(validationResult));
					break;
				case ValidationResult.Success:
					requestHandler2.Execute();
					break;
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"AppRequest threw an exception: {arg}");
				requestHandler2.SendError("server_error");
			}
			Facepunch.Pool.FreeDynamic(ref requestHandler2);
		}
		else
		{
			AppResponse appResponse = Facepunch.Pool.Get<AppResponse>();
			appResponse.seq = request.seq;
			appResponse.error = Facepunch.Pool.Get<AppError>();
			appResponse.error.error = "unhandled";
			message.Connection.Send(appResponse);
			request.Dispose();
		}
		bool Handle<TProto, THandler>(Func<AppRequest, TProto> protoSelector, out CompanionServer.Handlers.IHandler requestHandler) where TProto : class where THandler : BaseHandler<TProto>, new()
		{
			TProto val = protoSelector(request);
			if (val == null)
			{
				requestHandler = null;
				return false;
			}
			THandler val2 = Facepunch.Pool.Get<THandler>();
			val2.Initialize(_playerTokenBuckets, message.Connection, request, val);
			requestHandler = val2;
			return true;
		}
	}

	public void BroadcastTo(List<Connection> targets, AppBroadcast broadcast)
	{
		MemoryBuffer broadcastBuffer = GetBroadcastBuffer(broadcast);
		foreach (Connection target in targets)
		{
			target.SendRaw(broadcastBuffer.DontDispose());
		}
		broadcastBuffer.Dispose();
	}

	private static MemoryBuffer GetBroadcastBuffer(AppBroadcast broadcast)
	{
		MemoryBuffer memoryBuffer = new MemoryBuffer(65536);
		Stream.SetData(memoryBuffer.Data, 0, memoryBuffer.Length);
		AppMessage appMessage = Facepunch.Pool.Get<AppMessage>();
		appMessage.broadcast = broadcast;
		appMessage.ToProto(Stream);
		if (appMessage.ShouldPool)
		{
			appMessage.Dispose();
		}
		return memoryBuffer.Slice((int)Stream.Position);
	}

	public bool CanSendPairingNotification(ulong playerId)
	{
		return _pairingTokenBuckets.Get(playerId).TryTake(1.0);
	}
}
