using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class ChimerasFang : Ability
	{
		public class Instance : AbilityInstance<ChimerasFang>
		{
			public Instance(Character owner, ChimerasFang ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			}

			private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (!(target.character == null) && !target.character.health.dead && target.transform.gameObject.activeSelf && !(target.character.status == null) && target.character.status.poisoned && !(target.character.health.percent > (double)ability._healthPercent * 0.01))
				{
					ability._operationPosition.position = target.transform.position;
					ability._operations.Run(target.character);
					target.character.chronometer.animation.AttachTimeScale(this, ability._timeScaleDuringKilling);
					target.character.StartCoroutine(CDelayedKill(target.character));
				}
			}

			private IEnumerator CDelayedKill(Character target)
			{
				yield return Chronometer.global.WaitForSeconds(ability._killingDelay);
				target.chronometer.animation.DetachTimeScale(this);
				if (!target.health.dead)
				{
					Damage damage = owner.stat.GetDamage(target.health.currentHealth, MMMaths.RandomPointWithinBounds(target.collider.bounds), ability._hitInfo);
					owner.Attack(target, ref damage);
					if (!target.health.dead)
					{
						target.health.Kill();
					}
				}
			}
		}

		[SerializeField]
		private HitInfo _hitInfo;

		[SerializeField]
		private int _healthPercent = 10;

		[SerializeField]
		private float _timeScaleDuringKilling = 0.3f;

		[SerializeField]
		private float _killingDelay = 1f;

		[Space]
		[SerializeField]
		private Transform _operationPosition;

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
