using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkulStories
{
	public class PlaceText : Sequence
	{
		[SerializeField]
		private TextLinkInfos _textInfo;

		private List<string> _textList;

		private void Start()
		{
			_textList = new List<string>();
		}

		public override IEnumerator CRun()
		{
			TextLinkInfos.TextLink[] texts = _textInfo.texts;
			foreach (TextLinkInfos.TextLink textInfo in texts)
			{
				AddText(textInfo);
			}
			_narration.CombineTexts(_textList.ToArray());
			yield break;
		}

		private void AddText(TextLinkInfos.TextLink textInfo)
		{
			string localizedString = Lingua.GetLocalizedString(textInfo.text);
			switch (textInfo.position)
			{
			case TextLinkInfos.TextLink.Position.Normal:
				_textList.Add(localizedString);
				break;
			case TextLinkInfos.TextLink.Position.Below:
				_textList.Add(Environment.NewLine + localizedString);
				break;
			}
		}
	}
}
