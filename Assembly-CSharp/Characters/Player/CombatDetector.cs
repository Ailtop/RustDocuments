using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Characters.Player
{
	public sealed class CombatDetector
	{
		private const float _combatRetentionTime = 3f;

		private readonly Character _owner;

		private float _remainCombatTime;

		public bool inCombat { get; private set; }

		public float remainTimePercent => _remainCombatTime / 3f;

		public event Action onBeginCombat;

		public event Action onFinishCombat;

		internal CombatDetector(Character owner)
		{
			_owner = owner;
			Character owner2 = _owner;
			owner2.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(owner2.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			if (_owner.health != null)
			{
				_owner.health.onTookDamage += OnTookDamage;
			}
		}

		private void Begin()
		{
			if (!inCombat)
			{
				inCombat = true;
				this.onBeginCombat?.Invoke();
			}
			_remainCombatTime = 3f;
		}

		private void Finish()
		{
			inCombat = false;
			this.onFinishCombat?.Invoke();
		}

		private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			Begin();
		}

		private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			Begin();
		}

		public void Update(float deltaTime)
		{
			if (inCombat)
			{
				_remainCombatTime -= deltaTime;
				if (_remainCombatTime <= 0f)
				{
					Finish();
				}
			}
		}
	}
}
