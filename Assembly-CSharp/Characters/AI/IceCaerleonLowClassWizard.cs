using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class IceCaerleonLowClassWizard : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _icestom;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _essentialIdleAfterAttack;

		[SerializeField]
		[Subcomponent(typeof(ChaseTeleport))]
		private ChaseTeleport _chaseTeleport;

		[SerializeField]
		private Collider2D _attackCollider;

		[SerializeField]
		private Collider2D _escapeTrigger;

		[SerializeField]
		private bool _noEscape;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _icestom, _essentialIdleAfterAttack, _chaseTeleport };
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
			while (!base.dead)
			{
				yield return Combat();
			}
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				yield return null;
				if (!(base.target == null) && character.movement.controller.isGrounded && _chaseTeleport.result != Characters.AI.Behaviours.Behaviour.Result.Doing)
				{
					if ((bool)FindClosestPlayerBody(_escapeTrigger))
					{
						yield return _icestom.CRun(this);
					}
					else if ((bool)FindClosestPlayerBody(_attackCollider))
					{
						yield return _icestom.CRun(this);
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
