using TMPro;
using UnityEngine;

namespace UI
{
	public class TextLocalizer : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private TMP_Text _text;

		[SerializeField]
		private string _key;

		private void OnEnable()
		{
			if (!string.IsNullOrWhiteSpace(_key))
			{
				_text.text = Lingua.GetLocalizedString(_key);
			}
		}
	}
}
