using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public class EnemyDieEffect : CharacterDieEffect
	{
		[SerializeField]
		private AIController _aiController;

		[SerializeField]
		[Subcomponent(true, typeof(SequentialAction))]
		private SequentialAction _onDie;

		protected override void Awake()
		{
			base.Awake();
			_character.onDie += OnDie;
		}

		protected void OnDie()
		{
			if (!_aiController.dead)
			{
				_character.collider.enabled = false;
				_aiController.StopAllCoroutinesWithBehaviour();
				_onDie?.TryStart();
				_character.onDie -= OnDie;
			}
		}
	}
}
