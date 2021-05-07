using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class HolyKnightsManAtArmsrAI : AIController
	{
		[Header("Behaviours")]
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
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _tackle;

		[SerializeField]
		[Subcomponent(typeof(ContinuousTackle))]
		private ContinuousTackle _trippleTackle;

		[SerializeField]
		[Subcomponent(typeof(ActionAttack))]
		private ActionAttack _holyWord;

		[Space]
		[Header("Holy Word Buff")]
		[SerializeField]
		private Stat.Values _HolyWordBuff;

		[SerializeField]
		private int _maxBuffStack;

		[Space]
		[Header("Tools")]
		[SerializeField]
		private Collider2D _attackTrigger;

		[SerializeField]
		private Collider2D _tackleTrigger;

		private int _buffStack = -1;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chase, _attack, _tackle, _holyWord };
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(CProcess());
			StartCoroutine(CUpdateStopTrigger());
			StartCoroutine(_checkWithinSight.CRun(this));
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return CCombat();
		}

		private IEnumerator CCombat()
		{
			yield return _wander.CRun(this);
			while (!base.dead)
			{
				yield return null;
				if (base.target == null || base.stuned)
				{
					continue;
				}
				if (_holyWord.CanUse() && _buffStack < _maxBuffStack)
				{
					yield return _holyWord.CRun(this);
					if (_holyWord.result == Characters.AI.Behaviours.Behaviour.Result.Success && _buffStack >= 0)
					{
						character.stat.AttachValues(_HolyWordBuff);
					}
					_buffStack++;
				}
				if (FindClosestPlayerBody(_tackleTrigger) != null && _tackle.CanUse())
				{
					yield return _tackle.CRun(this);
					yield return _attack.CRun(this);
				}
				else if (FindClosestPlayerBody(_attackTrigger) != null)
				{
					yield return _attack.CRun(this);
				}
				else
				{
					yield return _chase.CRun(this);
				}
			}
		}

		private IEnumerator CUpdateStopTrigger()
		{
			while (!base.dead)
			{
				yield return null;
				if (_tackle.CanUse())
				{
					stopTrigger = _tackleTrigger;
				}
				else
				{
					stopTrigger = _attackTrigger;
				}
			}
		}
	}
}
