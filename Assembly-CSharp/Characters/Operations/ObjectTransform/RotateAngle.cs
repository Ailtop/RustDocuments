using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class RotateAngle : CharacterOperation
	{
		[SerializeField]
		private Transform _centerAxisPosition;

		[SerializeField]
		[Range(0f, 90f)]
		private float _angle = 5f;

		[SerializeField]
		private bool _isAdded;

		public override void Run(Character owner)
		{
			Vector3 eulerAngles = _centerAxisPosition.rotation.eulerAngles;
			_centerAxisPosition.rotation = Quaternion.Euler(eulerAngles + new Vector3(0f, 0f, (!_isAdded) ? _angle : (0f - _angle)));
		}
	}
}
