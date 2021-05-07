using Characters.AI.YggdrasillElderEnt;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public class SelectedHand : Condition
	{
		[SerializeField]
		private SweepHandController _controller;

		[SerializeField]
		private bool _left;

		protected override bool Check(AIController controller)
		{
			return _controller.left == _left;
		}
	}
}
