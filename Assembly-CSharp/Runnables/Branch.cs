using Runnables.Chances;
using UnityEngine;

namespace Runnables
{
	public sealed class Branch : Runnable
	{
		[SerializeField]
		[Chance.Subcomponent]
		private Chance _trueChance;

		[SerializeField]
		private Runnable _onTrue;

		[SerializeField]
		private Runnable _onFalse;

		public override void Run()
		{
			if (_trueChance.IsTrue())
			{
				_onTrue.Run();
			}
			else
			{
				_onFalse.Run();
			}
		}
	}
}
