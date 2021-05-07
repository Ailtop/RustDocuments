using System.Collections.Generic;
using Characters.Gear;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TestingTool
{
	public class GearList : MonoBehaviour
	{
		[SerializeField]
		private GearListElement _gearListElementPrefab;

		[SerializeField]
		private Button _head;

		[SerializeField]
		private Button _item;

		[SerializeField]
		private Button _essence;

		[SerializeField]
		private TMP_InputField _inputField;

		[SerializeField]
		private Transform _gridContainer;

		private readonly List<GearListElement> _gearListElements = new List<GearListElement>();

		private void Awake()
		{
			Resource.WeaponReference[] weapons = Resource.instance.weapons;
			foreach (Resource.WeaponReference gearReference in weapons)
			{
				GearListElement gearListElement = Object.Instantiate(_gearListElementPrefab, _gridContainer);
				gearListElement.Set(gearReference);
				_gearListElements.Add(gearListElement);
			}
			Resource.ItemInfo[] items = Resource.instance.items;
			foreach (Resource.ItemInfo gearReference2 in items)
			{
				GearListElement gearListElement2 = Object.Instantiate(_gearListElementPrefab, _gridContainer);
				gearListElement2.Set(gearReference2);
				_gearListElements.Add(gearListElement2);
			}
			Resource.QuintessenceInfo[] quintessences = Resource.instance.quintessences;
			foreach (Resource.QuintessenceInfo gearReference3 in quintessences)
			{
				GearListElement gearListElement3 = Object.Instantiate(_gearListElementPrefab, _gridContainer);
				gearListElement3.Set(gearReference3);
				_gearListElements.Add(gearListElement3);
			}
			_head.onClick.AddListener(delegate
			{
				FilterGearList(Gear.Type.Weapon);
			});
			_item.onClick.AddListener(delegate
			{
				FilterGearList(Gear.Type.Item);
			});
			_essence.onClick.AddListener(delegate
			{
				FilterGearList(Gear.Type.Quintessence);
			});
			_inputField.onValueChanged.AddListener(delegate
			{
				FilterGearList();
			});
		}

		private void FilterGearList(Gear.Type? type = null)
		{
			string value = _inputField.text.Trim().ToUpperInvariant();
			foreach (GearListElement gearListElement in _gearListElements)
			{
				bool flag = string.IsNullOrEmpty(value) || gearListElement.text.ToUpperInvariant().Contains(value) || gearListElement.gearReference.name.ToUpperInvariant().Contains(value);
				if (type.HasValue)
				{
					flag &= gearListElement.type == type;
				}
				gearListElement.gameObject.SetActive(flag);
			}
		}
	}
}
