using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class Elevator : IOEntity, IFlagNotify
{
	public enum Direction
	{
		Up,
		Down
	}

	public Transform LiftRoot;

	public GameObjectRef LiftEntityPrefab;

	public GameObjectRef IoEntityPrefab;

	public Transform IoEntitySpawnPoint;

	public GameObject FloorBlockerVolume;

	public float LiftSpeedPerMetre = 1f;

	public GameObject[] PoweredObjects;

	public MeshRenderer PoweredMesh;

	[ColorUsage(true, true)]
	public Color PoweredLightColour;

	[ColorUsage(true, true)]
	public Color UnpoweredLightColour;

	public SkinnedMeshRenderer[] CableRenderers;

	public LODGroup CableLod;

	public Transform CableRoot;

	protected const Flags TopFloorFlag = Flags.Reserved1;

	public const Flags ElevatorPowered = Flags.Reserved2;

	public ElevatorLift liftEntity;

	public IOEntity ioEntity;

	public int[] previousPowerAmount = new int[2];

	public virtual bool IsStatic => false;

	public int Floor { get; set; }

	public bool IsTop => HasFlag(Flags.Reserved1);

	public virtual float FloorHeight => 3f;

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.elevator != null)
		{
			Floor = info.msg.elevator.floor;
		}
		if (FloorBlockerVolume != null)
		{
			FloorBlockerVolume.SetActive(Floor > 0);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		Elevator elevatorInDirection = GetElevatorInDirection(Direction.Down);
		if (elevatorInDirection != null)
		{
			elevatorInDirection.SetFlag(Flags.Reserved1, false);
			Floor = elevatorInDirection.Floor + 1;
		}
		SetFlag(Flags.Reserved1, true);
	}

	public virtual void CallElevator()
	{
		EntityLinkBroadcast(delegate(Elevator elevatorEnt)
		{
			if (elevatorEnt.IsTop && Interface.CallHook("OnElevatorCall", this, elevatorEnt) == null)
			{
				float timeToTravel;
				elevatorEnt.RequestMoveLiftTo(Floor, out timeToTravel);
			}
		}, (ConstructionSocket socket) => socket.socketType == ConstructionSocket.Type.Elevator);
	}

	public void Server_RaiseLowerElevator(Direction dir, bool goTopBottom)
	{
		if (IsBusy())
		{
			return;
		}
		int num = LiftPositionToFloor();
		switch (dir)
		{
		case Direction.Up:
			num++;
			if (goTopBottom)
			{
				num = Floor;
			}
			break;
		case Direction.Down:
			num--;
			if (goTopBottom)
			{
				num = 0;
			}
			break;
		}
		float timeToTravel;
		RequestMoveLiftTo(num, out timeToTravel);
	}

	public bool RequestMoveLiftTo(int targetFloor, out float timeToTravel)
	{
		timeToTravel = 0f;
		if (Interface.CallHook("OnElevatorMove", this, targetFloor) != null)
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (!IsStatic && ioEntity != null && !ioEntity.IsPowered())
		{
			return false;
		}
		if (!IsValidFloor(targetFloor))
		{
			return false;
		}
		if (!liftEntity.CanMove())
		{
			return false;
		}
		if (LiftPositionToFloor() == targetFloor)
		{
			OnLiftCalledWhenAtTargetFloor();
			return false;
		}
		Vector3 worldSpaceFloorPosition = GetWorldSpaceFloorPosition(targetFloor);
		if (!GamePhysics.LineOfSight(liftEntity.transform.position, worldSpaceFloorPosition, 2097152))
		{
			return false;
		}
		OnMoveBegin();
		Vector3 vector = base.transform.InverseTransformPoint(worldSpaceFloorPosition);
		timeToTravel = TimeToTravelDistance(Mathf.Abs(liftEntity.transform.localPosition.y - vector.y));
		LeanTween.moveLocalY(liftEntity.gameObject, vector.y, timeToTravel);
		SetFlag(Flags.Busy, true);
		if (targetFloor < Floor)
		{
			liftEntity.ToggleHurtTrigger(true);
		}
		Invoke(ClearBusy, timeToTravel);
		if (ioEntity != null)
		{
			ioEntity.SetFlag(Flags.Busy, true);
			ioEntity.SendChangedToRoot(true);
		}
		return true;
	}

	public virtual void OnLiftCalledWhenAtTargetFloor()
	{
	}

	public virtual void OnMoveBegin()
	{
	}

	public float TimeToTravelDistance(float distance)
	{
		return distance / LiftSpeedPerMetre;
	}

	public virtual Vector3 GetWorldSpaceFloorPosition(int targetFloor)
	{
		int num = Floor - targetFloor;
		Vector3 vector = Vector3.up * ((float)num * FloorHeight);
		vector.y -= 1f;
		return base.transform.position - vector;
	}

	public virtual void ClearBusy()
	{
		SetFlag(Flags.Busy, false);
		if (liftEntity != null)
		{
			liftEntity.ToggleHurtTrigger(false);
		}
		if (ioEntity != null)
		{
			ioEntity.SetFlag(Flags.Busy, false);
			ioEntity.SendChangedToRoot(true);
		}
	}

	public virtual bool IsValidFloor(int targetFloor)
	{
		if (targetFloor <= Floor)
		{
			return targetFloor >= 0;
		}
		return false;
	}

	public Elevator GetElevatorInDirection(Direction dir)
	{
		EntityLink entityLink = FindLink((dir == Direction.Down) ? "elevator/sockets/elevator-male" : "elevator/sockets/elevator-female");
		if (entityLink != null && !entityLink.IsEmpty())
		{
			BaseEntity owner = entityLink.connections[0].owner;
			Elevator elevator;
			if (owner != null && owner.isServer && (object)(elevator = owner as Elevator) != null && elevator != this)
			{
				return elevator;
			}
		}
		return null;
	}

	public void UpdateChildEntities(bool isTop)
	{
		if (isTop)
		{
			if (liftEntity == null)
			{
				FindExistingLiftChild();
			}
			if (liftEntity == null)
			{
				liftEntity = GameManager.server.CreateEntity(LiftEntityPrefab.resourcePath, GetWorldSpaceFloorPosition(Floor), LiftRoot.rotation) as ElevatorLift;
				liftEntity.SetParent(this, true);
				liftEntity.Spawn();
			}
			if (ioEntity == null)
			{
				FindExistingIOChild();
			}
			if (ioEntity == null && IoEntityPrefab.isValid)
			{
				ioEntity = GameManager.server.CreateEntity(IoEntityPrefab.resourcePath, IoEntitySpawnPoint.position, IoEntitySpawnPoint.rotation) as IOEntity;
				ioEntity.SetParent(this, true);
				ioEntity.Spawn();
			}
		}
		else
		{
			if (liftEntity != null)
			{
				liftEntity.Kill();
			}
			if (ioEntity != null)
			{
				ioEntity.Kill();
			}
		}
	}

	public void FindExistingIOChild()
	{
		foreach (BaseEntity child in children)
		{
			IOEntity iOEntity;
			if ((object)(iOEntity = child as IOEntity) != null)
			{
				ioEntity = iOEntity;
				break;
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.elevator == null)
		{
			info.msg.elevator = Pool.Get<ProtoBuf.Elevator>();
		}
		info.msg.elevator.floor = Floor;
	}

	public int LiftPositionToFloor()
	{
		Vector3 position = liftEntity.transform.position;
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i <= Floor; i++)
		{
			float num2 = Vector3.Distance(GetWorldSpaceFloorPosition(i), position);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public override void DestroyShared()
	{
		Cleanup();
		base.DestroyShared();
	}

	private void Cleanup()
	{
		Elevator elevatorInDirection = GetElevatorInDirection(Direction.Down);
		if (elevatorInDirection != null)
		{
			elevatorInDirection.SetFlag(Flags.Reserved1, true);
		}
		Elevator elevatorInDirection2 = GetElevatorInDirection(Direction.Up);
		if (elevatorInDirection2 != null)
		{
			elevatorInDirection2.Kill(DestroyMode.Gib);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SetFlag(Flags.Busy, false);
		UpdateChildEntities(IsTop);
		if (ioEntity != null)
		{
			ioEntity.SetFlag(Flags.Busy, false);
		}
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputAmount > 0 && previousPowerAmount[inputSlot] == 0)
		{
			CallElevator();
		}
		previousPowerAmount[inputSlot] = inputAmount;
	}

	private void OnPhysicsNeighbourChanged()
	{
		if (!IsStatic && GetElevatorInDirection(Direction.Down) == null && !HasFloorSocketConnection())
		{
			Kill(DestroyMode.Gib);
		}
	}

	public bool HasFloorSocketConnection()
	{
		EntityLink entityLink = FindLink("elevator/sockets/block-male");
		if (entityLink != null && !entityLink.IsEmpty())
		{
			return true;
		}
		return false;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!Rust.Application.isLoading && base.isServer && old.HasFlag(Flags.Reserved1) != next.HasFlag(Flags.Reserved1))
		{
			UpdateChildEntities(next.HasFlag(Flags.Reserved1));
		}
		if (old.HasFlag(Flags.Busy) != next.HasFlag(Flags.Busy))
		{
			if (liftEntity == null)
			{
				FindExistingLiftChild();
			}
			if (liftEntity != null)
			{
				liftEntity.ToggleMovementCollider(!next.HasFlag(Flags.Busy));
			}
		}
		if (old.HasFlag(Flags.Reserved1) != next.HasFlag(Flags.Reserved1) && FloorBlockerVolume != null)
		{
			FloorBlockerVolume.SetActive(next.HasFlag(Flags.Reserved1));
		}
	}

	public void FindExistingLiftChild()
	{
		foreach (BaseEntity child in children)
		{
			ElevatorLift elevatorLift;
			if ((object)(elevatorLift = child as ElevatorLift) != null)
			{
				liftEntity = elevatorLift;
				break;
			}
		}
	}

	public void OnFlagToggled(bool state)
	{
		if (base.isServer)
		{
			SetFlag(Flags.Reserved2, state);
		}
	}
}
