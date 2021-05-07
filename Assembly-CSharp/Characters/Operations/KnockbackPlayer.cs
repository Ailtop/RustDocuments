using Characters.Movements;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class KnockbackPlayer : CharacterOperation
	{
		[SerializeField]
		private PushInfo _pushInfo = new PushInfo(false, false);

		public override void Run(Character owner)
		{
			Singleton<Service>.Instance.levelManager.player.movement.push.ApplyKnockback(owner, _pushInfo);
		}
	}
}
