using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class FireCaerleonLowClassWizardAI : AIController
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
		[Subcomponent(typeof(Teleport))]
		private Teleport _escapeTeleport;

		[SerializeField]
		private Collider2D _attackCollider;

		[SerializeField]
		private bool _noEscape;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _circularProjectileAttack, _essentialIdleAfterAttack, _chaseTeleport, _escapeTeleport };
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
			yield return Combat();
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				yield return null;
				if (!(base.target == null) && _escapeTeleport.result != Characters.AI.Behaviours.Behaviour.Result.Doing && _chaseTeleport.result != Characters.AI.Behaviours.Behaviour.Result.Doing)
				{
					if ((bool)FindClosestPlayerBody(_attackCollider))
					{
						yield return _circularProjectileAttack.CRun(this);
						yield return _essentialIdleAfterAttack.CRun(this);
					}
					else if (base.target.movement.controller.isGrounded && !_noEscape && _chaseTeleport.CanUse())
					{
						yield return _chaseTeleport.CRun(this);
					}
				}
			}
		}
	}
}
