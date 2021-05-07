using System;
using System.Collections;
using UnityEngine;

namespace Characters.AI.Chimera
{
	public class ChimeraAnimation : MonoBehaviour
	{
		[Serializable]
		private class Phase1
		{
			[SerializeField]
			internal CharacterAnimationController.AnimationInfo sleep;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo bite;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo stomp;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo bigstomp;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo venomFall;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo venomBall;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo venomCannon;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo subjectDrop;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo wreckDrop;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo venomBreath;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo idle;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo intro;

			[SerializeField]
			internal CharacterAnimationController.AnimationInfo die;
		}

		[SerializeField]
		private Character _character;

		[SerializeField]
		private Phase1 _phase1;

		public float speed { get; set; } = 1f;


		private IEnumerator PlayAndWaitAnimation(CharacterAnimationController.AnimationInfo animationInfo, float extraLength = 0f)
		{
			_character.animationController.Play(animationInfo, speed);
			yield return _character.chronometer.animation.WaitForSeconds(animationInfo.dictionary["Body"].length / speed + extraLength);
		}

		public IEnumerator PlaySleepAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.sleep);
		}

		public IEnumerator PlayBiteAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.bite);
		}

		public IEnumerator PlayStompAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.stomp);
		}

		public IEnumerator PlayVenomFallAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.venomFall);
		}

		public IEnumerator PlayVenomBallAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.venomBall);
		}

		public IEnumerator PlayVenomCannonAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.venomCannon);
		}

		public IEnumerator PlaySubjectDropAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.subjectDrop);
		}

		public IEnumerator PlayWreckDropAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.wreckDrop, 0.1f);
		}

		public IEnumerator PlayWreckDestroyAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.bigstomp);
		}

		public IEnumerator PlayVenomBreathAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.venomBreath);
		}

		public IEnumerator PlayIdleAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.idle);
		}

		public IEnumerator PlayIntroAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.intro);
		}

		public IEnumerator PlayDieAnimation()
		{
			yield return PlayAndWaitAnimation(_phase1.die);
		}
	}
}
