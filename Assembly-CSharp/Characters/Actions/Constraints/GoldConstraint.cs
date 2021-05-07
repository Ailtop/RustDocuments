using Data;
using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class GoldConstraint : Constraint
	{
		[Tooltip("필요 골드량, ConsumeGold 오퍼레이션으로 같은 Amount만큼 소모되게 해주어야 함")]
		[SerializeField]
		private int _amount;

		public override bool Pass()
		{
			return GameData.Currency.gold.Has(_amount);
		}
	}
}
