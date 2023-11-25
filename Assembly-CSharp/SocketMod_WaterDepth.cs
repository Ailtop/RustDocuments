public class SocketMod_WaterDepth : SocketMod
{
	public float MinimumWaterDepth = 2f;

	public float MaximumWaterDepth = 4f;

	public bool AllowWaterVolumes;

	public static Translate.Phrase TooDeepPhrase = new Translate.Phrase("error_toodeep", "Water is too deep");

	public static Translate.Phrase TooShallowPhrase = new Translate.Phrase("error_shallow", "Water is too shallow");

	public override bool DoCheck(Construction.Placement place)
	{
		WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(place.position + place.rotation * worldPosition, waves: false, AllowWaterVolumes, null, noEarlyExit: true);
		if (waterInfo.overallDepth > MinimumWaterDepth && waterInfo.overallDepth < MaximumWaterDepth)
		{
			return true;
		}
		if (waterInfo.overallDepth <= MinimumWaterDepth)
		{
			Construction.lastPlacementError = TooShallowPhrase.translated;
		}
		else
		{
			Construction.lastPlacementError = TooDeepPhrase.translated;
		}
		return false;
	}
}
