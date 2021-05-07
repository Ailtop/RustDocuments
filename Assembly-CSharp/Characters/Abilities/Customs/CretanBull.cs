using System;
using Characters.Actions;
using Characters.Gear.Weapons;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class CretanBull : Ability
	{
		public class Instance : AbilityInstance<CretanBull>
		{
			private float _remainCooldownTime;

			public override float iconFillAmount
			{
				get
				{
					if (ability._cooldownTime > 0f)
					{
						return 1f - _remainCooldownTime / ability._cooldownTime;
					}
					return base.iconFillAmount;
				}
			}

			public Instance(Character owner, CretanBull ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				if (ability._actionTiming == Timing.Start)
				{
					owner.onStartAction += OnCharacterAction;
				}
				else if (ability._actionTiming == Timing.End)
				{
					owner.onCancelAction += OnCharacterAction;
					owner.onEndAction += OnCharacterAction;
				}
			}

			protected override void OnDetach()
			{
				if (ability._actionTiming == Timing.Start)
				{
					owner.onStartAction -= OnCharacterAction;
				}
				else if (ability._actionTiming == Timing.End)
				{
					owner.onCancelAction -= OnCharacterAction;
					owner.onEndAction -= OnCharacterAction;
				}
			}

			private void OnCharacterAction(Characters.Actions.Action action)
			{
				if (ability._actionTypeFilter.GetOrDefault(action.type) && (action.type != Characters.Actions.Action.Type.Skill || !action.cooldown.usedByStreak) && !(_remainCooldownTime > 0f) && owner.playerComponents.inventory.weapon.polymorphOrCurrent.category == ability._headCategory)
				{
					ability._operations.Run(owner);
					_remainCooldownTime = ability._cooldownTime;
				}
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCooldownTime -= deltaTime;
			}
		}

		public enum Timing
		{
			Start,
			End
		}

		[SerializeField]
		private Weapon.Category _headCategory;

		[SerializeField]
		private Timing _actionTiming;

		[SerializeField]
		private ActionTypeBoolArray _actionTypeFilter;

		[SerializeField]
		private float _cooldownTime;

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
