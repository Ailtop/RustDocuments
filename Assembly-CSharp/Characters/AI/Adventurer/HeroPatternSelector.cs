using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class HeroPatternSelector : MonoBehaviour
	{
		[Header("Single Pattern")]
		[Space]
		[SerializeField]
		private HeroPattern _normalSingle;

		[SerializeField]
		private HeroPattern _outDashSingle;

		[SerializeField]
		private HeroPattern _afterBackDashSingle;

		[SerializeField]
		private HeroPattern _afterDashSingle;

		private WeightedPatternSelector _normalSingleSelector;

		private WeightedPatternSelector _outDashSingleSelector;

		private WeightedPatternSelector _afterBackDashSingleSelector;

		private WeightedPatternSelector _afterDashSingleSelector;

		private void Awake()
		{
			_normalSingleSelector = new WeightedPatternSelector(_normalSingle.patterns);
			_outDashSingleSelector = new WeightedPatternSelector(_outDashSingle.patterns);
			_afterBackDashSingleSelector = new WeightedPatternSelector(_afterBackDashSingle.patterns);
			_afterDashSingleSelector = new WeightedPatternSelector(_afterDashSingle.patterns);
		}

		public Pattern GetNormalSinglePattern()
		{
			return _normalSingleSelector.GetPattern();
		}

		public Pattern GetOutDashSinglePattern()
		{
			return _outDashSingleSelector.GetPattern();
		}

		public Pattern GetAfterBackDashSinglePattern()
		{
			return _afterBackDashSingleSelector.GetPattern();
		}

		public Pattern GetAfterDashSinglePattern()
		{
			return _afterDashSingleSelector.GetPattern();
		}
	}
}
