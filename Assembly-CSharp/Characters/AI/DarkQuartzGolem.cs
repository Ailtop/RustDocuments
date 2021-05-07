using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.DarkQuartzGolem;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class DarkQuartzGolem : AIController
	{
		[SerializeField]
		[Subcomponent(typeof(CheckWithinSight))]
		private CheckWithinSight _checkWithinSight;

		[SerializeField]
		[Subcomponent(typeof(Wander))]
		private Wander _wander;

		[SerializeField]
		[Subcomponent(typeof(Chase))]
		private Chase _chase;

		[SerializeField]
		[Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		[Subcomponent(true, typeof(SimpleAction))]
		private SimpleAction _summonAction;

		[SerializeField]
		[Subcomponent(typeof(Melee))]
		private Melee _melee;

		[SerializeField]
		[Subcomponent(typeof(Rush))]
		private Rush _rush;

		[SerializeField]
		[Subcomponent(typeof(Range))]
		private Range _range;

		[SerializeField]
		[Subcomponent(typeof(Targeting))]
		private Targeting _targeting;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chase, _melee, _rush, _range, _targeting, _idle };
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
			yield return CIntro();
			yield return _idle.CRun(this);
			yield return _rush.CRun(this);
			yield return _idle.CRun(this);
			StartCoroutine(CChangeStopTrigger());
			while (!base.dead)
			{
				yield return Combat();
			}
		}

		private IEnumerator Combat()
		{
			while (!base.dead)
			{
				if (base.target == null)
				{
					yield return null;
				}
				else if (base.stuned)
				{
					yield return null;
				}
				else if (_targeting.CanUse(this))
				{
					yield return _targeting.CRun(this);
					yield return _idle.CRun(this);
				}
				else if (_rush.CanUse(this))
				{
					yield return _rush.CRun(this);
					yield return _idle.CRun(this);
				}
				else if (_melee.CanUse(this))
				{
					if (MMMaths.RandomBool())
					{
						yield return _range.CRun(this);
					}
					else
					{
						yield return _melee.CRun(this);
					}
				}
				else if (_range.CanUse(this))
				{
					yield return _range.CRun(this);
				}
				else
				{
					yield return _targeting.CRun(this);
					yield return _idle.CRun(this);
				}
			}
		}

		private IEnumerator CIntro()
		{
			_summonAction.TryStart();
			while (_summonAction.running)
			{
				yield return null;
			}
		}

		private IEnumerator CChangeStopTrigger()
		{
			while (!base.dead)
			{
				if (_rush.CanUse(this))
				{
					stopTrigger = _range.trigger;
				}
				else
				{
					stopTrigger = _melee.trigger;
				}
				yield return null;
			}
		}
	}
}
