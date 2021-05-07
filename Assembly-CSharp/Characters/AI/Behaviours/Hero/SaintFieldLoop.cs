using System.Collections;
using Characters.AI.Hero;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public sealed class SaintFieldLoop : Decorator
	{
		[SerializeField]
		private SaintField _saintField;

		[SerializeField]
		[Subcomponent(true)]
		private Behaviour _behaviour;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			while (_saintField.isStuck)
			{
				yield return _behaviour.CRun(controller);
			}
			base.result = Result.Success;
		}
	}
}
