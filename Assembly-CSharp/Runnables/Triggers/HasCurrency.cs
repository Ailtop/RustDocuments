using Data;
using UnityEngine;

namespace Runnables.Triggers
{
	public class HasCurrency : Trigger
	{
		[SerializeField]
		private GameData.Currency.Type _type;

		[SerializeField]
		[CurrencyAmount.Subcomponent]
		private CurrencyAmount _currencyAmount;

		protected override bool Check()
		{
			int amount = _currencyAmount.GetAmount();
			switch (_type)
			{
			case GameData.Currency.Type.Gold:
				return GameData.Currency.gold.Has(amount);
			case GameData.Currency.Type.Bone:
				return GameData.Currency.bone.Has(amount);
			case GameData.Currency.Type.DarkQuartz:
				return GameData.Currency.darkQuartz.Has(amount);
			default:
				return GameData.Currency.gold.Has(amount);
			}
		}
	}
}
