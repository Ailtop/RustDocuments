using System.Collections;
using UnityEngine;

namespace SkulStories
{
	public sealed class ShowTexts : Sequence
	{
		public enum Type
		{
			SplitText,
			IntactText
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private string[] _texts;

		public Type type => _type;

		public override IEnumerator CRun()
		{
			string[] texts = _texts;
			foreach (string key in texts)
			{
				yield return _narration.CShowText(this, Lingua.GetLocalizedString(key));
			}
		}
	}
}
