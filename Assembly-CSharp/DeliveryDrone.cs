using Facepunch;
using ProtoBuf;
using UnityEngine;

public class DeliveryDrone : Drone
{
	private enum State
	{
		Invalid,
		Takeoff,
		FlyToVendingMachine,
		DescendToVendingMachine,
		PickUpItems,
		AscendBeforeReturn,
		ReturnToTerminal,
		Landing
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

	private State _state;

	private RealTimeSince _sinceLastStateChange;

	private Vector3? _stateGoalPosition;

	private float? _goToY;

	private TimeSince _sinceLastObstacleBlock;

	private float? _minimumYLock;

	private int _pickUpTicks;

	private BaseEntity _mapMarkerInstance;

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

	private void Think()
	{
		_003C_003Ec__DisplayClass24_0 _003C_003Ec__DisplayClass24_ = default(_003C_003Ec__DisplayClass24_0);
		_003C_003Ec__DisplayClass24_._003C_003E4__this = this;
		if ((float)_sinceLastStateChange > stateTimeout)
		{
			Debug.LogError("Delivery drone hasn't change state in too long, killing", this);
			ForceRemove();
			return;
		}
		MarketTerminal entity;
		if (!sourceMarketplace.TryGet(true, out _003C_003Ec__DisplayClass24_.marketplace) || !sourceTerminal.TryGet(true, out entity))
		{
			Debug.LogError("Delivery drone's marketplace or terminal was destroyed, killing", this);
			ForceRemove();
			return;
		}
		VendingMachine entity2;
		if (!targetVendingMachine.TryGet(true, out entity2) && _state <= State.AscendBeforeReturn)
		{
			_003CThink_003Eg__SetState_007C24_7(State.ReturnToTerminal, ref _003C_003Ec__DisplayClass24_);
		}
		_003C_003Ec__DisplayClass24_.currentPosition = base.transform.position;
		float num = _003CThink_003Eg__GetMinimumHeight_007C24_1(Vector3.zero, ref _003C_003Ec__DisplayClass24_);
		if (_goToY.HasValue)
		{
			if (!_003CThink_003Eg__IsAtGoToY_007C24_6(ref _003C_003Ec__DisplayClass24_))
			{
				targetPosition = _003C_003Ec__DisplayClass24_.currentPosition.WithY(_goToY.Value);
				return;
			}
			_goToY = null;
			_sinceLastObstacleBlock = 0f;
			_minimumYLock = _003C_003Ec__DisplayClass24_.currentPosition.y;
		}
		Vector3 waitPosition;
		switch (_state)
		{
		case State.Takeoff:
			_003CThink_003Eg__SetGoalPosition_007C24_3(_003C_003Ec__DisplayClass24_.marketplace.droneLaunchPoint.position + Vector3.up * 15f, ref _003C_003Ec__DisplayClass24_);
			if (_003CThink_003Eg__IsAtGoalPosition_007C24_4(ref _003C_003Ec__DisplayClass24_))
			{
				_003CThink_003Eg__SetState_007C24_7(State.FlyToVendingMachine, ref _003C_003Ec__DisplayClass24_);
			}
			break;
		case State.FlyToVendingMachine:
		{
			bool isBlocked;
			float num2 = _003CThink_003Eg__CalculatePreferredY_007C24_0(out isBlocked, ref _003C_003Ec__DisplayClass24_);
			if (isBlocked && _003C_003Ec__DisplayClass24_.currentPosition.y < num2)
			{
				_003CThink_003Eg__SetGoToY_007C24_5(num2 + marginAbovePreferredHeight, ref _003C_003Ec__DisplayClass24_);
				return;
			}
			Vector3 descendPosition;
			config.FindDescentPoints(entity2, num2 + marginAbovePreferredHeight, out waitPosition, out descendPosition);
			_003CThink_003Eg__SetGoalPosition_007C24_3(descendPosition, ref _003C_003Ec__DisplayClass24_);
			if (_003CThink_003Eg__IsAtGoalPosition_007C24_4(ref _003C_003Ec__DisplayClass24_))
			{
				_003CThink_003Eg__SetState_007C24_7(State.DescendToVendingMachine, ref _003C_003Ec__DisplayClass24_);
			}
			break;
		}
		case State.DescendToVendingMachine:
		{
			Vector3 waitPosition2;
			config.FindDescentPoints(entity2, _003C_003Ec__DisplayClass24_.currentPosition.y, out waitPosition2, out waitPosition);
			_003CThink_003Eg__SetGoalPosition_007C24_3(waitPosition2, ref _003C_003Ec__DisplayClass24_);
			if (_003CThink_003Eg__IsAtGoalPosition_007C24_4(ref _003C_003Ec__DisplayClass24_))
			{
				_003CThink_003Eg__SetState_007C24_7(State.PickUpItems, ref _003C_003Ec__DisplayClass24_);
			}
			break;
		}
		case State.PickUpItems:
			_pickUpTicks++;
			if (_pickUpTicks >= pickUpDelayInTicks)
			{
				_003CThink_003Eg__SetState_007C24_7(State.AscendBeforeReturn, ref _003C_003Ec__DisplayClass24_);
			}
			break;
		case State.AscendBeforeReturn:
		{
			Vector3 descendPosition2;
			config.FindDescentPoints(entity2, num + preferredCruiseHeight, out waitPosition, out descendPosition2);
			_003CThink_003Eg__SetGoalPosition_007C24_3(descendPosition2, ref _003C_003Ec__DisplayClass24_);
			if (_003CThink_003Eg__IsAtGoalPosition_007C24_4(ref _003C_003Ec__DisplayClass24_))
			{
				_003CThink_003Eg__SetState_007C24_7(State.ReturnToTerminal, ref _003C_003Ec__DisplayClass24_);
			}
			break;
		}
		case State.ReturnToTerminal:
		{
			bool isBlocked2;
			float num3 = _003CThink_003Eg__CalculatePreferredY_007C24_0(out isBlocked2, ref _003C_003Ec__DisplayClass24_);
			if (isBlocked2 && _003C_003Ec__DisplayClass24_.currentPosition.y < num3)
			{
				_003CThink_003Eg__SetGoToY_007C24_5(num3 + marginAbovePreferredHeight, ref _003C_003Ec__DisplayClass24_);
				return;
			}
			Vector3 vector = _003CThink_003Eg__LandingPosition_007C24_2(ref _003C_003Ec__DisplayClass24_);
			if (Vector3Ex.Distance2D(_003C_003Ec__DisplayClass24_.currentPosition, vector) < 30f)
			{
				vector.y = Mathf.Max(vector.y, num3 + marginAbovePreferredHeight);
			}
			else
			{
				vector.y = num3 + marginAbovePreferredHeight;
			}
			_003CThink_003Eg__SetGoalPosition_007C24_3(vector, ref _003C_003Ec__DisplayClass24_);
			if (_003CThink_003Eg__IsAtGoalPosition_007C24_4(ref _003C_003Ec__DisplayClass24_))
			{
				_003CThink_003Eg__SetState_007C24_7(State.Landing, ref _003C_003Ec__DisplayClass24_);
			}
			break;
		}
		case State.Landing:
			_003CThink_003Eg__SetGoalPosition_007C24_3(_003CThink_003Eg__LandingPosition_007C24_2(ref _003C_003Ec__DisplayClass24_), ref _003C_003Ec__DisplayClass24_);
			if (_003CThink_003Eg__IsAtGoalPosition_007C24_4(ref _003C_003Ec__DisplayClass24_))
			{
				_003C_003Ec__DisplayClass24_.marketplace.ReturnDrone(this);
				_003CThink_003Eg__SetState_007C24_7(State.Invalid, ref _003C_003Ec__DisplayClass24_);
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
	}

	private void ForceRemove()
	{
		Marketplace entity;
		if (sourceMarketplace.TryGet(true, out entity))
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
