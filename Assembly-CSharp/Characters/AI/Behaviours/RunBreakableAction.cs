using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class RunBreakableAction : Behaviour
	{
		[SerializeField]
		private Action _ready;

		[SerializeField]
		private RunAction _groggy;

		[SerializeField]
		private double _damageForGroggy;

		private double _damageAmount;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_damageAmount = 0.0;
			CharacterHealth targetHealth = controller.character.health;
			targetHealth.onTookDamage += TargetHealth_onTookDamage;
			bool doGroggy = false;
			_ready.TryStart();
			while (_ready.running)
			{
				yield return null;
				if (_damageAmount >= _damageForGroggy)
				{
					doGroggy = true;
					break;
				}
			}
			if (doGroggy)
			{
				yield return _groggy.CRun(controller);
			}
			targetHealth.onTookDamage -= TargetHealth_onTookDamage;
			base.result = Result.Success;
		}

		private void TargetHealth_onTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_damageAmount += damageDealt;
		}
	}
}
