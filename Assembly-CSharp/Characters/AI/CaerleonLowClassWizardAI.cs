using System.Collections;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class CaerleonLowClassWizardAI : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private CircularProjectileAttack _circularProjectileAttack;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _essentialIdleAfterAttack;

		[SerializeField]
		[Subcomponent(typeof(ChaseTeleport))]
		private ChaseTeleport _chaseTeleport;

		[SerializeField]
		private Collider2D _attackCollider;

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return Combat();
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				yield return null;
				if (!(base.target == null) && character.movement.controller.isGrounded)
				{
					if ((bool)FindClosestPlayerBody(_attackCollider))
					{
						yield return _circularProjectileAttack.CRun(this);
						yield return _essentialIdleAfterAttack.CRun(this);
					}
					else if (base.target.movement.controller.isGrounded && _chaseTeleport.CanUse())
					{
						yield return _chaseTeleport.CRun(this);
					}
				}
			}
		}
	}
}
