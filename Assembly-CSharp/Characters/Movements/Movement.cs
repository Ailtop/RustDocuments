using System;
using PhysicsUtils;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Movements
{
	public class Movement : MonoBehaviour
	{
		[Serializable]
		public class Config
		{
			public enum Type
			{
				Static,
				Walking,
				Flying,
				AcceleratingFlying,
				AcceleratingWalking
			}

			public static readonly Config @static = new Config(Type.Static);

			[SerializeField]
			internal Type type = Type.Walking;

			[SerializeField]
			internal bool keepMove;

			[SerializeField]
			internal bool snapToGround;

			[SerializeField]
			internal bool lockLookingDirection;

			[SerializeField]
			internal float gravity = -40f;

			[SerializeField]
			internal float maxFallSpeed = 25f;

			[SerializeField]
			internal float acceleration = 2f;

			[SerializeField]
			internal float friction = 0.95f;

			[SerializeField]
			internal bool ignoreGravity;

			[SerializeField]
			internal bool ignorePush;

			public Config()
			{
			}

			public Config(Type type)
			{
				this.type = type;
			}
		}

		public enum CollisionDirection
		{
			None,
			Above,
			Below,
			Left,
			Right
		}

		public enum JumpType
		{
			GroundJump,
			AirJump,
			DownJump
		}

		public delegate void onJumpDelegate(JumpType jumpType, float jumpHeight);

		public readonly Sum<int> airJumpCount = new Sum<int>(1);

		public readonly TrueOnlyLogicalSumList ignoreGravity = new TrueOnlyLogicalSumList();

		[NonSerialized]
		public int currentAirJumpCount;

		[NonSerialized]
		public bool binded;

		[NonSerialized]
		public TrueOnlyLogicalSumList blocked = new TrueOnlyLogicalSumList();

		[NonSerialized]
		public Vector2 move;

		[NonSerialized]
		public Vector2 force;

		[NonSerialized]
		public bool moveBackward;

		[SerializeField]
		private Character _character;

		[SerializeField]
		private Config _config;

		[SerializeField]
		[GetComponent]
		private CharacterController2D _controller;

		private Vector2 _moved;

		private Vector2 _velocity;

		private static readonly NonAllocCaster _belowCaster;

		public readonly PriorityList<Config> configs = new PriorityList<Config>();

		public Push push;

		private float speed => _character.stat.GetInterpolatedMovementSpeed();

		private float knockbackMultiplier => (float)_character.stat.GetFinal(Stat.Kind.KnockbackResistance);

		public Config config => configs[0];

		public Vector2 lastDirection { get; private set; }

		public CharacterController2D controller => _controller;

		public Character owner => _character;

		public Vector2 moved => _moved;

		public Vector2 velocity => _velocity;

		public float verticalVelocity
		{
			get
			{
				return _velocity.y;
			}
			set
			{
				_velocity.y = value;
			}
		}

		public bool isGrounded { get; private set; }

		public event Action onGrounded;

		public event Action onFall;

		public event onJumpDelegate onJump;

		public event Action<Vector2> onMoved;

		static Movement()
		{
			_belowCaster = new NonAllocCaster(15);
		}

		protected virtual void Awake()
		{
			push = new Push(_character);
			configs.Add(int.MinValue, _config);
			_controller = GetComponent<CharacterController2D>();
			_controller.collisionState.aboveCollisionDetector.OnEnter += delegate(RaycastHit2D hit)
			{
				OnControllerCollide(hit, CollisionDirection.Above);
			};
			_controller.collisionState.belowCollisionDetector.OnEnter += delegate(RaycastHit2D hit)
			{
				OnControllerCollide(hit, CollisionDirection.Below);
			};
			_controller.collisionState.leftCollisionDetector.OnEnter += delegate(RaycastHit2D hit)
			{
				OnControllerCollide(hit, CollisionDirection.Left);
			};
			_controller.collisionState.rightCollisionDetector.OnEnter += delegate(RaycastHit2D hit)
			{
				OnControllerCollide(hit, CollisionDirection.Right);
			};
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn += FindClosestBelowGround;
			currentAirJumpCount = 0;
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn -= FindClosestBelowGround;
			}
		}

		private void FindClosestBelowGround()
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, Vector2.down, float.PositiveInfinity, Layers.groundMask);
			if ((bool)raycastHit2D)
			{
				controller.collisionState.lastStandingCollider = raycastHit2D.collider;
			}
		}

		private void Start()
		{
			if (config.snapToGround)
			{
				_controller.Move(new Vector2(0f, -50f));
			}
		}

		private void OnControllerCollide(RaycastHit2D raycastHit, CollisionDirection direction)
		{
			if (push.smash && !push.expired)
			{
				push.CollideWith(raycastHit, direction);
			}
		}

		private bool HandlePush(float deltaTime)
		{
			if (_config.ignorePush)
			{
				return false;
			}
			if (!push.expired)
			{
				Vector2 vector;
				push.Update(out vector, deltaTime);
				_controller.ignoreAbovePlatform = !push.smash;
				vector *= knockbackMultiplier;
				if (push.ignoreOtherForce)
				{
					_moved = _controller.Move(vector);
					_velocity = Vector2.zero;
					return true;
				}
				force += vector;
				return false;
			}
			_controller.ignoreAbovePlatform = true;
			return false;
		}

		private Vector2 HandleMove(float deltaTime)
		{
			Vector2 zero = Vector2.zero;
			if (HandlePush(deltaTime))
			{
				return zero;
			}
			float num = (blocked.value ? 0f : speed);
			zero = move * num;
			_character.animationController.parameter.walk = zero.x != 0f;
			_character.animationController.parameter.movementSpeed = (moveBackward ? (0f - num) : num) * 0.25f;
			if (!config.lockLookingDirection)
			{
				if (moveBackward)
				{
					if (move.x > 0f)
					{
						_character.lookingDirection = Character.LookingDirection.Left;
					}
					else if (move.x < 0f)
					{
						_character.lookingDirection = Character.LookingDirection.Right;
					}
				}
				else if (move.x > 0f)
				{
					_character.lookingDirection = Character.LookingDirection.Right;
				}
				else if (move.x < 0f)
				{
					_character.lookingDirection = Character.LookingDirection.Left;
				}
			}
			if (_controller.isGrounded && _velocity.y < 0f)
			{
				_velocity.y = 0f;
			}
			switch (config.type)
			{
			case Config.Type.Walking:
				_velocity.x = zero.x;
				AddGravity(deltaTime);
				break;
			case Config.Type.Flying:
				_velocity = zero;
				break;
			case Config.Type.AcceleratingFlying:
				_velocity *= 1f - config.friction * deltaTime;
				_velocity += zero * config.acceleration * deltaTime;
				AddGravity(deltaTime);
				break;
			case Config.Type.AcceleratingWalking:
				_velocity.x *= 1f - config.friction * deltaTime;
				_velocity.x += zero.x * config.acceleration * deltaTime;
				if (Mathf.Abs(_velocity.x) > num)
				{
					_velocity.x = num * Mathf.Sign(_velocity.x);
				}
				AddGravity(deltaTime);
				break;
			}
			zero = _velocity * deltaTime + force;
			_moved = _controller.Move(zero);
			_velocity = _moved - force;
			if (zero.x > 0f != _velocity.x > 0f)
			{
				_velocity.x = 0f;
			}
			if (zero.y > 0f != _velocity.y > 0f)
			{
				_velocity.y = 0f;
			}
			_character.animationController.parameter.ySpeed = _velocity.y;
			if (deltaTime > 0f)
			{
				_velocity /= deltaTime;
				if (config.type == Config.Type.AcceleratingFlying && _velocity.sqrMagnitude > num * num)
				{
					_velocity = _velocity.normalized * num;
				}
				_controller.velocity = _velocity;
			}
			this.onMoved?.Invoke(_moved);
			return zero;
		}

		private void AddGravity(float deltaTime)
		{
			if (!ignoreGravity.value && !config.ignoreGravity)
			{
				_velocity.y += config.gravity * deltaTime;
			}
			if (_velocity.y < 0f - config.maxFallSpeed)
			{
				_velocity.y = 0f - config.maxFallSpeed;
			}
		}

		protected virtual void LateUpdate()
		{
			Config config = this.config;
			if (config.type == Config.Type.Static)
			{
				_character.animationController.parameter.grounded = true;
				_character.animationController.parameter.walk = false;
				_character.animationController.parameter.ySpeed = 0f;
				return;
			}
			float num = _character.chronometer.animation.DeltaTime();
			if (num == 0f)
			{
				return;
			}
			_controller.UpdateBounds();
			bool flag = isGrounded;
			_moved = Vector2.zero;
			Vector2 vector = HandleMove(num);
			if (config.type == Config.Type.Flying || config.type == Config.Type.AcceleratingFlying)
			{
				_character.animationController.parameter.grounded = true;
			}
			else
			{
				_character.animationController.parameter.grounded = _controller.isGrounded;
			}
			force = Vector2.zero;
			if (!config.keepMove)
			{
				move = Vector2.zero;
			}
			if (vector.y <= 0f && _controller.collisionState.below)
			{
				isGrounded = true;
				if (!flag)
				{
					this.onGrounded?.Invoke();
					currentAirJumpCount = 0;
					if (_velocity.y <= 0f && !push.expired && push.expireOnGround)
					{
						push.Expire();
					}
				}
			}
			else
			{
				if (isGrounded && this.onFall != null)
				{
					this.onFall();
				}
				isGrounded = false;
			}
		}

		public void Move(Vector2 normalizedDirection)
		{
			if (config.keepMove)
			{
				if (normalizedDirection == Vector2.zero)
				{
					return;
				}
				if (normalizedDirection.x > 0f)
				{
					normalizedDirection.x = 1f;
				}
				if (normalizedDirection.x < 0f)
				{
					normalizedDirection.x = -1f;
				}
			}
			move = normalizedDirection;
			lastDirection = move;
		}

		public void Move(float angle)
		{
			move.x = Mathf.Cos(angle);
			move.y = Mathf.Sin(angle);
			lastDirection = move;
		}

		public void MoveTo(Vector2 position)
		{
			Move(new Vector2(position.x - base.transform.position.x, position.y - base.transform.position.y).normalized);
		}

		public void Jump(float jumpHeight)
		{
			if (jumpHeight > float.Epsilon)
			{
				_velocity.y = Mathf.Sqrt(2f * jumpHeight * (0f - config.gravity));
			}
			this.onJump?.Invoke((!isGrounded) ? JumpType.AirJump : JumpType.GroundJump, jumpHeight);
		}

		public void JumpDown()
		{
			bool ignorePlatform = _controller.ignorePlatform;
			_controller.ignorePlatform = true;
			_controller.Move(new Vector3(0f, -0.1f, 0f));
			_controller.ignorePlatform = ignorePlatform;
			this.onJump?.Invoke(JumpType.DownJump, 0f);
		}

		public bool TryBelowRayCast(LayerMask mask, out RaycastHit2D point, float distance)
		{
			_belowCaster.contactFilter.SetLayerMask(mask);
			_belowCaster.RayCast(owner.transform.position, Vector2.down, distance);
			ReadonlyBoundedList<RaycastHit2D> results = _belowCaster.results;
			point = default(RaycastHit2D);
			if (results.Count < 0)
			{
				return false;
			}
			int index = 0;
			float num = results[0].distance;
			for (int i = 1; i < results.Count; i++)
			{
				float distance2 = results[i].distance;
				if (distance2 < num)
				{
					num = distance2;
					index = i;
				}
			}
			point = results[index];
			return true;
		}

		public bool TryGetClosestBelowCollider(out Collider2D collider, LayerMask layerMask, float distance = 100f)
		{
			_belowCaster.contactFilter.SetLayerMask(layerMask);
			ReadonlyBoundedList<RaycastHit2D> results = _belowCaster.BoxCast(owner.transform.position, owner.collider.bounds.size, 0f, Vector2.down, distance).results;
			if (results.Count <= 0)
			{
				collider = null;
				return false;
			}
			int index = 0;
			float num = results[0].distance;
			for (int i = 1; i < results.Count; i++)
			{
				float distance2 = results[i].distance;
				if (distance2 < num)
				{
					num = distance2;
					index = i;
				}
			}
			collider = results[index].collider;
			return true;
		}

		public void TurnOnEdge(ref Vector2 direction)
		{
			Collider2D lastStandingCollider = controller.collisionState.lastStandingCollider;
			if (!(lastStandingCollider == null))
			{
				float num = velocity.x * _character.chronometer.master.deltaTime;
				if (_character.collider.bounds.max.x + num >= lastStandingCollider.bounds.max.x)
				{
					direction = Vector2.left;
				}
				else if (_character.collider.bounds.min.x + num <= lastStandingCollider.bounds.min.x)
				{
					direction = Vector2.right;
				}
			}
		}
	}
}
