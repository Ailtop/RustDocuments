using Oxide.Core;
using UnityEngine;

public class VehicleEngineController<TOwner> where TOwner : BaseVehicle, IEngineControllerUser
{
	public enum EngineState
	{
		Off = 0,
		Starting = 1,
		On = 2
	}

	public readonly TOwner owner;

	private readonly bool isServer;

	public readonly float engineStartupTime;

	public readonly Transform waterloggedPoint;

	public readonly BaseEntity.Flags engineStartingFlag;

	public EngineState CurEngineState
	{
		get
		{
			if (owner.HasFlag(engineStartingFlag))
			{
				return EngineState.Starting;
			}
			if (owner.HasFlag(BaseEntity.Flags.On))
			{
				return EngineState.On;
			}
			return EngineState.Off;
		}
	}

	public bool IsOn => CurEngineState == EngineState.On;

	public bool IsOff => CurEngineState == EngineState.Off;

	public bool IsStarting => CurEngineState == EngineState.Starting;

	public bool IsStartingOrOn => CurEngineState != EngineState.Off;

	public EntityFuelSystem FuelSystem { get; private set; }

	public VehicleEngineController(TOwner owner, bool isServer, float engineStartupTime, GameObjectRef fuelStoragePrefab, Transform waterloggedPoint = null, BaseEntity.Flags engineStartingFlag = BaseEntity.Flags.Reserved1)
	{
		FuelSystem = new EntityFuelSystem(isServer, fuelStoragePrefab, owner.children);
		this.owner = owner;
		this.isServer = isServer;
		this.engineStartupTime = engineStartupTime;
		this.waterloggedPoint = waterloggedPoint;
		this.engineStartingFlag = engineStartingFlag;
	}

	public void TryStartEngine(BasePlayer player)
	{
		if (isServer && !owner.IsDead() && !IsStartingOrOn && player.net != null)
		{
			if (!CanRunEngine())
			{
				owner.OnEngineStartFailed();
			}
			else if (Interface.CallHook("OnEngineStart", ((VehicleEngineController<>)(object)this).owner, player) == null)
			{
				owner.SetFlag(engineStartingFlag, b: true);
				owner.SetFlag(BaseEntity.Flags.On, b: false);
				owner.Invoke(FinishStartingEngine, engineStartupTime);
				Interface.CallHook("OnEngineStarted", ((VehicleEngineController<>)(object)this).owner, player);
			}
		}
	}

	public void FinishStartingEngine()
	{
		if (isServer && !owner.IsDead() && !IsOn)
		{
			owner.SetFlag(BaseEntity.Flags.On, b: true);
			owner.SetFlag(engineStartingFlag, b: false);
			Interface.CallHook("OnEngineStartFinished", ((VehicleEngineController<>)(object)this).owner);
		}
	}

	public void StopEngine()
	{
		if (isServer && !IsOff && Interface.CallHook("OnEngineStop", ((VehicleEngineController<>)(object)this).owner) == null)
		{
			CancelEngineStart();
			owner.SetFlag(BaseEntity.Flags.On, b: false);
			owner.SetFlag(engineStartingFlag, b: false);
			Interface.CallHook("OnEngineStopped", ((VehicleEngineController<>)(object)this).owner);
		}
	}

	public EngineState EngineStateFrom(BaseEntity.Flags flags)
	{
		if (flags.HasFlag(engineStartingFlag))
		{
			return EngineState.Starting;
		}
		if (flags.HasFlag(BaseEntity.Flags.On))
		{
			return EngineState.On;
		}
		return EngineState.Off;
	}

	public void CheckEngineState()
	{
		if (IsStartingOrOn && !CanRunEngine())
		{
			StopEngine();
		}
	}

	public bool CanRunEngine()
	{
		if (owner.MeetsEngineRequirements() && FuelSystem.HasFuel() && !IsWaterlogged())
		{
			return !owner.IsDead();
		}
		return false;
	}

	public bool IsWaterlogged()
	{
		if (waterloggedPoint != null)
		{
			return WaterLevel.Test(waterloggedPoint.position, waves: true, owner);
		}
		return false;
	}

	public int TickFuel(float fuelPerSecond)
	{
		if (IsOn)
		{
			return FuelSystem.TryUseFuel(Time.fixedDeltaTime, fuelPerSecond);
		}
		return 0;
	}

	public void CancelEngineStart()
	{
		if (CurEngineState == EngineState.Starting)
		{
			owner.CancelInvoke(FinishStartingEngine);
		}
	}
}
