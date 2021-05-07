using System;
using System.Linq;
using UnityEngine;

namespace Characters.Projectiles.Operations.Decorator
{
	public class OperationWithWeight : MonoBehaviour
	{
		[Serializable]
		public class Subcomponents : SubcomponentArray<OperationWithWeight>
		{
			public void RunWeightedRandom(Projectile projectile)
			{
				Operation operation = null;
				float num = UnityEngine.Random.Range(0f, _components.Sum((OperationWithWeight c) => c._weight));
				for (int i = 0; i < _components.Length; i++)
				{
					num -= _components[i]._weight;
					if (num <= 0f)
					{
						operation = _components[i]._operation;
						break;
					}
				}
				if (!(operation == null))
				{
					operation.Run(projectile);
				}
			}
		}

		[SerializeField]
		private float _weight = 1f;

		[SerializeField]
		[Operation.Subcomponent]
		private Operation _operation;

		public override string ToString()
		{
			string arg = ((_operation == null) ? "Do Nothing" : _operation.GetType().Name);
			return $"{arg}({_weight})";
		}
	}
}
