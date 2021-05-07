using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours.Attacks
{
	public class CircularProjectileAttack : ActionAttack
	{
		private Vector3 _originalScale;

		private float _originalDirection;

		[SerializeField]
		private Transform _centerAxisPosition;

		[SerializeField]
		private Transform _weaponAxisPosition;

		[SerializeField]
		private bool _continuousLooking;

		[SerializeField]
		private bool _autoAim;

		private void Awake()
		{
			_originalScale = Vector3.one;
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			Character target = controller.target;
			base.result = Result.Doing;
			character.lookingDirection = ((!((target.transform.position - character.transform.position).x > 0f)) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
			Vector3 vector = target.transform.position - character.transform.position;
			float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			Vector3 originalScale = _originalScale;
			Vector3 originalScale2 = _originalScale;
			if ((num > 90f && num < 270f) || num < -90f)
			{
				originalScale.x *= -1f;
				originalScale2.y *= -1f;
				originalScale2.x *= -1f;
			}
			_weaponAxisPosition.localScale = originalScale;
			_centerAxisPosition.localScale = originalScale2;
			if (_autoAim)
			{
				yield return TakeAim(character, target);
			}
			if (!attack.TryStart())
			{
				yield break;
			}
			while (attack.running)
			{
				yield return null;
				if (_continuousLooking)
				{
					character.lookingDirection = ((!((target.transform.position - character.transform.position).x > 0f)) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
					vector = target.transform.position - character.transform.position;
					num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
					originalScale = _originalScale;
					originalScale2 = _originalScale;
					if ((num > 90f && num < 270f) || num < -90f)
					{
						originalScale.x *= -1f;
						originalScale2.y *= -1f;
						originalScale2.x *= -1f;
					}
					_weaponAxisPosition.localScale = originalScale;
					_centerAxisPosition.localScale = originalScale2;
				}
			}
			yield return idle.CRun(controller);
		}

		private IEnumerator TakeAim(Character character, Character target)
		{
			while (attack.running)
			{
				Vector3 vector = target.transform.position - character.transform.position;
				float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				_centerAxisPosition.rotation = Quaternion.Euler(0f, 0f, _originalDirection + num);
				yield return null;
			}
		}
	}
}
