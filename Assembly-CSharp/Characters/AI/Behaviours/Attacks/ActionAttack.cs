using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Attacks
{
	public class ActionAttack : Attack
	{
		[SerializeField]
		protected Action attack;

		[SerializeField]
		[Range(0f, 1f)]
		protected float chanceOfDelayAffterAttack;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Idle))]
		protected Idle idle;

		private void Start()
		{
			_childs = new List<Behaviour> { idle };
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			gaveDamage = false;
			if (attack.TryStart())
			{
				while (attack.running)
				{
					yield return null;
				}
				base.result = Result.Success;
				if (MMMaths.Chance(chanceOfDelayAffterAttack))
				{
					yield return idle.CRun(controller);
				}
			}
			else
			{
				base.result = Result.Fail;
			}
		}

		public bool CanUse()
		{
			return attack.canUse;
		}
	}
}
