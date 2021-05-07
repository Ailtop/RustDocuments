using System.Collections;
using Characters.Operations;
using Characters.Operations.Attack;
using Level;
using Services;
using Singletons;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Hero
{
	public class KilivanFinish : MonoBehaviour
	{
		[SerializeField]
		private Character _owner;

		[SerializeField]
		private float _speed;

		[SerializeField]
		private LayerMask _collision;

		[SerializeField]
		private Transform _firePosition;

		[SerializeField]
		private Transform _projectile;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _fireOperations;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _hitOperations;

		[SerializeField]
		[Subcomponent(typeof(SweepAttack))]
		private SweepAttack _sweepAttack;

		private Vector2 _direction;

		private void Awake()
		{
			_fireOperations.Initialize();
			_hitOperations.Initialize();
			_sweepAttack.Initialize();
		}

		public IEnumerator CFire()
		{
			_direction = ((_owner.lookingDirection == Character.LookingDirection.Right) ? Vector2.right : Vector2.left);
			yield return CMove(Singleton<Service>.Instance.levelManager.player.transform.position);
		}

		private IEnumerator CMove(Vector2 destination)
		{
			_projectile.transform.position = _firePosition.position;
			_sweepAttack.Run(_owner);
			Show();
			float num = _speed * Chronometer.global.deltaTime;
			while (!DetectCollision(destination, num))
			{
				yield return null;
				num = _speed * Chronometer.global.deltaTime;
				_projectile.Translate(_direction * num, Space.World);
			}
			_sweepAttack.Stop();
			Hide();
		}

		private bool DetectCollision(Vector2 destination, float speed)
		{
			float x = _projectile.transform.position.x;
			if ((_direction.x >= 0f && x > destination.x) || (_direction.x <= 0f && x < destination.x))
			{
				OnCollision(destination);
				return true;
			}
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_projectile.transform.position, _direction, speed, _collision);
			if ((bool)raycastHit2D)
			{
				OnCollision(raycastHit2D.point);
				return true;
			}
			return false;
		}

		private void OnCollision(Vector2 hitPoint)
		{
			_projectile.transform.position = hitPoint;
			_hitOperations.gameObject.SetActive(true);
			_hitOperations.Run(_owner);
			float x = hitPoint.x;
			Evaluate(ref x);
			hitPoint = new Vector2(x, hitPoint.y);
			_owner.movement.controller.TeleportUponGround(hitPoint, 3f);
		}

		private void Evaluate(ref float x)
		{
			float num = Map.Instance.bounds.max.x - _owner.collider.bounds.size.x;
			float num2 = Map.Instance.bounds.min.x + _owner.collider.bounds.size.x;
			if (x > num)
			{
				x = num;
			}
			if (x < num2)
			{
				x = num2;
			}
		}

		private void Show()
		{
			_projectile.gameObject.SetActive(true);
		}

		private void Hide()
		{
			_projectile.gameObject.SetActive(false);
		}
	}
}
