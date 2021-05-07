using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class PlayAnimationOnDied : MonoBehaviour
	{
		[SerializeField]
		private AIController _ai;

		[SerializeField]
		private CharacterAnimationController.AnimationInfo _onDieAnimationInfo;

		private void Start()
		{
			_ai.character.health.onDied += OnDied;
		}

		private void OnDied()
		{
			_ai.character.health.onDied -= OnDied;
			Character character = _ai.character;
			if (!(character == null))
			{
				_ai.StopAllCoroutinesWithBehaviour();
				character.animationController.Play(_onDieAnimationInfo, 1f);
			}
		}
	}
}
