using Level;

namespace Runnables.Triggers
{
	public class MapRewardActivated : Trigger
	{
		protected override bool Check()
		{
			return Map.Instance.mapReward.activated;
		}
	}
}
