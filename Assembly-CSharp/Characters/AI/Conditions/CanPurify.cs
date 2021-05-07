using Level.Chapter4;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public sealed class CanPurify : Condition
	{
		[SerializeField]
		private PlatformContainer _platformContainer;

		protected override bool Check(AIController controller)
		{
			return _platformContainer.CanPurify();
		}
	}
}
