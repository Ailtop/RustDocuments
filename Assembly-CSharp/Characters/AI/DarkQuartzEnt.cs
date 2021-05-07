using System.Collections;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class DarkQuartzEnt : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Wander.Subcomponent(true)]
		private Wander _wander;

		[SerializeField]
		[Subcomponent(typeof(Chase))]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _meleeAttack;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _areaAttack;

		[SerializeField]
		private Collider2D _meleeAttackCollider;

		[SerializeField]
		private Collider2D _areaAttackCollider;

		[SerializeField]
		private Transform _effect;

		[SerializeField]
		private Collider2D _effectCollider;

		private float _effectBoundsX;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return _wander.CRun(this);
			yield return Combat();
		}

		private void OnDestroy()
		{
			Object.Destroy(_effect.gameObject);
		}

		private IEnumerator Combat()
		{
			_effectBoundsX = _effectCollider.bounds.size.x;
			while (!base.dead)
			{
				yield return null;
				if (base.target == null || !character.movement.controller.isGrounded)
				{
					continue;
				}
				if (FindClosestPlayerBody(_meleeAttackCollider) != null)
				{
					if (_areaAttack.CanUse())
					{
						SetPosition();
						yield return _areaAttack.CRun(this);
					}
					else
					{
						yield return _meleeAttack.CRun(this);
					}
				}
				else if (FindClosestPlayerBody(_areaAttackCollider) != null)
				{
					if (base.target.movement.isGrounded)
					{
						if (_areaAttack.CanUse())
						{
							SetPosition();
							yield return _areaAttack.CRun(this);
						}
						else
						{
							yield return _chase.CRun(this);
						}
					}
				}
				else
				{
					yield return _chase.CRun(this);
				}
			}
		}

		private void SetPosition()
		{
			Bounds bounds = base.target.movement.controller.collisionState.lastStandingCollider.bounds;
			if (bounds.max.x - 1f < base.target.transform.position.x + _effectBoundsX / 2f)
			{
				_effect.position = new Vector2(bounds.max.x - _effectBoundsX / 2f, bounds.max.y);
			}
			else if (bounds.min.x + 1f > base.target.transform.position.x - _effectBoundsX / 2f)
			{
				_effect.position = new Vector2(bounds.min.x + _effectBoundsX / 2f, bounds.max.y);
			}
			else
			{
				_effect.position = new Vector2(base.target.transform.position.x, bounds.max.y);
			}
		}
	}
}
