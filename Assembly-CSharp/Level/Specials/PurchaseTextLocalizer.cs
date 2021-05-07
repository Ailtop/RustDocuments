using TMPro;
using UnityEngine;

namespace Level.Specials
{
	public class PurchaseTextLocalizer : MonoBehaviour
	{
		[SerializeField]
		private TimeCostEvent _timeCostEvent;

		[SerializeField]
		[GetComponent]
		private TMP_Text _text;

		[SerializeField]
		private string _key;

		private void OnEnable()
		{
			if (!string.IsNullOrWhiteSpace(_key))
			{
				_text.text = string.Format(Lingua.GetLocalizedString(_key), _timeCostEvent.GetValue());
			}
		}
	}
}
