using Characters.Gear;
using Characters.Gear.Items;
using Characters.Gear.Quintessences;
using Characters.Gear.Synergy.Keywords;
using Characters.Gear.Weapons;
using Data;
using Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInput;

namespace UI.GearPopup
{
	public class GearPopup : MonoBehaviour
	{
		private const float _detailViewThreshold = 0.5f;

		[SerializeField]
		private Image _image;

		[SerializeField]
		private Sprite _frame;

		[SerializeField]
		private Sprite _frameWithKeywords;

		[Space]
		[SerializeField]
		private RectTransform _rectTransform;

		[Space]
		[SerializeField]
		private GameObject _interactionGuide;

		[SerializeField]
		private TMP_Text _interactionDescription;

		[Space]
		[SerializeField]
		private TMP_Text _name;

		[Space]
		[SerializeField]
		private GameObject _rarityAndCategory;

		[SerializeField]
		private TMP_Text _rarity;

		[SerializeField]
		private GameObject _cooldownIcon;

		[SerializeField]
		private TMP_Text _categoryOrCooldown;

		[Space]
		[SerializeField]
		private TMP_Text _description;

		[Space]
		[SerializeField]
		private GameObject[] _essenceObjects;

		[SerializeField]
		private TMP_Text _essenceActiveName;

		[SerializeField]
		private TMP_Text _essenceActiveDesc;

		[Space]
		[SerializeField]
		private GameObject _extraOptionContainer;

		[SerializeField]
		private Image _extraOption;

		[SerializeField]
		private TMP_Text _extraOptionText;

		[Space]
		[SerializeField]
		private GameObject _extraOptionContainer2;

		[SerializeField]
		private Image _extraOption1;

		[SerializeField]
		private TMP_Text _extraOption1Text;

		[SerializeField]
		private Image _extraOption2;

		[SerializeField]
		private TMP_Text _extraOption2Text;

		[SerializeField]
		private GameObject _viewDetailContainer;

		[Space]
		[SerializeField]
		private GearPopupSkill _skill;

		[SerializeField]
		private GearPopupSkill _skill1;

		[SerializeField]
		private GearPopupSkill _skill2;

		[Space]
		[SerializeField]
		private GearPopupKeywordDetail _keywordDetail1;

		[SerializeField]
		private GearPopupKeywordDetail _keywordDetail2;

		[Space]
		[SerializeField]
		private PressingButton _pressToDestroy;

		[Space]
		[SerializeField]
		private GameObject _detailContainer;

		private Gear _gear;

		public RectTransform rectTransform => _rectTransform;

		private static string _interactionLootLabel => Lingua.GetLocalizedString("label/interaction/loot");

		private static string _interactionPurcaseLabel => Lingua.GetLocalizedString("label/interaction/purchase");

		private void Update()
		{
			if (_gear == null)
			{
				return;
			}
			ProcessDetailView();
			if (!(_pressToDestroy == null))
			{
				if (_gear.dropped.pressingPercent == 0f)
				{
					_pressToDestroy.StopPressing();
					return;
				}
				_pressToDestroy.PlayPressingSound();
				_pressToDestroy.SetPercent(_gear.dropped.pressingPercent);
			}
		}

		private void ProcessDetailView()
		{
			if (!_detailContainer.activeSelf && KeyMapper.Map.Down.Value > 0.5f)
			{
				_detailContainer.SetActive(true);
			}
			else if (_detailContainer.activeSelf && KeyMapper.Map.Down.Value < 0.5f)
			{
				_detailContainer.SetActive(false);
			}
		}

		private void OnDisable()
		{
			if (!(_pressToDestroy == null))
			{
				_pressToDestroy.StopPressing();
			}
		}

		private void SetBasic(Gear gear)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			_gear = gear;
			_name.text = gear.displayName;
			_rarity.text = Lingua.GetLocalizedString(string.Format("{0}/{1}/{2}", "label", "Rarity", gear.rarity));
			_description.text = gear.description;
			_rarityAndCategory.gameObject.SetActive(true);
			SetInteractionLabel(gear.dropped);
			SetDestructible(gear);
		}

		private void SetDestructible(Gear gear)
		{
			if (_pressToDestroy == null)
			{
				return;
			}
			_pressToDestroy.gameObject.SetActive(gear.destructible);
			if (gear.destructible)
			{
				_pressToDestroy.gameObject.SetActive(true);
				_pressToDestroy.description = Lingua.GetLocalizedString("label/inventory/discardItem");
				if (gear.currencyByDiscard > 0)
				{
					_pressToDestroy.description = $"{_pressToDestroy.description}(<color=#FFDE37>{gear.currencyByDiscard}</color>)";
				}
			}
		}

		private void DisableSetDestructible()
		{
			if (!(_pressToDestroy == null))
			{
				_pressToDestroy.gameObject.SetActive(false);
			}
		}

		private void SetEssenceActive(string name, string description)
		{
			GameObject[] essenceObjects = _essenceObjects;
			for (int i = 0; i < essenceObjects.Length; i++)
			{
				essenceObjects[i].SetActive(true);
			}
			_essenceActiveName.text = name;
			_essenceActiveDesc.text = description;
		}

		private void DisableEssenceActive()
		{
			GameObject[] essenceObjects = _essenceObjects;
			for (int i = 0; i < essenceObjects.Length; i++)
			{
				essenceObjects[i].SetActive(false);
			}
		}

		private void DisableExtraOptions()
		{
			_extraOptionContainer.SetActive(false);
			_extraOptionContainer2.SetActive(false);
			_viewDetailContainer.SetActive(false);
			_skill.gameObject.SetActive(false);
			_skill1.gameObject.SetActive(false);
			_skill2.gameObject.SetActive(false);
			_keywordDetail1.gameObject.SetActive(false);
			_keywordDetail2.gameObject.SetActive(false);
		}

