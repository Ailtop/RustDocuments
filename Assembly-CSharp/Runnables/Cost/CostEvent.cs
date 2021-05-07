using Level.Specials;
using UnityEngine;

namespace Runnables.Cost
{
	public class CostEvent : CurrencyAmount
	{
		[SerializeField]
		private Level.Specials.CostEvent _costEvent;

		public override int GetAmount()
		{
			return (int)_costEvent.GetValue();
		}
	}
}
