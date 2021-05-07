using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Warrior
{
	public class Guard : Behaviour
	{
		[SerializeField]
		private Action _guard;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_guard.TryStart();
			while (_guard.running)
			{
				yield return null;
			}
			base.result = Result.Done;
		}
	}
}
