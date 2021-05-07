using System.Collections;
using System.Collections.Generic;
using Characters.AI.Behaviours;
using Characters.AI.Behaviours.Attacks;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI
{
	public class MartyrFanaticAI : AIController
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
		[Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _moveForSuicide;

		[SerializeField]
		[Attack.Subcomponent(true)]
		private Attack _suicide;

		[Space]
		[Header("Tools")]
		[SerializeField]
		private Collider2D _attackTrigger;

		[SerializeField]
		private AttachAbility _speedBonus;

		[SerializeField]
		private CharacterAnimation _characterAnimation;

		[SerializeField]
		private AnimationClip _idleClipAfterWander;

		[SerializeField]
		private AnimationClip _walkClipAfterWander;

		private void Awake()
		{
			base.behaviours = new List<Characters.AI.Behaviours.Behaviour> { _checkWithinSight, _wander, _chase, _suicide };
			_speedBonus.Initialize();
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
				if (FindClosestPlayerBody(_attackTrigger) != null)
				{
					yield return CSuicide();
					continue;
				}
				yield return _chase.CRun(this);
				if (_chase.result == Characters.AI.Behaviours.Behaviour.Result.Success)
				{
					yield return CSuicide();
				}
			}
		}

		private IEnumerator CSuicide()
		{
			base.destination = base.target.transform.position;
			_speedBonus.Run(character);
			yield return _moveForSuicide.CRun(this);
			_speedBonus.Stop();
			yield return _suicide.CRun(this);
		}
	}
}
