using System;
using Characters.Operations;
using Level;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class ThiefPassive : Ability
	{
		public class Instance : AbilityInstance<ThiefPassive>
		{
			public Instance(Character owner, ThiefPassive ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				ThiefGold.onDespawn += OnThiefGoldDespawn;
			}

			protected override void OnDetach()
			{
				ThiefGold.onDespawn -= OnThiefGoldDespawn;
			}

			private void OnThiefGoldDespawn(double goldAmount, Vector3 position)
			{
				ability._goldDespawnPosition.transform.position = position;
				ability._operationOnGoldDespawn.Run(owner);
			}
		}

		[Tooltip("골드가 디스폰 되는 시점에 이 트랜스폼이 그 지점으로 이동됩니다.")]
		[SerializeField]
		private Transform _goldDespawnPosition;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operationOnGoldDespawn;

		public override void Initialize()
		{
			base.Initialize();
			_operationOnGoldDespawn.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
