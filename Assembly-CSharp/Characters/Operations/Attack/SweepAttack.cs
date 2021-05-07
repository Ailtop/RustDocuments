using System;
using System.Collections;
using Characters.Movements;
using Characters.Operations.Movement;
using Characters.Utils;
using FX.CastAttackVisualEffect;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public class SweepAttack : CharacterOperation, IAttack
	{
		[Serializable]
		public class CollisionDetector
		{
			public delegate void onTerrainHitDelegate(Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit);

			public delegate void onTargetHitDelegate(Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target);

			public onTerrainHitDelegate onTerrainHit;

			public onTargetHitDelegate onHit;

			private SweepAttack _sweepAttack;

			[SerializeField]
			private TargetLayer _layer = new TargetLayer(2048, false, true, false, false);

			[SerializeField]
			private LayerMask _terrainLayer = Layers.groundMask;

			[SerializeField]
			private Collider2D _collider;

			[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
			[SerializeField]
			private bool _optimizedCollider = true;

			[SerializeField]
			private int _maxHits = 512;

			[SerializeField]
			private int _maxHitsPerUnit = 1;

			[SerializeField]
			private float _hitIntervalPerUnit = 0.5f;

			private HitHistoryManager _hits = new HitHistoryManager(32);

			private int _propPenetratingHits;

			private ContactFilter2D _filter;

			private static readonly NonAllocCaster _caster;

			public HitHistoryManager hits
			{
				get
				{
					return _hits;
				}
				set
				{
					_hits = value;
				}
			}

			static CollisionDetector()
			{
				_caster = new NonAllocCaster(99);
			}

			internal void Initialize(SweepAttack sweepAttack)
			{
				_sweepAttack = sweepAttack;
				_filter.layerMask = _layer.Evaluate(sweepAttack.owner.gameObject);
				_hits.Clear();
				_propPenetratingHits = 0;
				if (_optimizedCollider && _collider != null)
				{
					_collider.enabled = false;
				}
				if (_maxHitsPerUnit == 0)
				{
					_maxHitsPerUnit = int.MaxValue;
				}
			}

			internal void Detect(Vector2 origin, Vector2 distance)
			{
				Detect(origin, distance.normalized, distance.magnitude);
			}

			internal void Detect(Vector2 origin, Vector2 direction, float distance)
			{
				_caster.contactFilter.SetLayerMask(_filter.layerMask);
				if ((bool)_collider)
				{
					if (_optimizedCollider)
					{
						_collider.enabled = true;
					}
					_caster.ColliderCast(_collider, direction, distance);
					if (_optimizedCollider)
					{
						_collider.enabled = false;
					}
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
						onTerrainHit(_collider, origin, direction, distance, raycastHit);
					}
					else
					{
						Target component = raycastHit.collider.GetComponent<Target>();
						if (component == null)
						{
							Debug.LogError(raycastHit.collider.name + " : Character has no Target component");
							continue;
						}
						if (!_hits.CanAttack(component, _maxHits, _maxHitsPerUnit, _hitIntervalPerUnit))
						{
							continue;
						}
						if (component.character != null)
						{
							if (!component.character.liveAndActive)
							{
								continue;
							}
							onHit(_collider, origin, direction, distance, raycastHit, component);
							_hits.AddOrUpdate(component);
						}
						else if (component.damageable != null)
						{
							onHit(_collider, origin, direction, distance, raycastHit, component);
							if (!component.damageable.blockCast)
							{
								_propPenetratingHits++;
							}
							_hits.AddOrUpdate(component);
						}
					}
					if (_hits.Count - _propPenetratingHits >= _maxHits)
					{
						_sweepAttack.Stop();
					}
				}
			}
		}

		[SerializeField]
		private HitInfo _hitInfo = new HitInfo(Damage.AttackType.Melee);

		[SerializeField]
		private ChronoInfo _chronoToGlobe;

		[SerializeField]
		private ChronoInfo _chronoToOwner;

		[SerializeField]
		private ChronoInfo _chronoToTarget;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private bool _trackMovement = true;

		[SerializeField]
		private CollisionDetector _collisionDetector;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operationToOwnerWhenHitInfo;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _onTerrainHit;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(TargetedOperationInfo))]
		private TargetedOperationInfo.Subcomponents _onCharacterHit;

		[SerializeField]
		[CastAttackVisualEffect.Subcomponent]
		private CastAttackVisualEffect.Subcomponents _effect;

		private CoroutineReference _detectReference;

		private PushInfo _pushInfo;

		private IAttackDamage _attackDamage;

		internal Character owner { get; private set; }

		public CollisionDetector collisionDetector => _collisionDetector;

		public HitInfo hitInfo => _hitInfo;

		public float duration
		{
			get
			{
				return _duration;
			}
			set
			{
				_duration = value;
			}
		}

		public event OnAttackHitDelegate onHit;

		private void Awake()
		{
			if (_duration == 0f)
			{
				_duration = float.PositiveInfinity;
			}
			_attackDamage = GetComponentInParent<IAttackDamage>();
			_onTerrainHit.Initialize();
			_onCharacterHit.Initialize();
			TargetedOperationInfo[] components = _onCharacterHit.components;
			foreach (TargetedOperationInfo targetedOperationInfo in components)
			{
				Knockback knockback;
				if ((object)(knockback = targetedOperationInfo.operation as Knockback) != null)
				{
					_pushInfo = knockback.pushInfo;
					break;
				}
				Smash smash;
				if ((object)(smash = targetedOperationInfo.operation as Smash) != null)
				{
					_pushInfo = smash.pushInfo;
				}
			}
			_collisionDetector.onTerrainHit = _003CAwake_003Eg__onTerrainHit_007C29_0;
			_collisionDetector.onHit = _003CAwake_003Eg__onTargetHit_007C29_1;
		}

		public override void Run(Character owner)
		{
			this.owner = owner;
			_collisionDetector.Initialize(this);
			_detectReference.Stop();
			_detectReference = owner.StartCoroutineWithReference(CDetect());
		}

		public override void Stop()
		{
			base.Stop();
			_operationToOwnerWhenHitInfo.StopAll();
			_detectReference.Stop();
		}

		private IEnumerator CDetect()
		{
			float time = 0f;
			for (Chronometer chronometer = owner.chronometer.master; time < _duration; time += chronometer.deltaTime)
			{
				_collisionDetector.Detect(base.transform.position, (_trackMovement && owner.movement != null) ? owner.movement.moved : Vector2.zero);
				yield return null;
			}
		}
	}
}
