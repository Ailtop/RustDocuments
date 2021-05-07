using System;
using Characters.Actions;
using Characters.Gear.Weapons.Gauges;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class WeaponPolymorph : Ability
	{
		public class Instance : AbilityInstance<WeaponPolymorph>
		{
			public override int iconStacks => 0;

			public override float iconFillAmount => 0f;

			public Instance(Character owner, WeaponPolymorph ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				ability._gauge.Add(ability._gauge.maxValue);
				if (ability._enteringAction != null)
				{
					ability._enteringAction.TryStart();
				}
			}

			protected override void OnDetach()
			{
				ability._exitingOperation.Run(owner);
			}

			public override void UpdateTime(float deltaTime)
			{
				ability._gauge.Add((0f - ability._gaugeLosingAmountPerSecond) * deltaTime);
				if (!(ability._gauge.currentValue > 0f))
				{
					owner.playerComponents.inventory.weapon.Unpolymorph();
				}
			}
		}

		[Header("Gauge")]
		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private float _gaugeLosingAmountPerSecond;

		[Header("Actions")]
		[SerializeField]
		[Tooltip("비워두면 없는 걸로 처리됨")]
		private Characters.Actions.Action _enteringAction;

		[Space]
		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _exitingOperation;

		public override void Initialize()
		{
			base.Initialize();
			_exitingOperation.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
