using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToPlayerBased : Policy
	{
		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private CustomFloat _amount;

		[SerializeField]
		private bool _behind;

		[SerializeField]
		private bool _onPlatform;

		[SerializeField]
		private bool _lastStandingCollider;

		public override Vector2 GetPosition()
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			Vector2 result = player.transform.position;
			Clamp(ref result, _amount.value);
			if (!_onPlatform)
			{
				return result;
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
			float x = result.x;
			float y = collider.bounds.max.y;
			return new Vector2(x, y);
		}

		private void Clamp(ref Vector2 result, float amount)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			Collider2D collider;
			if (_lastStandingCollider)
			{
				collider = player.movement.controller.collisionState.lastStandingCollider;
			}
			else
			{
				player.movement.TryGetClosestBelowCollider(out collider, Layers.groundMask);
			}
			float min = collider.bounds.min.x + _collider.bounds.size.x;
			float max = collider.bounds.max.x - _collider.bounds.size.x;
			if (player.lookingDirection == Character.LookingDirection.Right)
			{
				result = ClampX(result, _behind ? (result.x - amount) : (result.x + amount), min, max);
			}
			else
			{
				result = ClampX(result, _behind ? (result.x + amount) : (result.x - amount), min, max);
			}
		}

		private Vector2 ClampX(Vector2 result, float x, float min, float max)
		{
			float num = 0.05f;
			result = ((x <= min) ? new Vector2(min + num, result.y) : ((!(x >= max)) ? new Vector2(x, result.y) : new Vector2(max - num, result.y)));
			return result;
		}
	}
}
