using System;
using System.Collections;
using UnityEngine;

namespace Characters.Operations
{
	internal class TargetedOperationInfo : MonoBehaviour
	{
		[Serializable]
		internal class Subcomponents : SubcomponentArray<TargetedOperationInfo>
		{
			[SerializeField]
			private float _speed = 1f;

			public float speed => _speed;

			internal void Initialize()
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					base.components[i]._operation.Initialize();
				}
			}

			internal IEnumerator CRun(Character owner, Character target)
			{
				int operationIndex = 0;
				float time = 0f;
				while (operationIndex < base.components.Length)
				{
					for (; operationIndex < base.components.Length && time >= base.components[operationIndex].timeToTrigger; operationIndex++)
					{
						base.components[operationIndex].operation.Run(owner, target);
					}
					yield return null;
					time += owner.chronometer.animation.deltaTime * speed;
				}
			}
		}

		[SerializeField]
		[FrameTime]
		private float _timeToTrigger;

		[SerializeField]
		[TargetedCharacterOperation.Subcomponent]
		private TargetedCharacterOperation _operation;

		public TargetedCharacterOperation operation => _operation;

		public float timeToTrigger => _timeToTrigger;

		public override string ToString()
		{
			return $"{_timeToTrigger:0.##}s({_timeToTrigger * 60f:0.##}f), {_operation.GetAutoName()}";
		}
	}
}
