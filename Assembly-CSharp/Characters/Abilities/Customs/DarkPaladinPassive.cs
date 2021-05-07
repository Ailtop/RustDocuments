using System;
using Characters.Actions;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class DarkPaladinPassive : Ability, IAbilityInstance
	{
		[SerializeField]
		private int _shieldAmount;

		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private Stat.Values _stat;

		[Space]
		[SerializeField]
		private EnhanceableComboAction _enhanceableComboAction;

		private ReusableChronoSpriteEffect _loopEffectInstance;

		private Characters.Shield.Instance _shieldInstance;

		public int iconStacks => (int)_shieldInstance.amount;

		public Character owner { get; set; }

		public IAbility ability => this;

		public float remainTime { get; set; }

		public bool attached => true;

		public Sprite icon => _defaultIcon;

		public float iconFillAmount => 1f - remainTime / base.duration;

		public bool expired => remainTime <= 0f;

		public void Refresh()
		{
			remainTime = base.duration;
			_shieldInstance.amount = _shieldAmount;
		}

		private void OnShieldBroke()
		{
			owner.ability.Remove(this);
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			this.owner = owner;
			return this;
		}

		public void UpdateTime(float deltaTime)
		{
			remainTime -= deltaTime;
		}

		public void Attach()
		{
			remainTime = base.duration;
			owner.stat.AttachValues(_stat);
			_shieldInstance = owner.health.shield.Add(ability, _shieldAmount, OnShieldBroke);
			_enhanceableComboAction.enhanced = true;
			base.effectOnAttach.Spawn(owner.transform.position);
			_loopEffectInstance = ((base.loopEffect == null) ? null : base.loopEffect.Spawn(owner.transform.position, owner));
		}

		public void Detach()
		{
			owner.stat.DetachValues(_stat);
			if (_shieldInstance.amount > 0.0 && owner.health.shield.Remove(ability))
			{
				_shieldInstance = null;
			}
			_enhanceableComboAction.enhanced = false;
			if (_loopEffectInstance != null)
			{
				_loopEffectInstance.reusable.Despawn();
				_loopEffectInstance = null;
			}
		}
	}
}
