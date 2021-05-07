using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class MoveTransform : CharacterOperation
	{
		[SerializeField]
		[Range(-100f, 100f)]
		private float _xValue;

		[SerializeField]
		[Range(-100f, 100f)]
		private float _yValue;

		[SerializeField]
		private Transform _targetTransform;

		public override void Run(Character owner)
		{
			_targetTransform.position += new Vector3((owner.lookingDirection == Character.LookingDirection.Right) ? _xValue : (0f - _xValue), _yValue, 0f);
		}
	}
}