		public void Set(Gear gear)
		{
			if ((object)gear == null)
			{
				return;
			}
			Weapon weapon;
			if ((object)(weapon = gear as Weapon) == null)
			{
				Item item;
				if ((object)(item = gear as Item) == null)
				{
					Quintessence quintessence;
					if ((object)(quintessence = gear as Quintessence) != null)
					{
						Quintessence quintessence2 = quintessence;
						Set(quintessence2);
					}
				}
				else
				{
					Item item2 = item;
					Set(item2);
				}
			}
			else
			{
				Weapon weapon2 = weapon;
				Set(weapon2);
			}
		}

		public void Set(Weapon weapon)
		{
			SetBasic(weapon);
			_cooldownIcon.SetActive(false);
			_categoryOrCooldown.text = weapon.categoryDisplayName;
			SetSkills(weapon);
			DisableEssenceActive();
		}

		private void SetSkills(Weapon weapon)
		{
			_viewDetailContainer.SetActive(true);
			_keywordDetail1.gameObject.SetActive(false);
			_keywordDetail2.gameObject.SetActive(false);
			SkillInfo skillInfo = weapon.currentSkills[0];
			Sprite icon = skillInfo.GetIcon();
			string displayName = skillInfo.displayName;
			_extraOption.sprite = icon;
			_extraOptionText.text = displayName;
			_extraOption1.sprite = icon;
			_extraOption1Text.text = displayName;
			if (weapon.currentSkills.Count < 2)
			{
				_skill.Set(skillInfo);
				_skill.gameObject.SetActive(true);
				_skill1.gameObject.SetActive(false);
				_skill2.gameObject.SetActive(false);
				_extraOptionContainer.SetActive(true);
				_extraOptionContainer2.SetActive(false);
			}
			else
			{
				_extraOptionContainer.SetActive(false);
				_extraOptionContainer2.SetActive(true);
				SkillInfo skillInfo2 = weapon.currentSkills[1];
				_extraOption2.sprite = skillInfo2.GetIcon();
				_extraOption2Text.text = skillInfo2.displayName;
				_skill1.Set(skillInfo);
				_skill2.Set(skillInfo2);
				_skill1.gameObject.SetActive(true);
				_skill2.gameObject.SetActive(true);
			}
		}

		public void Set(Item item)
		{
			SetBasic(item);
			_cooldownIcon.SetActive(false);
			_categoryOrCooldown.text = string.Empty;
			SetKeywords(item);
			_keywordDetail1.Set(item.keyword1);
			_keywordDetail2.Set(item.keyword2);
			DisableEssenceActive();
		}

		private void SetKeywords(Item item)
		{
			_viewDetailContainer.SetActive(true);
			_extraOptionContainer.SetActive(false);
			_extraOptionContainer2.SetActive(true);
			_extraOption1.sprite = item.keyword1.GetIcon();
			_extraOption1Text.text = item.keyword1.GetName();
			_extraOption2.sprite = item.keyword2.GetIcon();
			_extraOption2Text.text = item.keyword2.GetName();
			_skill.gameObject.SetActive(false);
			_skill1.gameObject.SetActive(false);
			_skill2.gameObject.SetActive(false);
			_keywordDetail1.Set(item.keyword1);
			_keywordDetail2.Set(item.keyword2);
			_keywordDetail1.gameObject.SetActive(true);
			_keywordDetail2.gameObject.SetActive(true);
		}

		public void Set(Quintessence quintessence)
		{
			SetBasic(quintessence);
			if (quintessence.cooldown.time == null)
			{
				_cooldownIcon.SetActive(false);
				_categoryOrCooldown.text = string.Empty;
			}
			else
			{
				_cooldownIcon.SetActive(true);
				_categoryOrCooldown.text = quintessence.cooldown.time.cooldownTime.ToString();
			}
			SetEssenceActive(quintessence.activeName, quintessence.activeDescription);
			DisableExtraOptions();
		}

		public void Set(string name, string description, string rarity)
		{
			_name.text = name;
			_description.text = description;
			_rarityAndCategory.gameObject.SetActive(true);
			_rarity.text = rarity;
			_cooldownIcon.SetActive(false);
			_categoryOrCooldown.text = string.Empty;
			DisableEssenceActive();
			DisableExtraOptions();
			DisableSetDestructible();
		}

		public void Set(string name, string description)
		{
			Set(name, description, string.Empty);
		}

		public void Set(string name, string description, Rarity rarity)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			Set(name, description, Lingua.GetLocalizedString(string.Format("{0}/{1}/{2}", "label", "Rarity", rarity)));
		}

		public void SetInteractionLabel(DroppedGear dropped)
		{
			if (_interactionDescription == null)
			{
				return;
			}
			if (dropped.gear != null && !dropped.gear.lootable)
			{
				_interactionGuide.SetActive(false);
				return;
			}
			_interactionGuide.SetActive(true);
			if (dropped.price > 0)
			{
				SetInteractionLabelAsPurchase(dropped.priceCurrency, dropped.price);
			}
			else
			{
				SetInteractionLabelAsLoot();
			}
		}

		public void SetInteractionLabelAsLoot()
		{
			_interactionDescription.text = _interactionLootLabel;
		}

		public void SetInteractionLabelAsPurchase(GameData.Currency.Type currencyType, int price)
		{
			GameData.Currency currency = GameData.Currency.currencies[currencyType];
			string arg = currency.colorCode;
			if (!currency.Has(price))
			{
				arg = "EE1111";
			}
			string arg2 = $"<color=#{arg}>{price}</color>";
			_interactionDescription.text = string.Format(_interactionPurcaseLabel, arg2);
		}
	}
}
