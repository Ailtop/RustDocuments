using System;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class Skeleton_ShieldExplosionPassive : Ability
	{
		public class Instance : AbilityInstance<Skeleton_ShieldExplosionPassive>
		{
			public Instance(Character owner, Skeleton_ShieldExplosionPassive ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.health.shield.onUpdate += OnShieldRemoveOrUpdate;
				owner.health.shield.onRemove += OnShieldRemoveOrUpdate;
			}

			protected override void OnDetach()
			{
				owner.health.shield.onUpdate -= OnShieldRemoveOrUpdate;
				owner.health.shield.onRemove -= OnShieldRemoveOrUpdate;
			}

			private void OnShieldRemoveOrUpdate(Characters.Shield.Instance shieldInstance)
			{
				ability.component.attackDamage = (float)shieldInstance.originalAmount;
				ability._operation.Run(owner);
			}
		}

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operation;

		[NonSerialized]
		public Skeleton_ShieldExplosionPassiveComponent component;

		public override void Initialize()
		{
			base.Initialize();
			_operation.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
