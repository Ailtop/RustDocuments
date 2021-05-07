using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class AimPlayer : CharacterOperation
	{
		[SerializeField]
		private Transform _centerAxis;

		[SerializeField]
		private bool _platform;

		public override void Run(Character owner)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			Vector3 vector = new Vector3(y: (!_platform) ? (player.transform.position.y + player.collider.bounds.extents.y) : player.movement.controller.collisionState.lastStandingCollider.bounds.max.y, x: player.transform.position.x) - _centerAxis.transform.position;
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			_centerAxis.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}
}
