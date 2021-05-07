using Level;
using TMPro;
using UnityEngine;

namespace UI
{
	public class RemainEnemiesDisplay : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _amount;

		[SerializeField]
		private GameObject _container;

		private int _count;

		private void Update()
		{
			if (Map.Instance == null || Map.Instance.waveContainer == null || Map.Instance.waveContainer.enemyWaves.Length == 0)
			{
				if (_container.gameObject.activeSelf)
				{
					_container.gameObject.SetActive(false);
				}
				return;
			}
			if (!_container.gameObject.activeSelf)
			{
				_container.gameObject.SetActive(true);
			}
			int num = 0;
			EnemyWave[] enemyWaves = Map.Instance.waveContainer.enemyWaves;
			foreach (EnemyWave enemyWave in enemyWaves)
			{
				if (enemyWave.state == Wave.State.Spawned)
				{
					num += enemyWave.characters.Count;
				}
			}
			if (_count != num)
			{
				_count = num;
				_amount.text = _count.ToString();
			}
		}
	}
}
