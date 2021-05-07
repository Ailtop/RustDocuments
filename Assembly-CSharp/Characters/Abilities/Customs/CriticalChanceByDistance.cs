using System;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class CriticalChanceByDistance : Ability
	{
		public class Instance : AbilityInstance<CriticalChanceByDistance>
		{
			private int _remainCount;

			internal Instance(Character owner, CriticalChanceByDistance ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.onGiveDamage.Add(0, OnOwnerGiveDamage);
			}

			protected override void OnDetach()
			{
				owner.onGiveDamage.Remove(OnOwnerGiveDamage);
			}

			private bool OnOwnerGiveDamage(ITarget target, ref Damage damage)
			{
				if (!ability._motionFilter[damage.motionType] || !ability._attackFilter[damage.attackType])
				{
					return false;
				}
				Vector2 a = MMMaths.Vector3ToVector2(target.transform.position);
				Vector2 b = MMMaths.Vector3ToVector2(owner.transform.position);
				float distance = Vector2.Distance(a, b);
				damage.criticalChance += ability.GetBonusCriticalChance(distance);
				return false;
			}
		}

		[SerializeField]
		[Range(0f, 100f)]
		private int _maxBonusCriticalChance;

		[Header("Filter")]
		[SerializeField]
		private MotionTypeBoolArray _motionFilter;

		[SerializeField]
		private AttackTypeBoolArray _attackFilter;

		[Header("Distance")]
		[SerializeField]
		private float _minBonusDistance = 2f;

		[SerializeField]
		private float _maxBonusDistance = 7f;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}

		private float GetBonusCriticalChance(float distance)
		{
			return Mathf.Clamp01((distance - _minBonusDistance) / _maxBonusDistance) * (float)_maxBonusCriticalChance * 0.01f;
		}
	}
}
