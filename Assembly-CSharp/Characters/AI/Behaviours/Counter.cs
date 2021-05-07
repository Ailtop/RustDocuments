using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class Counter : Decorator
	{
		[MinMaxSlider(0f, 100f)]
		[SerializeField]
		private Vector2 _duration;

		[SerializeField]
		private Behaviour _behaviour;

		private Health _ownerHealth;

		private bool _success;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_ownerHealth = controller.character.health;
			float duration = Random.Range(_duration.x, _duration.y);
			float elapsed = 0f;
			_success = false;
			_ownerHealth.onTookDamage -= RunCounter;
			_ownerHealth.onTookDamage += RunCounter;
			while (elapsed < duration && !_success)
			{
				elapsed += controller.character.chronometer.master.deltaTime;
				yield return null;
			}
			if (_success)
			{
				_ownerHealth.onTookDamage -= RunCounter;
				yield return _behaviour.CRun(controller);
			}
			base.result = Result.Success;
		}

		private void RunCounter([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_success = true;
			_ownerHealth.onTookDamage -= RunCounter;
		}
	}
}
