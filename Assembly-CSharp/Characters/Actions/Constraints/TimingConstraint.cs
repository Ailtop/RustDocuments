using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class TimingConstraint : Constraint
	{
		[Information("현재 실행 중인 모션의 진행 정도가 범위 안에 있을 때만 캔슬 가능", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		[Range(0f, 1f)]
		private Vector2 _timingCanCancel;

		public override bool Pass()
		{
			if (!(_action.owner.runningMotion == null))
			{
				if (_timingCanCancel.x <= _action.owner.runningMotion.normalizedTime)
				{
					return _action.owner.runningMotion.normalizedTime <= _timingCanCancel.y;
				}
				return false;
			}
			return true;
		}
	}
}
