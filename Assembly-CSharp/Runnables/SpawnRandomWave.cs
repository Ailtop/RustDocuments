using System.Collections.Generic;
using System.Linq;
using Level;
using UnityEngine;

namespace Runnables
{
	public class SpawnRandomWave : Runnable
	{
		[SerializeField]
		private bool _effectOnSpawned;

		public override void Run()
		{
			IEnumerable<EnemyWave> enumerable = Map.Instance.waveContainer.enemyWaves.Where((EnemyWave wave) => wave.state == Wave.State.Waiting);
			if (enumerable.Count() != 0)
			{
				enumerable.Random().Spawn(_effectOnSpawned);
			}
		}
	}
}
