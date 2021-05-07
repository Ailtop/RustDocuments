using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours.Attacks
{
	public class HorizontalProjectileAttack : ActionAttack
	{
		[SerializeField]
		private Transform _weapon;

		private Vector3 _originalScale;

		private float _originalDircetion;

		private void Awake()
		{
			_originalScale = base.transform.localScale;
			_originalDircetion = base.transform.rotation.eulerAngles.z;
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			base.result = Result.Doing;
			Vector3 vector = controller.target.transform.position - character.transform.position;
			float x = vector.x;
			float num = 0f;
			if (vector.x < 0f)
			{
				character.lookingDirection = Character.LookingDirection.Left;
				Vector3 originalScale = _originalScale;
				originalScale.y *= -1f;
				_weapon.localScale = originalScale * -1f;
			}
			else
			{
				character.lookingDirection = Character.LookingDirection.Right;
				_weapon.localScale = _originalScale;
			}
			if (attack.TryStart())
			{
				gaveDamage = false;
				yield return attack.CWaitForEndOfRunning();
				if (!gaveDamage)
				{
					base.result = Result.Success;
					yield return character.chronometer.animation.WaitForSeconds(1.5f);
				}
				else
				{
					base.result = Result.Fail;
				}
			}
			else
			{
				base.result = Result.Fail;
			}
		}
	}
}
