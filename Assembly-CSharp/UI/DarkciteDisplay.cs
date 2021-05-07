using Data;
using TMPro;
using UnityEngine;

namespace UI
{
	public class DarkciteDisplay : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _amount;

		private void Update()
		{
			_amount.text = GameData.Currency.darkQuartz.balance.ToString();
		}
	}
}
