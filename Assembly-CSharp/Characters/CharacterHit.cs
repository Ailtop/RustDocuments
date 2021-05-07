using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Actions;
using Characters.AI;
using FX;
using Singletons;
using UnityEditor;
using UnityEngine;

namespace Characters
{
	public class CharacterHit : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[GetComponent]
		private CharacterHealth _health;

		[SerializeField]
		protected SoundInfo _hitSound;

		[SerializeField]
		[Subcomponent(true, typeof(SequentialAction))]
		private SequentialAction _action;

		[SerializeField]
		private EnemyDiedAction _deadAction;

		private int _motionIndex;

		public Action action => _action;

		private void Awake()
		{
			_health.onTookDamage += onTookDamage;
		}

		private void onTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (damageDealt > 0.0)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_hitSound, base.transform.position);
			}
		}

		public void Stop(float stoppingPower)
		{
			stoppingPower *= (float)_character.stat.GetFinal(Stat.Kind.StoppingResistance);
			if (!(stoppingPower <= 0f) && !(_action == null) && !(_action.currentMotion == null) && !_character.stunedOrFreezed && !_character.health.dead && (!(_deadAction != null) || !_deadAction.diedAction.running))
			{
				_action.currentMotion.length = stoppingPower;
				_action.TryStart();
			}
		}
	}
}
