using System;
using System.Runtime.CompilerServices;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class MoveToPlayer : Movement
	{
		[SerializeField]
		private float _speed = 1f;

		private Vector2 _destination;

		private Projectile _projectile;

		public override void Initialize(Projectile projectile, float direction)
		{
			_projectile = projectile;
			Character player = Singleton<Service>.Instance.levelManager.player;
			_destination = new Vector2(player.transform.position.x, projectile.transform.position.y);
			base.Initialize(projectile, direction);
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			if (base.directionVector.x > 0f && base.projectile.transform.position.x >= _destination.x)
			{
				_projectile.Despawn();
			}
			else if (base.directionVector.x < 0f && base.projectile.transform.position.x <= _destination.x)
			{
				_projectile.Despawn();
			}
			return new ValueTuple<Vector2, float>(base.directionVector, _speed * base.projectile.speedMultiplier);
		}
	}
}
