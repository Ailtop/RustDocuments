using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class MoveTransformFromPosition : CharacterOperation
	{
		[SerializeField]
		private Transform _fromPosition;

		[SerializeField]
		private Transform _targetTransform;

		public override void Run(Character owner)
		{
			_targetTransform.position = _fromPosition.position;
		}
	}
}
