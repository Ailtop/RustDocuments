using Characters.AI;
using Services;
using Singletons;
using UnityEngine;

namespace Characters
{
	public class EnemyCharacterSpecificator : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private Vector2 _movementSpeedRange = new Vector2(-0.2f, 0.2f);

		[SerializeField]
		private float _speedBonusAtChaseTarget = 0.3f;

		private AIController _aiController;

		private Stat.Values _statValue;

		private bool _movementSpeedAttached;

		private void Awake()
		{
			_aiController = _character.GetComponentInChildren<AIController>();
			_statValue = new Stat.Values(new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MovementSpeed, 0.0), new Stat.Value(Stat.Category.Constant, Stat.Kind.MovementSpeed, Random.Range(_movementSpeedRange.x, _movementSpeedRange.y)), new Stat.Value(Stat.Category.Percent, Stat.Kind.Health, Singleton<Service>.Instance.levelManager.currentChapter.currentStage.healthMultiplier));
			_character.stat.AttachValues(_statValue);
		}

		private void Update()
		{
			if (_aiController == null)
			{
				return;
			}
			if (_aiController.target == null)
			{
				if (_movementSpeedAttached)
				{
					_statValue.values[0].value = 0.0;
					_movementSpeedAttached = false;
					_character.stat.SetNeedUpdate();
				}
			}
			else if (!_movementSpeedAttached)
			{
				_statValue.values[0].value = _speedBonusAtChaseTarget;
				_movementSpeedAttached = true;
				_character.stat.SetNeedUpdate();
			}
		}
	}
}
