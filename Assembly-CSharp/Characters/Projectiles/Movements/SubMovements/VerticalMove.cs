using System.Collections;
using UnityEngine;

namespace Characters.Projectiles.Movements.SubMovements
{
	public class VerticalMove : SubMovement
	{
		[SerializeField]
		private float _height = 3f;

		[SerializeField]
		private Curve _curve;

		private Projectile _projectile;

		public override void Move(Projectile projectile)
		{
			_projectile = projectile;
			StartCoroutine(CMove());
		}

		private void OnEnable()
		{
			base.transform.localPosition = Vector2.zero;
		}

		private void OnDisable()
		{
			base.transform.localPosition = Vector2.zero;
			StopAllCoroutines();
		}

		private IEnumerator CMove()
		{
			float elpased = 0f;
			float startY = 0f;
			int lookingDirection = ((_projectile.owner.lookingDirection == Character.LookingDirection.Right) ? 1 : (-1));
			float destinationY = _height * (float)lookingDirection;
			while (elpased < _curve.duration)
			{
				yield return null;
				elpased += Chronometer.global.deltaTime;
				float y = Mathf.Lerp(startY, destinationY, _curve.Evaluate(elpased / _curve.duration));
				Vector2 vector = new Vector2(0f, y);
				Vector2 direction = _projectile.direction;
				if (elpased >= _curve.duration / 2f)
				{
					direction += Vector2.down * lookingDirection;
				}
				else
				{
					direction += Vector2.up * lookingDirection;
				}
				_projectile.DetectCollision(base.transform.position, direction.normalized, _projectile.owner.chronometer.projectile.deltaTime);
				base.transform.localPosition = vector;
			}
		}
	}
}
