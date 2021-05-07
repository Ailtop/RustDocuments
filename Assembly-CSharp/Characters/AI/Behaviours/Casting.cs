using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Casting : Decorator
	{
		[SerializeField]
		private double _breakTotalDamage;

		[SerializeField]
		private Behaviour _behaviour;

		private double _cumulativeDamage;

		private AIController _controller;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character owner = controller.character;
			_controller = controller;
			_cumulativeDamage = 0.0;
			owner.health.onTookDamage -= OnTookDamage;
			owner.health.onTookDamage += OnTookDamage;
			yield return _behaviour.CRun(controller);
			owner.health.onTookDamage -= OnTookDamage;
			base.result = Result.Success;
		}

		private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_cumulativeDamage += damageDealt;
			if (_cumulativeDamage >= _breakTotalDamage)
			{
				base.result = Result.Fail;
				_controller.character.health.onTookDamage -= OnTookDamage;
				_controller.StopAllCoroutinesWithBehaviour();
			}
		}
	}
}
