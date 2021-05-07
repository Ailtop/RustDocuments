using System;
using System.Collections;
using System.Collections.Generic;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public sealed class CastAttack : CharacterOperation, IAttack
	{
		[Serializable]
		private class CollisionDetector
		{
			public delegate void onTerrainHitDelegate(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit);

			public delegate void onTargetHitDelegate(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target);

			private static readonly NonAllocCaster _caster = new NonAllocCaster(15);

			private CastAttack _castAttack;

			[SerializeField]
			private TargetLayer _layer = new TargetLayer(2048, false, true, false, false);

			[SerializeField]
			private Collider2D _collider;

			[SerializeField]
			[Range(1f, 15f)]
			private int _maxHits = 15;

			private List<Target> _hits = new List<Target>(15);

			private int _propPenetratingHits;

			private ContactFilter2D _filter;

			public Collider2D collider => _collider;

			public event onTerrainHitDelegate onTerrainHit;

			public event onTargetHitDelegate onHit;

			internal void Initialize(CastAttack castAttack)
			{
				_castAttack = castAttack;
				_filter.layerMask = _layer.Evaluate(castAttack.owner.gameObject);
				_hits.Clear();
				_propPenetratingHits = 0;
				if (_collider != null)
				{
					_collider.enabled = false;
				}
			}

			internal void Detect(Vector2 origin, Vector2 direction, float distance)
			{
				_003C_003Ec__DisplayClass19_0 _003C_003Ec__DisplayClass19_ = default(_003C_003Ec__DisplayClass19_0);
				_003C_003Ec__DisplayClass19_._003C_003E4__this = this;
				_003C_003Ec__DisplayClass19_.origin = origin;
				_003C_003Ec__DisplayClass19_.direction = direction;
				_003C_003Ec__DisplayClass19_.distance = distance;
				_caster.contactFilter.SetLayerMask(_filter.layerMask);
				_caster.RayCast(_003C_003Ec__DisplayClass19_.origin, _003C_003Ec__DisplayClass19_.direction, _003C_003Ec__DisplayClass19_.distance);
				if ((bool)_collider)
				{
					_collider.enabled = true;
					_caster.ColliderCast(_collider, _003C_003Ec__DisplayClass19_.direction, _003C_003Ec__DisplayClass19_.distance);
					_collider.enabled = false;
				}
				else
				{
					_caster.RayCast(_003C_003Ec__DisplayClass19_.origin, _003C_003Ec__DisplayClass19_.direction, _003C_003Ec__DisplayClass19_.distance);
				}
				_003C_003Ec__DisplayClass19_1 _003C_003Ec__DisplayClass19_2 = default(_003C_003Ec__DisplayClass19_1);
				for (int i = 0; i < _caster.results.Count; i++)
				{
					_003C_003Ec__DisplayClass19_2.result = _caster.results[i];
					_003CDetect_003Eg__HandleResult_007C19_0(ref _003C_003Ec__DisplayClass19_, ref _003C_003Ec__DisplayClass19_2);
					if (_hits.Count - _propPenetratingHits >= _maxHits)
					{
						break;
					}
				}
			}
		}

		[SerializeField]
		private float _distance;

		[SerializeField]
		private CollisionDetector _collisionDetector;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _onTerrainHit;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(CastAttackInfoSequence))]
		private CastAttackInfoSequence.Subcomponents _attackAndEffect;

		private IAttackDamage _attackDamage;

		internal Character owner { get; private set; }

		public event OnAttackHitDelegate onHit;

		private void Awake()
		{
			_collisionDetector.onTerrainHit += _003CAwake_003Eg__onTerrainHit_007C13_0;
			if (_attackAndEffect.noDelay)
			{
				_collisionDetector.onHit += _003CAwake_003Eg__onTargetHitWithoutDelay_007C13_2;
			}
			else
			{
				_collisionDetector.onHit += _003CAwake_003Eg__onTargetHit_007C13_1;
			}
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
			this.owner = owner;
			_collisionDetector.Initialize(this);
			_collisionDetector.Detect(base.transform.position, (owner.lookingDirection == Character.LookingDirection.Right) ? Vector2.right : Vector2.left, _distance);
		}

		private void Attack(CastAttackInfo attackInfo, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target)
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
					attackInfo.effect.Spawn(owner, _collisionDetector.collider, origin, direction, distance, raycastHit, damage, target);
					if (!target.character.invulnerable.value)
					{
						owner.StartCoroutine(attackInfo.operationsToCharacter.CRun(owner, target.character));
						this.onHit?.Invoke(target, ref damage);
						owner.AttackCharacter(target, ref damage);
					}
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

		private IEnumerator CAttack(Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target)
		{
			int index = 0;
			float time = 0f;
			while (this != null && index < _attackAndEffect.components.Length)
			{
				for (; index < _attackAndEffect.components.Length; index++)
				{
					CastAttackInfoSequence castAttackInfoSequence;
					if (!(time >= (castAttackInfoSequence = _attackAndEffect.components[index]).timeToTrigger))
					{
						break;
					}
					Attack(castAttackInfoSequence.attackInfo, origin, direction, distance, raycastHit, target);
				}
				yield return null;
				time += owner.chronometer.animation.deltaTime;
			}
		}
	}
}
