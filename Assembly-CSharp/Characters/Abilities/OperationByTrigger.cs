using System;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class OperationByTrigger : Ability
	{
		public class Instance : AbilityInstance<OperationByTrigger>
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

			public Instance(Character owner, OperationByTrigger ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				ability.trigger.Attach(owner);
				ability.trigger.onTriggered += OnTriggered;
			}

			protected override void OnDetach()
			{
				ability.trigger.Detach();
				ability.trigger.onTriggered -= OnTriggered;
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainCooldownTime -= deltaTime;
				ability.trigger.UpdateTime(deltaTime);
			}

			private void OnTriggered()
			{
				if (!(_remainCooldownTime > 0f))
				{
					for (int i = 0; i < ability.operations.Length; i++)
					{
						ability.operations[i].Run(owner);
					}
					_remainCooldownTime = ability._cooldownTime;
				}
			}
		}

		public ITrigger trigger;

		[NonSerialized]
		public CharacterOperation[] operations;

		[SerializeField]
		private float _cooldownTime;

		public OperationByTrigger()
		{
		}

		public OperationByTrigger(ITrigger trigger, CharacterOperation[] operations)
		{
			this.trigger = trigger;
			this.operations = operations;
		}

		public override void Initialize()
		{
			base.Initialize();
			for (int i = 0; i < operations.Length; i++)
			{
				operations[i].Initialize();
			}
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
