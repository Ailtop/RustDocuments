using Data;
using TMPro;
using UnityEngine;

namespace UI
{
	public class BoneChipDisplay : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _amount;

		private void Update()
		{
			_amount.text = GameData.Currency.bone.balance.ToString();
		}
	}
}
