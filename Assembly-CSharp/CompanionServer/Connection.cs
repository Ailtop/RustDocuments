using CCTVRender;
using ConVar;
using Facepunch;
using Fleck;
using ProtoBuf;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace CompanionServer
{
	public class Connection : IConnection, IReceiver
	{
		private static readonly MemoryStream MessageStream = new MemoryStream(1048576);

		private readonly Listener _listener;

		private readonly IWebSocketConnection _connection;

		private readonly HashSet<PlayerTarget> _subscribedPlayers;

		private readonly HashSet<EntityTarget> _subscribedEntities;

		public IPAddress Address => _connection.ConnectionInfo.ClientIpAddress;

		public Connection(Listener listener, IWebSocketConnection connection)
		{
			_listener = listener;
			_connection = connection;
			_subscribedPlayers = new HashSet<PlayerTarget>();
			_subscribedEntities = new HashSet<EntityTarget>();
		}

		public void OnClose()
		{
			foreach (PlayerTarget subscribedPlayer in _subscribedPlayers)
			{
				_listener.PlayerSubscribers.Remove(subscribedPlayer, this);
			}
			foreach (EntityTarget subscribedEntity in _subscribedEntities)
			{
				_listener.EntitySubscribers.Remove(subscribedEntity, this);
			}
		}

		public void OnMessage(Span<byte> data)
		{
			if (App.update && App.queuelimit > 0)
			{
				MemoryBuffer buffer = new MemoryBuffer(data.Length);
				data.CopyTo(buffer);
				_listener.Enqueue(this, buffer.Slice(data.Length));
			}
		}

		public void Close()
		{
			_connection?.Close();
		}

		public void Send(AppResponse response)
		{
			AppMessage appMessage = Facepunch.Pool.Get<AppMessage>();
			appMessage.response = response;
			MessageStream.Position = 0L;
			appMessage.ToProto(MessageStream);
			int num = (int)MessageStream.Position;
			MessageStream.Position = 0L;
			MemoryBuffer memoryBuffer = new MemoryBuffer(num);
			MessageStream.Read(memoryBuffer.Data, 0, num);
			if (appMessage.ShouldPool)
			{
				appMessage.Dispose();
			}
			SendRaw(memoryBuffer.Slice(num));
		}

		public void Subscribe(PlayerTarget target)
		{
			if (_subscribedPlayers.Add(target))
			{
				_listener.PlayerSubscribers.Add(target, this);
			}
		}

		public void Unsubscribe(PlayerTarget target)
		{
			if (_subscribedPlayers.Remove(target))
			{
				_listener.PlayerSubscribers.Remove(target, this);
			}
		}

		public void Subscribe(EntityTarget target)
		{
			if (_subscribedEntities.Add(target))
			{
				_listener.EntitySubscribers.Add(target, this);
			}
		}

		public void Unsubscribe(EntityTarget target)
		{
			if (_subscribedEntities.Remove(target))
			{
				_listener.EntitySubscribers.Remove(target, this);
			}
		}

		public void SendRaw(MemoryBuffer data)
		{
			try
			{
				_connection.Send(data);
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed to send message to app client {_connection.ConnectionInfo.ClientIpAddress}: {arg}");
			}
		}

		public void RenderCompleted(uint requestId, uint frame, Span<byte> jpgImage)
		{
			byte[] array = ArrayPool<byte>.Shared.Rent(jpgImage.Length);
			jpgImage.CopyTo(MemoryExtensions.AsSpan(array));
			ArraySegment<byte> jpgImage2 = new ArraySegment<byte>(array, 0, jpgImage.Length);
			AppResponse appResponse = Facepunch.Pool.Get<AppResponse>();
			appResponse.seq = requestId;
			appResponse.cameraFrame = Facepunch.Pool.Get<AppCameraFrame>();
			appResponse.cameraFrame.frame = frame;
			appResponse.cameraFrame.jpgImage = jpgImage2;
			Send(appResponse);
		}
	}
}
