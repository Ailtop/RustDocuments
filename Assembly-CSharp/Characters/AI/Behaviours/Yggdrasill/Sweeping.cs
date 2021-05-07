using System.Collections;
using Characters.AI.YggdrasillElderEnt;
using UnityEngine;

namespace Characters.AI.Behaviours.Yggdrasill
{
	public sealed class Sweeping : Behaviour
	{
		[SerializeField]
		private YggdrasillAnimationController _animationController;

		[SerializeField]
		private SweepHandController _sweepHandcontroller;

		[SerializeField]
		private YggdrasillAnimation.Tag _left;

		[SerializeField]
		private YggdrasillAnimation.Tag _right;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_sweepHandcontroller.Select();
			if (_sweepHandcontroller.left)
			{
				yield return _animationController.CPlayAndWaitAnimation(_left);
			}
			else
			{
				yield return _animationController.CPlayAndWaitAnimation(_right);
			}
			base.result = Result.Done;
		}
	}
}
