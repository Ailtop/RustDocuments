using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class Missile : Movement
	{
		public enum RotateMethod
		{
			Constant,
			Lerp,
			Slerp
		}

		[SerializeField]
		private TargetFinder _finder;

		[SerializeField]
		private float _delay;

		[Header("Roatation")]
		[SerializeField]
		private RotateMethod _rotateMethod;

		[SerializeField]
		private float _rotateSpeed = 300f;

		[Header("Speed")]
		[SerializeField]
		private float _initialSpeed = 1f;

		[SerializeField]
		private float _acceleration = 2f;

		[SerializeField]
		private float _maxSpeed = 3f;

		private Target _target;

		private Vector2 _speed;

		private Quaternion _rotation;

		public override void Initialize(Projectile projectile, float direction)
		{
			base.Initialize(projectile, direction);
			_finder.Initialize(projectile);
			_target = null;
			_speed = Vector2.zero;
			_rotation = Quaternion.Euler(0f, 0f, direction);
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			if (time >= _delay)
			{
				UpdateTarget();
				UpdateDirection(deltaTime);
			}
			_speed += _acceleration * base.directionVector * deltaTime;
			Vector2 normalized = _speed.normalized;
			float magnitude = _speed.magnitude;
			if (magnitude > _maxSpeed)
			{
				_speed = _maxSpeed * normalized;
			}
			return new ValueTuple<Vector2, float>(normalized, magnitude * base.projectile.speedMultiplier);
		}

		private void UpdateTarget()
		{
			if ((!(_target != null) || (!(_target.character == null) && _target.character.health.dead)) && !(_finder.range == null))
			{
				_target = _finder.Find();
			}
		}

		private void UpdateDirection(float deltaTime)
		{
			if (!(_target == null))
			{
				Vector3 vector = _target.collider.bounds.center - base.transform.position;
				float angle = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				switch (_rotateMethod)
				{
				case RotateMethod.Constant:
					_rotation = Quaternion.RotateTowards(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), _rotateSpeed * 100f * deltaTime);
					break;
				case RotateMethod.Lerp:
					_rotation = Quaternion.Lerp(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), _rotateSpeed * deltaTime);
					break;
				case RotateMethod.Slerp:
					_rotation = Quaternion.Slerp(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), _rotateSpeed * deltaTime);
					break;
				}
				base.direction = _rotation.eulerAngles.z;
				base.directionVector = _rotation * Vector3.right;
			}
		}
	}
}
