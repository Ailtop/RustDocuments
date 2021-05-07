using UnityEngine;

namespace BT
{
	public class RangeWander : Node
	{
		private static readonly int _idleHash = Animator.StringToHash("Idle");

		private static readonly int _walkHash = Animator.StringToHash("Walk");

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private CustomFloat _speed;

		[SerializeField]
		[MinMaxSlider(0f, 20f)]
		private Vector2 _idleTime;

		[SerializeField]
		[MinMaxSlider(0f, 20f)]
		private Vector2 _wanderTime;

		[SerializeField]
		private Collider2D _customRange;

		private const float _groundFindingRayDistance = 9f;

		private const float _minDistanceFromSide = 2f;

		private bool _isWandering;

		private float _remainTime;

		private Vector2 _direction;

		private float _speedValue;

		private Collider2D _range;

		protected override void OnInitialize()
		{
			Initialize();
			base.OnInitialize();
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			Transform transform = context.Get<Transform>(Key.OwnerTransform);
			if (transform == null)
			{
				Debug.LogError("OwnerTransform is null");
				return NodeState.Fail;
			}
			float deltaTime = context.deltaTime;
			if (_isWandering)
			{
				Wander(transform, deltaTime);
			}
			_remainTime -= deltaTime;
			if (_remainTime <= 0f)
			{
				Initialize();
			}
			return NodeState.Success;
		}

		private void Initialize()
		{
			_speedValue = _speed.value;
			_isWandering = MMMaths.RandomBool();
			if (_customRange == null)
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, Vector2.down, 9f, Layers.groundMask);
				if ((bool)raycastHit2D)
				{
					_range = raycastHit2D.collider;
				}
			}
			else
			{
				_range = _customRange;
			}
			if (_isWandering)
			{
				OnStartWander();
			}
			else
			{
				OnStartIdle();
			}
		}

		private void Wander(Transform owner, float deltaTime)
		{
			Flip(owner);
			if (_direction.x > 0f)
			{
				_spriteRenderer.flipX = false;
			}
			else
			{
				_spriteRenderer.flipX = true;
			}
			owner.Translate(_direction * deltaTime * _speedValue);
		}

		private void OnStartWander()
		{
			_direction = (MMMaths.RandomBool() ? Vector2.right : Vector2.left);
			_remainTime = UnityEngine.Random.Range(_wanderTime.x, _wanderTime.y);
			_animator.Play(_walkHash);
		}

		private void OnStartIdle()
		{
			_remainTime = UnityEngine.Random.Range(_idleTime.x, _idleTime.y);
			_animator.Play(_idleHash);
		}

		private void Flip(Transform owner)
		{
			Bounds bounds = _range.bounds;
			if (_direction == Vector2.right && bounds.max.x - 2f < owner.transform.position.x)
			{
				_direction = Vector2.left;
			}
			if (_direction == Vector2.left && bounds.min.x + 2f > owner.transform.position.x)
			{
				_direction = Vector2.right;
			}
		}
	}
}
