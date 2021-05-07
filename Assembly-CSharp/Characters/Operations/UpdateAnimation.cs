using System.Collections;
using UnityEngine;

namespace Characters.Operations
{
	public class UpdateAnimation : CharacterOperation
	{
		private enum State
		{
			Idle,
			Walk
		}

		[SerializeField]
		private State _state;

		[SerializeField]
		private AnimationClip _origin;

		[SerializeField]
		private AnimationClip _clip;

		[SerializeField]
		private CharacterAnimation _characterAnimation;

		[SerializeField]
		private float _duration;

		public override void Run(Character owner)
		{
			if (_characterAnimation == null)
			{
				_characterAnimation = owner.animationController.animations[0];
			}
			SetAnimation(_clip);
			if (_duration > 0f)
			{
				StartCoroutine(CExpire(owner.chronometer.master));
			}
		}

		public override void Stop()
		{
			if (_origin != null)
			{
				SetAnimation(_origin);
			}
		}

		private void SetAnimation(AnimationClip clip)
		{
			if (!(clip == null))
			{
				switch (_state)
				{
				case State.Idle:
					_characterAnimation.SetIdle(clip);
					break;
				case State.Walk:
					_characterAnimation.SetWalk(clip);
					break;
				}
			}
		}

		private IEnumerator CExpire(Chronometer chronometer)
		{
			yield return chronometer.WaitForSeconds(_duration);
			Stop();
		}
	}
}
