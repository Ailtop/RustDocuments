using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToPlayer : Policy
	{
		[SerializeField]
		private bool _onPlatform;

		[SerializeField]
		private bool _lastStandingCollider;

		public override Vector2 GetPosition()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			if (!_onPlatform)
			{
				return player.transform.position;
			}
			Collider2D collider;
			if (_lastStandingCollider)
			{
				collider = player.movement.controller.collisionState.lastStandingCollider;
			}
			else
			{
				player.movement.TryGetClosestBelowCollider(out collider, Layers.groundMask);
			}
			float x = player.transform.position.x;
			float y = collider.bounds.max.y;
			return new Vector2(x, y);
		}
	}
}
