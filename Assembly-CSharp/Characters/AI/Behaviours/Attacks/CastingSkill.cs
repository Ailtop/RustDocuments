using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Actions;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.Behaviours.Attacks
{
	public class CastingSkill : Behaviour
	{
		[SerializeField]
		private Action _casting;

		[SerializeField]
		private float _amountOfCastingBreakDamage;

		private double damage;

		private void Awake()
		{
			_amountOfCastingBreakDamage *= Singleton<Service>.Instance.levelManager.currentChapter.currentStage.adventurerCastingBreakDamageMultiplier;
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			damage = 0.0;
			character.health.onTookDamage += SumTookDamage;
			_casting.TryStart();
			while (_casting.running)
			{
				yield return null;
				if ((double)_amountOfCastingBreakDamage <= damage)
				{
					base.result = Result.Fail;
					break;
				}
			}
			character.health.onTookDamage -= SumTookDamage;
			if (base.result != 0 && !character.stunedOrFreezed)
			{
				base.result = Result.Success;
			}
		}

		private void SumTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			damage += damageDealt;
		}

		private bool OnTakeDamage(ref Damage damage)
		{
			if (damage.attackType == Damage.AttackType.Additional)
			{
				return true;
			}
			this.damage += damage.amount;
			return false;
		}

		public bool CanUse()
		{
			return _casting.canUse;
		}
	}
}
