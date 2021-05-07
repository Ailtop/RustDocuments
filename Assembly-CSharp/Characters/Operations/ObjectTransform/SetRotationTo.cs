using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class SetRotationTo : CharacterOperation
	{
		[SerializeField]
		private Transform _transform;

		[SerializeField]
		private CustomFloat _rotation;

		public override void Run(Character owner)
		{
			_transform.rotation = Quaternion.Euler(0f, 0f, _rotation.value);
		}
	}
}
