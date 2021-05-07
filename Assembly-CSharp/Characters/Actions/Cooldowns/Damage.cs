using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Actions.Cooldowns
{
	public class Damage : Basic
	{
		[SerializeField]
		protected float _damagePerStack;

		[SerializeField]
		[ReadOnly(true)]
		protected bool _stackOnGive;

		[SerializeField]
		[ReadOnly(true)]
		protected bool _stackOnTake;

		protected double _stackedDamage;

		public override float remainPercent
		{
			get
			{
				if (base.stacks <= 0)
				{
					return 1f - (float)_stackedDamage / _damagePerStack;
				}
				return 1f;
			}
		}

		protected virtual void OnEnable()
		{
			if (_stackOnGive)
			{
				Character character = _character;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(StackDamage));
			}
		}

		protected virtual void OnDisable()
		{
			if (_stackOnGive)
			{
				Character character = _character;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(StackDamage));
			}
		}

		protected virtual void OnResume()
		{
			if (_stackOnGive)
			{
				Character character = _character;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(StackDamage));
			}
		}

		private void StackDamage(ITarget target, [In][IsReadOnly] ref Characters.Damage originalDamage, [In][IsReadOnly] ref Characters.Damage tookDamage, double damageDealt)
		{
			if (base.stacks != _maxStacks)
			{
				_stackedDamage += damageDealt;
				if (_stackedDamage >= (double)_damagePerStack)
				{
					_stackedDamage = 0.0;
					base.stacks++;
				}
			}
		}
	}
}
