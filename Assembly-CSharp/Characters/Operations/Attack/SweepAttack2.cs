using System;
using System.Collections;
using Characters.Utils;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public class SweepAttack2 : CharacterOperation, IAttack
	{
		[Serializable]
		public class CollisionDetector
		{
			public delegate void onTerrainHitDelegate(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit);

			public delegate void onTargetHitDelegate(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target);

			private static readonly NonAllocCaster _caster = new NonAllocCaster(99);

			private SweepAttack2 _sweepAttack;

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

			public Collider2D collider
			{
				get
				{
					return _collider;
				}
				set
				{
					_collider = value;
				}
			}

			public event onTerrainHitDelegate onTerrainHit;

			public event onTargetHitDelegate onHit;

			internal void Initialize(SweepAttack2 sweepAttack)
			{
				_sweepAttack = sweepAttack;
				_filter.layerMask = _layer.Evaluate(sweepAttack.owner.gameObject);
				_hits.Clear();
				_propPenetratingHits = 0;
				if (_optimizedCollider && _collider != null)
				{
					_collider.enabled = false;
				}
			}

			internal void Detect(Vector2 origin, Vector2 distance)
			{
				Detect(origin, distance.normalized, distance.magnitude);
			}

			internal void Detect(Vector2 origin, Vector2 direction, float distance)
			{
				_003C_003Ec__DisplayClass25_0 _003C_003Ec__DisplayClass25_ = default(_003C_003Ec__DisplayClass25_0);
				_003C_003Ec__DisplayClass25_._003C_003E4__this = this;
				_003C_003Ec__DisplayClass25_.origin = origin;
				_003C_003Ec__DisplayClass25_.direction = direction;
				_003C_003Ec__DisplayClass25_.distance = distance;
				_caster.contactFilter.SetLayerMask(_filter.layerMask);
				if (_collider != null)
				{
					if (_optimizedCollider)
					{
						_collider.enabled = true;
						_caster.ColliderCast(_collider, _003C_003Ec__DisplayClass25_.direction, _003C_003Ec__DisplayClass25_.distance);
						_collider.enabled = false;
					}
					else
					{
						_caster.ColliderCast(_collider, _003C_003Ec__DisplayClass25_.direction, _003C_003Ec__DisplayClass25_.distance);
					}
				}
				else
				{
					_caster.RayCast(_003C_003Ec__DisplayClass25_.origin, _003C_003Ec__DisplayClass25_.direction, _003C_003Ec__DisplayClass25_.distance);
				}
				_003C_003Ec__DisplayClass25_1 _003C_003Ec__DisplayClass25_2 = default(_003C_003Ec__DisplayClass25_1);
				for (int i = 0; i < _caster.results.Count; i++)
				{
					_003C_003Ec__DisplayClass25_2.result = _caster.results[i];
					_003CDetect_003Eg__HandleResult_007C25_0(ref _003C_003Ec__DisplayClass25_, ref _003C_003Ec__DisplayClass25_2);
					if (_hits.Count - _propPenetratingHits >= _maxHits)
					{
						_sweepAttack.Stop();
					}
				}
			}
		}

		[SerializeField]
		private float _duration;

		[SerializeField]
		private bool _trackMovement = true;

		[SerializeField]
		private CollisionDetector _collisionDetector;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _onTerrainHit;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(CastAttackInfoSequence))]
		protected CastAttackInfoSequence.Subcomponents _attackAndEffect;

		private IAttackDamage _attackDamage;

		private CoroutineReference _detectReference;

		internal Character owner { get; private set; }

		private CollisionDetector collisionDetector => _collisionDetector;

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

		public Collider2D range
		{
			get
			{
				return collisionDetector.collider;
			}
			set
			{
				collisionDetector.collider = value;
			}
		}

		public event OnAttackHitDelegate onHit;

		private void Awake()
		{
			_collisionDetector.onTerrainHit += _003CAwake_003Eg__onTerrainHit_007C23_0;
			if (_attackAndEffect.noDelay)
			{
				_collisionDetector.onHit += _003CAwake_003Eg__onTargetHitWithoutDelay_007C23_2;
			}
			else
			{
				_collisionDetector.onHit += _003CAwake_003Eg__onTargetHit_007C23_1;
			}
		}

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<IAttackDamage>();
			_attackAndEffect.Initialize();
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
			_attackAndEffect.StopAllOperationsToOwner();
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

		protected void Attack(CastAttackInfo attackInfo, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target)
		{
			Vector2 force = Vector2.zero;
			if (attackInfo.pushInfo != null)
			{
				ValueTuple<Vector2, Vector2> valueTuple = attackInfo.pushInfo.EvaluateTimeIndependent(owner, target);
				force = valueTuple.Item1 + valueTuple.Item2;
			}
			if (target.character != null)
			{
				if (target.character.liveAndActive && !(target.character == owner))
				{
					attackInfo.ApplyChrono(owner, target.character);
					if (attackInfo.operationsToOwner.components.Length != 0)
					{
						owner.StartCoroutine(attackInfo.operationsToOwner.CRun(owner));
					}
					Damage damage = owner.stat.GetDamage(_attackDamage.amount, raycastHit.point, attackInfo.hitInfo);
					if (attackInfo.hitInfo.attackType != 0)
					{
						Resource.instance.hitParticle.Emit(target.transform.position, target.collider.bounds, force);
					}
					if (target.character.invulnerable.value)
					{
						attackInfo.effect.Spawn(owner, _collisionDetector.collider, origin, direction, distance, raycastHit, damage, target);
						return;
					}
					owner.StartCoroutine(attackInfo.operationsToCharacter.CRun(owner, target.character));
					owner.AttackCharacter(target, ref damage);
					attackInfo.effect.Spawn(owner, _collisionDetector.collider, origin, direction, distance, raycastHit, damage, target);
				}
			}
			else if (target.damageable != null)
			{
				attackInfo.ApplyChrono(owner);
				owner.StartCoroutine(attackInfo.operationsToOwner.CRun(owner));
				Damage damage2 = owner.stat.GetDamage(_attackDamage.amount, raycastHit.point, attackInfo.hitInfo);
				if (target.damageable.spawnEffectOnHit && attackInfo.hitInfo.attackType != 0)
				{
					Resource.instance.hitParticle.Emit(target.transform.position, target.collider.bounds, force);
					attackInfo.effect.Spawn(owner, _collisionDetector.collider, origin, direction, distance, raycastHit, damage2, target);
				}
				if (attackInfo.hitInfo.attackType != 0)
				{
					target.damageable.Hit(owner, ref damage2, force);
				}
			}
		}

		protected virtual IEnumerator CAttack(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target)
		{
			int index = 0;
			float time = 0f;
			Vector3 originOffset = MMMaths.Vector2ToVector3(origin) - target.transform.position;
			Vector3 hitPointOffset = MMMaths.Vector2ToVector3(raycastHit.point) - target.transform.position;
			while (this != null && index < _attackAndEffect.components.Length)
			{
				for (; index < _attackAndEffect.components.Length; index++)
				{
					CastAttackInfoSequence castAttackInfoSequence;
					if (!(time >= (castAttackInfoSequence = _attackAndEffect.components[index]).timeToTrigger))
					{
						break;
					}
					raycastHit.point = target.transform.position + hitPointOffset;
					Attack(castAttackInfoSequence.attackInfo, target.transform.position + originOffset, direction, distance, raycastHit, target);
				}
				yield return null;
				time += owner.chronometer.animation.deltaTime;
			}
		}
	}
}
