using System.Collections;
using Characters.Abilities;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Chase : Keyword
	{
		protected class Ability : IAbility, IAbilityInstance
		{
			public float additionalDamageMultiplier;

			private readonly float _minBonusDistance;

			private readonly float _maxBonusDistance;

			private Character _owner;

			private int _stacks;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon { get; set; }

			public float iconFillAmount => 1f - remainTime / duration;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => 0;

			public bool expired => false;

			public float duration { get; set; }

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public Ability(Character owner, float minBonusDistance, float maxBonusDistance)
			{
				_owner = owner;
				_minBonusDistance = minBonusDistance;
				_maxBonusDistance = maxBonusDistance;
			}

			public void Initialize()
			{
			}

			public void UpdateTime(float deltaTime)
			{
			}

			public void Refresh()
			{
			}

			void IAbilityInstance.Attach()
			{
				_owner.onGiveDamage.Add(0, OnGiveDamage);
			}

			void IAbilityInstance.Detach()
			{
				_owner.onGiveDamage.Remove(OnGiveDamage);
			}

			private bool OnGiveDamage(ITarget target, ref Damage damage)
			{
				Vector2 a = MMMaths.Vector3ToVector2(target.transform.position);
				Vector2 b = MMMaths.Vector3ToVector2(_owner.transform.position);
				float num = Vector2.Distance(a, b) - _minBonusDistance;
				if (num < 0f)
				{
					return false;
				}
				damage.multiplier += Mathf.Min(num / _maxBonusDistance, 1f) * additionalDamageMultiplier;
				return false;
			}
		}

		[SerializeField]
		private float _minBonusDistance = 2f;

		[SerializeField]
		private float _maxBonusDistance = 7f;

		[SerializeField]
		private float[] _multiplierByLevel;

		private Ability _ability;

		public override Key key => Key.Chase;

		protected override IList valuesByLevel => _multiplierByLevel;

		protected override void Initialize()
		{
			_ability = new Ability(base.character, _minBonusDistance, _maxBonusDistance);
		}

		protected override void UpdateBonus()
		{
			_ability.additionalDamageMultiplier = _multiplierByLevel[base.level] * 0.01f;
		}

		protected override void OnAttach()
		{
			base.character.ability.Add(_ability);
		}

		protected override void OnDetach()
		{
			base.character.ability.Remove(_ability);
		}
	}
}
