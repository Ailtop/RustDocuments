using System;
using System.Collections;
using Characters.Abilities;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Alchemy : Keyword
	{
		[Serializable]
		public class StatBonus : Ability
		{
			public class Instance : AbilityInstance<StatBonus>
			{
				public Instance(Character owner, StatBonus ability)
					: base(owner, ability)
				{
				}

				protected override void OnAttach()
				{
					owner.stat.AttachValues(ability.stat);
				}

				protected override void OnDetach()
				{
					owner.stat.DetachValues(ability.stat);
				}
			}

			[NonSerialized]
			public Stat.Values stat;

			public override IAbilityInstance CreateInstance(Character owner)
			{
				return new Instance(owner, this);
			}
		}

		private Stat.Values _stat = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.TakingDamage, 1.0));

		[SerializeField]
		private float[] _damageMultiplierByLevel;

		[SerializeField]
		private StatBonus _statBonus;

		public override Key key => Key.Alchemy;

		protected override IList valuesByLevel => _damageMultiplierByLevel;

		protected override void Initialize()
		{
			_statBonus.stat = _stat;
			_statBonus.Initialize();
			_statBonus.duration = float.PositiveInfinity;
		}

		protected override void UpdateBonus()
		{
			_stat.values[0].value = (double)_damageMultiplierByLevel[base.level] * 0.01 + 1.0;
		}

		protected override void OnAttach()
		{
			Character obj = base.character;
			obj.onGaveStatus = (Character.OnGaveStatusDelegate)Delegate.Combine(obj.onGaveStatus, new Character.OnGaveStatusDelegate(OnGaveStatus));
		}

		protected override void OnDetach()
		{
			Character obj = base.character;
			obj.onGaveStatus = (Character.OnGaveStatusDelegate)Delegate.Remove(obj.onGaveStatus, new Character.OnGaveStatusDelegate(OnGaveStatus));
		}

		private void OnGaveStatus(Character target, CharacterStatus.ApplyInfo applyInfo, bool result)
		{
			if (result && !((double)applyInfo.duration * target.stat.GetStatusResistacneFor(applyInfo.kind) < 1.4012984643248171E-45))
			{
				target.ability.Add(_statBonus);
			}
		}
	}
}
