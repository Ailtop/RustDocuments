using System;
using System.Runtime.CompilerServices;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class Ground : Movement
	{
		private class TerrainCheckCaster
		{
			private RayCaster _groundLeft;

			private RayCaster _groundRight;

			private RayCaster _wallLeft;

			private RayCaster _wallRight;

			internal TerrainCheckCaster()
			{
				_groundLeft = new RayCaster
				{
					direction = Vector2.down
				};
				_groundRight = new RayCaster
				{
					direction = Vector2.down
				};
				_wallLeft = new RayCaster
				{
					direction = Vector2.left
				};
				_wallRight = new RayCaster
				{
					direction = Vector2.right
				};
			}

			[return: TupleElementNames(new string[] { "groundLeft", "groundRight", "wallLeft", "wallRight" })]
			internal ValueTuple<RaycastHit2D, RaycastHit2D, RaycastHit2D, RaycastHit2D> Cast(Bounds bounds, float groundDistance, float wallDistance)
			{
				_groundLeft.contactFilter.SetLayerMask(Layers.footholdMask);
				_groundRight.contactFilter.SetLayerMask(Layers.footholdMask);
				Vector2 origin = new Vector2(bounds.min.x, bounds.min.y);
				Vector2 origin2 = new Vector2(bounds.max.x, bounds.min.y);
				_groundLeft.origin = origin;
				_groundLeft.distance = groundDistance;
				_groundRight.origin = origin2;
				_groundRight.distance = groundDistance;
				_wallLeft.contactFilter.SetLayerMask(Layers.terrainMask);
				_wallRight.contactFilter.SetLayerMask(Layers.terrainMask);
				_wallLeft.origin = origin;
				_wallLeft.distance = wallDistance;
				_wallRight.origin = origin2;
				_wallRight.distance = wallDistance;
				return new ValueTuple<RaycastHit2D, RaycastHit2D, RaycastHit2D, RaycastHit2D>(_groundLeft.SingleCast(), _groundRight.SingleCast(), _wallLeft.SingleCast(), _wallRight.SingleCast());
			}
		}

		public enum Action
		{
			Hold,
			Despawn,
			Return,
			Cotinue
		}

		private const float offset = 0.03125f;

		private TerrainCheckCaster _caster;

		[SerializeField]
		private Action _onFaceClif;

		[SerializeField]
		private Action _onFaceWall = Action.Return;

		private Action _state;

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
		private bool _flipXByFacingDirection = true;

		private float _ySpeed;

		private bool _grounded;

		public override void Initialize(Projectile projectile, float direction)
		{
			base.Initialize(projectile, direction);
			_ySpeed = 0f;
			_state = Action.Cotinue;
			_grounded = false;
			_caster = new TerrainCheckCaster();
			SetScaleByFacingDirection();
		}

		private void SetScaleByFacingDirection()
		{
			if (_flipXByFacingDirection)
			{
				Vector3 localScale = base.projectile.transform.localScale;
				localScale.x = Mathf.Abs(localScale.x) * (float)((base.directionVector.x > 0f) ? 1 : (-1));
				base.projectile.transform.localScale = localScale;
			}
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			Vector2 vector = (_startSpeed + (_targetSpeed - _startSpeed) * _curve.Evaluate(time / _easingTime)) * base.directionVector * base.projectile.speedMultiplier;
			_ySpeed -= _gravity * deltaTime;
			vector.y += _ySpeed;
			base.projectile.collider.enabled = true;
			Bounds bounds = base.projectile.collider.bounds;
			base.projectile.collider.enabled = false;
			Vector3 vector2 = vector * deltaTime;
			bounds.center += vector2;
			ValueTuple<RaycastHit2D, RaycastHit2D, RaycastHit2D, RaycastHit2D> valueTuple = _caster.Cast(bounds, Mathf.Abs(vector2.y) + 0.0625f, Mathf.Abs(vector2.x) + 0.0625f);
			if ((bool)valueTuple.Item1 && (bool)valueTuple.Item2)
			{
				float num = Mathf.Min(valueTuple.Item1.distance, valueTuple.Item2.distance);
				vector.y = 0f - Mathf.Max(-0.03125f, num - 0.03125f);
				_ySpeed = 0f;
				_grounded = true;
			}
			else if ((bool)valueTuple.Item1)
			{
				vector.y = 0f - Mathf.Max(-0.03125f, valueTuple.Item1.distance - 0.03125f);
				_ySpeed = 0f;
				_grounded = true;
			}
			else if ((bool)valueTuple.Item2)
			{
				vector.y = 0f - Mathf.Max(-0.03125f, valueTuple.Item2.distance - 0.03125f);
				_ySpeed = 0f;
				_grounded = true;
			}
			if ((bool)valueTuple.Item3 || (bool)valueTuple.Item4)
			{
				switch (_onFaceWall)
				{
				case Action.Hold:
					_state = Action.Hold;
					break;
				case Action.Return:
					base.direction -= 180f;
					base.directionVector = new Vector2(0f - base.directionVector.x, base.directionVector.y);
					vector.x *= -1f;
					SetScaleByFacingDirection();
					break;
				case Action.Despawn:
					base.projectile.Despawn();
					break;
				}
			}
			else if (_grounded && (!valueTuple.Item1 || !valueTuple.Item2))
			{
				switch (_onFaceClif)
				{
				case Action.Hold:
					_state = Action.Hold;
					break;
				case Action.Return:
					base.direction -= 180f;
					base.directionVector = new Vector2(0f - base.directionVector.x, base.directionVector.y);
					vector.x *= -1f;
					SetScaleByFacingDirection();
					break;
				case Action.Despawn:
					base.projectile.Despawn();
					break;
				}
			}
			_grounded = false;
			if (_state == Action.Hold)
			{
				return new ValueTuple<Vector2, float>(base.directionVector, 0f);
			}
			return new ValueTuple<Vector2, float>(vector.normalized, vector.magnitude);
		}
	}
}
