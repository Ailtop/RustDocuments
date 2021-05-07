using System;
using System.Linq;
using UnityEngine;

namespace Characters.Operations.Decorator
{
	public class OperationWithWeight : MonoBehaviour
	{
		[Serializable]
		public class Subcomponents : SubcomponentArray<OperationWithWeight>
		{
			internal void Initialize()
			{
				for (int i = 0; i < _components.Length; i++)
				{
					OperationWithWeight operationWithWeight = _components[i];
					if (!(operationWithWeight._operation == null))
					{
						operationWithWeight._operation.Initialize();
					}
				}
			}

			public void RunWeightedRandom(Character owner)
			{
				CharacterOperation characterOperation = null;
				float num = UnityEngine.Random.Range(0f, _components.Sum((OperationWithWeight c) => c._weight));
				for (int i = 0; i < _components.Length; i++)
				{
					num -= _components[i]._weight;
					if (num <= 0f)
					{
						characterOperation = _components[i]._operation;
						break;
					}
				}
				if (!(characterOperation == null))
				{
					characterOperation.Run(owner);
				}
			}

			internal void StopAll()
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					OperationWithWeight operationWithWeight = _components[i];
					if (!(operationWithWeight._operation == null))
					{
						operationWithWeight._operation.Stop();
					}
				}
			}
		}

		[SerializeField]
		private float _weight = 1f;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation _operation;

		public override string ToString()
		{
			string arg = ((_operation == null) ? "Do Nothing" : _operation.GetType().Name);
			return $"{arg}({_weight})";
		}
	}
}
