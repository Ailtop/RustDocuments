using Characters.Actions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.AI
{
	public class EnemyDieAction : MonoBehaviour
	{
		[SerializeField]
		private AIController _aiController;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		[Subcomponent(true, typeof(SequentialAction))]
		[FormerlySerializedAs("_onDied")]
		private SequentialAction _onDie;

		[SerializeField]
		private int _deathRattleCount;

		private void Awake()
		{
			_aiController.character.health.onDie += OnDie;
		}

		private void OnDie()
		{
			if (!_aiController.dead)
			{
				_aiController.StopAllCoroutinesWithBehaviour();
				_onDie?.TryStart();
				if (_deathRattleCount > 0)
				{
					_deathRattleCount--;
					_aiController.character.health.ResetToMaximumHealth();
				}
				else
				{
					_aiController.character.health.onDie -= OnDie;
				}
			}
		}
	}
}
