using Data;
using UnityEngine;

namespace Runnables
{
	public class ConsumeCurrency : Runnable
	{
		[SerializeField]
		private GameData.Currency.Type _type;

		[SerializeField]
		[CurrencyAmount.Subcomponent]
		private CurrencyAmount _currencyAmount;

		public override void Run()
		{
			int amount = _currencyAmount.GetAmount();
			switch (_type)
			{
			case GameData.Currency.Type.Gold:
				if (GameData.Currency.gold.Has(amount))
				{
					GameData.Currency.gold.Consume(amount);
				}
				break;
			case GameData.Currency.Type.Bone:
				if (GameData.Currency.bone.Has(amount))
				{
					GameData.Currency.bone.Consume(amount);
				}
				break;
			case GameData.Currency.Type.DarkQuartz:
				if (GameData.Currency.darkQuartz.Has(amount))
				{
					GameData.Currency.darkQuartz.Consume(amount);
				}
				break;
			default:
				if (GameData.Currency.gold.Has(amount))
				{
					GameData.Currency.gold.Consume(amount);
				}
				break;
			}
		}
	}
}
