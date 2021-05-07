using Characters.Gear.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInput;

namespace UI.Inventory
{
	public class ItemOption : MonoBehaviour
	{
		[SerializeField]
		private Image _thumnailIcon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _rarity;

		[Space]
		[SerializeField]
		private GameObject _simpleContainer;

		[SerializeField]
		private GameObject _detailContainer;

		[Space]
		[SerializeField]
		private TMP_Text _flavorSimple;

		[SerializeField]
		private TMP_Text _flavorDetail;

		[SerializeField]
		private TMP_Text _description;

		[Space]
		[SerializeField]
		private PressingButton _itemDiscardKey;

		[SerializeField]
		private TMP_Text _itemDiscardText;

		[Space]
		[SerializeField]
		private KeywordOption _keyword1;

		[SerializeField]
		private KeywordOption _keyword2;

		[Space]
		[SerializeField]
		private KeywordOption _keyword1Detail;

		[SerializeField]
		private KeywordOption _keyword2Detail;

		public void Set(Item item)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			_thumnailIcon.enabled = true;
			_thumnailIcon.sprite = item.thumbnail;
			_thumnailIcon.transform.localScale = Vector3.one * 3f;
			_thumnailIcon.SetNativeSize();
			_name.text = item.displayName;
			_rarity.text = Lingua.GetLocalizedString(string.Format("{0}/{1}/{2}", "label", "Rarity", item.rarity));
			string text = (item.hasFlavor ? item.flavor : string.Empty);
			_flavorSimple.text = text;
			_flavorDetail.text = text;
			_description.text = item.description;
			_itemDiscardKey.gameObject.SetActive(true);
			_itemDiscardText.text = Lingua.GetLocalizedString("label/inventory/discardItem");
			if (item.currencyByDiscard > 0)
			{
				_itemDiscardText.text = $"{_itemDiscardText.text}(<color=#FFDE37>{item.currencyByDiscard}</color>)";
			}
			_keyword1.Set(item.keyword1);
			_keyword2.Set(item.keyword2);
			_keyword1Detail.Set(item.keyword1);
			_keyword2Detail.Set(item.keyword2);
		}

		private void Update()
		{
			if (_detailContainer.activeSelf && !KeyMapper.Map.Quintessence.IsPressed)
			{
				_simpleContainer.SetActive(true);
				_detailContainer.SetActive(false);
			}
			else if (!_detailContainer.activeSelf && KeyMapper.Map.Quintessence.IsPressed)
			{
				_simpleContainer.SetActive(false);
				_detailContainer.SetActive(true);
			}
		}
	}
}
