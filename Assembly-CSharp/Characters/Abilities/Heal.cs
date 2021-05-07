using System;
using System.Collections;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class Heal : Ability
	{
		public class Instance : AbilityInstance<Heal>
		{
			private CoroutineReference _cHealReference;

			public Instance(Character owner, Heal ability)
				: base(owner, ability)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				OnAttach();
			}

			protected override void OnAttach()
			{
				_cHealReference.Stop();
				_cHealReference = owner.StartCoroutineWithReference(CHeal());
			}

			protected override void OnDetach()
			{
				_cHealReference.Stop();
			}

			private IEnumerator CHeal()
			{
				for (int i = 0; i < ability._count; i++)
				{
					owner.health.PercentHeal((float)(ability._totalPercent / ability._count) * 0.01f);
					yield return owner.chronometer.master.WaitForSeconds(ability.duration / (float)ability._count);
				}
			}
		}

		[SerializeField]
		private int _totalPercent;

		[SerializeField]
		private int _count = 3;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
