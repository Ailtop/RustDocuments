using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Cameras;

public class CameraRenderer : Pool.IPooled
{
	[ServerVar]
	public static bool enabled = true;

	[ServerVar]
	public static float completionFrameBudgetMs = 5f;

	[ServerVar]
	public static int maxRendersPerFrame = 25;

	[ServerVar]
	public static int maxRaysPerFrame = 100000;

	[ServerVar]
	public static int width = 160;

	[ServerVar]
	public static int height = 90;

	[ServerVar]
	public static float verticalFov = 65f;

	[ServerVar]
	public static float nearPlane = 0f;

	[ServerVar]
	public static float farPlane = 250f;

	[ServerVar]
	public static int layerMask = 1218656529;

	[ServerVar]
	public static float renderInterval = 0.05f;

	[ServerVar]
	public static int samplesPerRender = 3000;

	[ServerVar]
	public static int entityMaxAge = 5;

	[ServerVar]
	public static int entityMaxDistance = 100;

	[ServerVar]
	public static int playerMaxDistance = 30;

	[ServerVar]
	public static int playerNameMaxDistance = 10;

	private static readonly Dictionary<NetworkableId, NetworkableId> _entityIdMap = new Dictionary<NetworkableId, NetworkableId>();

	private readonly Dictionary<int, (byte MaterialIndex, int Age)> _knownColliders = new Dictionary<int, (byte, int)>();

	private readonly Dictionary<int, BaseEntity> _colliderToEntity = new Dictionary<int, BaseEntity>();

	private double _lastRenderTimestamp;

	private float _fieldOfView;

	private int _sampleOffset;

	private int _nextSampleOffset;

	private int _sampleCount;

	private CameraRenderTask _task;

	private ulong? _cachedViewerSteamId;

	private BasePlayer _cachedViewer;

	public CameraRendererState state;

	public IRemoteControllable rc;

	public BaseEntity entity;

	public CameraRenderer()
	{
		Reset();
	}

	public void EnterPool()
	{
		Reset();
	}

	public void LeavePool()
	{
	}

	public void Reset()
	{
		_knownColliders.Clear();
		_colliderToEntity.Clear();
		_lastRenderTimestamp = 0.0;
		_fieldOfView = 0f;
		_sampleOffset = 0;
		_nextSampleOffset = 0;
		_sampleCount = 0;
		if (_task != null)
		{
			CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
			if (instance != null)
			{
				instance.ReturnTask(ref _task);
			}
		}
		_cachedViewerSteamId = null;
		_cachedViewer = null;
		state = CameraRendererState.Invalid;
		rc = null;
		entity = null;
	}

	public void Init(IRemoteControllable remoteControllable)
	{
		if (remoteControllable == null)
		{
			throw new ArgumentNullException("remoteControllable");
		}
		rc = remoteControllable;
		entity = remoteControllable.GetEnt();
		if (entity == null || !BaseNetworkableEx.IsValid(entity))
		{
			throw new ArgumentException("RemoteControllable's entity is null or invalid", "rc");
		}
		state = CameraRendererState.WaitingToRender;
	}

	public bool CanRender()
	{
		if (state != CameraRendererState.WaitingToRender)
		{
			return false;
		}
		if (TimeEx.realtimeSinceStartup - _lastRenderTimestamp < (double)renderInterval)
		{
			return false;
		}
		return true;
	}

