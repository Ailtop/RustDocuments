using UnityEngine;

namespace Characters.Operations.Customs
{
	public class TeleportToSkulHead : CharacterOperation
	{
		[SerializeField]
		private SkulHeadController _skulHeadController;

		public override void Run(Character owner)
		{
			Vector3 position = SkulHeadToTeleport.instance.transform.position;
			if (owner.movement.controller.TeleportUponGround(position, 1.5f) || owner.movement.controller.Teleport(position, 3f))
			{
				_skulHeadController.cooldown.time.remainTime = 0f;
				SkulHeadToTeleport.instance.Despawn();
			}
		}
	}
}
