namespace Rust.Modular;

public static class EngineItemTypeEx
{
	public static bool BoostsAcceleration(this EngineStorage.EngineItemTypes engineItemType)
	{
		if (engineItemType != EngineStorage.EngineItemTypes.SparkPlug)
		{
			return engineItemType == EngineStorage.EngineItemTypes.Piston;
		}
		return true;
	}

	public static bool BoostsTopSpeed(this EngineStorage.EngineItemTypes engineItemType)
	{
		if (engineItemType != EngineStorage.EngineItemTypes.Carburetor && engineItemType != 0)
		{
			return engineItemType == EngineStorage.EngineItemTypes.Piston;
		}
		return true;
	}

	public static bool BoostsFuelEconomy(this EngineStorage.EngineItemTypes engineItemType)
	{
		if (engineItemType != EngineStorage.EngineItemTypes.Carburetor)
		{
			return engineItemType == EngineStorage.EngineItemTypes.Valve;
		}
		return true;
	}
}
