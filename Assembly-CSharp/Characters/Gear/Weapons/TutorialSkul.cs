using Characters.Actions;
using UnityEngine;

namespace Characters.Gear.Weapons
{
	public class TutorialSkul : MonoBehaviour
	{
		[SerializeField]
		private Action _idle;

		[SerializeField]
		private Action _openEyes;

		[SerializeField]
		private Action _equipHead;

		[SerializeField]
		private Action _scratchHead;

		[SerializeField]
		private Action _blink;

		[SerializeField]
		private Action _getBone;

		public Action idle => _idle;

		public Action openEyes => _openEyes;

		public Action equipHead => _equipHead;

		public Action scratchHead => _scratchHead;

		public Action blink => _blink;

		public Action getBone => _getBone;
	}
}
