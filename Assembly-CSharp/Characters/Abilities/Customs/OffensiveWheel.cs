using System;
using Characters.Actions;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class OffensiveWheel : Ability
	{
		public class Instance : AbilityInstance<OffensiveWheel>
		{
			private int _currentSkillCount;

			public override Sprite icon
			{
				get
				{
					if (_currentSkillCount <= 0)
					{
						return null;
					}
					return base.icon;
				}
			}

			public override int iconStacks => _currentSkillCount;

			public override float iconFillAmount => ((float)_currentSkillCount != ability._skillCount) ? 1 : 0;

			public Instance(Character owner, OffensiveWheel ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.onStartAction += OnOwnerStartAction;
				owner.playerComponents.inventory.weapon.onSwap += OnOwnerSwap;
			}

			protected override void OnDetach()
			{
				owner.onStartAction -= OnOwnerStartAction;
				owner.playerComponents.inventory.weapon.onSwap -= OnOwnerSwap;
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
			}

			private void OnOwnerStartAction(Characters.Actions.Action action)
			{
				if (action.type == Characters.Actions.Action.Type.Skill && ability._skillCount != (float)_currentSkillCount)
				{
					_currentSkillCount++;
				}
			}

			private void OnOwnerSwap()
			{
				if (!((float)_currentSkillCount < ability._skillCount))
				{
					_currentSkillCount = 0;
					ability._operations.Run(owner);
				}
			}
		}

		[SerializeField]
		private float _skillCount = 5f;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operations;

		public override void Initialize()
		{
			base.Initialize();
			_operations.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
