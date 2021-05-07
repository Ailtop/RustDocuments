using Characters.AI.Hero.LightSwords;
using UnityEngine;

namespace Characters.AI.Conditions.Customs
{
	public sealed class LightSwordCount : Condition
	{
		private enum Comparer
		{
			GreaterThan,
			LessThan
		}

		[SerializeField]
		private LightSwordFieldHelper _helper;

		[SerializeField]
		private int _count;

		[SerializeField]
		private Comparer _comparer;

		protected override bool Check(AIController controller)
		{
			int activatedSwordCount = _helper.GetActivatedSwordCount();
			switch (_comparer)
			{
			case Comparer.GreaterThan:
				return activatedSwordCount >= _count;
			case Comparer.LessThan:
				return activatedSwordCount <= _count;
			default:
				return false;
			}
		}
	}
}
