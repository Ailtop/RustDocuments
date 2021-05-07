using Level;

namespace Runnables.Triggers
{
	public class StoppedEnemyContainer : Trigger
	{
		protected override bool Check()
		{
			EnemyWaveContainer waveContainer = Map.Instance.waveContainer;
			if (waveContainer.enemyWaves.Length != 0)
			{
				return waveContainer.state != EnemyWaveContainer.State.Remain;
			}
			return true;
		}
	}
}
