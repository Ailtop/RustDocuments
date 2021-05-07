using System;
using System.Collections;
using Characters.Marks;
using Characters.Operations.Attack;
using PhysicsUtils;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public sealed class Samurai2IlseomInstantAttack : CharacterOperation, IAttack
	{
		private static readonly NonAllocOverlapper _sharedOverlapper = new NonAllocOverlapper(2048);

		private NonAllocOverlapper _overlapper;

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

		[Space]
		[SerializeField]
		private MarkInfo _mark;

		[SerializeField]
		[Tooltip("표식이 없을 때인 0개부터 시작")]
		private double[] _damagePercents;

		[SerializeField]
		[FrameTime]
		private float _attack1Time;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(BoundsAttackInfo))]
		private BoundsAttackInfo _attack1;

		[SerializeField]
		[FrameTime]
		private float _attack2Time;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(BoundsAttackInfo))]
		private BoundsAttackInfo _attack2;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		internal OperationInfo.Subcomponents _operationOnMaxStackHit;

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
			_attackDamage = GetComponentInParent<IAttackDamage>();
			_attack1.Initialize();
			_attack2.Initialize();
		}

		public override void Stop()
		{
			_operationOnMaxStackHit.StopAll();
		}

		public override void Run(Character owner)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
			_collider.enabled = true;
			_overlapper.OverlapCollider(_collider);
			Bounds bounds = _collider.bounds;
			if (_optimizedCollider)
			{
				_collider.enabled = false;
			}
			ReadonlyBoundedList<Collider2D> results = _overlapper.results;
			bool flag = false;
			for (int i = 0; i < results.Count; i++)
			{
				Target component = results[i].GetComponent<Target>();
				if (component == null)
				{
					Debug.LogError("Target is null in InstantAttack2");
					return;
				}
				float num = 0f;
				if (component.character != null)
				{
					num = component.character.mark.TakeAllStack(_mark);
					if (num == (float)_mark.maxStack)
					{
						flag = true;
					}
				}
				component.StartCoroutine(CAttack(owner, bounds, component, num));
			}
			if (flag && _operationOnMaxStackHit.components.Length != 0)
			{
				StartCoroutine(_operationOnMaxStackHit.CRun(owner));
			}
		}

		private void Attack(Character owner, Bounds bounds, Target target, BoundsAttackInfo attackInfo, double multiplier = 1.0)
		{
			if (target == null)
			{
				Debug.LogError("Target is null in InstantAttack2");
			}
			else
			{
				if (!target.isActiveAndEnabled)
				{
					return;
				}
				Bounds bounds2 = bounds;
				Bounds bounds3 = target.collider.bounds;
				Bounds bounds4 = default(Bounds);
				bounds4.min = MMMaths.Max(bounds2.min, bounds3.min);
				bounds4.max = MMMaths.Min(bounds2.max, bounds3.max);
				Vector2 hitPoint = MMMaths.RandomPointWithinBounds(bounds4);
				Vector2 force = Vector2.zero;
				if (attackInfo.pushInfo != null)
				{
					ValueTuple<Vector2, Vector2> valueTuple = attackInfo.pushInfo.EvaluateTimeIndependent(owner, target);
					force = valueTuple.Item1 + valueTuple.Item2;
				}
				if (target.character != null)
				{
					if (target.character.liveAndActive)
					{
						attackInfo.ApplyChrono(owner, target.character);
						if (attackInfo.operationsToOwner.components.Length != 0)
						{
							owner.StartCoroutine(attackInfo.operationsToOwner.CRun(owner));
						}
						Damage damage = owner.stat.GetDamage((double)_attackDamage.amount * multiplier, hitPoint, attackInfo.hitInfo);
						if (attackInfo.hitInfo.attackType != 0)
						{
							Resource.instance.hitParticle.Emit(target.transform.position, target.collider.bounds, force);
						}
						if (target.character.invulnerable.value)
						{
							attackInfo.effect.Spawn(owner, bounds4, ref damage, target);
							return;
						}
						owner.StartCoroutine(attackInfo.operationInfo.CRun(owner, target.character));
						this.onHit?.Invoke(target, ref damage);
						owner.AttackCharacter(target, ref damage);
						attackInfo.effect.Spawn(owner, bounds4, ref damage, target);
					}
				}
				else if (target.damageable != null)
				{
					attackInfo.ApplyChrono(owner);
					owner.StartCoroutine(attackInfo.operationsToOwner.CRun(owner));
					Damage damage2 = owner.stat.GetDamage(_attackDamage.amount, hitPoint, attackInfo.hitInfo);
					if (target.damageable.spawnEffectOnHit && attackInfo.hitInfo.attackType != 0)
					{
						Resource.instance.hitParticle.Emit(target.transform.position, target.collider.bounds, force);
						attackInfo.effect.Spawn(owner, bounds4, ref damage2, target);
					}
					if (attackInfo.hitInfo.attackType != 0)
					{
						this.onHit?.Invoke(target, ref damage2);
						target.damageable.Hit(owner, ref damage2, force);
					}
				}
			}
		}

		private IEnumerator CAttack(Character owner, Bounds bounds, Target target, float stacks)
		{
			float time = 0f;
			while (this != null && time < _attack1Time)
			{
				yield return null;
				time += owner.chronometer.animation.deltaTime;
			}
			Attack(owner, bounds, target, _attack1);
			while (this != null && time < _attack2Time)
			{
				yield return null;
				time += owner.chronometer.animation.deltaTime;
			}
			float damageMultiplier = _attack2.hitInfo.damageMultiplier;
			int num = (int)math.min(stacks, _mark.maxStack);
			Attack(owner, bounds, target, _attack2, _damagePercents[num]);
			_attack2.hitInfo.damageMultiplier = damageMultiplier;
		}
	}
}
