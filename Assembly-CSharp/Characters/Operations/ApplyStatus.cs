using UnityEngine;

namespace Characters.Operations
{
	public class ApplyStatus : CharacterOperation
	{
		[SerializeField]
		private CharacterStatus.ApplyInfo _status;

		[SerializeField]
		[Range(1f, 100f)]
		private int _chance = 100;

		public override void Run(Character owner, Character target)
		{
			if (MMMaths.PercentChance(_chance))
			{
				owner.GiveStatus(target, _status);
			}
		}

		public override void Run(Character target)
		{
			if (MMMaths.PercentChance(_chance))
			{
				target.status?.Apply(null, _status);
			}
		}
	}
}
