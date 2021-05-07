using Characters.Gear;
using Characters.Gear.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GearPopup
{
	public class GearPopupForItemSelection : MonoBehaviour
	{
		[SerializeField]
		private Image _image;

		[Space]
		[SerializeField]
		private RectTransform _rectTransform;

		[Space]
		[SerializeField]
		private TMP_Text _name;

		[Space]
		[SerializeField]
		private TMP_Text _rarity;

		[Space]
		[SerializeField]
		private TMP_Text _description;

		[Space]
		[SerializeField]
		private GearPopupKeyword _keyword1;

		[SerializeField]
		private GearPopupKeyword _keyword2;

		private Gear _gear;

		public RectTransform rectTransform => _rectTransform;

		private static string _interactionLootLabel => Lingua.GetLocalizedString("label/interaction/loot");

		private static string _interactionPurcaseLabel => Lingua.GetLocalizedString("label/interaction/purchase");

		public void Set(Item item)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			_gear = item;
			_name.text = item.displayName;
			_rarity.text = Lingua.GetLocalizedString(string.Format("{0}/{1}/{2}", "label", "Rarity", item.rarity));
			_description.text = item.description;
			_keyword1.Set(item.keyword1);
			_keyword2.Set(item.keyword2);
		}
	}
}
