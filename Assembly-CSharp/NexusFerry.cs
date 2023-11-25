using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Facepunch.Extend;
using ProtoBuf;
using ProtoBuf.Nexus;
using Rust;
using Rust.UI;
using UnityEngine;

public class NexusFerry : BaseEntity
{
	public enum State
	{
		Invalid = 0,
		SailingIn = 1,
		Queued = 2,
		Arrival = 3,
		Docking = 4,
		Stopping = 5,
		Waiting = 6,
		CastingOff = 7,
		Departure = 8,
		SailingOut = 9,
		Transferring = 10
	}

	private readonly struct Edge
	{
		public readonly Node Next;

		public readonly float Distance;

		public Edge(Node next, float distance)
		{
			Next = next;
			Distance = distance;
		}
	}

	private class Node : IComparable<Node>, Pool.IPooled
	{
		public int Index;

		public Vector3 Position;

		public readonly List<Edge> Edges;

		public Node Parent;

		public float G;

		public float H;

		public float F => G + H;

		public Node()
		{
			Edges = new List<Edge>();
		}

		public Node ConnectTo(Node other)
		{
			float distance = Vector3.Distance(Position, other.Position);
			Edges.Add(new Edge(other, distance));
			other.Edges.Add(new Edge(this, distance));
			return this;
		}

		public void Reset()
		{
			Parent = null;
			G = 0f;
			H = 0f;
		}

		public int CompareTo(Node other)
		{
			if (this == other || Index == other.Index)
			{
				return 0;
			}
			if (!(F < other.F))
			{
				return -1;
			}
			return 1;
		}

		public void EnterPool()
		{
			Index = 0;
			Position = Vector3.zero;
			Edges.Clear();
			Reset();
		}

		public void LeavePool()
		{
		}
	}

	private class Graph : Pool.IPooled
	{
		private readonly List<Node> _nodes;

		public Graph()
		{
			_nodes = new List<Node>();
		}

		public Node AddNode(Vector3 position)
		{
			Node node = Pool.Get<Node>();
			node.Index = _nodes.Count;
			node.Position = position;
			_nodes.Add(node);
			return node;
		}

		public Node FindClosest(Vector3 position)
		{
			float num = float.MaxValue;
			Node result = null;
			foreach (Node node in _nodes)
			{
				float num2 = Vector3.Distance(node.Position, position);
				if (!(num2 >= num))
				{
					num = num2;
					result = node;
				}
			}
			return result;
		}

		public bool TryFindPath(Node start, Node end, List<Vector3> path)
		{
			foreach (Node node3 in _nodes)
			{
				node3.Reset();
			}
			bool[] array = System.Buffers.ArrayPool<bool>.Shared.Rent(_nodes.Count);
			Array.Clear(array, 0, array.Length);
			List<Node> obj = Pool.GetList<Node>();
			obj.Add(start);
			while (obj.Count > 0)
			{
				int index = obj.Count - 1;
				Node node = obj[index];
				obj.RemoveAt(index);
				array[node.Index] = true;
				if (node == end)
				{
					System.Buffers.ArrayPool<bool>.Shared.Return(array);
					Pool.FreeList(ref obj);
					path.Clear();
					for (Node node2 = node; node2 != null; node2 = node2.Parent)
					{
						path.Add(node2.Position);
						if (path.Count > _nodes.Count)
						{
							Debug.LogError("Pathfinding code is broken!");
							path.Clear();
							return false;
						}
					}
					path.Reverse();
					return true;
				}
				foreach (Edge edge in node.Edges)
				{
					Node next = edge.Next;
					if (!array[next.Index])
					{
						float num = node.G + edge.Distance;
						if (next.Parent == null)
						{
							next.Parent = node;
							next.G = num;
							next.H = Vector3.Distance(next.Position, end.Position);
						}
						else if (num < next.G)
						{
							next.Parent = node;
							next.G = num;
						}
						int num2 = obj.BinarySearch(next);
						if (num2 < 0)
						{
							obj.Insert(~num2, next);
						}
					}
				}
			}
			System.Buffers.ArrayPool<bool>.Shared.Return(array);
			Pool.FreeList(ref obj);
			path.Clear();
			return false;
		}

