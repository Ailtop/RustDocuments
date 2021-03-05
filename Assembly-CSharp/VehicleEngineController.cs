using Oxide.Core;

public class VehicleEngineController
{
	public enum EngineState
	{
		Off,
		Starting,
		On
	}

	public IEngineControllerUser owner;

	private bool isServer;

	public float engineStartupTime;

	public BaseEntity.Flags engineStartingFlag;

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

	public VehicleEngineController(IEngineControllerUser owner, bool isServer, float engineStartupTime, BaseEntity.Flags engineStartingFlag = BaseEntity.Flags.Reserved1)
	{
		this.owner = owner;
		this.isServer = isServer;
		this.engineStartupTime = engineStartupTime;
		this.engineStartingFlag = engineStartingFlag;
	}

	public void TryStartEngine(BasePlayer player)
	{
		if (isServer && !owner.IsDead() && CurEngineState == EngineState.Off && player.net != null)
		{
			if (!owner.CanRunEngines())
			{
				owner.OnEngineStartFailed();
			}
			else if (Interface.CallHook("OnEngineStart", owner, player) == null)
			{
				owner.SetFlag(engineStartingFlag, true);
				owner.SetFlag(BaseEntity.Flags.On, false);
				owner.Invoke(FinishStartingEngine, engineStartupTime);
				Interface.CallHook("OnEngineStarted", owner, player);
			}
		}
	}

	public void FinishStartingEngine()
	{
		if (isServer && !owner.IsDead() && CurEngineState != EngineState.On)
		{
			owner.SetFlag(BaseEntity.Flags.On, true);
			owner.SetFlag(engineStartingFlag, false);
		}
	}

	public void StopEngine()
	{
		if (isServer && CurEngineState != 0 && Interface.CallHook("OnEngineStop", this) == null)
		{
			CancelEngineStart();
			owner.SetFlag(BaseEntity.Flags.On, false);
			owner.SetFlag(engineStartingFlag, false);
			Interface.CallHook("OnEngineStopped", this);
		}
	}

	public void CancelEngineStart()
	{
		if (CurEngineState == EngineState.Starting)
		{
			owner.CancelInvoke(FinishStartingEngine);
		}
	}
}
