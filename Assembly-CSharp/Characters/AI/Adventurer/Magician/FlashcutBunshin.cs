using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Adventurer.Magician
{
	public class FlashcutBunshin : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private GameObject _body;

		[SerializeField]
		private Action _action;

		private void OnEnable()
		{
			StartCoroutine(CAttack());
		}

		private IEnumerator CAttack()
		{
			Collider2D lastStandingCollider = _character.movement.controller.collisionState.lastStandingCollider;
			while (lastStandingCollider == null)
			{
				yield return null;
				lastStandingCollider = _character.movement.controller.collisionState.lastStandingCollider;
			}
			_character.ForceToLookAt(lastStandingCollider.bounds.center.x);
			_action.TryStart();
			while (_action.running)
			{
				yield return null;
			}
			base.gameObject.SetActive(false);
		}
	}
}
