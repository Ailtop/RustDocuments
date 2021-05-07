using Level;
using UnityEngine;

namespace Runnables.Triggers
{
	public class WaveOnClear : Trigger
	{
		[SerializeField]
		private EnemyWave _wave;

		private bool _result;

		private void Start()
		{
			_wave.onClear += delegate
			{
				_result = true;
			};
		}

		protected override bool Check()
		{
			return _result;
		}
	}
}
