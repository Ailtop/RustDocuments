using Level;
using PhysicsUtils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Movements
{
	public class CharacterController2D : MonoBehaviour
	{
		public class CollisionState
		{
			internal readonly ManualCollisionDetector aboveCollisionDetector = new ManualCollisionDetector();

			internal readonly ManualCollisionDetector belowCollisionDetector = new ManualCollisionDetector();

			internal readonly ManualCollisionDetector leftCollisionDetector = new ManualCollisionDetector();

			internal readonly ManualCollisionDetector rightCollisionDetector = new ManualCollisionDetector();

			public bool above => aboveCollisionDetector.colliding;

			public bool below => belowCollisionDetector.colliding;

			public bool right => rightCollisionDetector.colliding;

			public bool left => leftCollisionDetector.colliding;

			public bool horizontal
			{
				get
				{
					if (!right)
					{
						return left;
					}
					return true;
				}
			}

			public bool vertical
			{
				get
				{
					if (!right)
					{
						return left;
					}
					return true;
				}
			}

			public bool any
			{
				get
				{
					if (!below && !right && !left)
					{
						return above;
					}
					return true;
				}
			}

			public Collider2D lastStandingCollider { get; internal set; }

			internal CollisionState()
			{
			}
		}

		[HideInInspector]
		public bool ignorePlatform;

		[HideInInspector]
		public bool ignoreAbovePlatform = true;

		private const float _extraRayLength = 0.001f;

		[Range(0.001f, 0.3f)]
		private float _skinWidth = 0.03f;

		[FormerlySerializedAs("platformMask")]
		public LayerMask terrainMask = 0;

		public LayerMask triggerMask = 0;

		public LayerMask oneWayPlatformMask = 0;

		public float jumpingThreshold = 0.07f;

		[Range(2f, 20f)]
		public int horizontalRays = 8;

		[Range(2f, 20f)]
		public int verticalRays = 4;

		[SerializeField]
		protected BoxCollider2D _boxCollider;

		[SerializeField]
		protected Rigidbody2D _rigidBody;

		public readonly CollisionState collisionState = new CollisionState();

		[HideInInspector]
		public Vector3 velocity;

		private BoxSequenceNonAllocCaster _boxCaster;

		private Bounds _bounds;

		public bool isGrounded
		{
			get
			{
				if (collisionState.below)
				{
					return velocity.y <= 0.001f;
				}
				return false;
			}
		}

		public bool onTerrain
		{
			get
			{
				if (collisionState.below)
				{
					return terrainMask.Contains(collisionState.lastStandingCollider.gameObject.layer);
				}
				return false;
			}
		}

		public bool onPlatform
		{
			get
			{
				if (collisionState.below)
				{
					return oneWayPlatformMask.Contains(collisionState.lastStandingCollider.gameObject.layer);
				}
				return false;
			}
		}

		private void Awake()
		{
			Bounds bounds = _boxCollider.bounds;
			bounds.center -= base.transform.position;
			_boxCaster = new BoxSequenceNonAllocCaster(1, horizontalRays, verticalRays);
			_boxCaster.SetOriginsFromBounds(bounds);
		}

		public void ResetBounds()
		{
			_bounds.size = Vector2.zero;
			_boxCaster.SetOriginsFromBounds(_bounds);
		}

		public void UpdateBounds()
		{
			Bounds bounds = _boxCollider.bounds;
			bounds.center -= base.transform.position;
			if (!(_bounds == bounds) && _boxCaster != null)
			{
				_boxCaster.origin = base.transform.position;
				UpdateTopCasterPosition(bounds);
				UpdateBottomCasterPosition(bounds);
				UpdateLeftCasterPosition(bounds);
				UpdateRightCasterPosition(bounds);
				_boxCaster.SetOriginsFromBounds(_bounds);
			}
		}

		private void UpdateTopCasterPosition(Bounds bounds)
		{
			Vector2 mostLeftTop = bounds.GetMostLeftTop();
			Vector2 mostRightTop = bounds.GetMostRightTop();
			LineSequenceNonAllocCaster topRaycaster = _boxCaster.topRaycaster;
			float num = mostLeftTop.y;
			if (topRaycaster.start.y < mostLeftTop.y - _skinWidth)
			{
				topRaycaster.caster.contactFilter.SetLayerMask(terrainMask);
				topRaycaster.CastToLine(mostLeftTop, mostRightTop);
				for (int i = 0; i < topRaycaster.nonAllocCasters.Count; i++)
				{
					ReadonlyBoundedList<RaycastHit2D> results = topRaycaster.nonAllocCasters[i].results;
					if (results.Count != 0)
					{
						num = math.min(num, results[0].point.y - _boxCaster.origin.y);
					}
				}
				num -= _skinWidth;
				if (num < _bounds.max.y)
				{
					return;
				}
			}
			Vector3 max = _bounds.max;
			max.y = num;
			_bounds.max = max;
		}

		private void UpdateBottomCasterPosition(Bounds bounds)
		{
			Vector2 mostLeftBottom = bounds.GetMostLeftBottom();
			Vector2 mostRightBottom = bounds.GetMostRightBottom();
			LineSequenceNonAllocCaster bottomRaycaster = _boxCaster.bottomRaycaster;
			float num = mostLeftBottom.y;
			if (bottomRaycaster.start.y > mostLeftBottom.y + _skinWidth)
			{
				bottomRaycaster.caster.contactFilter.SetLayerMask(terrainMask);
				bottomRaycaster.CastToLine(mostLeftBottom, mostRightBottom);
				for (int i = 0; i < bottomRaycaster.nonAllocCasters.Count; i++)
				{
					ReadonlyBoundedList<RaycastHit2D> results = bottomRaycaster.nonAllocCasters[i].results;
					if (results.Count != 0)
					{
						num = math.max(num, results[0].point.y - _boxCaster.origin.y);
					}
				}
				num += _skinWidth;
				if (num > _bounds.min.y)
				{
					return;
				}
			}
			Vector3 min = _bounds.min;
			min.y = num;
			_bounds.min = min;
		}

		private void UpdateLeftCasterPosition(Bounds bounds)
		{
			Vector2 mostLeftTop = bounds.GetMostLeftTop();
			Vector2 mostLeftBottom = bounds.GetMostLeftBottom();
			LineSequenceNonAllocCaster leftRaycaster = _boxCaster.leftRaycaster;
			float num = mostLeftTop.x;
			if (leftRaycaster.start.x > mostLeftTop.x + _skinWidth)
			{
				leftRaycaster.caster.contactFilter.SetLayerMask(terrainMask);
				leftRaycaster.CastToLine(mostLeftTop, mostLeftBottom);
				for (int i = 0; i < leftRaycaster.nonAllocCasters.Count; i++)
				{
					ReadonlyBoundedList<RaycastHit2D> results = leftRaycaster.nonAllocCasters[i].results;
					if (results.Count != 0)
					{
						num = math.max(num, results[0].point.x - _boxCaster.origin.x);
					}
				}
				num += _skinWidth;
				if (num > _bounds.min.x)
				{
					return;
				}
			}
			Vector3 min = _bounds.min;
			min.x = num;
			_bounds.min = min;
		}

		private void UpdateRightCasterPosition(Bounds bounds)
		{
			Vector2 mostRightTop = bounds.GetMostRightTop();
			Vector2 mostRightBottom = bounds.GetMostRightBottom();
			LineSequenceNonAllocCaster rightRaycaster = _boxCaster.rightRaycaster;
			float num = mostRightTop.x;
			if (rightRaycaster.start.x < mostRightTop.x - _skinWidth)
			{
				rightRaycaster.caster.contactFilter.SetLayerMask(terrainMask);
				rightRaycaster.CastToLine(mostRightTop, mostRightBottom);
				for (int i = 0; i < rightRaycaster.nonAllocCasters.Count; i++)
				{
					ReadonlyBoundedList<RaycastHit2D> results = rightRaycaster.nonAllocCasters[i].results;
					if (results.Count != 0)
					{
						num = math.min(num, results[0].point.x - _boxCaster.origin.x);
					}
				}
				num -= _skinWidth;
				if (num < _bounds.max.x)
				{
					return;
				}
			}
			Vector3 max = _bounds.max;
			max.x = num;
			_bounds.max = max;
		}

		public Vector2 Move(Vector2 deltaMovement)
		{
			Vector3 origin = base.transform.position;
			Move(ref origin, ref deltaMovement);
			origin.x += deltaMovement.x;
			origin.y += deltaMovement.y;
			Map instance = Map.Instance;
			if (instance != null && !instance.IsInMap(origin))
			{
				Debug.LogWarning("The new position of character " + base.name + " is out of the map. The move was ignored.");
				return Vector2.zero;
			}
			base.transform.position = origin;
			return deltaMovement;
		}

		private bool TeleportUponGround(Vector2 direction, float distance, bool recursive)
		{
			if (recursive)
			{
				Vector2 vector = base.transform.position;
				while (distance > 0f)
				{
					if (TeleportUponGround(vector + direction * distance))
					{
						return true;
					}
					distance -= 1f;
				}
			}
			else
			{
				TeleportUponGround((Vector2)base.transform.position + direction * distance);
			}
			return false;
		}

		public bool TeleportUponGround(Vector2 destination, float distance = 4f)
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(destination, Vector2.down, distance, (int)terrainMask | (int)oneWayPlatformMask);
			if ((bool)raycastHit2D)
			{
				destination = raycastHit2D.point;
				destination.y += _skinWidth * 2f;
				return Teleport(destination);
			}
			return false;
		}

		public bool Teleport(Vector2 destination, float maxRetryDistance)
		{
			return Teleport(destination, (MMMaths.Vector3ToVector2(base.transform.position) - destination).normalized, maxRetryDistance);
		}

		public bool Teleport(Vector2 destination, Vector2 direction, float maxRetryDistance)
		{
			for (int i = 0; (float)i <= maxRetryDistance; i++)
			{
				if (Teleport(destination + direction * i))
				{
					return true;
				}
			}
			return false;
		}

		public bool Teleport(Vector2 destination)
		{
			Bounds bounds = _boxCollider.bounds;
			bounds.center = new Vector2(destination.x, destination.y + (bounds.center.y - bounds.min.y));
			NonAllocOverlapper.shared.contactFilter.SetLayerMask(terrainMask);
			if (NonAllocOverlapper.shared.OverlapBox(bounds.center, bounds.size, 0f).results.Count == 0)
			{
				base.transform.position = destination;
				return true;
			}
			return false;
		}

		public bool IsInTerrain()
		{
			NonAllocOverlapper.shared.contactFilter.SetLayerMask(terrainMask);
			return NonAllocOverlapper.shared.OverlapBox(base.transform.position + _bounds.center, _bounds.size, 0f).results.Count > 0;
		}

		private void Move(ref Vector3 origin, ref Vector2 deltaMovement)
		{
			int num = 0;
			bool flag;
			do
			{
				flag = false;
				num++;
				if (!CastLeft(ref origin, ref deltaMovement))
				{
					origin.x += 0.1f * (float)num;
					flag = true;
				}
				if (!CastRight(ref origin, ref deltaMovement))
				{
					origin.x -= 0.1f * (float)num;
					flag = true;
				}
				if (!CastUp(ref origin, ref deltaMovement))
				{
					origin.y -= 0.1f * (float)num;
					flag = true;
				}
				if (!CastDown(ref origin, ref deltaMovement))
				{
					origin.y += 0.1f * (float)num;
					flag = true;
				}
			}
			while (flag && num < 30);
		}

		private bool CastRight(ref Vector3 origin, ref Vector2 deltaMovement)
		{
			float num = _skinWidth + 0.001f;
			if (deltaMovement.x > 0f)
			{
				num += deltaMovement.x;
			}
			_boxCaster.rightRaycaster.caster.contactFilter.SetLayerMask(terrainMask);
			_boxCaster.rightRaycaster.caster.origin = origin;
			_boxCaster.rightRaycaster.caster.distance = num;
			_boxCaster.rightRaycaster.Cast();
			using (collisionState.rightCollisionDetector.scope)
			{
				for (int i = 0; i < _boxCaster.rightRaycaster.nonAllocCasters.Count; i++)
				{
					NonAllocCaster nonAllocCaster = _boxCaster.rightRaycaster.nonAllocCasters[i];
					if (nonAllocCaster.results.Count == 0)
					{
						continue;
					}
					RaycastHit2D raycastHit2D = nonAllocCaster.results[0];
					if ((bool)raycastHit2D)
					{
						collisionState.rightCollisionDetector.Add(raycastHit2D);
						if (raycastHit2D.distance == 0f)
						{
							return false;
						}
						deltaMovement.x = math.min(deltaMovement.x, raycastHit2D.distance - _skinWidth);
					}
				}
			}
			return true;
		}

		private bool CastLeft(ref Vector3 origin, ref Vector2 deltaMovement)
		{
			float num = _skinWidth + 0.001f;
			if (deltaMovement.x < 0f)
			{
				num += 0f - deltaMovement.x;
			}
			_boxCaster.leftRaycaster.caster.contactFilter.SetLayerMask(terrainMask);
			_boxCaster.leftRaycaster.caster.origin = origin;
			_boxCaster.leftRaycaster.caster.distance = num;
			_boxCaster.leftRaycaster.Cast();
			using (collisionState.leftCollisionDetector.scope)
			{
				for (int i = 0; i < _boxCaster.leftRaycaster.nonAllocCasters.Count; i++)
				{
					NonAllocCaster nonAllocCaster = _boxCaster.leftRaycaster.nonAllocCasters[i];
					if (nonAllocCaster.results.Count == 0)
					{
						continue;
					}
					RaycastHit2D raycastHit2D = nonAllocCaster.results[0];
					if ((bool)raycastHit2D)
					{
						collisionState.leftCollisionDetector.Add(raycastHit2D);
						if (raycastHit2D.distance == 0f)
						{
							return false;
						}
						deltaMovement.x = math.max(deltaMovement.x, 0f - raycastHit2D.distance + _skinWidth);
					}
				}
			}
			return true;
		}

		private bool CastUp(ref Vector3 origin, ref Vector2 deltaMovement)
		{
			float num = _skinWidth + 0.001f;
			if (deltaMovement.y > 0f)
			{
				num += deltaMovement.y;
			}
			if (ignoreAbovePlatform)
			{
				_boxCaster.topRaycaster.caster.contactFilter.SetLayerMask(terrainMask);
			}
			else
			{
				_boxCaster.topRaycaster.caster.contactFilter.SetLayerMask((int)terrainMask | (int)oneWayPlatformMask);
			}
			_boxCaster.topRaycaster.caster.origin = origin;
			_boxCaster.topRaycaster.caster.distance = num;
			_boxCaster.topRaycaster.Cast();
			using (collisionState.aboveCollisionDetector.scope)
			{
				for (int i = 0; i < _boxCaster.topRaycaster.nonAllocCasters.Count; i++)
				{
					NonAllocCaster nonAllocCaster = _boxCaster.topRaycaster.nonAllocCasters[i];
					if (nonAllocCaster.results.Count == 0)
					{
						continue;
					}
					RaycastHit2D raycastHit2D = nonAllocCaster.results[0];
					if ((bool)raycastHit2D)
					{
						collisionState.aboveCollisionDetector.Add(raycastHit2D);
						if (raycastHit2D.distance == 0f)
						{
							return false;
						}
						deltaMovement.y = math.min(deltaMovement.y, raycastHit2D.distance - _skinWidth);
					}
				}
			}
			return true;
		}

		private bool CastDown(ref Vector3 origin, ref Vector2 deltaMovement)
		{
			float num = _skinWidth + 0.001f;
			if (deltaMovement.y < 0f)
			{
				num += 0f - deltaMovement.y;
			}
			if (ignorePlatform)
			{
				_boxCaster.bottomRaycaster.caster.contactFilter.SetLayerMask(terrainMask);
			}
			else
			{
				_boxCaster.bottomRaycaster.caster.contactFilter.SetLayerMask((int)terrainMask | (int)oneWayPlatformMask);
			}
			_boxCaster.bottomRaycaster.caster.origin = origin;
			_boxCaster.bottomRaycaster.caster.distance = num;
			_boxCaster.bottomRaycaster.Cast();
			using (collisionState.belowCollisionDetector.scope)
			{
				for (int i = 0; i < _boxCaster.bottomRaycaster.nonAllocCasters.Count; i++)
				{
					NonAllocCaster nonAllocCaster = _boxCaster.bottomRaycaster.nonAllocCasters[i];
					if (nonAllocCaster.results.Count == 0)
					{
						continue;
					}
					RaycastHit2D raycastHit2D = nonAllocCaster.results[0];
					if ((bool)raycastHit2D)
					{
						collisionState.lastStandingCollider = raycastHit2D.collider;
						collisionState.belowCollisionDetector.Add(raycastHit2D);
						if (raycastHit2D.distance == 0f)
						{
							return false;
						}
						deltaMovement.y = math.max(deltaMovement.y, 0f - raycastHit2D.distance + _skinWidth);
					}
				}
			}
			return true;
		}
	}
}
