using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	internal class OperationInfo : MonoBehaviour
	{
		[Serializable]
		internal class Subcomponents : SubcomponentArray<OperationInfo>
		{
			internal void Sort()
			{
				_components = _components.OrderBy((OperationInfo operation) => operation.timeToTrigger).ToArray();
			}

			internal IEnumerator CRun(Projectile projectile)
			{
				int operationIndex = 0;
				float time = 0f;
				while (operationIndex < _components.Length)
				{
					for (; operationIndex < _components.Length && time >= _components[operationIndex].timeToTrigger; operationIndex++)
					{
						_components[operationIndex].operation.Run(projectile);
					}
					yield return null;
					time += projectile.owner.chronometer.projectile.deltaTime;
				}
			}
		}

		[SerializeField]
		[FrameTime]
		private float _timeToTrigger;

		[SerializeField]
		[Operation.Subcomponent]
		private Operation _operation;

		public Operation operation => _operation;

		public float timeToTrigger => _timeToTrigger;

		public override string ToString()
		{
			string arg = ((_operation == null) ? "null" : _operation.GetType().Name);
			return $"{_timeToTrigger:0.##}s({_timeToTrigger * 60f:0.##}f), {arg}";
		}
	}
}