		public void EnterPool()
		{
			foreach (Node node in _nodes)
			{
				Node obj = node;
				Pool.Free(ref obj);
			}
			_nodes.Clear();
		}

		public void LeavePool()
		{
		}
	}

	[Header("NexusFerry")]
	public float TravelVelocity = 20f;

	public float ApproachVelocity = 5f;

	public float StoppingVelocity = 1f;

	public float AccelerationSpeed = 1f;

	public float TurnSpeed = 1f;

	public float VelocityPreservationOnTurn = 0.1f;

	public float TargetDistanceThreshold = 10f;

	public GameObjectRef hornEffect;

	public Transform hornEffectTransform;

	public float departureHornLeadTime = 5f;

	[Header("Pathing")]
	public SphereCollider SphereCaster;

	public int CastSweepDegrees = 16;

	[Range(0f, 1f)]
	public float CastSweepNoise = 0.25f;

	public LayerMask CastLayers = 134283264;

	public float CastInterval = 1f;

	public float CastHitProtection = 5f;

	public int PathLookahead = 4;

	public int PathLookaheadThreshold = 5;

	[Header("UI")]
	public RustText[] NextZoneLabels;

	private long _timestamp;

	private string _ownerZone;

	private List<string> _schedule;

	private int _scheduleIndex;

	private State _state;

	private bool _isRetiring;

	private int _nextScheduleIndex;

	private bool _departureHornPlayed;

	public static readonly ListHashSet<NexusFerry> All = new ListHashSet<NexusFerry>();

	private List<NetworkableId> _transferredIds;

	private NexusDock _targetDock;

	private bool _isTransferring;

	private TimeSince _sinceStartedWaiting;

	private TimeSince _sinceLastTransferAttempt;

	private RealTimeSince _sinceLastNextIndexUpdate;

	private TimeSince _sincePathCalculation;

	private Vector3? _pathTargetPosition;

	private Quaternion? _pathTargetRotation;

	private Vector3 _velocity;

	public string OwnerZone => _ownerZone;

	public bool IsRetiring => _isRetiring;

