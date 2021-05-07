using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Magician
{
	public class WorldOnFire : Behaviour
	{
		[SerializeField]
		private Action _attack;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_attack.TryStart();
			while (_attack.running)
			{
				yield return null;
			}
			base.result = Result.Done;
		}

		public bool CanUse()
		{
			return _attack.canUse;
		}
	}
}
