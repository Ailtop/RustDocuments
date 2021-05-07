using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public sealed class FanaticAI : AIController
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
		[Attack.Subcomponent(true)]
		private Attack _attack;

		[SerializeField]
		[Subcomponent(typeof(Sacrifice))]
		private Sacrifice _sacrifice;

		[Space]
		[Header("Tools")]
		[SerializeField]
		private Collider2D _attackTrigger;

		[SerializeField]
		private CharacterAnimation _characterAnimation;

		[SerializeField]
		private AnimationClip _idleClipAfterWander;

		[SerializeField]
		private AnimationClip _walkClipAfterWander;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chase, _attack, _sacrifice };
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			StartCoroutine(_checkWithinSight.CRun(this));
			StartCoroutine(CProcess());
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return CCombat();
		}

		private IEnumerator CCombat()
		{
			yield return _wander.CRun(this);
			_characterAnimation.SetIdle(_idleClipAfterWander);
			_characterAnimation.SetWalk(_walkClipAfterWander);
			while (!base.dead)
			{
				if (_sacrifice.result.Equals(Characters.AI.Behaviours.Behaviour.Result.Doing))
				{
					yield return null;
					continue;
				}
				if (FindClosestPlayerBody(_attackTrigger) != null)
				{
					yield return _attack.CRun(this);
					continue;
				}
				yield return _chase.CRun(this);
				if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return _attack.CRun(this);
				}
			}
		}
	}
}
