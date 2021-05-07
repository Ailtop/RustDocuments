using System.Collections;
using Characters.Marks;
using Characters.Operations.Attack;
using UnityEngine;

namespace Characters.Operations.Customs.Samurai
{
	public class NapdoSweepAttak2 : SweepAttack2
	{
		[SerializeField]
		private MarkInfo _mark;

		[SerializeField]
		[Tooltip("표식 개수 * _attackLengthMultiplier값까지의 AttackInfo가 적용됨")]
		private float _attackLengthMultiplier;

		protected override IEnumerator CAttack(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target)
		{
			int index = 0;
			float time = 0f;
			if (target.character == null)
			{
				yield break;
			}
			float stack = target.character.mark.GetStack(_mark);
			int length = (int)Mathf.Min(stack * _attackLengthMultiplier, _attackAndEffect.components.Length);
			while (this != null && index < length)
			{
				for (; index < length; index++)
				{
					CastAttackInfoSequence castAttackInfoSequence;
					if (!(time >= (castAttackInfoSequence = _attackAndEffect.components[index]).timeToTrigger))
					{
						break;
					}
					target.character.mark.TakeStack(_mark, 1f / _attackLengthMultiplier);
					Attack(castAttackInfoSequence.attackInfo, origin, direction, distance, raycastHit, target);
				}
				yield return null;
				time += base.owner.chronometer.animation.deltaTime;
			}
			target.character.mark.TakeAllStack(_mark);
		}
	}
}
