using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class TrajectoryToPoint : Movement
	{
		[SerializeField]
		private TargetFinder _finder;

		[SerializeField]
		private float _easingTime;

		[SerializeField]
		private float _gravity;

		private float _elapseTime;

		private float _targetDistance;

		private Target _target;

		private bool _isInitialized;

		private float _firingAngle = 45f;

		private Vector3 _targetPosition = Vector3.zero;

		public void OnEnable()
		{
			if (_isInitialized)
			{
				InitializedTrajectory();
			}
		}

		public override void Initialize(Projectile projectile, float direction)
		{
			if (_finder.range != null)
			{
				_finder.Initialize(projectile);
				InitializedTrajectory();
				Target target = _finder.Find();
				Vector3 vector = target.collider.bounds.center - base.transform.position;
				_targetDistance = Vector3.Distance(base.transform.position, target.collider.bounds.center);
				_firingAngle = ((base.transform.position.x < target.transform.position.x) ? _firingAngle : (_firingAngle + 90f));
				Debug.Log(_firingAngle);
				_firingAngle = 135f;
				Debug.Log(_firingAngle);
				float f = _targetDistance / (Mathf.Sin(2f * _firingAngle * ((float)Math.PI / 180f)) / _gravity);
				float num = Mathf.Sqrt(f) * Mathf.Cos(_firingAngle * ((float)Math.PI / 180f));
				Mathf.Sqrt(f);
				Mathf.Sin(_firingAngle * ((float)Math.PI / 180f));
				float num2 = _targetDistance / num;
				if (target != null)
				{
					direction = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
					Debug.Log(direction);
				}
				_elapseTime = 0f;
			}
			base.Initialize(projectile, direction);
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			_targetDistance = Vector3.Distance(base.transform.position, _targetPosition);
			float f = _targetDistance / (Mathf.Sin(2f * _firingAngle * ((float)Math.PI / 180f)) / _gravity);
			float x = Mathf.Sqrt(f) * Mathf.Cos(_firingAngle * ((float)Math.PI / 180f));
			float num = Mathf.Sqrt(f) * Mathf.Sin(_firingAngle * ((float)Math.PI / 180f));
			_elapseTime += Time.deltaTime;
			Vector2 vector = default(Vector2);
			vector.x = x;
			vector.y = num - _gravity * _elapseTime;
			float magnitude = new Vector2(x, num - _gravity * _elapseTime).magnitude;
			return new ValueTuple<Vector2, float>(vector.normalized, vector.magnitude);
		}

		public bool InitializedTrajectory()
		{
			if (_finder == null || _finder.range == null)
			{
				return false;
			}
			_target = _finder.Find();
			_elapseTime = 0f;
			Bounds bounds = _finder.Find().collider.bounds;
			_targetPosition = new Vector3((bounds.min.x + bounds.max.x) / 2f, bounds.min.y);
			_firingAngle = ((_target.transform.position.x > base.transform.position.x) ? _firingAngle : (0f - _firingAngle));
			if (!(_target != null))
			{
				return false;
			}
			return true;
		}
	}
}