	public void Render(int maxSampleCount)
	{
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if (instance == null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (state != CameraRendererState.WaitingToRender)
		{
			throw new InvalidOperationException($"CameraRenderer cannot render in state {state}");
		}
		if (ObjectEx.IsUnityNull(rc) || !BaseNetworkableEx.IsValid(entity))
		{
			state = CameraRendererState.Invalid;
			return;
		}
		Transform eyes = rc.GetEyes();
		if (eyes == null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (_task != null)
		{
			Debug.LogError("CameraRenderer: Trying to render but a task is already allocated?", entity);
			instance.ReturnTask(ref _task);
		}
		_fieldOfView = verticalFov / Mathf.Clamp(rc.GetFovScale(), 1f, 8f);
		_sampleCount = Mathf.Clamp(samplesPerRender, 1, Mathf.Min(width * height, maxSampleCount));
		_task = instance.BorrowTask();
		_nextSampleOffset = _task.Start(width, height, _fieldOfView, nearPlane, farPlane, layerMask, eyes, _sampleCount, _sampleOffset, _knownColliders);
		state = CameraRendererState.Rendering;
	}

	public void CompleteRender()
	{
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if (instance == null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (state != CameraRendererState.Rendering)
		{
			throw new InvalidOperationException($"CameraRenderer cannot complete render in state {state}");
		}
		if (_task == null)
		{
			Debug.LogError("CameraRenderer: Trying to complete render but no task is allocated?", this.entity);
			state = CameraRendererState.Invalid;
		}
		else
		{
			if (_task.keepWaiting)
			{
				return;
			}
			if (ObjectEx.IsUnityNull(rc) || !BaseNetworkableEx.IsValid(this.entity))
			{
				instance.ReturnTask(ref _task);
				state = CameraRendererState.Invalid;
				return;
			}
			Transform eyes = rc.GetEyes();
			if (eyes == null)
			{
				instance.ReturnTask(ref _task);
				state = CameraRendererState.Invalid;
				return;
			}
			int minimumLength = _sampleCount * 4;
			byte[] array = System.Buffers.ArrayPool<byte>.Shared.Rent(minimumLength);
			List<int> obj = Pool.GetList<int>();
			List<int> obj2 = Pool.GetList<int>();
			int count = _task.ExtractRayData(array, obj, obj2);
			instance.ReturnTask(ref _task);
			UpdateCollidersMap(obj2);
			Pool.FreeList(ref obj);
			Pool.FreeList(ref obj2);
			ulong num = rc.ControllingViewerId?.SteamId ?? 0;
			if (num == 0L)
			{
				_cachedViewerSteamId = null;
				_cachedViewer = null;
			}
			else if (num != _cachedViewerSteamId)
			{
				_cachedViewerSteamId = num;
				_cachedViewer = BasePlayer.FindByID(num) ?? BasePlayer.FindSleeping(num);
			}
			float distance = (BaseNetworkableEx.IsValid(_cachedViewer) ? Mathf.Clamp01(Vector3.Distance(_cachedViewer.transform.position, this.entity.transform.position) / rc.MaxRange) : 0f);
			Vector3 position = eyes.position;
			Quaternion rotation = eyes.rotation;
			Matrix4x4 worldToLocalMatrix = eyes.worldToLocalMatrix;
			NetworkableId iD = this.entity.net.ID;
			_entityIdMap.Clear();
			AppBroadcast appBroadcast = Pool.Get<AppBroadcast>();
			appBroadcast.cameraRays = Pool.Get<AppCameraRays>();
			appBroadcast.cameraRays.verticalFov = _fieldOfView;
			appBroadcast.cameraRays.sampleOffset = _sampleOffset;
			appBroadcast.cameraRays.rayData = new ArraySegment<byte>(array, 0, count);
			appBroadcast.cameraRays.distance = distance;
			appBroadcast.cameraRays.entities = Pool.GetList<AppCameraRays.Entity>();
			appBroadcast.cameraRays.timeOfDay = ((TOD_Sky.Instance != null) ? TOD_Sky.Instance.LerpValue : 1f);
			foreach (BaseEntity value in _colliderToEntity.Values)
			{
				if (!BaseNetworkableEx.IsValid(value))
				{
					continue;
				}
				Vector3 position2 = value.transform.position;
				float num2 = Vector3.Distance(position2, position);
				if (num2 > (float)entityMaxDistance)
				{
					continue;
				}
				string name = null;
				if (value is BasePlayer basePlayer)
				{
					if (num2 > (float)playerMaxDistance)
					{
						continue;
					}
					if (num2 <= (float)playerNameMaxDistance)
					{
						name = basePlayer.displayName;
					}
				}
				AppCameraRays.Entity entity = Pool.Get<AppCameraRays.Entity>();
				entity.entityId = RandomizeEntityId(value.net.ID);
				entity.type = ((value is TreeEntity) ? AppCameraRays.EntityType.Tree : AppCameraRays.EntityType.Player);
				entity.position = worldToLocalMatrix.MultiplyPoint3x4(position2);
				entity.rotation = (Quaternion.Inverse(value.transform.rotation) * rotation).eulerAngles * (MathF.PI / 180f);
				entity.size = Vector3.Scale(value.bounds.size, value.transform.localScale);
				entity.name = name;
				appBroadcast.cameraRays.entities.Add(entity);
			}
			appBroadcast.cameraRays.entities.Sort((AppCameraRays.Entity x, AppCameraRays.Entity y) => x.entityId.Value.CompareTo(y.entityId.Value));
			Server.Broadcast(new CameraTarget(iD), appBroadcast);
			_sampleOffset = _nextSampleOffset;
			if (!Server.HasAnySubscribers(new CameraTarget(iD)))
			{
				state = CameraRendererState.Invalid;
				return;
			}
			_lastRenderTimestamp = TimeEx.realtimeSinceStartup;
			state = CameraRendererState.WaitingToRender;
		}
	}

	private void UpdateCollidersMap(List<int> foundColliderIds)
	{
		List<int> obj = Pool.GetList<int>();
		foreach (int key in _knownColliders.Keys)
		{
			obj.Add(key);
		}
		List<int> obj2 = Pool.GetList<int>();
		foreach (int item2 in obj)
		{
			if (_knownColliders.TryGetValue(item2, out (byte, int) value))
			{
				if (value.Item2 > entityMaxAge)
				{
					obj2.Add(item2);
				}
				else
				{
					_knownColliders[item2] = (value.Item1, value.Item2 + 1);
				}
			}
		}
		Pool.FreeList(ref obj);
		foreach (int item3 in obj2)
		{
			_knownColliders.Remove(item3);
			_colliderToEntity.Remove(item3);
		}
		Pool.FreeList(ref obj2);
		foreach (int foundColliderId in foundColliderIds)
		{
			if (_knownColliders.Count >= 512)
			{
				break;
			}
			Collider collider = CompanionServer.Cameras.BurstUtil.GetCollider(foundColliderId);
			if (collider == null)
			{
				continue;
			}
			byte item;
			if (collider is TerrainCollider)
			{
				item = 1;
			}
			else
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
				item = GetMaterialIndex(collider.sharedMaterial, baseEntity);
				if (baseEntity is TreeEntity || baseEntity is BasePlayer)
				{
					_colliderToEntity[foundColliderId] = baseEntity;
				}
			}
			_knownColliders[foundColliderId] = (item, 0);
		}
	}

	private static NetworkableId RandomizeEntityId(NetworkableId realId)
	{
		if (_entityIdMap.TryGetValue(realId, out var value))
		{
			return value;
		}
		NetworkableId networkableId;
		do
		{
			networkableId = new NetworkableId((ulong)UnityEngine.Random.Range(0, 2500));
		}
		while (_entityIdMap.ContainsKey(networkableId));
		_entityIdMap.Add(realId, networkableId);
		return networkableId;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetMaterialIndex(PhysicMaterial material, BaseEntity entity)
	{
		switch (AssetNameCache.GetName(material))
		{
		case "Water":
			return 2;
		case "Rock":
			return 3;
		case "Stones":
			return 4;
		case "Wood":
			return 5;
		case "Metal":
			return 6;
		default:
			if (entity != null && entity is BasePlayer)
			{
				return 7;
			}
			return 0;
		}
	}
}
