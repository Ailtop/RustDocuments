using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.Controllers;
using Characters.Gear.Items;
using Characters.Gear.Synergy;
using Characters.Gear.Synergy.Keywords;
using Characters.Player;
using FX;
using InControl;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UserInput;

namespace UI.GearPopup
{
	public class ItemSelect : Dialogue
	{
		[SerializeField]
		private RectTransform _canvas;

		[Space]
		[SerializeField]
		private GameObject _moreinscriptionFrame;

		[SerializeField]
		private KeywordElement[] _keywordElements;

		[Space]
		[SerializeField]
		private RectTransform _fieldGearContainer;

		[SerializeField]
		private GearPopupForItemSelection _fieldGearPopup;

		[Space]
		[SerializeField]
		private RectTransform _inventoryGearContainer;

		[SerializeField]
		private GearPopupForItemSelection _inventoryGearPopup;

		[Space]
		[SerializeField]
		private ItemSelectNavigation _navigation;

		[Header("Sound")]
		[SerializeField]
		private SoundInfo _openSound;

		[SerializeField]
		private SoundInfo _closeSound;

		[SerializeField]
		private SoundInfo _selectSound;

		private Item _fieldItem;

		private Character _player;

		private ItemInventory _itemInventory;

		private Synergy _synergy;

		private PlayerInput _playerInput;

		public override bool closeWithPauseKey => true;

		private void Awake()
		{
			_navigation.onItemSelected += OnItemSelected;
			Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.AddComponent<PlaySoundOnSelected>().soundInfo = _selectSound;
			}
		}

		private void Update()
		{
			UpdateContainerSizeAndPosition(_fieldGearContainer);
			UpdateContainerSizeAndPosition(_inventoryGearContainer);
			FixFocus();
			if (KeyMapper.Map.Interaction.WasPressed || (KeyMapper.Map.SimplifiedLastInputType == BindingSourceType.KeyBindingSource && KeyMapper.Map.Submit.WasPressed))
			{
				_itemInventory.Drop(_navigation.selectedItemIndex);
				_fieldItem.dropped.InteractWith(_player);
				base.gameObject.SetActive(false);
			}
		}

		private void FixFocus()
		{
			if (base.focused)
			{
				GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
				if (!(currentSelectedGameObject != null) || !(currentSelectedGameObject.GetComponent<ItemSelectElement>() != null))
				{
					ItemSelectElement component = _defaultFocus.GetComponent<ItemSelectElement>();
					EventSystem.current.SetSelectedGameObject(component.gameObject);
					component.onSelected?.Invoke();
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			PersistentSingleton<SoundManager>.Instance.PlaySound(_openSound, Vector3.zero);
			PlayerInput.blocked.Attach(this);
			Chronometer.global.AttachTimeScale(this, 0f);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			PersistentSingleton<SoundManager>.Instance.PlaySound(_closeSound, Vector3.zero);
			PlayerInput.blocked.Detach(this);
			Chronometer.global.DetachTimeScale(this);
		}

		private void OnItemSelected(int index)
		{
			Item item = _itemInventory.items[index];
			SetInventoryGear(item);
			EnumArray<Keyword.Key, int> enumArray = new EnumArray<Keyword.Key, int>();
			EnumArray<Keyword.Key, int> enumArray2 = _synergy.keywordCounts.Clone();
			enumArray[_fieldItem.keyword1]++;
			enumArray[_fieldItem.keyword2]++;
			enumArray2[_fieldItem.keyword1]++;
			enumArray2[_fieldItem.keyword2]++;
			enumArray[item.keyword1]--;
			enumArray[item.keyword2]--;
			KeyValuePair<Keyword.Key, int>[] array = (from pair in enumArray2.ToKeyValuePairs()
				where pair.Value > 0
				select pair into keywordCount
				orderby keywordCount.Value descending
				select keywordCount).ToArray();
			KeywordElement[] keywordElements = _keywordElements;
			for (int i = 0; i < keywordElements.Length; i++)
			{
				keywordElements[i].gameObject.SetActive(false);
			}
			int num = 0;
			if (array.Length <= 12)
			{
				_moreinscriptionFrame.SetActive(false);
				num = 12;
			}
			else
			{
				_moreinscriptionFrame.SetActive(true);
			}
			int num2 = Math.Min(array.Length, _keywordElements.Length);
			for (int j = 0; j < num2; j++)
			{
				_keywordElements[j + num].gameObject.SetActive(true);
				_keywordElements[j + num].Set(array[j].Key, enumArray[array[j].Key]);
			}
		}

		private void UpdateContainerSizeAndPosition(RectTransform container)
		{
			Vector2 vector = container.sizeDelta / 2f;
			vector.x *= container.lossyScale.x;
			vector.y *= container.lossyScale.y;
			float num = _canvas.sizeDelta.x * _canvas.localScale.x;
			Vector3 position = container.position;
			position.x = Mathf.Clamp(position.x, vector.x, num - vector.x);
			container.position = position;
		}

		public void Open(Item fieldItem)
		{
			_player = Singleton<Service>.Instance.levelManager.player;
			_itemInventory = _player.playerComponents.inventory.item;
			_synergy = _player.playerComponents.inventory.synergy;
			_playerInput = _player.GetComponent<PlayerInput>();
			_fieldItem = fieldItem;
			_fieldGearPopup.Set(fieldItem);
			Open();
		}

		public void SetInventoryGear(Item item)
		{
			_inventoryGearPopup.Set(item);
		}
	}
}
