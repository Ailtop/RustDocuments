using Characters.Monsters;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public class MonsterCount : Condition
	{
		private enum Comparer
		{
			GreaterThan,
			LessThan
		}

		[SerializeField]
		private MonsterContainer _minionContainer;

		[SerializeField]
		private Comparer _compare;

		[SerializeField]
		[Range(0f, 100f)]
		private int _count;

		protected override bool Check(AIController controller)
		{
			int num = _minionContainer.Count();
			switch (_compare)
			{
			case Comparer.GreaterThan:
				return num >= _count;
			case Comparer.LessThan:
				return num <= _count;
			default:
				return false;
			}
		}
	}
}
