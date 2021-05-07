using Characters.Gear.Synergy.Keywords;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GearPopup
{
	public class GearPopupKeyword : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TMP_Text _name;

		public void Set(Keyword.Key keyword)
		{
			_icon.sprite = keyword.GetIcon();
			_name.text = keyword.GetName();
		}
	}
}
