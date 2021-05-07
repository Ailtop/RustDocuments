using TMPro;
using UnityEngine;

namespace UI.Adventurer
{
	public class AdventurerNamePool : MonoBehaviour
	{
		[SerializeField]
		private string _poolKey;

		[SerializeField]
		[GetComponent]
		private TMP_Text _text;

		private void OnEnable()
		{
			if (!string.IsNullOrWhiteSpace(_poolKey))
			{
				string[] localizedStringArray = Lingua.GetLocalizedStringArray(_poolKey);
				if (localizedStringArray.Length != 0)
				{
					_text.text = localizedStringArray.Random();
				}
			}
		}
	}
}
