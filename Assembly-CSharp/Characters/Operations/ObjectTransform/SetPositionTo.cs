using Characters.Operations.SetPosition;
using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class SetPositionTo : CharacterOperation
	{
		[SerializeField]
		private Transform _object;

		[SerializeField]
		private TargetInfo _targetInfo;

		public override void Run(Character owner)
		{
			_object.position = _targetInfo.GetPosition();
		}
	}
}
