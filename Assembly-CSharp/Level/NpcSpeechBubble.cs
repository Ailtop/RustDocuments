using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Level
{
	public class NpcSpeechBubble : MonoBehaviour
	{
		[SerializeField]
		private Text _text;

		private readonly List<string> _scripts = new List<string>();

		public void Initialize(string key)
		{
			int num = 1;
			string @string;
			while (Lingua.TryGetLocalizedString($"{key}_bubble{num}", out @string))
			{
				num++;
				_scripts.Add(@string);
			}
		}

		public void Show()
		{
			_text.text = _scripts.Random();
			base.gameObject.SetActive(true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
		}
	}
}