	public string NextZone
	{
		get
		{
			int? num = TryGetNextScheduleIndex();
			if (!num.HasValue)
			{
				return null;
			}
			return _schedule[num.Value];
		}
	}

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	public void Initialize(string ownerZone, List<string> schedule)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(ownerZone))
			{
				throw new ArgumentNullException("ownerZone");
			}
			if (schedule == null)
			{
				throw new ArgumentNullException("schedule");
			}
			if (schedule.Count <= 1 || !schedule.Contains(ownerZone, StringComparer.InvariantCultureIgnoreCase))
			{
				throw new ArgumentException("Ferry schedule is invalid", "schedule");
			}
			_timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			_ownerZone = ownerZone;
			_schedule = schedule;
			_scheduleIndex = schedule.FindIndex(ownerZone, StringComparer.InvariantCultureIgnoreCase);
			_state = State.Stopping;
			_departureHornPlayed = false;
			if (_scheduleIndex < 0)
			{
				throw new InvalidOperationException("Ferry couldn't find the owner zone in its schedule");
			}
			EnsureInitialized();
			Transform targetTransform = GetTargetTransform(_state);
			base.transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
		}
		catch
		{
			Kill();
			throw;
		}
	}

	private void EnsureInitialized()
	{
		_targetDock = SingletonComponent<NexusDock>.Instance;
		if (_targetDock == null)
		{
			throw new InvalidOperationException("Ferry has no dock to go to!");
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave)
		{
			if (!NexusServer.Started)
			{
				Debug.LogError("NexusFerry will not work without being connected to a nexus - destroying.");
				Kill();
				return;
			}
			if (string.IsNullOrWhiteSpace(_ownerZone) || _schedule == null || _schedule.Count <= 1 || !_schedule.Contains(_ownerZone))
			{
				Debug.LogError("NexusFerry has not been initialized (you can't spawn them manually) - destroying.");
				Kill();
				return;
			}
		}
		EnsureInitialized();
		All.Add(this);
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (base.isServer)
		{
			All.Remove(this);
		}
		if (_transferredIds != null)
		{
			Pool.FreeList(ref _transferredIds);
		}
	}

	public void FixedUpdate()
	{
		if (!base.isServer)
		{
			return;
		}
		if ((float)_sinceLastNextIndexUpdate > 10f)
		{
			_sinceLastNextIndexUpdate = 0f;
			int num = TryGetNextScheduleIndex() ?? (-1);
			if (num != _nextScheduleIndex)
			{
				_nextScheduleIndex = num;
				SendNetworkUpdate();
			}
		}
		if (_state == State.Waiting)
		{
			EnsureInitialized();
			if (!_departureHornPlayed && _targetDock.WaitTime - (float)_sinceStartedWaiting < departureHornLeadTime)
			{
				PlayDepartureHornEffect();
			}
			if (!((float)_sinceStartedWaiting >= _targetDock.WaitTime))
			{
				return;
			}
			SwitchToNextState();
		}
		if (MoveTowardsTarget())
		{
			SwitchToNextState();
		}
	}

	public FerryStatus GetStatus()
	{
		FerryStatus ferryStatus = Pool.Get<FerryStatus>();
		ferryStatus.entityId = net.ID;
		ferryStatus.timestamp = _timestamp;
		ferryStatus.ownerZone = _ownerZone;
		ferryStatus.schedule = _schedule.ShallowClonePooled();
		ferryStatus.scheduleIndex = _scheduleIndex;
		ferryStatus.state = (int)_state;
		ferryStatus.isRetiring = _isRetiring;
		return ferryStatus;
	}

	public void Retire()
	{
		_isRetiring = true;
	}

	public void UpdateSchedule(List<string> schedule)
	{
		if (_schedule != null)
		{
			Pool.FreeList(ref _schedule);
		}
		_schedule = schedule.ShallowClonePooled();
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	private void SwitchToNextState()
	{
		if (_state == State.SailingOut)
		{
			if (!_isTransferring && (float)_sinceLastTransferAttempt >= 5f)
			{
				_sinceLastTransferAttempt = 0f;
				TransferToNextZone();
			}
			return;
		}
		if (_state == State.Departure && _targetDock != null)
		{
			_targetDock.Depart(this);
		}
		State nextState = GetNextState(_state);
		_state = nextState;
		SendNetworkUpdate();
		if (_state == State.Waiting)
		{
			_sinceStartedWaiting = 0f;
			_departureHornPlayed = false;
		}
		if (_state == State.CastingOff)
		{
			EjectInactiveEntities(_isRetiring);
			if (_isRetiring)
			{
				Kill();
			}
		}
	}

	private static State GetNextState(State currentState)
	{
		State state = currentState + 1;
		if (state >= State.SailingOut)
		{
			state = State.SailingOut;
		}
		return state;
	}

	private static State GetPreviousState(State currentState)
	{
		if ((uint)currentState <= 3u || (uint)(currentState - 9) <= 1u)
		{
			return State.Invalid;
		}
		return currentState - 1;
	}

	private async void TransferToNextZone()
	{
		if (_isTransferring)
		{
			return;
		}
		int? num = TryGetNextScheduleIndex();
		if (!num.HasValue)
		{
			return;
		}
		_isTransferring = true;
		int oldScheduleIndex = _scheduleIndex;
		State oldState = _state;
		try
		{
			_scheduleIndex = num.Value;
			string text = _schedule[_scheduleIndex];
			_state = State.Transferring;
			Debug.Log("Sending ferry to " + text);
			await NexusServer.TransferEntity(this, text, "ferry");
		}
		finally
		{
			_isTransferring = false;
			_scheduleIndex = oldScheduleIndex;
			_state = oldState;
		}
	}

	private int? TryGetNextScheduleIndex()
	{
		string zoneKey = NexusServer.ZoneKey;
		int num = (_scheduleIndex + 1) % _schedule.Count;
		for (int i = 0; i < _schedule.Count; i++)
		{
			string text = _schedule[num];
			if (!string.Equals(text, zoneKey, StringComparison.InvariantCultureIgnoreCase) && NexusServer.TryGetZoneStatus(text, out var status) && status.IsOnline)
			{
				return num;
			}
			num++;
			if (num >= _schedule.Count)
			{
				num = 0;
			}
		}
		return null;
	}

	private void EjectInactiveEntities(bool forceAll = false)
	{
		HashSet<NetworkableId> obj = Pool.Get<HashSet<NetworkableId>>();
		obj.Clear();
		if (_transferredIds != null)
		{
			foreach (NetworkableId transferredId in _transferredIds)
			{
				obj.Add(transferredId);
			}
		}
		List<BaseEntity> obj2 = Pool.GetList<BaseEntity>();
		foreach (BaseEntity child in children)
		{
			if (!(child is NPCAutoTurret) && (obj.Contains(child.net.ID) || forceAll) && (!IsEntityActive(child) || forceAll))
			{
				obj2.Add(child);
			}
		}
		foreach (BaseEntity item in obj2)
		{
			EjectEntity(item);
		}
		Pool.FreeList(ref obj2);
		obj.Clear();
		Pool.Free(ref obj);
	}

	private void EjectEntity(BaseEntity entity)
	{
		if (!(entity == null))
		{
			if (_targetDock != null && _targetDock.TryFindEjectionPosition(out var position))
			{
				entity.SetParent(null);
				entity.ServerPosition = position;
				entity.SendNetworkUpdateImmediate();
			}
			else
			{
				Debug.LogWarning($"Couldn't find an ejection point for {entity}", entity);
			}
		}
	}

	private static bool IsEntityActive(BaseEntity entity)
	{
		bool result = false;
		if (entity is BasePlayer player)
		{
			result = IsPlayerReady(player);
		}
		else if (entity is BaseVehicle baseVehicle)
		{
			List<BasePlayer> obj = Pool.GetList<BasePlayer>();
			baseVehicle.GetMountedPlayers(obj);
			foreach (BasePlayer item in obj)
			{
				if (IsPlayerReady(item))
				{
					result = true;
					break;
				}
			}
			Pool.FreeList(ref obj);
		}
		return result;
	}

	private static bool IsPlayerReady(BasePlayer player)
	{
		if (player != null && player.IsConnected)
		{
			return !player.IsLoadingAfterTransfer();
		}
		return false;
	}

	private void PlayDepartureHornEffect()
	{
		if (hornEffect.isValid)
		{
			Effect.server.Run(hornEffect.resourcePath, this, 0u, hornEffectTransform.localPosition, Vector3.up);
		}
		_departureHornPlayed = true;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Invoke(base.DisableTransferProtectionAction, 0.1f);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.nexusFerry = Pool.Get<ProtoBuf.NexusFerry>();
		info.msg.nexusFerry.timestamp = _timestamp;
		info.msg.nexusFerry.ownerZone = _ownerZone;
		info.msg.nexusFerry.schedule = _schedule.ShallowClonePooled();
		info.msg.nexusFerry.scheduleIndex = _scheduleIndex;
		info.msg.nexusFerry.state = (int)_state;
		info.msg.nexusFerry.isRetiring = _isRetiring;
		info.msg.nexusFerry.nextScheduleIndex = _nextScheduleIndex;
		if (info.forTransfer)
		{
			List<NetworkableId> list = Pool.GetList<NetworkableId>();
			foreach (BaseEntity child in children)
			{
				list.Add(child.net.ID);
			}
			info.msg.nexusFerry.transferredIds = list;
		}
		else
		{
			info.msg.nexusFerry.transferredIds = _transferredIds.ShallowClonePooled() ?? Pool.GetList<NetworkableId>();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.nexusFerry == null)
		{
			return;
		}
		_timestamp = info.msg.nexusFerry.timestamp;
		_ownerZone = info.msg.nexusFerry.ownerZone;
		if (_schedule != null)
		{
			Pool.FreeList(ref _schedule);
		}
		_schedule = info.msg.nexusFerry.schedule.ShallowClonePooled();
		_scheduleIndex = info.msg.nexusFerry.scheduleIndex;
		_state = (State)info.msg.nexusFerry.state;
		_isRetiring = info.msg.nexusFerry.isRetiring;
		_nextScheduleIndex = info.msg.nexusFerry.nextScheduleIndex;
		if (base.isServer)
		{
			if (_transferredIds != null)
			{
				Pool.FreeList(ref _transferredIds);
			}
			_transferredIds = info.msg.nexusFerry.transferredIds.ShallowClonePooled();
			if (_state == State.Transferring)
			{
				_state = State.SailingIn;
			}
		}
	}

	public static NexusFerry Get(NetworkableId entityId, long timestamp)
	{
		if (BaseNetworkable.serverEntities.Find(entityId) is NexusFerry nexusFerry && nexusFerry._timestamp == timestamp)
		{
			return nexusFerry;
		}
		return null;
	}

	private bool MoveTowardsTarget()
	{
		EnsureInitialized();
		switch (_state)
		{
		case State.Transferring:
			return false;
		case State.SailingIn:
			return MoveTowardsPositionAvoidObstacles(_targetDock.FerryWaypoint.position);
		case State.Queued:
		{
			bool entered;
			Transform entryPoint = _targetDock.GetEntryPoint(this, out entered);
			return MoveTowardsPositionAvoidObstacles(entryPoint.position) && entered;
		}
		case State.SailingOut:
			return MoveTowardsPositionAvoidObstacles(GetIslandTransferPosition());
		default:
			return MoveTowardsTargetTransform();
		}
	}

	private bool MoveTowardsPositionAvoidObstacles(Vector3 targetPosition)
	{
		if (!_pathTargetPosition.HasValue || !_pathTargetRotation.HasValue || (float)_sincePathCalculation > CastInterval)
		{
			Vector3 vector = ChooseWaypoint(targetPosition);
			_sincePathCalculation = 0f;
			_pathTargetPosition = null;
			_pathTargetRotation = null;
			Vector3 position = base.transform.position;
			Vector3 forward = base.transform.forward;
			float num = Vector3Ex.Distance2D(vector, position);
			float num2 = ((num > 0.01f) ? Quaternion.LookRotation(Vector3Ex.Direction2D(vector, position)).eulerAngles.y : 0f);
			float num3 = (float)UnityEngine.Random.Range(0, CastSweepDegrees) * CastSweepNoise;
			int num4 = Mathf.FloorToInt(360f / (float)CastSweepDegrees);
			List<(float, float, float, Vector3, Quaternion)> obj = Pool.GetList<(float, float, float, Vector3, Quaternion)>();
			float num5 = 0f;
			for (int i = 1; i < num4; i++)
			{
				int num6 = (((i & 1) == 0) ? 1 : (-1));
				int num7 = i / 2 * num6;
				Quaternion quaternion = Quaternion.Euler(0f, num2 + num3 + (float)CastSweepDegrees * 0.5f * (float)num7, 0f);
				Vector3 vector2 = quaternion * Vector3.forward;
				float travelDistance;
				Vector3 endPosition;
				bool num8 = SphereCast(vector2, num, out travelDistance, out endPosition);
				float item = Mathf.Clamp(Vector3.Dot(forward, vector2), 0.5f, 1f);
				float item2 = (num8 ? Mathf.Clamp01(travelDistance / 30f) : 1f);
				float num9 = Vector3Ex.Distance2D(vector, endPosition);
				obj.Add((item, item2, num9, endPosition, quaternion));
				num5 = Mathf.Max(num5, num9);
				if (!num8)
				{
					break;
				}
			}
			float num10 = -1f;
			Vector3 value = Vector3.zero;
			Quaternion value2 = Quaternion.identity;
			foreach (var item8 in obj)
			{
				float item3 = item8.Item1;
				float item4 = item8.Item2;
				float item5 = item8.Item3;
				Vector3 item6 = item8.Item4;
				Quaternion item7 = item8.Item5;
				float num11 = 1f - Mathf.Clamp01(item5 / num5);
				float num12 = item3 * item4 * num11;
				if (!(num12 <= num10))
				{
					num10 = num12;
					value = item6;
					value2 = item7;
				}
			}
			Pool.FreeList(ref obj);
			_pathTargetPosition = value;
			_pathTargetRotation = value2;
		}
		if (_pathTargetPosition.HasValue && _pathTargetRotation.HasValue)
		{
			return MoveTowardsPosition(_pathTargetPosition.Value, _pathTargetRotation.Value);
		}
		return false;
		Vector3 ChooseWaypoint(Vector3 target)
		{
			List<Vector3> obj2 = Pool.GetList<Vector3>();
			if (TryFindWaypointsTowards(target, obj2))
			{
				Vector3 position2 = base.transform.position;
				for (int num13 = obj2.Count - 1; num13 >= 0; num13--)
				{
					(obj2[num13] - position2).ToDirectionAndMagnitude(out var direction, out var magnitude);
					if (!SphereCast(direction, magnitude, out var _, out var _))
					{
						Vector3 result = obj2[num13];
						Pool.FreeList(ref obj2);
						return result;
					}
				}
				Vector3 result2 = obj2[0];
				Pool.FreeList(ref obj2);
				return result2;
			}
			Pool.FreeList(ref obj2);
			return target;
		}
	}

	private bool MoveTowardsTargetTransform()
	{
		Transform targetTransform = GetTargetTransform(_state);
		Vector3 position = targetTransform.position;
		Quaternion rotation = targetTransform.rotation;
		return MoveTowardsPosition(position, rotation);
	}

	private Transform GetTargetTransform(State state)
	{
		EnsureInitialized();
		switch (state)
		{
		case State.Arrival:
			return _targetDock.Arrival;
		case State.Docking:
			return _targetDock.Docking;
		case State.Stopping:
		case State.Waiting:
			return _targetDock.Docked;
		case State.CastingOff:
			return _targetDock.CastingOff;
		case State.Departure:
			return _targetDock.Departure;
		default:
			Debug.LogError($"Cannot call GetTargetTransform in state {state}");
			return base.transform;
		}
	}

	private bool MoveTowardsPosition(Vector3 targetPosition, Quaternion targetRotation)
	{
		bool flag = _state >= State.Queued && _state <= State.CastingOff;
		Vector3 position = base.transform.position;
		targetPosition.y = position.y;
		(targetPosition - position).ToDirectionAndMagnitude(out var direction, out var magnitude);
		if (magnitude < 0.1f)
		{
			return true;
		}
		_velocity.ToDirectionAndMagnitude(out var direction2, out var magnitude2);
		float to = ((!flag) ? TravelVelocity : ((_state == State.Stopping) ? StoppingVelocity : ApproachVelocity));
		magnitude2 = Mathx.Lerp(magnitude2, to, AccelerationSpeed);
		if (flag)
		{
			_velocity = magnitude2 * direction;
		}
		else
		{
			float num = Mathf.Clamp(Vector3.Dot(direction2, direction), 0.1f, 1f);
			_velocity = num * magnitude2 * direction + (1f - num) * VelocityPreservationOnTurn * _velocity;
		}
		Quaternion rotation = base.transform.rotation;
		State previousState = GetPreviousState(_state);
		Vector3 position3;
		Quaternion rotation3;
		if (previousState != 0)
		{
			Transform targetTransform = GetTargetTransform(previousState);
			Vector3 position2 = targetTransform.position;
			Quaternion rotation2 = targetTransform.rotation;
			position2.y = position.y;
			float num2 = Vector3Ex.Distance2D(position2, targetPosition);
			float num3 = _velocity.magnitude * Time.deltaTime;
			float num4 = Mathf.Min(num3, magnitude);
			position3 = position + direction * num4;
			rotation3 = Quaternion.Slerp(targetRotation, rotation2, magnitude / num2);
			base.transform.SetPositionAndRotation(position3, rotation3);
			if (!Mathf.Approximately(num3, 0f))
			{
				return num4 < num3;
			}
			return true;
		}
		Vector3 vector = _velocity * Time.deltaTime;
		vector.ToDirectionAndMagnitude(out var direction3, out var magnitude3);
		position3 = ((!(magnitude3 >= magnitude) || !((double)Vector3.Dot(direction3, direction) > 0.5)) ? (position + vector) : targetPosition);
		targetRotation = ((direction.sqrMagnitude > 0.01f) ? Quaternion.LookRotation(direction) : Quaternion.identity);
		rotation3 = Mathx.Lerp(rotation, targetRotation, TurnSpeed);
		base.transform.SetPositionAndRotation(position3, rotation3);
		return Vector3.Distance(position3, targetPosition) < TargetDistanceThreshold;
	}

	private bool SphereCast(Vector3 direction, float distance, out float travelDistance, out Vector3 endPosition)
	{
		Vector3 vector = SphereCaster.transform.position + SphereCaster.center;
		float radius = SphereCaster.radius;
		List<RaycastHit> obj = Pool.GetList<RaycastHit>();
		GamePhysics.TraceAll(new Ray(vector, direction), radius, obj, distance, CastLayers, QueryTriggerInteraction.Collide);
		bool flag = false;
		travelDistance = 0f;
		foreach (RaycastHit item in obj)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((!(entity != null) || (!(entity == this) && !entity.EqualNetID(this))) && (!item.collider.isTrigger || item.collider.CompareTag("FerryAvoid")))
			{
				flag = true;
				travelDistance = Mathf.Max(item.distance - CastHitProtection, 0f);
				break;
			}
		}
		Pool.FreeList(ref obj);
		if (!flag)
		{
			travelDistance = distance;
		}
		endPosition = vector + direction * travelDistance;
		return flag;
	}

	private Vector3 GetIslandTransferPosition()
	{
		EnsureInitialized();
		int? num = TryGetNextScheduleIndex();
		if (num.HasValue)
		{
			string zoneKey = _schedule[num.Value];
			if (NexusServer.TryGetIsland(zoneKey, out var island))
			{
				return island.FerryWaypoint.position;
			}
			if (NexusServer.TryGetIslandPosition(zoneKey, out var position))
			{
				return position;
			}
		}
		if (NexusIsland.All.Count > 0)
		{
			return NexusIsland.All[0].FerryWaypoint.position;
		}
		return _targetDock.FerryWaypoint.position;
	}

	private bool TryFindWaypointsTowards(Vector3 targetPosition, List<Vector3> waypoints)
	{
		Vector3 vector = TerrainMeta.Center.WithY(0f);
		Vector3 size = TerrainMeta.Size;
		float num = Mathf.Sqrt(size.x * size.x + size.z * size.z) / 2f;
		Graph obj = Pool.Get<Graph>();
		Node node = null;
		Node node2 = null;
		float num2 = 0f;
		int num3 = 0;
		while (num3 < 64)
		{
			Vector3 position = vector + Quaternion.Euler(0f, num2, 0f) * Vector3.forward * num;
			Node node3 = obj.AddNode(position);
			if (node2 != null)
			{
				node3.ConnectTo(node2);
			}
			if (node == null)
			{
				node = node3;
			}
			node2 = node3;
			num3++;
			num2 += 5.5384617f;
		}
		if (node != null && node2 != null && node != node2)
		{
			node.ConnectTo(node2);
		}
		foreach (NexusIsland item in NexusIsland.All)
		{
			Vector3 position2 = item.FerryWaypoint.position;
			Node other = obj.FindClosest(position2);
			obj.AddNode(position2).ConnectTo(other);
		}
		if (SingletonComponent<NexusDock>.Instance != null)
		{
			Vector3 position3 = SingletonComponent<NexusDock>.Instance.FerryWaypoint.position;
			Node node4 = obj.FindClosest(position3);
			Vector3 vector2 = (position3 + node4.Position) * 0.5f;
			Vector3 position4 = (vector2 + node4.Position) * 0.5f;
			Vector3 position5 = (vector2 + position3) * 0.5f;
			Node other2 = obj.AddNode(position4).ConnectTo(node4);
			Node other3 = obj.AddNode(vector2).ConnectTo(other2);
			Node other4 = obj.AddNode(position5).ConnectTo(other3);
			obj.AddNode(position3).ConnectTo(other4);
		}
		Node node5 = obj.FindClosest(base.transform.position);
		Node node6 = obj.FindClosest(targetPosition);
		if (node5 == node6)
		{
			waypoints.Add(targetPosition);
			Pool.Free(ref obj);
			return true;
		}
		List<Vector3> obj2 = Pool.GetList<Vector3>();
		if (node5 == null || node6 == null || !obj.TryFindPath(node5, node6, obj2) || obj2.Count == 0)
		{
			Pool.FreeList(ref obj2);
			Pool.Free(ref obj);
			return false;
		}
		Pool.Free(ref obj);
		if (obj2.Count == 1)
		{
			waypoints.Add(obj2[0]);
			Pool.FreeList(ref obj2);
			return true;
		}
		int num4 = obj2.Count - 1;
		int num5 = ((num4 < PathLookaheadThreshold) ? 1 : Mathf.Min(PathLookahead, num4));
		for (int i = 1; i <= num5; i++)
		{
			waypoints.Add(obj2[i]);
		}
		Pool.FreeList(ref obj2);
		return true;
	}
}
