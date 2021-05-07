using Characters.Operations.SetPosition;
using UnityEngine;

namespace BT
{
	public class SetPositionTo : Node
	{
		[SerializeField]
		private Transform _object;

		[SerializeField]
		private TargetInfo _targetInfo;

		protected override NodeState UpdateDeltatime(Context context)
		{
			_object.position = _targetInfo.GetPosition();
			return NodeState.Success;
		}
	}
}
