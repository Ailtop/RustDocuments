using System.Linq;
using UnityEngine;

namespace Characters.Actions
{
	public class MultipleActionHoldingCharge : MultipleAction
	{
		[SerializeField]
		private Motion[] _chargingMotions;

		public override bool TryStart()
		{
			if (!base.cooldown.canUse)
			{
				return false;
			}
			for (int i = 0; i < _motions.components.Length; i++)
			{
				if (PassAllConstraints(_motions.components[i]) && ConsumeCooldownIfNeeded())
				{
					if (_chargingMotions.Contains(base.owner.motion))
					{
						DoActionNonBlock(_motions.components[i]);
					}
					else
					{
						DoAction(_motions.components[i]);
					}
					return true;
				}
			}
			return false;
		}
	}
}
