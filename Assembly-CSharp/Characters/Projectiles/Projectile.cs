using System;
using System.Collections;
using Characters.Operations;
using Characters.Projectiles.Movements;
using Characters.Projectiles.Operations;
using Characters.Utils;
using FX.ProjectileAttackVisualEffect;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Projectiles
{
	[RequireComponent(typeof(PoolObject))]
	public class Projectile : MonoBehaviour
	{
		[Serializable]
		private class CollisionDetector
		{
			public delegate void onTerrainHitDelegate(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit);

			public delegate void onTargetHitDelegate(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target);

			private const int maxHits = 99;

			private Projectile _projectile;

			[SerializeField]
			private TargetLayer _layer;

			[SerializeField]
			private LayerMask _terrainLayer = Layers.terrainMaskForProjectile;

			[SerializeField]
			private Collider2D _collider;

			[SerializeField]
			[Range(1f, 99f)]
			private int _maxHits = 1;

			[SerializeField]
			private int _maxHitsPerUnit = 1;

			[SerializeField]
			private float _hitIntervalPerUnit = 0.5f;

			internal HitHistoryManager hitHistoryManager;

			private HitHistoryManager _internalHitHistory = new HitHistoryManager(99);

			private int _propPenetratingHits;

			private static readonly NonAllocCaster _caster;

			public Collider2D collider => _collider;

			public event onTerrainHitDelegate onTerrainHit;

			public event onTargetHitDelegate onHit;

			static CollisionDetector()
			{
				_caster = new NonAllocCaster(64);
			}

			internal void Initialize(Projectile projectile)
			{
				Initialize(projectile, _internalHitHistory);
			}

			internal void Initialize(Projectile projectile, HitHistoryManager hitHistory)
			{
				_projectile = projectile;
				hitHistoryManager = hitHistory;
				hitHistoryManager.Clear();
				_propPenetratingHits = 0;
				if (_collider != null)
				{
					_collider.enabled = false;
				}
			}

			internal void SetHitHistoryManager(HitHistoryManager hitHistory)
			{
				hitHistoryManager = hitHistory;
			}

			internal void Detect(Vector2 origin, Vector2 direction, float distance)
			{
				if (_projectile.owner == null)
				{
					return;
				}
				_caster.contactFilter.SetLayerMask((int)_layer.Evaluate(_projectile.owner.gameObject) | (int)_terrainLayer);
				if (_collider != null)
				{
					_collider.enabled = true;
					_caster.ColliderCast(_collider, direction, distance);
					_collider.enabled = false;
				}
				else
				{
					_caster.RayCast(origin, direction, distance);
				}
				for (int i = 0; i < _caster.results.Count; i++)
				{
					RaycastHit2D raycastHit = _caster.results[i];
					if (_terrainLayer.Contains(raycastHit.collider.gameObject.layer))
					{
						this.onTerrainHit(origin, direction, distance, raycastHit);
						break;
					}
					Target component = raycastHit.collider.GetComponent<Target>();
					if (component == null)
					{
						Debug.LogError("Need a target component to: " + raycastHit.collider.name + "!");
					}
					else
					{
						if (!hitHistoryManager.CanAttack(component, _maxHits + _propPenetratingHits, _maxHitsPerUnit, _hitIntervalPerUnit))
						{
							continue;
						}
						if (component.character != null)
						{
							if (component.character == _projectile.owner || !component.character.liveAndActive)
							{
								continue;
							}
							this.onHit(origin, direction, distance, raycastHit, component);
							hitHistoryManager.AddOrUpdate(component);
						}
						else if (component.damageable != null)
						{
							this.onHit(origin, direction, distance, raycastHit, component);
							if (!component.damageable.blockCast)
							{
								_propPenetratingHits++;
							}
							hitHistoryManager.AddOrUpdate(component);
						}
						if (hitHistoryManager.Count - _propPenetratingHits >= _maxHits)
						{
							_projectile.Despawn();
							break;
						}
					}
				}
			}
		}

		[SerializeField]
		[GetComponent]
		private PoolObject _reusable;

		[SerializeField]
		private float _maxLifeTime;

		[SerializeField]
		private float _collisionTime;

		[SerializeField]
		private Transform _rotatable;

		[SerializeField]
		private bool _disableCollisionDetect;

		[SerializeField]
		private CollisionDetector _collisionDetector;

		[SerializeField]
		private HitInfo _hitInfo = new HitInfo(Damage.AttackType.Projectile);

		[Space]
		[SerializeField]
		[Subcomponent(typeof(Characters.Projectiles.Operations.OperationInfo))]
		private Characters.Projectiles.Operations.OperationInfo.Subcomponents _operations;

		[Space]
		[SerializeField]
		[Characters.Projectiles.Operations.Operation.Subcomponent]
		private Characters.Projectiles.Operations.Operation.Subcomponents _onSpawned;

		[SerializeField]
		[Characters.Projectiles.Operations.Operation.Subcomponent]
		private Characters.Projectiles.Operations.Operation.Subcomponents _onDespawn;

		[SerializeField]
		private bool _despawnOnTerrainHit = true;

		[SerializeField]
		[HitOperation.Subcomponent]
		private HitOperation.Subcomponents _onTerrainHit;

		[SerializeField]
		[Characters.Projectiles.Operations.CharacterHitOperation.Subcomponent]
		private Characters.Projectiles.Operations.CharacterHitOperation.Subcomponents _onCharacterHit;

		[SerializeField]
		[ProjectileAttackVisualEffect.Subcomponent]
		private ProjectileAttackVisualEffect.Subcomponents _effect;

		[SerializeField]
		[Movement.Subcomponent]
		private Movement _movement;

		private float _direction;

		private float _time;

		public Character owner { get; private set; }

		public PoolObject reusable => _reusable;

		public float maxLifeTime
		{
			get
			{
				return _maxLifeTime;
			}
			set
			{
				_maxLifeTime = value;
			}
		}

		public Movement movement => _movement;

		public Collider2D collider => _collisionDetector.collider;

		public float baseDamage { get; private set; }

		public float speedMultiplier { get; private set; }

		public Vector2 direction { get; private set; }

		public float speed { get; private set; }

		private void Awake()
		{
			if (_operations == null)
			{
				_operations = new Characters.Projectiles.Operations.OperationInfo.Subcomponents();
			}
			else
			{
				_operations.Sort();
			}
			_collisionDetector.onTerrainHit += _003CAwake_003Eg__onTerrainHit_007C47_0;
			_collisionDetector.onHit += _003CAwake_003Eg__onTargetHit_007C47_1;
		}

		private IEnumerator CUpdate(float delay)
		{
			while (delay > 0f)
			{
				delay -= ((owner != null) ? owner.chronometer.projectile.deltaTime : Chronometer.global.deltaTime);
				yield return null;
			}
			_time = 0f;
			while (_time <= _maxLifeTime)
			{
				float num = ((owner != null) ? owner.chronometer.projectile.deltaTime : Chronometer.global.deltaTime);
				_time += num;
				ValueTuple<Vector2, float> valueTuple = _movement.GetSpeed(_time, num);
				direction = valueTuple.Item1;
				speed = valueTuple.Item2;
				speed *= num;
				if (_rotatable != null)
				{
					_rotatable.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
				}
				if (_time >= _collisionTime && !_disableCollisionDetect)
				{
					_collisionDetector.Detect(base.transform.position, direction, speed);
				}
				base.transform.Translate(direction * speed, Space.World);
				yield return null;
			}
			_effect.SpawnExpire(this);
			Despawn();
		}

		internal void Despawn()
		{
			for (int i = 0; i < _onDespawn.components.Length; i++)
			{
				_onDespawn.components[i].Run(this);
			}
			_effect.SpawnDespawn(this);
			_reusable.Despawn();
		}

		public void Fire(Character owner, float attackDamage, float direction, bool flipX = false, bool flipY = false, float speedMultiplier = 1f, HitHistoryManager hitHistoryManager = null, float delay = 0f)
		{
			this.owner = owner;
			_direction = direction;
			Vector3 localScale = base.transform.localScale;
			if (flipX)
			{
				localScale.x *= -1f;
			}
			if (flipY)
			{
				localScale.y *= -1f;
			}
			base.transform.localScale = localScale;
			baseDamage = attackDamage;
			this.speedMultiplier = speedMultiplier;
			base.gameObject.layer = (TargetLayer.IsPlayer(owner.gameObject.layer) ? 15 : 16);
			if (_rotatable != null)
			{
				_rotatable.transform.rotation = Quaternion.Euler(0f, 0f, _direction);
			}
			_movement.Initialize(this, _direction);
			_collisionDetector.Initialize(this);
			if (hitHistoryManager != null)
			{
				SetHitHistroyManager(hitHistoryManager);
			}
			for (int i = 0; i < _onSpawned.components.Length; i++)
			{
				_onSpawned.components[i].Run(this);
			}
			StartCoroutine(_operations.CRun(this));
			StartCoroutine(CUpdate(delay));
		}

		public void ClearHitHistroy()
		{
			_collisionDetector.hitHistoryManager.ClearHits();
		}

		public void SetHitHistroyManager(HitHistoryManager hitHistoryManager)
		{
			_collisionDetector.hitHistoryManager = hitHistoryManager;
		}

		public void DetectCollision(Vector2 origin, Vector2 direction, float speed)
		{
			_collisionDetector.Detect(base.transform.position, direction, speed);
		}

		public override string ToString()
		{
			return base.name;
		}
	}
}
