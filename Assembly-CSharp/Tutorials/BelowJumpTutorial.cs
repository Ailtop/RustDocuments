using System.Collections;

namespace Tutorials
{
	public class BelowJumpTutorial : Tutorial
	{
		public override void Activate()
		{
			_state = State.Progress;
			StartCoroutine(Process());
		}

		public override void Deactivate()
		{
			base.state = State.Done;
		}

		protected override IEnumerator Process()
		{
			yield return Converse(0f);
		}
	}
}
