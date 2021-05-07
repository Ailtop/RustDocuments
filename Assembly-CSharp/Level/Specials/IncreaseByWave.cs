using UnityEngine;

namespace Level.Specials
{
	[RequireComponent(typeof(EnemyWave))]
	public class IncreaseByWave : MonoBehaviour
	{
		[GetComponent]
		[SerializeField]
		private EnemyWave _wave;

		[SerializeField]
		private TimeCostEvent _event;

		[SerializeField]
		private double _amountPerSeconds;

		private void Awake()
		{
			_wave.onSpawn += delegate
			{
				_event.AddIncrease(_amountPerSeconds * (double)_event.updateInterval);
			};
		}
	}
}
