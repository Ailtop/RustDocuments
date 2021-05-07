using UnityEngine;

namespace Characters.AI.Adventurer
{
	public class ClericPatternSelector : MonoBehaviour
	{
		[Header("Dual Pattern")]
		[Space]
		[SerializeField]
		private ClericPattern _normalDual;

		private WeightedPatternSelector _normalDualSelector;

		private void Awake()
		{
			_normalDualSelector = new WeightedPatternSelector(_normalDual.patterns);
		}

		public Pattern GetNormalDualPattern()
		{
			return _normalDualSelector.GetPattern();
		}
	}
}
