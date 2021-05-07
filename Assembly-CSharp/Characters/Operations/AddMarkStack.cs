using Characters.Marks;
using UnityEngine;

namespace Characters.Operations
{
	public class AddMarkStack : CharacterOperation
	{
		[SerializeField]
		private MarkInfo _mark;

		[SerializeField]
		[Range(1f, 100f)]
		private int _chance = 100;

		[SerializeField]
		private float _count = 1f;

		public override void Run(Character target)
		{
			if (MMMaths.PercentChance(_chance))
			{
				target.mark.AddStack(_mark, _count);
			}
		}
	}
}
