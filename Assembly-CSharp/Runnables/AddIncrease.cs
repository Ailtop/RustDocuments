using Level.Specials;
using UnityEngine;

namespace Runnables
{
	public abstract class AddIncrease : Runnable
	{
		[SerializeField]
		private TimeCostEvent _costReward;

		public override void Run()
		{
			_costReward.AddIncrease(GetIncrease());
		}

		protected abstract int GetIncrease();
	}
}
