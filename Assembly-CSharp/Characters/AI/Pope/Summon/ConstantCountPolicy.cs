using UnityEngine;

namespace Characters.AI.Pope.Summon
{
	public sealed class ConstantCountPolicy : CountPolicy
	{
		[SerializeField]
		private int _count;

		public override int GetCount()
		{
			return _count;
		}
	}
}
