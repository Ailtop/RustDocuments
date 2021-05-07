using System;
using System.Collections;
using UnityEngine;

namespace Level.MapEvent.Behavior
{
	internal class BehaviorInfo : MonoBehaviour
	{
		[Serializable]
		internal class Subcomponents : SubcomponentArray<BehaviorInfo>
		{
			internal IEnumerator CRun()
			{
				int operationIndex = 0;
				float time = 0f;
				while (operationIndex < base.components.Length)
				{
					BehaviorInfo behaviorInfo = base.components[operationIndex];
					for (; operationIndex < base.components.Length && time >= base.components[operationIndex]._timeToTrigger; operationIndex++)
					{
						base.components[operationIndex].behavior.Run();
					}
					yield return null;
					time += Chronometer.global.deltaTime;
				}
			}
		}

		[SerializeField]
		private float _timeToTrigger;

		[SerializeField]
		[Behavior.Subcomponent]
		private Behavior _behavior;

		public Behavior behavior => _behavior;

		public override string ToString()
		{
			return $"{_timeToTrigger:0.##}s {_behavior.GetAutoName()}";
		}
	}
}
