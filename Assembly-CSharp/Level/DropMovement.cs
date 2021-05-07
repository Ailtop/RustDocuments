using System;
using System.Collections;
using System.Collections.Generic;
using PhysicsUtils;
using UnityEngine;

namespace Level
{
	public class DropMovement : MonoBehaviour
	{
		private const float _droppedGearHorizontalInterval = 1.5f;

		private const float _droppedGearBasicHorizontalSpeed = 0.5f;

		[SerializeField]
		private Transform _graphic;

		[SerializeField]
		private float _minDistanceFromGround = 1f;

		[SerializeField]
		private float _minDistanceFromSides = 1f;

		[SerializeField]
		private float _jumpPower = 15f;

		[SerializeField]
		private float _gravity = 40f;

		[SerializeField]
		private float _maxFallSpeed = 40f;

		[SerializeField]
		private float _floatAmplitude = 0.5f;

		[SerializeField]
		private float _floatFrequency = 1f;

		private float _movedHorizontalDistance;

		[NonSerialized]
		public float horizontalSpeed;

		[NonSerialized]
		public float maxHorizontalDistance;

		private Vector2 _speed;

		private RayCaster _aboveCaster;

		private RayCaster _belowCaster;

		private RayCaster _leftCaster;

		private RayCaster _rightCaster;

		public event Action onGround;

		public static void SetMultiDropHorizontalInterval(IList<DropMovement> dropMovements)
		{
			if (dropMovements.Count <= 1)
			{
				return;
			}
			int num = dropMovements.Count / 2;
			float num2 = ((dropMovements.Count % 2 == 0) ? 0.5f : 0f);
			for (int i = 0; i <= num; i++)
			{
				DropMovement dropMovement = dropMovements[i];
				DropMovement dropMovement2 = dropMovements[dropMovements.Count - 1 - i];
				if (dropMovement == dropMovement2)
				{
					dropMovement.maxHorizontalDistance = 0f;
					continue;
				}
				float num3 = 1.5f * ((float)(num - i) + num2);
				float num4 = num3 + 0.5f;
				dropMovement.horizontalSpeed = 0f - num4;
				dropMovement.maxHorizontalDistance = num3;
				dropMovement2.horizontalSpeed = num4;
				dropMovement2.maxHorizontalDistance = num3;
			}
		}

		public void Stop()
		{
			StopAllCoroutines();
			this.onGround?.Invoke();
		}

		public void Float()
		{
			StartCoroutine(CFloat());
		}

		private void Awake()
		{
			ContactFilter2D contactFilter = default(ContactFilter2D);
			contactFilter.SetLayerMask(Layers.groundMask);
			_aboveCaster = new RayCaster
			{
				direction = Vector2.up,
				contactFilter = contactFilter
			};
			_belowCaster = new RayCaster
			{
				direction = Vector2.down,
				contactFilter = contactFilter
			};
			_leftCaster = new RayCaster
			{
				direction = Vector2.left,
				contactFilter = contactFilter
			};
			_rightCaster = new RayCaster
			{
				direction = Vector2.right,
				contactFilter = contactFilter
			};
		}

		private void OnEnable()
		{
			_speed = new Vector2(0f, _jumpPower);
			_movedHorizontalDistance = 0f;
			horizontalSpeed = 0f;
			maxHorizontalDistance = 0f;
			StartCoroutine(CMove());
		}

		private IEnumerator CMove()
		{
			yield return null;
			bool moveVertical = true;
			bool moveHorizontal = true;
			while (true)
			{
				float deltaTime = Chronometer.global.deltaTime;
				if (moveVertical)
				{
					_speed.y -= _gravity * deltaTime;
					if (_speed.y > 0f)
					{
						_aboveCaster.origin = base.transform.position;
						_aboveCaster.distance = _minDistanceFromGround + _speed.y * Time.deltaTime;
						if ((bool)_aboveCaster.SingleCast())
						{
							_speed.y = 0f;
						}
					}
					else
					{
						_belowCaster.origin = base.transform.position;
						_belowCaster.distance = _minDistanceFromGround - _speed.y * Time.deltaTime;
						_belowCaster.contactFilter.SetLayerMask(Layers.groundMask);
						RaycastHit2D raycastHit2D = _belowCaster.SingleCast();
						if ((bool)raycastHit2D)
						{
							base.transform.position = raycastHit2D.point + new Vector2(0f, _minDistanceFromGround);
							_speed.y = 0f;
							moveVertical = false;
							Stop();
							Float();
						}
					}
				}
				if (moveHorizontal)
				{
					_leftCaster.origin = base.transform.position;
					_leftCaster.distance = _minDistanceFromSides + Mathf.Abs(_speed.x * Time.deltaTime);
					RaycastHit2D raycastHit2D2 = _leftCaster.SingleCast();
					_rightCaster.origin = base.transform.position;
					_rightCaster.distance = _minDistanceFromSides + Mathf.Abs(_speed.x * Time.deltaTime);
					RaycastHit2D raycastHit2D3 = _rightCaster.SingleCast();
					if ((bool)raycastHit2D2 && raycastHit2D2.distance <= _minDistanceFromSides)
					{
						_speed.x += 2f * deltaTime;
					}
					else if ((bool)raycastHit2D3 && raycastHit2D3.distance <= _minDistanceFromSides)
					{
						_speed.x -= 2f * deltaTime;
					}
					else if (_movedHorizontalDistance < maxHorizontalDistance)
					{
						_speed.x = horizontalSpeed;
						_movedHorizontalDistance += Mathf.Abs(_speed.x * deltaTime);
					}
					else
					{
						_speed.x = 0f;
						moveHorizontal = false;
					}
				}
				if (!moveHorizontal && !moveVertical)
				{
					break;
				}
				if (_speed.y < 0f - _maxFallSpeed)
				{
					_speed.y = 0f - _maxFallSpeed;
				}
				base.transform.Translate(_speed * deltaTime);
				yield return null;
			}
			Stop();
			Float();
		}

		private IEnumerator CFloat()
		{
			float t = 0f;
			while (true)
			{
				Vector3 zero = Vector3.zero;
				t += Chronometer.global.deltaTime;
				zero.y = Mathf.Sin(t * (float)Math.PI * _floatFrequency) * _floatAmplitude;
				_graphic.localPosition = zero;
				yield return null;
			}
		}
	}
}
