using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class WarriorPatternSelector : MonoBehaviour
	{
		[Header("Single Pattern")]
		[Space]
		[SerializeField]
		private WarriorPattern _normalSingle;

		[SerializeField]
		private WarriorPattern _afterStampingSingle;

		[SerializeField]
		private WarriorPattern _onEarthquakeTriggerStaySingle;

		private WeightedPatternSelector _normalSingleSelector;

		private WeightedPatternSelector _afterStampingSingleSelector;

		private WeightedPatternSelector _onEarthquakeTriggerStaySingleSelector;

		[Header("Dual Pattern")]
		[Space]
		[SerializeField]
		private WarriorPattern _normalDual;

		[SerializeField]
		private WarriorPattern _afterStampingDual;

		[SerializeField]
		private WarriorPattern _onEarthquakeTriggerStayDual;

		private WeightedPatternSelector _normalDualSelector;

		private WeightedPatternSelector _afterStampingDualSelector;

		private WeightedPatternSelector _onEarthquakeTriggerStayDualSelector;

		private void Awake()
		{
			_normalSingleSelector = new WeightedPatternSelector(_normalSingle.patterns);
			_afterStampingSingleSelector = new WeightedPatternSelector(_afterStampingSingle.patterns);
			_onEarthquakeTriggerStaySingleSelector = new WeightedPatternSelector(_onEarthquakeTriggerStaySingle.patterns);
			_normalDualSelector = new WeightedPatternSelector(_normalDual.patterns);
			_afterStampingDualSelector = new WeightedPatternSelector(_afterStampingDual.patterns);
			_onEarthquakeTriggerStayDualSelector = new WeightedPatternSelector(_onEarthquakeTriggerStayDual.patterns);
		}

		public Pattern GetNormalSinglePattern()
		{
			return _normalSingleSelector.GetPattern();
		}

		public Pattern GetAfterStampingSinglePattern()
		{
			return _afterStampingSingleSelector.GetPattern();
		}

		public Pattern GetOnEarthquakeTriggerStaySinglePattern()
		{
			return _onEarthquakeTriggerStaySingleSelector.GetPattern();
		}

		public Pattern GetNormalDualPattern()
		{
			return _normalDualSelector.GetPattern();
		}

		public Pattern GetAfterStampingDualPattern()
		{
			return _afterStampingDualSelector.GetPattern();
		}

		public Pattern GetOnEarthquakeTriggerStayDualPattern()
		{
			return _onEarthquakeTriggerStayDualSelector.GetPattern();
		}
	}
}
