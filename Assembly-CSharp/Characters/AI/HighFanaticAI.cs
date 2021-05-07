using System.Collections;
using Characters.AI.Behaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public class HighFanaticAI : AIController
	{
		[Header("Behaviours")]
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(KeepDistance))]
		private KeepDistance _keepDistance;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _keepDistanceIdle;

		[SerializeField]
		[Chase.Subcomponent(true)]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(Wander))]
		private Wander _wander;

		[Header("Fanatic Call", order = 2)]
		[Space]
		[SerializeField]
		[Subcomponent(typeof(FanaticCall))]
		private FanaticCall _fanaticCall;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _fanaticCallIdle;

		[Header("Mass Sacrifice", order = 3)]
		[SerializeField]
		[Subcomponent(typeof(MassSacrifice))]
		private MassSacrifice _massSacrifice;

		[Subcomponent(typeof(Idle))]
		[SerializeField]
		private Idle _massSacrificeIdle;

		[Header("Tools")]
		[SerializeField]
		private Collider2D _fanaticCallTrigger;

		[SerializeField]
		private Collider2D _massSacrificeTrigger;

		[SerializeField]
		private Collider2D _keepDistanceTrigger;

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

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				yield return null;
				if (!(base.target == null))
				{
					if (_fanaticCall.CanUse() && (bool)FindClosestPlayerBody(_fanaticCallTrigger))
					{
						yield return _fanaticCall.CRun(this);
						yield return _fanaticCallIdle.CRun(this);
					}
					else if ((bool)FindClosestPlayerBody(_massSacrificeTrigger) && _massSacrifice.CanUse(this))
					{
						yield return _massSacrifice.CRun(this);
					}
					else if ((bool)FindClosestPlayerBody(_keepDistanceTrigger) && _keepDistance.CanUseBackMove())
					{
						yield return _keepDistance.CRun(this);
						yield return _keepDistanceIdle.CRun(this);
					}
					else
					{
						yield return _chase.CRun(this);
					}
				}
			}
		}
	}
}
