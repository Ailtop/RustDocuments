using Level.MapEvent.Behavior;
using Level.MapEvent.Condition;
using UnityEditor;
using UnityEngine;

namespace Level.MapEvent
{
	public class MapEvent : MonoBehaviour
	{
		[SerializeField]
		[Level.MapEvent.Condition.Condition.Subcomponent]
		private Level.MapEvent.Condition.Condition _condition;

		[SerializeField]
		[Subcomponent(typeof(BehaviorInfo))]
		private BehaviorInfo.Subcomponents _behavior;

		private void Awake()
		{
			if (_condition != null)
			{
				_condition.onSatisfy += Run;
			}
		}

		public void Run()
		{
			StartCoroutine(_behavior.CRun());
		}
	}
}
