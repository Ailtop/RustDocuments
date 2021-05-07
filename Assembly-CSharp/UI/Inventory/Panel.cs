using System;
using System.Linq;
using Characters;
using Characters.Controllers;
using Characters.Gear.Items;
using Characters.Gear.Quintessences;
using Characters.Gear.Weapons;
using Characters.Player;
using FX;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UserInput;

namespace UI.Inventory
{
	public class Panel : Dialogue
	{
		[SerializeField]
		private GearOption _gearOption;

		[SerializeField]
		private Image _focus;

		[Space]
		[SerializeField]
		private KeywordDisplay _keywordDisplay;

		[Space]
		[SerializeField]
		[FormerlySerializedAs("_itemDiscardButton")]
		private PressingButton _gearDiscardButton;

		[Space]
		[SerializeField]
		private GearElement[] _weapons;

		[SerializeField]
		private GearElement[] _quintessences;

		[SerializeField]
		private GearElement[] _items;

		[Space]
		[SerializeField]
		private SoundInfo _openSound;

		[SerializeField]
		private SoundInfo _closeSound;

		[SerializeField]
		private SoundInfo _selectSound;

		private GameObject _lastValidFocus;

		private Action _swapSkill;

		private Action _discardGear;

		public override bool closeWithPauseKey => true;

		private void Awake()
		{
			EventSystem.current.sendNavigationEvents = true;
			Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.AddComponent<PlaySoundOnSelected>().soundInfo = _selectSound;
			}
			_gearDiscardButton.onPressed += delegate
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(GlobalSoundSettings.instance.gearDestroying, base.transform.position);
				_discardGear?.Invoke();
			};
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			PersistentSingleton<SoundManager>.Instance.PlaySound(_openSound, Vector3.zero);
			PlayerInput.blocked.Attach(this);
			Chronometer.global.AttachTimeScale(this, 0f);
			WeaponInventory weapon = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon;
			UpdateGearInfo();
		}

		private void ClearOption()
		{
			_gearOption.Clear();
			_discardGear = null;
		}

		private void UpdateGearInfo()
		{
			_keywordDisplay.UpdateElements();
			Character player = Singleton<Service>.Instance.levelManager.player;
			WeaponInventory weapon2 = player.playerComponents.inventory.weapon;
			QuintessenceInventory quintessenceInventory = player.playerComponents.inventory.quintessence;
			ItemInventory itemInventory = player.playerComponents.inventory.item;
			for (int i = 0; i < _weapons.Length; i++)
			{
				Weapon weapon;
				if (i == 0)
				{
					weapon = weapon2.polymorphOrCurrent;
				}
				else
				{
					weapon = weapon2.next;
				}
				if (weapon != null)
				{
					Sprite icon = weapon.icon;
					_weapons[i].SetIcon(icon);
					_weapons[i].onSelected = delegate
					{
						_gearOption.gameObject.SetActive(true);
						_gearOption.Set(weapon);
						_discardGear = null;
						_swapSkill = delegate
						{
							weapon.SwapSkillOrder();
							_gearOption.Set(weapon);
						};
					};
				}
				else
				{
					_weapons[i].Deactivate();
					_weapons[i].onSelected = ClearOption;
				}
			}
			for (int j = 0; j < _quintessences.Length; j++)
			{
				Quintessence quintessence = quintessenceInventory.items[j];
				if (quintessence != null)
				{
					Sprite icon2 = quintessence.icon;
					_quintessences[j].SetIcon(icon2);
					int cachedIndex2 = j;
					_quintessences[j].onSelected = delegate
					{
						_gearOption.gameObject.SetActive(true);
						_gearOption.Set(quintessence);
						_discardGear = delegate
						{
							if (quintessenceInventory.Discard(cachedIndex2))
							{
								_quintessences[cachedIndex2].Deactivate();
								UpdateGearInfo();
								_quintessences[cachedIndex2].onSelected();
							}
						};
						_swapSkill = null;
					};
				}
				else
				{
					_quintessences[j].Deactivate();
					_quintessences[j].onSelected = ClearOption;
				}
			}
			string[] itemKeys = (from item in itemInventory.items
				where item != null
				select item.name).ToArray();
			for (int k = 0; k < _items.Length; k++)
			{
				Item item2 = itemInventory.items[k];
				GearElement gearElement = _items[k];
				if (item2 != null)
				{
					Sprite icon3 = item2.icon;
					gearElement.SetIcon(icon3);
					if (item2.setItemKeys.Any((string setKey) => itemKeys.Contains(setKey, StringComparer.OrdinalIgnoreCase)))
					{
						if (item2.setItemImage != null)
						{
							gearElement.SetSetImage(item2.setItemImage);
						}
						if (item2.setItemAnimator != null)
						{
							gearElement.SetSetAnimator(item2.setItemAnimator);
						}
					}
					else
					{
						gearElement.DisableSetEffect();
					}
					int cachedIndex = k;
					gearElement.onSelected = delegate
					{
						_gearOption.gameObject.SetActive(true);
						_gearOption.Set(item2);
						_discardGear = delegate
						{
							if (itemInventory.Discard(cachedIndex))
							{
								itemInventory.Trim();
								_items[cachedIndex].Deactivate();
								UpdateGearInfo();
								_items[cachedIndex].onSelected();
							}
						};
						_swapSkill = null;
					};
				}
				else
				{
					gearElement.Deactivate();
					gearElement.onSelected = ClearOption;
				}
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			PersistentSingleton<SoundManager>.Instance.PlaySound(_closeSound, Vector3.zero);
			PlayerInput.blocked.Detach(this);
			Chronometer.global.DetachTimeScale(this);
		}

		private void Update()
		{
			if (!base.focused)
			{
				return;
			}
			if (KeyMapper.Map.Inventory.WasPressed)
			{
				Close();
				return;
			}
			if (KeyMapper.Map.Interaction.WasPressed && _discardGear == null)
			{
				_swapSkill?.Invoke();
			}
			GameObject gameObject = EventSystem.current.currentSelectedGameObject;
			if (gameObject == null || gameObject.GetComponent<GearElement>() == null)
			{
				if (_lastValidFocus == null)
				{
					return;
				}
				EventSystem.current.SetSelectedGameObject(_lastValidFocus);
				gameObject = _lastValidFocus;
			}
			else
			{
				_lastValidFocus = gameObject;
			}
			_focus.rectTransform.position = gameObject.transform.position;
		}
	}
}
