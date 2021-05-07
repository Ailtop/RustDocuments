using UnityEngine;

namespace Level.MapEvent.Behavior
{
	public class SpawnWave : Behavior
	{
		[SerializeField]
		private EnemyWave _wave;

		[SerializeField]
		private bool _showEffect = true;

		public override void Run()
		{
			_wave.Spawn(_showEffect);
		}
	}
}
