using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class ElevatorStatic : Elevator
{
	public bool StaticTop;

	public const Flags LiftRecentlyArrived = Flags.Reserved3;

	public List<ElevatorStatic> floorPositions = new List<ElevatorStatic>();

	public ElevatorStatic ownerElevator;

	public override bool IsStatic => true;

	public override void Spawn()
	{
		base.Spawn();
		SetFlag(Flags.Reserved2, b: true);
		SetFlag(Flags.Reserved1, StaticTop);
		if (!base.IsTop)
		{
			return;
		}
		List<RaycastHit> obj = Pool.GetList<RaycastHit>();
		GamePhysics.TraceAll(new Ray(base.transform.position, -Vector3.up), 0f, obj, 200f, 262144, QueryTriggerInteraction.Collide);
		foreach (RaycastHit item in obj)
		{
			if (item.transform.parent != null)
			{
				ElevatorStatic component = item.transform.parent.GetComponent<ElevatorStatic>();
				if (component != null && component != this && component.isServer)
				{
					floorPositions.Add(component);
				}
			}
		}
		Pool.FreeList(ref obj);
		floorPositions.Reverse();
		base.Floor = floorPositions.Count;
		for (int i = 0; i < floorPositions.Count; i++)
		{
			floorPositions[i].SetFloorDetails(i, this);
		}
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		UpdateChildEntities(base.IsTop);
	}

	public override bool IsValidFloor(int targetFloor)
	{
		if (targetFloor >= 0)
		{
			return targetFloor <= base.Floor;
		}
		return false;
	}

	public override Vector3 GetWorldSpaceFloorPosition(int targetFloor)
	{
		if (targetFloor == base.Floor)
		{
			return base.transform.position + Vector3.up * 1f;
		}
		Vector3 position = base.transform.position;
		position.y = floorPositions[targetFloor].transform.position.y + 1f;
		return position;
	}

	public void SetFloorDetails(int floor, ElevatorStatic owner)
	{
		ownerElevator = owner;
		base.Floor = floor;
	}

	public override void CallElevator()
	{
		if (ownerElevator != null)
		{
			ownerElevator.RequestMoveLiftTo(base.Floor, out var _, this);
		}
		else if (base.IsTop)
		{
			RequestMoveLiftTo(base.Floor, out var _, this);
		}
	}

	public ElevatorStatic ElevatorAtFloor(int floor)
	{
		if (floor == base.Floor)
		{
			return this;
		}
		if (floor >= 0 && floor < floorPositions.Count)
		{
			return floorPositions[floor];
		}
		return null;
	}

	protected override void OpenDoorsAtFloor(int floor)
	{
		base.OpenDoorsAtFloor(floor);
		if (floor == floorPositions.Count)
		{
			OpenLiftDoors();
		}
		else
		{
			floorPositions[floor].OpenLiftDoors();
		}
	}

	public override void OnMoveBegin()
	{
		base.OnMoveBegin();
		ElevatorStatic elevatorStatic = ElevatorAtFloor(LiftPositionToFloor());
		if (elevatorStatic != null)
		{
			elevatorStatic.OnLiftLeavingFloor();
		}
		NotifyLiftEntityDoorsOpen(state: false);
	}

	public void OnLiftLeavingFloor()
	{
		ClearPowerOutput();
		if (IsInvoking(ClearPowerOutput))
		{
			CancelInvoke(ClearPowerOutput);
		}
	}

	public override void ClearBusy()
	{
		base.ClearBusy();
		ElevatorStatic elevatorStatic = ElevatorAtFloor(LiftPositionToFloor());
		if (elevatorStatic != null)
		{
			elevatorStatic.OnLiftArrivedAtFloor();
		}
		NotifyLiftEntityDoorsOpen(state: true);
	}

	protected override void OpenLiftDoors()
	{
		base.OpenLiftDoors();
		OnLiftArrivedAtFloor();
	}

	public void OnLiftArrivedAtFloor()
	{
		SetFlag(Flags.Reserved3, b: true);
		MarkDirty();
		Invoke(ClearPowerOutput, 10f);
	}

	public void ClearPowerOutput()
	{
		SetFlag(Flags.Reserved3, b: false);
		MarkDirty();
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!HasFlag(Flags.Reserved3))
		{
			return 0;
		}
		return 1;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			SetFlag(Flags.Reserved3, b: false);
		}
	}
}
