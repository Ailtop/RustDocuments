using Facepunch;
using ProtoBuf;
using UnityEngine;

public class DeliveryDrone : Drone
{
	private enum State
	{
		Invalid = 0,
		Takeoff = 1,
		FlyToVendingMachine = 2,
		DescendToVendingMachine = 3,
		PickUpItems = 4,
		AscendBeforeReturn = 5,
		ReturnToTerminal = 6,
		Landing = 7
	}

	[Header("Delivery Drone")]
	public float stateTimeout = 300f;

	public float targetPositionTolerance = 1f;

	public float preferredCruiseHeight = 20f;

	public float preferredHeightAboveObstacle = 5f;

	public float marginAbovePreferredHeight = 3f;

	public float obstacleHeightLockDuration = 3f;

	public int pickUpDelayInTicks = 3;

	public DeliveryDroneConfig config;

	public GameObjectRef mapMarkerPrefab;

	public EntityRef<Marketplace> sourceMarketplace;

	public EntityRef<MarketTerminal> sourceTerminal;

	public EntityRef<VendingMachine> targetVendingMachine;

	public State _state;

	public RealTimeSince _sinceLastStateChange;

	public Vector3? _stateGoalPosition;

	public float? _goToY;

	public TimeSince _sinceLastObstacleBlock;

	public float? _minimumYLock;

	public int _pickUpTicks;

	public BaseEntity _mapMarkerInstance;

	public void Setup(Marketplace marketplace, MarketTerminal terminal, VendingMachine vendingMachine)
	{
		sourceMarketplace.Set(marketplace);
		sourceTerminal.Set(terminal);
		targetVendingMachine.Set(vendingMachine);
		_state = State.Takeoff;
		_sinceLastStateChange = 0f;
		_pickUpTicks = 0;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRandomized(Think, 0f, 0.5f, 0.25f);
		CreateMapMarker();
	}

