using System;
using System.Runtime.CompilerServices;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class LeapToPlayer : Movement
	{
		[SerializeField]
		private float _duration = 1f;

		private Vector2 _directionVector;

		private float _distance;

		public override void Initialize(Projectile projectile, float direction)
		{
			Character player = Singleton<Service>.Instance.levelManager.player;
			Vector2 vector = new Vector2(player.transform.position.x, projectile.transform.position.y);
			Vector2 vector2 = projectile.transform.position;
			_directionVector = vector - vector2;
			_distance = Mathf.Abs(vector2.x - vector.x);
			base.Initialize(projectile, direction);
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			float item = ((time > _duration) ? 0f : _distance);
			return new ValueTuple<Vector2, float>(_directionVector.normalized, item);
		}
	}
}
