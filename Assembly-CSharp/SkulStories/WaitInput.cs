using System.Collections;
using UserInput;

namespace SkulStories
{
	public class WaitInput : Sequence
	{
		public override IEnumerator CRun()
		{
			_narration.skippable = false;
			if (!_narration.skipped)
			{
				yield return CWaitInput();
			}
			_narration.skipped = false;
		}

		private IEnumerator CWaitInput()
		{
			do
			{
				yield return null;
				_narration.skippable = false;
			}
			while (!KeyMapper.Map.Attack.WasPressed && !KeyMapper.Map.Jump.WasPressed && !KeyMapper.Map.Submit.WasPressed);
			_narration.textVisible = false;
		}
	}
}
