using System.Collections;
using UnityEngine;

namespace CutScenes.Shots.Sequences
{
	public class ShowLine : Sequence
	{
		[SerializeField]
		private TextMessageInfo _messageInfo;

		[SerializeField]
		private LineText _lineText;

		[SerializeField]
		private float _time;

		public override IEnumerator CRun()
		{
			string localizedString = Lingua.GetLocalizedString(_messageInfo.messageKey);
			yield return _lineText.CDisplay(localizedString, _time);
		}
	}
}
