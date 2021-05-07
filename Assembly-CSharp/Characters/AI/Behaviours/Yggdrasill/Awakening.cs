using System.Collections;
using Characters.AI.YggdrasillElderEnt;
using UnityEngine;

namespace Characters.AI.Behaviours.Yggdrasill
{
	public sealed class Awakening : Behaviour
	{
		[SerializeField]
		private YggdrasillAnimationController _controller;

		[SerializeField]
		private YggdrasillAnimation.Tag[] _tags;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (_tags == null || _tags.Length == 0)
			{
				Debug.LogError("tag list error in PlayAnimations Behaviour");
				base.result = Result.Fail;
				yield break;
			}
			controller.character.invulnerable.Attach(this);
			YggdrasillAnimation.Tag[] tags = _tags;
			foreach (YggdrasillAnimation.Tag tag in tags)
			{
				yield return _controller.CPlayAndWaitAnimation(tag);
			}
			controller.character.invulnerable.Detach(this);
			base.result = Result.Success;
		}
	}
}
