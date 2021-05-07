using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace SkulStories
{
	public class NarrationText : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _text;

		[SerializeField]
		private int _textSpeed = 2;

		private List<string> _narrationList;

		private StringBuilder _visible;

		private StringBuilder _invisible;

		private StringBuilder _alpha;

		private StringBuilder _display;

		private StringBuilder _intact;

		private int _linesIndex;

		private string _intactText;

		private bool _skippable;

		private const int _colorIndex = 256;

		private void Awake()
		{
			_narrationList = new List<string>();
			_visible = new StringBuilder(100);
			_invisible = new StringBuilder(100);
			_alpha = new StringBuilder(100);
			_display = new StringBuilder(100);
			_intact = new StringBuilder(100);
		}

		public void InitializeVisibleBuilder()
		{
			_visible.Clear();
		}

		public void InitializeInvisibleBuilder()
		{
			_invisible.Clear();
		}

		public IEnumerator CFadeInText(string text)
		{
			_skippable = false;
			_text.text = " " + text;
			_intactText = _text.text;
			for (int index = 0; index < 256; index += _textSpeed)
			{
				_alpha.AppendFormat("<alpha=#{0:X2}>", index);
				_display.Append(_alpha.ToString()).Append(" ").Append(text);
				_text.text = _display.ToString();
				yield return null;
				_alpha.Clear();
				_display.Clear();
				if (_skippable)
				{
					break;
				}
			}
		}

		public void AddText(string[] texts)
		{
			_intact.Clear();
			foreach (string text in texts)
			{
				_intact.Append(" ").Append(text);
				_intactText = _intact.ToString();
				_narrationList.Add(" <alpha=#00>" + text);
			}
		}

		public void InsertText()
		{
			foreach (string narration in _narrationList)
			{
				_text.text = _text.text.Insert(_text.text.Length, narration);
			}
		}

		public void CheckContainsText(string text)
		{
			for (int i = 0; i < _narrationList.Count; i++)
			{
				if (_narrationList[i].Contains(text))
				{
					_linesIndex = i;
				}
			}
		}

		public void ReplaceText()
		{
			_narrationList[_linesIndex] = _narrationList[_linesIndex].Replace("<alpha=#00>", "");
		}

		public void AppendInvisibleText()
		{
			for (int i = 0; i < _narrationList.Count; i++)
			{
				if (_narrationList[i].Contains("<alpha=#00>"))
				{
					_invisible.Append(_narrationList[i]);
				}
			}
		}

		public IEnumerator CFadeInText()
		{
			_skippable = false;
			string visible = _visible.ToString();
			string currentText = _narrationList[_linesIndex];
			string invisible = _invisible.ToString();
			for (int index = 0; index < 256; index += _textSpeed)
			{
				_alpha.AppendFormat("<alpha=#{0:X2}>", index);
				_display.Append(visible).Append(_alpha.ToString()).Append(currentText)
					.Append(invisible);
				_text.text = _display.ToString();
				yield return null;
				_alpha.Clear();
				_display.Clear();
				if (_skippable)
				{
					break;
				}
			}
		}

		public void AppendVisibleText()
		{
			if (!_skippable)
			{
				_visible.Append(_narrationList[_linesIndex]);
			}
		}

		public bool Condition()
		{
			return _linesIndex == _narrationList.Count - 1;
		}

		public void Show()
		{
			_skippable = true;
			_text.text = _intactText;
		}

		public void Clear()
		{
			_text.text = string.Empty;
			_narrationList.Clear();
			_skippable = true;
		}

		private void OnDisable()
		{
			Clear();
		}
	}
}
