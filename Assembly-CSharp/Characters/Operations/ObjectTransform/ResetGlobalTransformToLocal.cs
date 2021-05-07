using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class ResetGlobalTransformToLocal : Operation
	{
		[SerializeField]
		private GlobalTransformHolder _transformHolder;

		public override void Run()
		{
			_transformHolder.ResetChildrenToLocal();
		}
	}
}
