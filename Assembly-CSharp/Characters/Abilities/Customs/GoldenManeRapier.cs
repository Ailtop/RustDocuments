using System;
using Characters.Actions;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class GoldenManeRapier : Ability
	{
		public class Instance : AbilityInstance<GoldenManeRapier>
		{
			private int _currentBasicAttackCount;

			public override int iconStacks => _currentBasicAttackCount;

			public Instance(Character owner, GoldenManeRapier ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.onStartAction += OnOwnerStartAction;
			}

			protected override void OnDetach()
			{
				owner.onStartAction -= OnOwnerStartAction;
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
			}

			private void OnOwnerStartAction(Characters.Actions.Action action)
			{
				if (action.type == Characters.Actions.Action.Type.BasicAttack || action.type == Characters.Actions.Action.Type.JumpAttack)
				{
					_currentBasicAttackCount++;
					if (!((float)_currentBasicAttackCount < ability._basicAttackCount))
					{
						_currentBasicAttackCount = 0;
						ability._operations.Run(owner);
					}
				}
			}
		}

		[SerializeField]
		private float _basicAttackCount = 3f;

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
