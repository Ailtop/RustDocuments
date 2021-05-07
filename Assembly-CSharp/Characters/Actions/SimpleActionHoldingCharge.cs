using System.Linq;
using UnityEngine;

namespace Characters.Actions
{
	public class SimpleActionHoldingCharge : SimpleAction
	{
		[SerializeField]
		private Motion[] _chargingMotions;

		public override bool TryStart()
		{
			if (!canUse || !ConsumeCooldownIfNeeded())
			{
				return false;
			}
			if (_chargingMotions.Contains(base.owner.motion))
			{
				DoActionNonBlock(_motion);
			}
			else
			{
				DoAction(_motion);
			}
			return true;
		}
	}
}
