using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class MagicianPatternSelector : MonoBehaviour
	{
		[Header("Single Pattern")]
		[Space]
		[SerializeField]
		private MagicianPattern _normalSingle;

		private WeightedPatternSelector _normalSingleSelector;

		[Header("Dual Pattern")]
		[Space]
		[SerializeField]
		private MagicianPattern _normalDual;

		private WeightedPatternSelector _normalDualSelector;

		private void Awake()
		{
			_normalSingleSelector = new WeightedPatternSelector(_normalSingle.patterns);
			_normalDualSelector = new WeightedPatternSelector(_normalDual.patterns);
		}

		public Pattern GetNormalSinglePattern()
		{
			return _normalSingleSelector.GetPattern();
		}

		public Pattern GetNormalDualPattern()
		{
			return _normalDualSelector.GetPattern();
		}
	}
}
