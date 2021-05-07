using System;
using System.Globalization;
using Data;
using TMPro;
using UnityEngine;

namespace UI
{
	public class PlayTimeDisplay : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		private void Update()
		{
			string text = new TimeSpan(0, 0, GameData.Progress.playTime).ToString("hh\\ \\:\\ mm\\ \\:\\ ss", CultureInfo.InvariantCulture);
			_text.text = text;
		}
	}
}
