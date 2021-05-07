using System;
using Characters.Projectiles;
using UnityEngine;

namespace Characters
{
	[Serializable]
	public class PushForce
	{
		public enum Method
		{
			OwnerDirection,
			OutsideCenter,
			Constant,
			LastSmashedDirection
		}

		public enum DirectionComputingMethod
		{
			XOnly,
			YOnly,
			Both
		}

		[SerializeField]
		private Method _method;

		[SerializeField]
		private CustomFloat _angle;

		[SerializeField]
		private CustomFloat _power;

		[SerializeField]
		private DirectionComputingMethod _directionMethod;

		[SerializeField]
		private Transform _centerTransform;

		[SerializeField]
		private Collider2D _centerCollider;

		private Vector2? _force;

		public CustomFloat angle
		{
			get
			{
				return _angle;
			}
			set
			{
				_angle = value;
			}
		}

		public CustomFloat power
		{
			get
			{
				return _power;
			}
			set
			{
				_power = value;
			}
		}

		public PushForce()
		{
			_method = Method.OwnerDirection;
			_angle = new CustomFloat(0f);
			_power = new CustomFloat(0f);
			_directionMethod = DirectionComputingMethod.XOnly;
		}

		private Vector2 EvaluateDirection(Vector2 originalDirection)
		{
			switch (_directionMethod)
			{
			case DirectionComputingMethod.XOnly:
				originalDirection.y = 0f;
				break;
			case DirectionComputingMethod.YOnly:
				originalDirection.x = 0f;
				break;
			}
			return originalDirection.normalized;
		}

		private Vector2 EvaluateOutsideCenter(Transform from, ITarget to, Vector2 force)
		{
			Vector3 vector = ((_centerTransform != null) ? _centerTransform.position : ((!(_centerCollider != null)) ? from.transform.position : _centerCollider.bounds.center));
			Vector2 vector2 = EvaluateDirection(to.transform.position - vector);
			return new Vector2(force.x * vector2.x - force.y * vector2.y, force.x * vector2.y + force.y * vector2.x);
		}

		private Vector2 RotateByDirection(Vector2 direction, Vector2 value)
		{
			return new Vector2(value.x * direction.x - value.y * direction.y, value.x * direction.y + value.y * direction.x);
		}

		public Vector2 Evaluate(Transform from, ITarget to)
		{
			float value = _angle.value;
			Vector2 vector = new Vector2(Mathf.Cos(value * ((float)Math.PI / 180f)), Mathf.Sin(value * ((float)Math.PI / 180f))) * _power.value;
			switch (_method)
			{
			case Method.OwnerDirection:
			{
				bool flag = false;
				if (from.lossyScale.x < 0f)
				{
					flag = !flag;
				}
				if (from.rotation.eulerAngles.z == 180f)
				{
					flag = !flag;
				}
				if (flag)
				{
					vector.x *= -1f;
				}
				break;
			}
			case Method.OutsideCenter:
				vector = EvaluateOutsideCenter(from.transform, to, vector);
				break;
			case Method.LastSmashedDirection:
				if (to.character != null)
				{
					vector = RotateByDirection(EvaluateDirection(to.character.movement.push.direction), vector);
				}
				break;
			}
			return vector;
		}

		public Vector2 Evaluate(Projectile from, ITarget to)
		{
			float value = _angle.value;
			Vector2 vector = new Vector2(Mathf.Cos(value * ((float)Math.PI / 180f)), Mathf.Sin(value * ((float)Math.PI / 180f))) * _power.value;
			switch (_method)
			{
			case Method.OwnerDirection:
			{
				Vector2 directionVector = from.movement.directionVector;
				vector = new Vector2(vector.x * directionVector.x - vector.y * directionVector.y, vector.x * directionVector.y + vector.y * directionVector.x);
				break;
			}
			case Method.OutsideCenter:
				vector = EvaluateOutsideCenter(from.transform, to, vector);
				break;
			case Method.LastSmashedDirection:
				if (to.character != null)
				{
					vector = RotateByDirection(EvaluateDirection(to.character.movement.push.direction), vector);
				}
				break;
			}
			return vector;
		}

		public Vector2 Evaluate(Character from, ITarget to)
		{
			float value = _angle.value;
			Vector2 vector = new Vector2(Mathf.Cos(value * ((float)Math.PI / 180f)), Mathf.Sin(value * ((float)Math.PI / 180f))) * _power.value;
			switch (_method)
			{
			case Method.OwnerDirection:
				if (from.lookingDirection != 0)
				{
					vector.x *= -1f;
				}
				break;
			case Method.OutsideCenter:
				vector = EvaluateOutsideCenter(from.transform, to, vector);
				break;
			case Method.LastSmashedDirection:
				if (to.character != null)
				{
					vector = RotateByDirection(EvaluateDirection(to.character.movement.push.direction), vector);
				}
				break;
			}
			return vector;
		}
	}
}
