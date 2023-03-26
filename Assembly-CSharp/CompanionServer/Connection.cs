using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ConVar;
using Facepunch;
using Fleck;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public class Connection : IConnection
{
	private static readonly MemoryStream MessageStream = new MemoryStream(1048576);

	private readonly Listener _listener;

	private readonly IWebSocketConnection _connection;

	private PlayerTarget? _subscribedPlayer;

	private readonly HashSet<EntityTarget> _subscribedEntities;

	private IRemoteControllable _currentCamera;

	private ulong _cameraViewerSteamId;

	private bool _isControllingCamera;

	public long ConnectionId { get; private set; }

	public IPAddress Address => _connection.ConnectionInfo.ClientIpAddress;

	public IRemoteControllable CurrentCamera => _currentCamera;

	public bool IsControllingCamera => _isControllingCamera;

	public ulong ControllingSteamId => _cameraViewerSteamId;

	public InputState InputState { get; set; }

	public Connection(long connectionId, Listener listener, IWebSocketConnection connection)
	{
		ConnectionId = connectionId;
		_listener = listener;
		_connection = connection;
		_subscribedEntities = new HashSet<EntityTarget>();
	}

	public void OnClose()
	{
		if (_subscribedPlayer.HasValue)
		{
			_listener.PlayerSubscribers.Remove(_subscribedPlayer.Value, this);
			_subscribedPlayer = null;
		}
		foreach (EntityTarget subscribedEntity in _subscribedEntities)
		{
			_listener.EntitySubscribers.Remove(subscribedEntity, this);
		}
		_subscribedEntities.Clear();
		_currentCamera?.StopControl(new CameraViewerId(_cameraViewerSteamId, ConnectionId));
		if (TryGetCameraTarget(_currentCamera, out var target))
		{
			_listener.CameraSubscribers.Remove(target, this);
		}
		_currentCamera = null;
		_cameraViewerSteamId = 0uL;
		_isControllingCamera = false;
	}

	public void OnMessage(Span<byte> data)
	{
		if (App.update && App.queuelimit > 0)
		{
			MemoryBuffer memoryBuffer = new MemoryBuffer(data.Length);
			data.CopyTo(memoryBuffer);
			_listener.Enqueue(this, memoryBuffer.Slice(data.Length));
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
		if (!(_subscribedPlayer == target))
		{
			EndViewing();
			if (_subscribedPlayer.HasValue)
			{
				_listener.PlayerSubscribers.Remove(_subscribedPlayer.Value, this);
				_subscribedPlayer = null;
			}
			_listener.PlayerSubscribers.Add(target, this);
			_subscribedPlayer = target;
		}
	}

	public void Subscribe(EntityTarget target)
	{
		if (_subscribedEntities.Add(target))
		{
			_listener.EntitySubscribers.Add(target, this);
		}
	}

	public bool BeginViewing(IRemoteControllable camera)
	{
		if (!_subscribedPlayer.HasValue)
		{
			return false;
		}
		if (!TryGetCameraTarget(camera, out var target))
		{
			if (_currentCamera == camera)
			{
				_currentCamera?.StopControl(new CameraViewerId(_cameraViewerSteamId, ConnectionId));
				_currentCamera = null;
				_isControllingCamera = false;
				_cameraViewerSteamId = 0uL;
			}
			return false;
		}
		if (_currentCamera == camera)
		{
			_listener.CameraSubscribers.Add(target, this);
			return true;
		}
		if (TryGetCameraTarget(_currentCamera, out var target2))
		{
			_listener.CameraSubscribers.Remove(target2, this);
			_currentCamera.StopControl(new CameraViewerId(_cameraViewerSteamId, ConnectionId));
			_currentCamera = null;
			_isControllingCamera = false;
			_cameraViewerSteamId = 0uL;
		}
		ulong steamId = _subscribedPlayer.Value.SteamId;
		if (!camera.CanControl(steamId))
		{
			return false;
		}
		_listener.CameraSubscribers.Add(target, this);
		_currentCamera = camera;
		_isControllingCamera = _currentCamera.InitializeControl(new CameraViewerId(steamId, ConnectionId));
		_cameraViewerSteamId = steamId;
		InputState?.Clear();
		return true;
	}

	public void EndViewing()
	{
		if (TryGetCameraTarget(_currentCamera, out var target))
		{
			_listener.CameraSubscribers.Remove(target, this);
		}
		_currentCamera?.StopControl(new CameraViewerId(_cameraViewerSteamId, ConnectionId));
		_currentCamera = null;
		_isControllingCamera = false;
		_cameraViewerSteamId = 0uL;
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

	private static bool TryGetCameraTarget(IRemoteControllable camera, out CameraTarget target)
	{
		BaseEntity baseEntity = camera?.GetEnt();
		if (ObjectEx.IsUnityNull(camera) || baseEntity == null || !BaseNetworkableEx.IsValid(baseEntity))
		{
			target = default(CameraTarget);
			return false;
		}
		target = new CameraTarget(baseEntity.net.ID);
		return true;
	}
}
