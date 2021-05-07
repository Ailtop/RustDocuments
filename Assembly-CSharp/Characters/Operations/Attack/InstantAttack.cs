using System;
using Characters.Movements;
using Characters.Operations.Movement;
using FX.BoundsAttackVisualEffect;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public sealed class InstantAttack : CharacterOperation, IAttack
	{
		private static readonly NonAllocOverlapper _sharedOverlapper = new NonAllocOverlapper(2048);

		private NonAllocOverlapper _overlapper;

		[SerializeField]
		private HitInfo _hitInfo = new HitInfo(Damage.AttackType.Melee);

		[SerializeField]
		private ChronoInfo _chronoToGlobe;

		[SerializeField]
		private ChronoInfo _chronoToOwner;

		[SerializeField]
		private ChronoInfo _chronoToTarget;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		internal OperationInfo.Subcomponents _operationToOwnerWhenHitInfo;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private TargetLayer _layer = new TargetLayer(2048, false, true, false, false);

		[Tooltip("한 번에 공격 가능한 적의 수(프롭 포함), 특별한 경우가 아니면 기본값인 512로 두는 게 좋음.")]
		[SerializeField]
		private int _maxHits = 512;

		[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
		[SerializeField]
		private bool _optimizedCollider = true;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(TargetedOperationInfo))]
		private TargetedOperationInfo.Subcomponents _operationInfo;

		[SerializeField]
		[BoundsAttackVisualEffect.Subcomponent]
		private BoundsAttackVisualEffect.Subcomponents _effect;

		private PushInfo _pushInfo;

		private IAttackDamage _attackDamage;

		public Collider2D range
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

		public event OnAttackHitDelegate onHit;

		private void Awake()
		{
			Array.Sort(_operationInfo.components, (TargetedOperationInfo x, TargetedOperationInfo y) => x.timeToTrigger.CompareTo(y.timeToTrigger));
			if (_optimizedCollider && _collider != null)
			{
				_collider.enabled = false;
			}
			_maxHits = Math.Min(_maxHits, 2048);
			_overlapper = ((_maxHits == _sharedOverlapper.capacity) ? _sharedOverlapper : new NonAllocOverlapper(_maxHits));
		}

		public override void Initialize()
		{
			base.Initialize();
			_attackDamage = GetComponentInParent<IAttackDamage>();
			_operationInfo.Initialize();
			TargetedOperationInfo[] components = _operationInfo.components;
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
		}

		public override void Run(Character owner)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
			_collider.enabled = true;
			Bounds bounds = _collider.bounds;
			_overlapper.OverlapCollider(_collider);
			if (_optimizedCollider)
			{
				_collider.enabled = false;
			}
			bool flag = false;
			for (int i = 0; i < _overlapper.results.Count; i++)
			{
				Target component = _overlapper.results[i].GetComponent<Target>();
				if (component == null)
				{
					continue;
				}
				Bounds bounds2 = component.collider.bounds;
				Bounds bounds3 = default(Bounds);
				bounds3.min = MMMaths.Max(bounds.min, bounds2.min);
				bounds3.max = MMMaths.Min(bounds.max, bounds2.max);
				Vector2 hitPoint = MMMaths.RandomPointWithinBounds(bounds3);
				Vector2 force = Vector2.zero;
				if (_pushInfo != null)
				{
					ValueTuple<Vector2, Vector2> valueTuple = _pushInfo.EvaluateTimeIndependent(owner, component);
					force = valueTuple.Item1 + valueTuple.Item2;
				}
				if (component.character != null)
				{
					if (component.character.liveAndActive && !(component.character == owner))
					{
						flag = true;
						_chronoToTarget.ApplyTo(component.character);
						Damage damage = owner.stat.GetDamage(_attackDamage.amount, hitPoint, _hitInfo);
						if (_hitInfo.attackType != 0)
						{
							Resource.instance.hitParticle.Emit(component.transform.position, bounds3, force);
						}
						if (component.character.invulnerable.value)
						{
							_effect.Spawn(owner, bounds3, ref damage, component);
							continue;
						}
						StartCoroutine(_operationInfo.CRun(owner, component.character));
						this.onHit?.Invoke(component, ref damage);
						owner.AttackCharacter(component, ref damage);
						_effect.Spawn(owner, bounds3, ref damage, component);
					}
				}
				else if (component.damageable != null)
				{
					Damage damage2 = owner.stat.GetDamage(_attackDamage.amount, hitPoint, _hitInfo);
					if (component.damageable.spawnEffectOnHit && _hitInfo.attackType != 0)
					{
						Resource.instance.hitParticle.Emit(component.transform.position, bounds3, force);
						_effect.Spawn(owner, bounds3, ref damage2, component);
					}
					if (_hitInfo.attackType == Damage.AttackType.None)
					{
						return;
					}
					if (component.damageable.blockCast)
					{
						flag = true;
						this.onHit?.Invoke(component, ref damage2);
					}
					component.damageable.Hit(owner, ref damage2, force);
				}
			}
			if (flag)
			{
				_chronoToGlobe.ApplyGlobe();
				_chronoToOwner.ApplyTo(owner);
				if (_operationToOwnerWhenHitInfo.components.Length != 0)
				{
					StartCoroutine(_operationToOwnerWhenHitInfo.CRun(owner));
				}
			}
		}

		public override void Stop()
		{
			_operationToOwnerWhenHitInfo.StopAll();
		}
	}
}
