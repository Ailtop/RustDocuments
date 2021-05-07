using System.Collections;
using UnityEngine;
using UserInput;

namespace SkulStories
{
	public class NarrationBody : MonoBehaviour
	{
		[SerializeField]
		private NarrationText _textInfo;

		private bool _isClear;

		public bool isClear
		{
			get
			{
				return _isClear;
			}
			set
			{
				_isClear = value;
				skippable = value;
			}
		}

		public bool skippable { get; set; }

		public IEnumerator CShow(ShowTexts sequence, string text)
		{
			_isClear = false;
			StartCoroutine(CWaitInput());
			switch (sequence.type)
			{
			case ShowTexts.Type.IntactText:
				yield return CShowIntactText(text);
				break;
			case ShowTexts.Type.SplitText:
				yield return CShowSplitText(text);
				break;
			}
		}

		public IEnumerator CShowIntactText(string text)
		{
			yield return _textInfo.CFadeInText(text);
			isClear = true;
		}

		public void PlaceText(string[] texts)
		{
			skippable = false;
			_textInfo.InitializeVisibleBuilder();
			_textInfo.AddText(texts);
			_textInfo.InsertText();
		}

		public IEnumerator CShowSplitText(string text)
		{
			_textInfo.InitializeInvisibleBuilder();
			_textInfo.CheckContainsText(text);
			_textInfo.ReplaceText();
			_textInfo.AppendInvisibleText();
			yield return _textInfo.CFadeInText();
			_textInfo.AppendVisibleText();
			if (_textInfo.Condition())
			{
				isClear = true;
			}
		}

		public IEnumerator CWaitInput()
		{
			yield return Chronometer.global.WaitForSeconds(0.5f);
			do
			{
				yield return null;
			}
			while (!KeyMapper.Map.Attack.WasPressed && !KeyMapper.Map.Jump.WasPressed && !KeyMapper.Map.Submit.WasPressed);
			if (!_isClear)
			{
				ShowText();
			}
		}

		private void ShowText()
		{
			_textInfo.Show();
			isClear = true;
		}

		public void Clear()
		{
			_isClear = true;
			_textInfo.Clear();
		}

		private void OnDisable()
		{
			Clear();
		}
	}
}
