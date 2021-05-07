using System;
using Characters.Movements;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class OverrideMovementConfig : Ability
	{
		public class Instance : AbilityInstance<OverrideMovementConfig>
		{
			public Instance(Character owner, OverrideMovementConfig ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.movement.configs.Add(ability._priority, ability._config);
				if (ability._config.keepMove)
				{
					owner.movement.Move((owner.lookingDirection == Character.LookingDirection.Right) ? Vector2.right : Vector2.left);
				}
			}

			protected override void OnDetach()
			{
				owner.movement.configs.Remove(ability._config);
			}
		}

		[SerializeField]
		private Movement.Config _config;

		[SerializeField]
		private int _priority;

		public OverrideMovementConfig()
		{
		}

		public OverrideMovementConfig(Movement.Config config)
		{
			_config = config;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
