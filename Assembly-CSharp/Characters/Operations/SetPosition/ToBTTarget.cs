using BT;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToBTTarget : Policy
	{
		[SerializeField]
		private BehaviourTreeRunner _bt;

		[SerializeField]
		private bool _onPlatform;

		[SerializeField]
		private bool _lastStandingCollider;

		private Vector2 _default => base.transform.position;

		public override Vector2 GetPosition()
		{
			Character character = _bt.context.Get<Character>(BT.Key.Target);
			if (character == null)
			{
				return base.transform.position;
			}
			if (!_onPlatform)
			{
				return character.transform.position;
			}
			Collider2D collider;
			if (_lastStandingCollider)
			{
				collider = character.movement.controller.collisionState.lastStandingCollider;
				if (collider == null)
				{
					character.movement.TryGetClosestBelowCollider(out collider, Layers.groundMask);
					if (collider == null)
					{
						return _default;
					}
				}
			}
			else
			{
				character.movement.TryGetClosestBelowCollider(out collider, Layers.groundMask);
				if (collider == null)
				{
					return _default;
				}
			}
			float x = character.transform.position.x;
			float y = collider.bounds.max.y;
			return new Vector2(x, y);
		}
	}
}
