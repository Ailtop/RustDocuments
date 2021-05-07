using System;
using System.Collections;
using System.Collections.Generic;
using Characters.Operations;
using Characters.Operations.Attack;
using FX.CastAttackVisualEffect;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class YggdrasillElderEntCollisionDetector : MonoBehaviour
	{
		public delegate void onTerrainHitDelegate(Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit);

		public delegate void onTargetHitDelegate(Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target);

		[SerializeField]
		protected HitInfo _hitInfo = new HitInfo(Damage.AttackType.Melee);

		[SerializeField]
		[Subcomponent(typeof(TargetedOperationInfo))]
		private TargetedOperationInfo.Subcomponents _onCharacterHit;

		[SerializeField]
		private Character _owner;

		[SerializeField]
		private TargetLayer _layer;

		[SerializeField]
		private Collider2D _collider;

		[SerializeField]
		[Range(1f, 15f)]
		private int _maxHits = 1;

		private List<Target> _hits = new List<Target>(15);

		private ContactFilter2D _filter;

		[SerializeField]
		protected ChronoInfo _chronoToGlobe;

		[SerializeField]
		protected ChronoInfo _chronoToOwner;

		[SerializeField]
		protected ChronoInfo _chronoToTarget;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		internal OperationInfo.Subcomponents _operationToOwnerWhenHitInfo;

		[SerializeField]
		[Subcomponent(typeof(CastAttackInfoSequence))]
		private CastAttackInfoSequence.Subcomponents _attackAndEffect;

		[SerializeField]
		[CastAttackVisualEffect.Subcomponent]
		private CastAttackVisualEffect.Subcomponents _effect;

		private CoroutineReference _expireReference;

		private IAttackDamage _attackDamage;

		private int _propHits;

		private static readonly NonAllocCaster _caster;

		private bool _running;

		public event Action<RaycastHit2D> onTerrainHit;

		public event onTargetHitDelegate onHit;

		static YggdrasillElderEntCollisionDetector()
		{
			_caster = new NonAllocCaster(15);
		}

		private void Awake()
		{
			onHit += delegate(Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target)
			{
				if (!(target.character == null))
				{
					Damage damage = _owner.stat.GetDamage(_attackDamage.amount, raycastHit.point, _hitInfo);
					_chronoToOwner.ApplyTo(_owner);
					_chronoToTarget.ApplyTo(target.character);
					StartCoroutine(_onCharacterHit.CRun(_owner, target.character));
					if (!target.character.invulnerable.value)
					{
						_owner.AttackCharacter(target, ref damage);
						StartCoroutine(_operationToOwnerWhenHitInfo.CRun(_owner));
					}
				}
			};
		}

		internal void Initialize(GameObject owner, Collider2D collider)
		{
			_filter.layerMask = _layer.Evaluate(owner);
			_propHits = 0;
			_hits.Clear();
			_collider = collider;
			_attackDamage = GetComponentInParent<IAttackDamage>();
			_attackAndEffect.Initialize();
			_onCharacterHit.Initialize();
		}

		private void Detect(Vector2 origin, Vector2 distance)
		{
			Detect(origin, distance.normalized, distance.magnitude);
		}

		private void Detect(Vector2 origin, Vector2 direction, float distance)
		{
			_caster.contactFilter.SetLayerMask(_filter.layerMask);
			_caster.RayCast(origin, direction, distance);
			if ((bool)_collider)
			{
				_caster.ColliderCast(_collider, direction, distance);
			}
			else
			{
				_caster.RayCast(origin, direction, distance);
			}
			for (int i = 0; i < _caster.results.Count; i++)
			{
				Target component = _caster.results[i].collider.GetComponent<Target>();
				if (component == null)
				{
					break;
				}
				if (!_hits.Contains(component))
				{
					if (component.character != null)
					{
						if (component.character.liveAndActive)
						{
							this.onHit(_collider, origin, direction, distance, _caster.results[i], component);
							_hits.Add(component);
						}
					}
					else if (component.damageable != null)
					{
						Damage damage = _owner.stat.GetDamage(_attackDamage.amount, _caster.results[i].point, _hitInfo);
						component.damageable.Hit(_owner, ref damage);
						this.onHit(_collider, origin, direction, distance, _caster.results[i], component);
						_hits.Add(component);
					}
				}
				if (_hits.Count >= _maxHits)
				{
					Stop();
				}
			}
		}

		public void Stop()
		{
			_running = false;
			_attackAndEffect.StopAllOperationsToOwner();
		}

		public IEnumerator CRun(Transform moveTarget)
		{
			Vector2 vector = moveTarget.position;
			_running = true;
			while (_running)
			{
				Vector2 nextPosition = moveTarget.position;
				Detect(vector, nextPosition - vector);
				yield return null;
				vector = nextPosition;
			}
		}
	}
}
