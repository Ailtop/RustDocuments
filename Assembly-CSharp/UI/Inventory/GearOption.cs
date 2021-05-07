using Characters.Gear.Items;
using Characters.Gear.Quintessences;
using Characters.Gear.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
	public class GearOption : MonoBehaviour
	{
		[SerializeField]
		private WeaponOption _weaponOption;

		[SerializeField]
		private ItemOption _itemOption;

		[SerializeField]
		private QuintessenceOption _essenceOption;

		[Space]
		[SerializeField]
		private Image _thumnailIcon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _rarity;

		[Space]
		[SerializeField]
		private PressingButton _itemDiscardKey;

		[SerializeField]
		private TMP_Text _itemDiscardText;

		[SerializeField]
		private GameObject _skillSwapKey;

		public void Clear()
		{
			_thumnailIcon.enabled = false;
			_name.text = string.Empty;
			_rarity.text = string.Empty;
			_itemDiscardKey.gameObject.SetActive(false);
			_skillSwapKey.SetActive(false);
			_weaponOption.gameObject.SetActive(false);
			_itemOption.gameObject.SetActive(false);
			_essenceOption.gameObject.SetActive(false);
		}

		public void Set(Weapon weapon)
		{
			Clear();
			_weaponOption.gameObject.SetActive(true);
			_weaponOption.Set(weapon);
		}

		public void Set(Item item)
		{
			Clear();
			_itemOption.gameObject.SetActive(true);
			_itemOption.Set(item);
		}

		public void Set(Quintessence essence)
		{
			Clear();
			_essenceOption.gameObject.SetActive(true);
			_essenceOption.Set(essence);
		}
	}
}
