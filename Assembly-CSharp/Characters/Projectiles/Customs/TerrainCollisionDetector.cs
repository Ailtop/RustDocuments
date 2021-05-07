using System;
using System.Collections;
using Characters.Projectiles.Movements;
using Characters.Projectiles.Operations;
using Characters.Utils;
using FX.ProjectileAttackVisualEffect;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Projectiles.Customs
{
	public class TerrainCollisionDetector : MonoBehaviour
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

			static CollisionDetector()
			{
				_caster = new NonAllocCaster(15);
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
				_caster.contactFilter.SetLayerMask(_terrainLayer);
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
						continue;
					}
					Target component = raycastHit.collider.GetComponent<Target>();
					if (component == null)
					{
						Debug.LogError("Need a target component to: " + raycastHit.collider.name + "!");
					}
					else if (hitHistoryManager.CanAttack(component, _maxHits + _propPenetratingHits, _maxHitsPerUnit, _hitIntervalPerUnit) && hitHistoryManager.Count - _propPenetratingHits >= _maxHits)
					{
						_projectile.Despawn();
						break;
					}
				}
			}
		}

		[SerializeField]
		private Projectile _projectile;

		[SerializeField]
		private float _maxLifeTime;

		[SerializeField]
		private float _collisionTime;

		[SerializeField]
		private CollisionDetector _collisionDetector;

		[SerializeField]
		[Operation.Subcomponent]
		private Operation.Subcomponents _onDespawn;

		[SerializeField]
		[HitOperation.Subcomponent]
		private HitOperation.Subcomponents _onTerrainHit;

		[SerializeField]
		[ProjectileAttackVisualEffect.Subcomponent]
		private ProjectileAttackVisualEffect.Subcomponents _effect;

		[SerializeField]
		private Movement _movement;

		private float _direction;

		private float _time;

		public Vector2 direction { get; private set; }

		public float speed { get; private set; }

		private void Awake()
		{
			_collisionDetector.onTerrainHit += _003CAwake_003Eg__onTerrainHit_007C19_0;
		}

		public void Run()
		{
			StartCoroutine(CUpdate());
		}

		private IEnumerator CUpdate()
		{
			_time = 0f;
			while (_time <= _maxLifeTime)
			{
				yield return null;
				float deltaTime = Chronometer.global.deltaTime;
				_time += deltaTime;
				ValueTuple<Vector2, float> valueTuple = _movement.GetSpeed(_time, deltaTime);
				direction = valueTuple.Item1;
				speed = valueTuple.Item2;
				speed *= deltaTime;
				if (_time >= _collisionTime)
				{
					_collisionDetector.Detect(base.transform.position, direction, speed);
				}
			}
			_effect.SpawnExpire(_projectile);
			Despawn();
		}

		internal void Despawn()
		{
			for (int i = 0; i < _onDespawn.components.Length; i++)
			{
				_onDespawn.components[i].Run(_projectile);
			}
			_effect.SpawnDespawn(_projectile);
		}
	}
}
