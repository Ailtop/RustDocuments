using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Fleck;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer
{
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

		public readonly IPAddress Address;

		public readonly int Port;

		public readonly ConnectionLimiter Limiter;

		public readonly SubscriberList<PlayerTarget, Connection, AppBroadcast> PlayerSubscribers;

		public readonly SubscriberList<EntityTarget, Connection, AppBroadcast> EntitySubscribers;

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
					Connection conn = new Connection(this, socket);
					socket.OnClose = delegate
					{
						Limiter.Remove(address);
						conn.OnClose();
					};
					socket.OnBinary = conn.OnMessage;
					socket.OnError = UnityEngine.Debug.LogError;
				}
			});
			_stopwatch = new Stopwatch();
			PlayerSubscribers = new SubscriberList<PlayerTarget, Connection, AppBroadcast>(this);
			EntitySubscribers = new SubscriberList<EntityTarget, Connection, AppBroadcast>(this);
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
			_003C_003Ec__DisplayClass19_0 _003C_003Ec__DisplayClass19_ = default(_003C_003Ec__DisplayClass19_0);
			_003C_003Ec__DisplayClass19_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass19_.message = message;
			MemoryBuffer buffer = _003C_003Ec__DisplayClass19_.message.Buffer;
			try
			{
				Stream.SetData(_003C_003Ec__DisplayClass19_.message.Buffer.Data, 0, _003C_003Ec__DisplayClass19_.message.Buffer.Length);
				_003C_003Ec__DisplayClass19_.request = AppRequest.Deserialize(Stream);
			}
			catch
			{
				DebugEx.LogWarning($"Malformed companion packet from {_003C_003Ec__DisplayClass19_.message.Connection.Address}");
				_003C_003Ec__DisplayClass19_.message.Connection.Close();
				throw;
			}
			finally
			{
				buffer.Dispose();
			}
			CompanionServer.Handlers.IHandler requestHandler;
			if (_003CDispatch_003Eg__Handle_007C19_13<AppEmpty, Info>((AppRequest r) => r.getInfo, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppEmpty, CompanionServer.Handlers.Time>((AppRequest r) => r.getTime, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppEmpty, Map>((AppRequest r) => r.getMap, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppEmpty, TeamInfo>((AppRequest r) => r.getTeamInfo, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppEmpty, TeamChat>((AppRequest r) => r.getTeamChat, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppSendMessage, SendTeamChat>((AppRequest r) => r.sendTeamMessage, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppEmpty, EntityInfo>((AppRequest r) => r.getEntityInfo, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppSetEntityValue, SetEntityValue>((AppRequest r) => r.setEntityValue, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppEmpty, CheckSubscription>((AppRequest r) => r.checkSubscription, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppFlag, SetSubscription>((AppRequest r) => r.setSubscription, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppEmpty, MapMarkers>((AppRequest r) => r.getMapMarkers, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppCameraFrameRequest, CameraFrame>((AppRequest r) => r.getCameraFrame, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_13<AppPromoteToLeader, PromoteToLeader>((AppRequest r) => r.promoteToLeader, out requestHandler, ref _003C_003Ec__DisplayClass19_))
			{
				try
				{
					ValidationResult validationResult = requestHandler.Validate();
					switch (validationResult)
					{
					case ValidationResult.Rejected:
						_003C_003Ec__DisplayClass19_.message.Connection.Close();
						break;
					default:
						requestHandler.SendError(Util.ToErrorCode(validationResult));
						break;
					case ValidationResult.Success:
						requestHandler.Execute();
						break;
					}
				}
				catch (Exception arg)
				{
					UnityEngine.Debug.LogError($"AppRequest threw an exception: {arg}");
					requestHandler.SendError("server_error");
				}
				Facepunch.Pool.FreeDynamic(ref requestHandler);
			}
			else
			{
				AppResponse appResponse = Facepunch.Pool.Get<AppResponse>();
				appResponse.seq = _003C_003Ec__DisplayClass19_.request.seq;
				appResponse.error = Facepunch.Pool.Get<AppError>();
				appResponse.error.error = "unhandled";
				_003C_003Ec__DisplayClass19_.message.Connection.Send(appResponse);
				_003C_003Ec__DisplayClass19_.request.Dispose();
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
}
