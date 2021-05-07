using System.Collections;
using Characters.AI.Hero;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class SanitFieldEmerge : Behaviour
	{
		[SerializeField]
		private RunAction _attack;

		[SerializeField]
		private RunAction _teleport;

		[SerializeField]
		private SaintField _field;

		public override IEnumerator CRun(AIController controller)
		{
			yield return _attack.CRun(controller);
			if (_field.isStuck)
			{
				yield return _teleport.CRun(controller);
			}
		}
	}
}
