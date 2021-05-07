using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class ThiefPatternSelector : MonoBehaviour
	{
		[Header("Single Pattern")]
		[Space]
		[SerializeField]
		private ThiefPattern _normalSingle;

		[SerializeField]
		private ThiefPattern _afterShadowStepSingle;

		[SerializeField]
		private ThiefPattern _afterFlashCutSingle;

		private WeightedPatternSelector _normalSingleSelector;

		private WeightedPatternSelector _afterShadowStepSingleSelector;

		private WeightedPatternSelector _afterFlashCutSingleSelector;

		[Header("Dual Pattern")]
		[Space]
		[SerializeField]
		private ThiefPattern _normalDual;

		[SerializeField]
		private ThiefPattern _afterShadowStepDual;

		[SerializeField]
		private ThiefPattern _afterFlashCutDual;

		private WeightedPatternSelector _normalDualSelector;

		private WeightedPatternSelector _afterShadowStepDualSelector;

		private WeightedPatternSelector _afterFlashCutDualSelector;

		private void Awake()
		{
			_normalSingleSelector = new WeightedPatternSelector(_normalSingle.patterns);
			_afterShadowStepSingleSelector = new WeightedPatternSelector(_afterShadowStepSingle.patterns);
			_afterFlashCutSingleSelector = new WeightedPatternSelector(_afterFlashCutSingle.patterns);
			_normalDualSelector = new WeightedPatternSelector(_normalDual.patterns);
			_afterShadowStepDualSelector = new WeightedPatternSelector(_afterShadowStepDual.patterns);
			_afterFlashCutDualSelector = new WeightedPatternSelector(_afterFlashCutDual.patterns);
		}

		public Pattern GetNormalSinglePattern()
		{
			return _normalSingleSelector.GetPattern();
		}

		public Pattern GetAfterShadowSinglePattern()
		{
			return _afterShadowStepSingleSelector.GetPattern();
		}

		public Pattern GetAfterFlashCutSinglePattern()
		{
			return _afterFlashCutSingleSelector.GetPattern();
		}

		public Pattern GetNormalDualPattern()
		{
			return _normalDualSelector.GetPattern();
		}

		public Pattern GetAfterShadowDualPattern()
		{
			return _afterShadowStepDualSelector.GetPattern();
		}

		public Pattern GetAfterFlashCutDualPattern()
		{
			return _afterFlashCutDualSelector.GetPattern();
		}
	}
}
