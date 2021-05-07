using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class Maid02AI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(Chase))]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(Confusing))]
		private Confusing _confusing;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _jumpAttack;

		[SerializeField]
		private Collider2D _attackCollider;

		public void Awake()
		{
			character.health.onTookDamage += Health_onTookDamage;
		}

		private void Health_onTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_jumpAttack.StopPropagation();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return CCombat();
		}

		private IEnumerator CCombat()
		{
			while (!base.dead)
			{
				yield return null;
				if (base.target == null)
				{
					continue;
				}
				if (FindClosestPlayerBody(_attackCollider) != null)
				{
					yield return _jumpAttack.CRun(this);
					yield return _confusing.CRun(this);
					continue;
				}
				yield return _chase.CRun(this);
				if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return _jumpAttack.CRun(this);
					yield return _confusing.CRun(this);
				}
			}
		}
	}
}
