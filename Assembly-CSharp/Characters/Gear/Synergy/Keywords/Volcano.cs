using System;
using System.Collections;
using Characters.Abilities;
using Characters.Abilities.Constraints;
using Characters.Operations;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Volcano : Keyword
	{
		[Serializable]
		private class Ability : IAbility, IAbilityInstance
		{
			[SerializeField]
			[Constraint.Subcomponent]
			private Constraint.Subcomponents _constraints;

			[NonSerialized]
			public int level;

			private Character _owner;

			[SerializeField]
			private Sprite _icon;

			[SerializeField]
			[CharacterOperation.Subcomponent]
			private CharacterOperation.Subcomponents _operations;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon
			{
				get
				{
					if (level <= 0)
					{
						return null;
					}
					return _icon;
				}
			}

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

			public void AttachTo(Character owner)
			{
				_owner = owner;
				owner.ability.Add(this);
			}

			public void Initialize()
			{
				_operations.Initialize();
			}

			public void UpdateTime(float deltaTime)
			{
				if (level < 1)
				{
					return;
				}
				remainTime -= deltaTime;
				if (remainTime < 0f)
				{
					remainTime += duration;
					if (_constraints.components.Pass())
					{
						_operations.Run(_owner);
					}
				}
			}

			public void Refresh()
			{
			}

			void IAbilityInstance.Attach()
			{
			}

			void IAbilityInstance.Detach()
			{
			}
		}

		[SerializeField]
		private Ability _ability;

		[SerializeField]
		private float[] _cooldownByLevel = new float[6] { 0f, 10f, 8f, 6f, 4f, 2f };

		public override Key key => Key.Volcano;

		protected override IList valuesByLevel => _cooldownByLevel;

		protected override void Initialize()
		{
			_ability.Initialize();
			_ability.AttachTo(base.character);
		}

		protected override void UpdateBonus()
		{
			_ability.level = base.level;
			_ability.duration = _cooldownByLevel[base.level];
		}

		protected override void OnAttach()
		{
		}

		protected override void OnDetach()
		{
		}
	}
}
