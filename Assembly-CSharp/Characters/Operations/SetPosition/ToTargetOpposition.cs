using Characters.AI;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToTargetOpposition : Policy
	{
		[SerializeField]
		private AIController _ai;

		[SerializeField]
		private bool _onPlatform;

		[SerializeField]
		private bool _randomX;

		public override Vector2 GetPosition()
		{
			Character character = _ai.character;
			Character target = _ai.target;
			if (target == null)
			{
				return character.transform.position;
			}
			if (!_onPlatform)
			{
				return target.transform.position;
			}
			Bounds platform = target.movement.controller.collisionState.lastStandingCollider.bounds;
			Vector3 center = platform.center;
			float x = CalculateX(target, ref platform, center);
			float y = CalculateY(target, platform);
			x = ClampX(character, x, platform);
			return new Vector2(x, y);
		}

		private float ClampX(Character owner, float x, Bounds platform)
		{
			if (x <= platform.min.x + owner.collider.size.x)
			{
				x = platform.min.x + owner.collider.size.x;
			}
			else if (x >= platform.max.x - owner.collider.size.x)
			{
				x = platform.max.x - owner.collider.size.x;
			}
			return x;
		}

		private float CalculateY(Character target, Bounds platform)
		{
			if (!_onPlatform)
			{
				return target.transform.position.y;
			}
			return platform.max.y;
		}

		private float CalculateX(Character target, ref Bounds platform, Vector3 center)
		{
			if (target.transform.position.x > center.x)
			{
				return _randomX ? Random.Range(platform.min.x, platform.center.x) : platform.min.x;
			}
			return _randomX ? Random.Range(platform.center.x, platform.max.x) : platform.max.x;
		}
	}
}
