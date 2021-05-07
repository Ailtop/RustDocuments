using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class PlayAnimation : Event
	{
		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private AnimationClip _animation;

		public override void Run()
		{
			_animator.Play(_animation.name);
		}
	}
}
