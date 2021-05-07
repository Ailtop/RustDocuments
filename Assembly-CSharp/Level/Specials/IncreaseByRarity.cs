using UnityEngine;

namespace Level.Specials
{
	[RequireComponent(typeof(TimeCostEventReward))]
	public class IncreaseByRarity : MonoBehaviour
	{
		[SerializeField]
		private TimeCostEvent _costReward;

		[SerializeField]
		private TimeCostEventReward _reward;

		[SerializeField]
		private ValueByRarity _multiplierByRarity;

		private void OnEnable()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			float num = _multiplierByRarity[_reward.rarity];
			_costReward.Multiply(num);
		}
	}
}
