using Characters.Abilities;
using UnityEngine;

namespace Characters.Actions.Constraints.Customs
{
	public class FighterRageReadyConstraint : Constraint
	{
		[SerializeField]
		private FighterPassiveAttacher _fighterPassiveAttacher;

		public override bool Pass()
		{
			return _fighterPassiveAttacher.rageReady;
		}
	}
}
