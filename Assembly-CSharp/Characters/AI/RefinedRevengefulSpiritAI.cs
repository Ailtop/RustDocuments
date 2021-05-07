using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class RefinedRevengefulSpiritAI : AIController
	{
		[SerializeField]
		private bool _introSkip;

		[SerializeField]
		private Collider2D _attackTrigger;

		[SerializeField]
		private Collider2D _dashTrigger;

		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(FlyChase))]
		private FlyChase _flyChase;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private ActionAttack _attack;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		private Action _intro;

		[SerializeField]
		[Range(0f, 100f)]
		private float _attackAccelerationNear;

		[SerializeField]
		[Range(0f, 100f)]
		private float _attackAccelerationFar;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _flyChase, _attack };
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(_checkWithinSight.CRun(this));
			StartCoroutine(Dash());
		}

		protected override IEnumerator CProcess()
		{
			character.movement.config.acceleration = _attackAccelerationFar;
			character.movement.Move(Random.rotation * Vector3.forward);
			yield return CPlayStartOption();
			if (!_introSkip)
			{
				_intro.TryStart();
				yield return _idle.CRun(this);
			}
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
				if (!(base.target == null))
				{
					if (FindClosestPlayerBody(_attackTrigger) != null)
					{
						yield return _attack.CRun(this);
						continue;
					}
					yield return _flyChase.CRun(this);
					yield return _attack.CRun(this);
				}
			}
		}

		private IEnumerator Dash()
		{
			while (!base.dead)
			{
				if (FindClosestPlayerBody(_dashTrigger) != null)
				{
					character.movement.config.acceleration = _attackAccelerationNear;
				}
				else
				{
					character.movement.config.acceleration = _attackAccelerationFar;
				}
				yield return null;
			}
		}
	}
}
