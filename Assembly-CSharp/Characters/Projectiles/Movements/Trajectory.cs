using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class Trajectory : Movement
	{
		[SerializeField]
		private TargetFinder _finder;

		[SerializeField]
		private float _startSpeed;

		[SerializeField]
		private float _targetSpeed;

		[SerializeField]
		private AnimationCurve _curve;

		[SerializeField]
		private float _easingTime;

		[SerializeField]
		private float _gravity;

		[SerializeField]
		[Tooltip("이 값만큼 초기 발사각에 더해집니다. 주로 투사체에 노이즈를 추가하여 산발되게하는 식으로 사용할 수 있습니다.")]
		private CustomFloat _extraAngle;

		private float _ySpeed;

		public override void Initialize(Projectile projectile, float direction)
		{
			if (_finder.range != null)
			{
				_finder.Initialize(projectile);
				Target target = _finder.Find();
				if (target != null)
				{
					Vector3 vector = target.collider.bounds.center - base.transform.position;
					direction = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				}
			}
			direction += _extraAngle.value;
			base.Initialize(projectile, direction);
			_ySpeed = 0f;
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			float num = ((!(time < _easingTime)) ? _targetSpeed : (_startSpeed + (_targetSpeed - _startSpeed) * _curve.Evaluate(time / _easingTime)));
			num *= base.projectile.speedMultiplier;
			Vector2 vector = num * base.directionVector;
			_ySpeed -= _gravity * deltaTime;
			vector.y += _ySpeed;
			return new ValueTuple<Vector2, float>(vector.normalized, vector.magnitude);
		}
	}
}
