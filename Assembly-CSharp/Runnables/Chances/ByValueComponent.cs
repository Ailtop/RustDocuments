using UnityEngine;

namespace Runnables.Chances
{
	public sealed class ByValueComponent : Chance
	{
		[SerializeField]
		private FloatComponent _component;

		public override bool IsTrue()
		{
			return MMMaths.Chance(_component.value);
		}
	}
}
