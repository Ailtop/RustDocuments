using System;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class ModifyTimeScale : Ability
	{
		public enum ChronometerType
		{
			Master,
			Animation,
			Effect,
			Projectile
		}

		public class Instance : AbilityInstance<ModifyTimeScale>
		{
			public Instance(Character owner, ModifyTimeScale ability)
				: base(owner, ability)
			{
			}

			private Chronometer GetChronometer()
			{
				switch (ability._chronometerType)
				{
				case ChronometerType.Animation:
					return owner.chronometer.animation;
				case ChronometerType.Effect:
					return owner.chronometer.effect;
				case ChronometerType.Projectile:
					return owner.chronometer.projectile;
				default:
					return owner.chronometer.master;
				}
			}

			protected override void OnAttach()
			{
				GetChronometer().AttachTimeScale(this, ability._timeScale);
				Chronometer.global.AttachTimeScale(this, ability._globalTimeScale);
			}

			protected override void OnDetach()
			{
				GetChronometer().DetachTimeScale(this);
				Chronometer.global.DetachTimeScale(this);
			}
		}

		[SerializeField]
		private ChronometerType _chronometerType;

		[SerializeField]
		private float _timeScale = 1f;

		[SerializeField]
		private float _globalTimeScale = 1f;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
