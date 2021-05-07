using Level;
using UnityEngine;

namespace Runnables.Triggers
{
	public class WavesOnClear : Trigger
	{
		[SerializeField]
		private EnemyWave[] _waves;

		private int _clearCount;

		private void Start()
		{
			EnemyWave[] waves = _waves;
			for (int i = 0; i < waves.Length; i++)
			{
				waves[i].onClear += delegate
				{
					_clearCount++;
				};
			}
		}

		protected override bool Check()
		{
			return _clearCount >= _waves.Length;
		}
	}
}
