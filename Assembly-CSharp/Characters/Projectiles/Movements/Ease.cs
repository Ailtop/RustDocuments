using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class Ease : Movement
	{
		[SerializeField]
		private float _startSpeed;

		[SerializeField]
		private float _targetSpeed;

		[SerializeField]
		private float _easingTime;

		[SerializeField]
		private EasingFunction.Method _easingMethod;

		private EasingFunction.Function _easingFunction;

		public override void Initialize(Projectile projectile, float direction)
		{
			base.Initialize(projectile, direction);
			_easingFunction = EasingFunction.GetEasingFunction(_easingMethod);
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			float num = _easingFunction(_startSpeed, _targetSpeed, time / _easingTime);
			return new ValueTuple<Vector2, float>(base.directionVector, num * base.projectile.speedMultiplier);
		}
	}
}
