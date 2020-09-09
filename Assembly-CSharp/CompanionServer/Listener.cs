using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Fleck;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass15_0
		{
			public IPAddress address;

			public Connection conn;

			public Listener _003C_003E4__this;

			internal void _003C_002Ector_003Eb__1()
			{
				_003C_003E4__this.Limiter.Remove(address);
				conn.OnClose();
			}
		}

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003C_003Ec__DisplayClass19_0
		{
			public AppRequest request;

			public Listener _003C_003E4__this;

			public Message message;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Func<AppRequest, AppEmpty> _003C_003E9__19_0;

			public static Func<AppRequest, AppEmpty> _003C_003E9__19_1;

			public static Func<AppRequest, AppEmpty> _003C_003E9__19_2;

			public static Func<AppRequest, AppEmpty> _003C_003E9__19_3;

			public static Func<AppRequest, AppEmpty> _003C_003E9__19_4;

			public static Func<AppRequest, AppSendMessage> _003C_003E9__19_5;

			public static Func<AppRequest, AppEmpty> _003C_003E9__19_6;

			public static Func<AppRequest, AppSetEntityValue> _003C_003E9__19_7;

			public static Func<AppRequest, AppEmpty> _003C_003E9__19_8;

			public static Func<AppRequest, AppFlag> _003C_003E9__19_9;

			public static Func<AppRequest, AppEmpty> _003C_003E9__19_10;

			public static Func<AppRequest, AppCameraFrameRequest> _003C_003E9__19_11;

			internal AppEmpty _003CDispatch_003Eb__19_0(AppRequest r)
			{
				return r.getInfo;
			}

			internal AppEmpty _003CDispatch_003Eb__19_1(AppRequest r)
			{
				return r.getTime;
			}

			internal AppEmpty _003CDispatch_003Eb__19_2(AppRequest r)
			{
				return r.getMap;
			}

			internal AppEmpty _003CDispatch_003Eb__19_3(AppRequest r)
			{
				return r.getTeamInfo;
			}

			internal AppEmpty _003CDispatch_003Eb__19_4(AppRequest r)
			{
				return r.getTeamChat;
			}

			internal AppSendMessage _003CDispatch_003Eb__19_5(AppRequest r)
			{
				return r.sendTeamMessage;
			}

			internal AppEmpty _003CDispatch_003Eb__19_6(AppRequest r)
			{
				return r.getEntityInfo;
			}

			internal AppSetEntityValue _003CDispatch_003Eb__19_7(AppRequest r)
			{
				return r.setEntityValue;
			}

			internal AppEmpty _003CDispatch_003Eb__19_8(AppRequest r)
			{
				return r.checkSubscription;
			}

			internal AppFlag _003CDispatch_003Eb__19_9(AppRequest r)
			{
				return r.setSubscription;
			}

			internal AppEmpty _003CDispatch_003Eb__19_10(AppRequest r)
			{
				return r.getMapMarkers;
			}

			internal AppCameraFrameRequest _003CDispatch_003Eb__19_11(AppRequest r)
			{
				return r.getCameraFrame;
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
				Listener listener = this;
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
						listener.Limiter.Remove(address);
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
				}
				else
				{
					Message item = new Message(connection, data);
					_messageQueue.Enqueue(item);
				}
			}
		}

		public void Update()
		{
			if (App.update)
			{
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
			finally
			{
				buffer.Dispose();
			}
			CompanionServer.Handlers.IHandler requestHandler;
			if (_003CDispatch_003Eg__Handle_007C19_12<AppEmpty, Info>((AppRequest r) => r.getInfo, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppEmpty, CompanionServer.Handlers.Time>((AppRequest r) => r.getTime, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppEmpty, Map>((AppRequest r) => r.getMap, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppEmpty, TeamInfo>((AppRequest r) => r.getTeamInfo, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppEmpty, TeamChat>((AppRequest r) => r.getTeamChat, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppSendMessage, SendTeamChat>((AppRequest r) => r.sendTeamMessage, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppEmpty, EntityInfo>((AppRequest r) => r.getEntityInfo, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppSetEntityValue, SetEntityValue>((AppRequest r) => r.setEntityValue, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppEmpty, CheckSubscription>((AppRequest r) => r.checkSubscription, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppFlag, SetSubscription>((AppRequest r) => r.setSubscription, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppEmpty, MapMarkers>((AppRequest r) => r.getMapMarkers, out requestHandler, ref _003C_003Ec__DisplayClass19_) || _003CDispatch_003Eg__Handle_007C19_12<AppCameraFrameRequest, CameraFrame>((AppRequest r) => r.getCameraFrame, out requestHandler, ref _003C_003Ec__DisplayClass19_))
			{
				try
				{
					ValidationResult validationResult = requestHandler.Validate();
					if (validationResult != 0)
					{
						requestHandler.SendError(Util.ToErrorCode(validationResult));
					}
					else
					{
						requestHandler.Execute();
					}
				}
				catch (Exception arg)
				{
					UnityEngine.Debug.LogError($"AppRequest threw an exception: {arg}");
					requestHandler.SendError("server_error");
				}
				Facepunch.Pool.FreeDynamic(ref requestHandler);
				return;
			}
			AppResponse appResponse = Facepunch.Pool.Get<AppResponse>();
			appResponse.seq = _003C_003Ec__DisplayClass19_.request.seq;
			appResponse.error = Facepunch.Pool.Get<AppError>();
			appResponse.error.error = "unhandled";
			_003C_003Ec__DisplayClass19_.message.Connection.Send(appResponse);
			_003C_003Ec__DisplayClass19_.request.Dispose();
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

		[CompilerGenerated]
		private bool _003CDispatch_003Eg__Handle_007C19_12<TProto, THandler>(Func<AppRequest, TProto> protoSelector, out CompanionServer.Handlers.IHandler requestHandler, ref _003C_003Ec__DisplayClass19_0 P_2) where TProto : class where THandler : BaseHandler<TProto>, new()
		{
			TProto val = protoSelector(P_2.request);
			if (val == null)
			{
				requestHandler = null;
				return false;
			}
			THandler val2 = Facepunch.Pool.Get<THandler>();
			val2.Initialize(_playerTokenBuckets, P_2.message.Connection, P_2.request, val);
			requestHandler = val2;
			return true;
		}
	}
}
