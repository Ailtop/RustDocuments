using System;
using Characters.Movements;
using Characters.Operations;
using Characters.Operations.Movement;
using FX.BoundsAttackVisualEffect;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class InstantAttack : Operation
	{
		private static readonly NonAllocOverlapper _sharedOverlapper = new NonAllocOverlapper(2048);

		[SerializeField]
		private int _limit = 15;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		private TargetLayer _layer = new TargetLayer(2048, false, true, false, false);

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(TargetedOperationInfo))]
		private TargetedOperationInfo.Subcomponents _operationInfo;

		[SerializeField]
		[BoundsAttackVisualEffect.Subcomponent]
		private BoundsAttackVisualEffect.Subcomponents _effect;

		[SerializeField]
		protected HitInfo _hitInfo = new HitInfo(Damage.AttackType.Ranged);

		[SerializeField]
		protected ChronoInfo _chrono;

		private NonAllocOverlapper _overlapper;

		private PushInfo _pushInfo;

		private IAttackDamage _attackDamage;

		private void Awake()
		{
			_limit = Math.Min(_limit, 2048);
			_overlapper = ((_limit == _sharedOverlapper.capacity) ? _sharedOverlapper : new NonAllocOverlapper(_limit));
			Array.Sort(_operationInfo.components, (TargetedOperationInfo x, TargetedOperationInfo y) => x.timeToTrigger.CompareTo(y.timeToTrigger));
			_collider.enabled = false;
			_attackDamage = GetComponentInParent<IAttackDamage>();
			_operationInfo.Initialize();
			TargetedOperationInfo[] components = _operationInfo.components;
			foreach (TargetedOperationInfo targetedOperationInfo in components)
			{
				Characters.Operations.Movement.Knockback knockback;
				if ((object)(knockback = targetedOperationInfo.operation as Characters.Operations.Movement.Knockback) != null)
				{
					_pushInfo = knockback.pushInfo;
					break;
				}
				Characters.Operations.Movement.Smash smash;
				if ((object)(smash = targetedOperationInfo.operation as Characters.Operations.Movement.Smash) != null)
				{
					_pushInfo = smash.pushInfo;
				}
			}
		}

		public override void Run(Projectile projectile)
		{
			Character owner = projectile.owner;
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
			if (_attackDamage == null)
			{
				if (owner.playerComponents?.inventory.weapon != null)
				{
					_attackDamage = owner.playerComponents.inventory.weapon.weaponAttackDamage;
				}
				else
				{
					_attackDamage = owner.GetComponent<IAttackDamage>();
				}
			}
			_collider.enabled = true;
			_overlapper.OverlapCollider(_collider);
			Bounds bounds = _collider.bounds;
			_collider.enabled = false;
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
				if (projectile.owner == null)
				{
					continue;
				}
				Vector2 force = Vector2.zero;
				if (_pushInfo != null)
				{
					ValueTuple<Vector2, Vector2> valueTuple = _pushInfo.EvaluateTimeIndependent(owner, component);
					force = valueTuple.Item1 + valueTuple.Item2;
				}
				if (component.character != null && component.character.liveAndActive && component.character != owner)
				{
					Damage damage = owner.stat.GetDamage(_attackDamage.amount, hitPoint, _hitInfo);
					_chrono.ApplyTo(component.character);
					if (_hitInfo.attackType != 0)
					{
						Resource.instance.hitParticle.Emit(component.transform.position, bounds3, force);
					}
					if (component.character.invulnerable.value)
					{
						_effect.Spawn(owner, bounds3, ref damage, component);
						continue;
					}
					owner.AttackCharacter(component, ref damage);
					StartCoroutine(_operationInfo.CRun(owner, component.character));
					_effect.Spawn(owner, bounds3, ref damage, component);
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
						break;
					}
					component.damageable.Hit(owner, ref damage2, force);
				}
			}
		}
	}
}