	public void CreateMapMarker()
	{
		if (_mapMarkerInstance != null)
		{
			_mapMarkerInstance.Kill();
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(mapMarkerPrefab?.resourcePath, Vector3.zero, Quaternion.identity);
		baseEntity.OwnerID = base.OwnerID;
		baseEntity.Spawn();
		baseEntity.SetParent(this);
		_mapMarkerInstance = baseEntity;
	}

	public void Think()
	{
		if ((float)_sinceLastStateChange > stateTimeout)
		{
			Debug.LogError("Delivery drone hasn't change state in too long, killing", this);
			ForceRemove();
			return;
		}
		if (!sourceMarketplace.TryGet(serverside: true, out var marketplace) || !sourceTerminal.TryGet(serverside: true, out var _))
		{
			Debug.LogError("Delivery drone's marketplace or terminal was destroyed, killing", this);
			ForceRemove();
			return;
		}
		if (!targetVendingMachine.TryGet(serverside: true, out var entity2) && _state <= State.AscendBeforeReturn)
		{
			SetState(State.ReturnToTerminal);
		}
		Vector3 currentPosition = base.transform.position;
		float num = GetMinimumHeight(Vector3.zero);
		if (_goToY.HasValue)
		{
			if (!IsAtGoToY())
			{
				targetPosition = currentPosition.WithY(_goToY.Value);
				return;
			}
			_goToY = null;
			_sinceLastObstacleBlock = 0f;
			_minimumYLock = currentPosition.y;
		}
		Vector3 waitPosition;
		switch (_state)
		{
		case State.Takeoff:
			SetGoalPosition(marketplace.droneLaunchPoint.position + Vector3.up * 15f);
			if (IsAtGoalPosition())
			{
				SetState(State.FlyToVendingMachine);
			}
			break;
		case State.FlyToVendingMachine:
		{
			bool isBlocked2;
			float num2 = CalculatePreferredY(out isBlocked2);
			if (isBlocked2 && currentPosition.y < num2)
			{
				SetGoToY(num2 + marginAbovePreferredHeight);
				return;
			}
			config.FindDescentPoints(entity2, num2 + marginAbovePreferredHeight, out waitPosition, out var descendPosition);
			SetGoalPosition(descendPosition);
			if (IsAtGoalPosition())
			{
				SetState(State.DescendToVendingMachine);
			}
			break;
		}
		case State.DescendToVendingMachine:
		{
			config.FindDescentPoints(entity2, currentPosition.y, out var waitPosition2, out waitPosition);
			SetGoalPosition(waitPosition2);
			if (IsAtGoalPosition())
			{
				SetState(State.PickUpItems);
			}
			break;
		}
		case State.PickUpItems:
			_pickUpTicks++;
			if (_pickUpTicks >= pickUpDelayInTicks)
			{
				SetState(State.AscendBeforeReturn);
			}
			break;
		case State.AscendBeforeReturn:
		{
			config.FindDescentPoints(entity2, num + preferredCruiseHeight, out waitPosition, out var descendPosition2);
			SetGoalPosition(descendPosition2);
			if (IsAtGoalPosition())
			{
				SetState(State.ReturnToTerminal);
			}
			break;
		}
		case State.ReturnToTerminal:
		{
			bool isBlocked3;
			float num3 = CalculatePreferredY(out isBlocked3);
			if (isBlocked3 && currentPosition.y < num3)
			{
				SetGoToY(num3 + marginAbovePreferredHeight);
				return;
			}
			Vector3 vector = LandingPosition();
			if (Vector3Ex.Distance2D(currentPosition, vector) < 30f)
			{
				vector.y = Mathf.Max(vector.y, num3 + marginAbovePreferredHeight);
			}
			else
			{
				vector.y = num3 + marginAbovePreferredHeight;
			}
			SetGoalPosition(vector);
			if (IsAtGoalPosition())
			{
				SetState(State.Landing);
			}
			break;
		}
		case State.Landing:
			SetGoalPosition(LandingPosition());
			if (IsAtGoalPosition())
			{
				marketplace.ReturnDrone(this);
				SetState(State.Invalid);
			}
			break;
		default:
			ForceRemove();
			break;
		}
		if (_minimumYLock.HasValue)
		{
			if ((float)_sinceLastObstacleBlock > obstacleHeightLockDuration)
			{
				_minimumYLock = null;
			}
			else if (targetPosition.HasValue && targetPosition.Value.y < _minimumYLock.Value)
			{
				targetPosition = targetPosition.Value.WithY(_minimumYLock.Value);
			}
		}
		float CalculatePreferredY(out bool isBlocked)
		{
			body.velocity.WithY(0f).ToDirectionAndMagnitude(out var direction, out var magnitude);
			if (magnitude < 0.5f)
			{
				float num4 = GetMinimumHeight(Vector3.zero) + preferredCruiseHeight;
				Vector3 origin = currentPosition.WithY(num4 + 1000f);
				currentPosition.WithY(num4);
				isBlocked = Physics.Raycast(origin, Vector3.down, out var hitInfo, 1000f, config.layerMask);
				if (!isBlocked)
				{
					return num4;
				}
				return num4 + (1000f - hitInfo.distance) + preferredHeightAboveObstacle;
			}
			float num5 = magnitude * 2f;
			float minimumHeight = GetMinimumHeight(Vector3.zero);
			float minimumHeight2 = GetMinimumHeight(new Vector3(0f, 0f, num5 / 2f));
			float num6 = Mathf.Max(b: GetMinimumHeight(new Vector3(0f, 0f, num5)), a: Mathf.Max(minimumHeight, minimumHeight2)) + preferredCruiseHeight;
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, direction);
			Vector3 halfExtents = config.halfExtents.WithZ(num5 / 2f);
			Vector3 vector2 = (currentPosition.WithY(num6) + quaternion * new Vector3(0f, 0f, halfExtents.z / 2f)).WithY(num6 + 1000f);
			isBlocked = Physics.BoxCast(vector2, halfExtents, Vector3.down, out var hitInfo2, quaternion, 1000f, config.layerMask);
			if (isBlocked)
			{
				Ray ray = new Ray(vector2, Vector3.down);
				Vector3 b = RayEx.ClosestPoint(ray, hitInfo2.point);
				float num7 = Vector3.Distance(ray.origin, b);
				return num6 + (1000f - num7) + preferredHeightAboveObstacle;
			}
			return num6;
		}
		float GetMinimumHeight(Vector3 offset)
		{
			Vector3 vector3 = base.transform.TransformPoint(offset);
			float height = TerrainMeta.HeightMap.GetHeight(vector3);
			float height2 = WaterSystem.GetHeight(vector3);
			return Mathf.Max(height, height2);
		}
		bool IsAtGoalPosition()
		{
			if (_stateGoalPosition.HasValue)
			{
				return Vector3.Distance(_stateGoalPosition.Value, currentPosition) < targetPositionTolerance;
			}
			return false;
		}
		bool IsAtGoToY()
		{
			if (_goToY.HasValue)
			{
				return Mathf.Abs(_goToY.Value - currentPosition.y) < targetPositionTolerance;
			}
			return false;
		}
		Vector3 LandingPosition()
		{
			return marketplace.droneLaunchPoint.position;
		}
		void SetGoalPosition(Vector3 position)
		{
			_goToY = null;
			_stateGoalPosition = position;
			targetPosition = position;
		}
		void SetGoToY(float y)
		{
			_goToY = y;
			targetPosition = currentPosition.WithY(y);
		}
		void SetState(State newState)
		{
			_state = newState;
			_sinceLastStateChange = 0f;
			_pickUpTicks = 0;
			_stateGoalPosition = null;
			_goToY = null;
			SetFlag(Flags.Reserved1, _state >= State.AscendBeforeReturn);
		}
	}

	public void ForceRemove()
	{
		if (sourceMarketplace.TryGet(serverside: true, out var entity))
		{
			entity.ReturnDrone(this);
		}
		else
		{
			Kill();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.deliveryDrone = Pool.Get<ProtoBuf.DeliveryDrone>();
			info.msg.deliveryDrone.marketplaceId = sourceMarketplace.uid;
			info.msg.deliveryDrone.terminalId = sourceTerminal.uid;
			info.msg.deliveryDrone.vendingMachineId = targetVendingMachine.uid;
			info.msg.deliveryDrone.state = (int)_state;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.deliveryDrone != null)
		{
			sourceMarketplace = new EntityRef<Marketplace>(info.msg.deliveryDrone.marketplaceId);
			sourceTerminal = new EntityRef<MarketTerminal>(info.msg.deliveryDrone.terminalId);
			targetVendingMachine = new EntityRef<VendingMachine>(info.msg.deliveryDrone.vendingMachineId);
			_state = (State)info.msg.deliveryDrone.state;
		}
	}

	public override bool CanControl()
	{
		return false;
	}
}
