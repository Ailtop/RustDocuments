using UnityEngine;

namespace Runnables.Cost
{
	public class Constant : CurrencyAmount
	{
		[SerializeField]
		private int _amount;

		public override int GetAmount()
		{
			return _amount;
		}
	}
}
