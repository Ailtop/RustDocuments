using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class Simple : Movement
	{
		[SerializeField]
		private float _speed;

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			return new ValueTuple<Vector2, float>(base.directionVector, _speed * base.projectile.speedMultiplier);
		}
	}
}
