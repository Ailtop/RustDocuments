using Data;
using UnityEngine;

namespace Characters.Operations
{
	public class ConsumeGold : Operation
	{
		[Tooltip("소모할 골드량, GoldConstraint의 Amount와 값이 서로 다르지 않도록 주의")]
		[SerializeField]
		private int _amount;

		public override void Run()
		{
			GameData.Currency.gold.Consume(_amount);
		}
	}
}
