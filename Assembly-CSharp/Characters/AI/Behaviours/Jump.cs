using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Jump : Behaviour
	{
		[SerializeField]
		private Action _jump;

		[SerializeField]
		private Behaviour _onFallBehaviour;

		[SerializeField]
		private bool _waitForJump;

		[SerializeField]
		private bool _waitForGrounded;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		private bool _skipIdle = true;

		[SerializeField]
		private float _minimumTimeForFallAction;

		private float _elapsedTime;

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			_elapsedTime = 0f;
			_jump.TryStart();
			if (_waitForJump)
			{
				yield return CDoBehaviourAtInflectionPoint(controller);
				while (_waitForGrounded && !character.movement.isGrounded)
				{
					yield return null;
				}
				if (!_skipIdle)
				{
					yield return _idle.CRun(controller);
				}
			}
		}

		private IEnumerator CDoBehaviourAtInflectionPoint(AIController controller)
		{
			Character character = controller.character;
			while (character.movement.verticalVelocity > 0f)
			{
				_elapsedTime += character.chronometer.animation.deltaTime;
				yield return null;
			}
			while (_jump.running)
			{
				_elapsedTime += character.chronometer.animation.deltaTime;
				yield return null;
			}
			if (_elapsedTime > _minimumTimeForFallAction)
			{
				yield return _onFallBehaviour?.CRun(controller);
			}
		}

		public bool CanUse()
		{
			return _jump.canUse;
		}
	}
}
