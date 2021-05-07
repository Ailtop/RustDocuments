using UnityEngine;

namespace Level.MapEvent.Condition
{
	public class WaveEvent : Condition
	{
		private enum Type
		{
			Spawn,
			Clear
		}

		[SerializeField]
		private EnemyWave _wave;

		[SerializeField]
		private Type _type = Type.Clear;

		private void Awake()
		{
			if (_type == Type.Spawn)
			{
				_wave.onSpawn += base.Run;
			}
			if (_type == Type.Clear)
			{
				_wave.onClear += base.Run;
			}
		}
	}
}
