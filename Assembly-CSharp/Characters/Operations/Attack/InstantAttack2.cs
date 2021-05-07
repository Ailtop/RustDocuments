using System;
using System.Collections;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public sealed class InstantAttack2 : CharacterOperation, IAttack
	{
		private static readonly NonAllocOverlapper _sharedOverlapper = new NonAllocOverlapper(2048);

		private NonAllocOverlapper _overlapper;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private TargetLayer _layer = new TargetLayer(2048, false, true, false, false);

		[Tooltip("한 번에 공격 가능한 적의 수(프롭 포함), 특별한 경우가 아니면 기본값인 99로 두는 게 좋음.")]
		[SerializeField]
		private int _maxHits = 512;

		[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
		[SerializeField]
		private bool _optimizedCollider = true;

		[SerializeField]
		[Tooltip("공격자 자기 자신을 대상에서 제외할지 여부")]
		private bool _excludeItself = true;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(BoundsAttackInfoSequence))]
		private BoundsAttackInfoSequence.Subcomponents _attackAndEffect;

		private IAttackDamage _attackDamage;

		public event OnAttackHitDelegate onHit;

		private void Awake()
		{
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
			_attackAndEffect.Initialize();
		}

		public override void Stop()
		{
			_attackAndEffect.StopAllOperationsToOwner();
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
			for (int i = 0; i < results.Count; i++)
			{
				Target component = results[i].GetComponent<Target>();
				if (component == null)
				{
					Debug.LogError("Target is null in InstantAttack2");
					break;
				}
				if (_attackAndEffect.noDelay)
				{
					BoundsAttackInfoSequence[] components = _attackAndEffect.components;
					foreach (BoundsAttackInfoSequence boundsAttackInfoSequence in components)
					{
						Attack(owner, bounds, component, boundsAttackInfoSequence.attackInfo);
					}
				}
				else
				{
					component.StartCoroutine(CAttack(owner, bounds, component));
				}
			}
		}

		private void Attack(Character owner, Bounds bounds, Target target, BoundsAttackInfo attackInfo)
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
					if (target.character.liveAndActive && (!_excludeItself || !(target.character == owner)))
					{
						attackInfo.ApplyChrono(owner, target.character);
						if (attackInfo.operationsToOwner.components.Length != 0)
						{
							owner.StartCoroutine(attackInfo.operationsToOwner.CRun(owner));
						}
						Damage damage = owner.stat.GetDamage(_attackDamage.amount, hitPoint, attackInfo.hitInfo);
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

		private IEnumerator CAttack(Character owner, Bounds bounds, Target target)
		{
			int index = 0;
			float time = 0f;
			while (this != null && index < _attackAndEffect.components.Length)
			{
				for (; index < _attackAndEffect.components.Length; index++)
				{
					BoundsAttackInfoSequence boundsAttackInfoSequence;
					if (!(time >= (boundsAttackInfoSequence = _attackAndEffect.components[index]).timeToTrigger))
					{
						break;
					}
					Attack(owner, bounds, target, boundsAttackInfoSequence.attackInfo);
				}
				yield return null;
				time += owner.chronometer.animation.deltaTime;
			}
		}
	}
}
