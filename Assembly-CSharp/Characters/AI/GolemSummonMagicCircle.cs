using UnityEngine;

namespace Characters.AI
{
	public sealed class GolemSummonMagicCircle : MonoBehaviour
	{
		[SerializeField]
		private AlchemistSummonerAI _leftAlchemist;

		[SerializeField]
		private AlchemistSummonerAI _rightAlchemist;

		[SerializeField]
		private GameObject _effectContainer;

		private int _deathCount;

		private void OnEnable()
		{
			_leftAlchemist.character.health.onDied += CountDeath;
			_rightAlchemist.character.health.onDied += CountDeath;
		}

		private void CountDeath()
		{
			_deathCount++;
			if (_deathCount >= 2)
			{
				_effectContainer.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			if (_leftAlchemist != null && !_leftAlchemist.dead)
			{
				_leftAlchemist.character.health.onDied -= CountDeath;
			}
			if (_rightAlchemist != null && !_leftAlchemist.dead)
			{
				_rightAlchemist.character.health.onDied -= CountDeath;
			}
		}
	}
}
