using Level;
using UnityEngine;

namespace Runnables
{
	public class SpawnWave : Runnable
	{
		[SerializeField]
		private EnemyWave _wave;

		[SerializeField]
		private bool _showEffect;

		public override void Run()
		{
			_wave.Spawn(_showEffect);
		}
	}
}
