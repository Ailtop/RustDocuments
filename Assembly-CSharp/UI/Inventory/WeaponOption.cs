using Characters.Gear.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInput;

namespace UI.Inventory
{
	public class WeaponOption : MonoBehaviour
	{
		[SerializeField]
		private Image _thumnailIcon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _rarity;

		[SerializeField]
		private TMP_Text _category;

		[Space]
		[SerializeField]
		private GameObject _simpleContainer;

		[SerializeField]
		private GameObject _detailContainer;

		[Space]
		[SerializeField]
		private TMP_Text _flavor;

		[SerializeField]
		private TMP_Text _passive;

		[Space]
		[SerializeField]
		private TMP_Text _swapSkillNameSimple;

		[SerializeField]
		private TMP_Text _swapSkillNameDetail;

		[SerializeField]
		private TMP_Text _swapSkillDescription;

		[Space]
		[SerializeField]
		private GameObject _skillSwapKey;

		[Space]
		[SerializeField]
		private GameObject _skillContainer;

		[SerializeField]
		private SkillOption _skill;

		[Space]
		[SerializeField]
		private GameObject _skill2Container;

		[SerializeField]
		private SkillOption _skill1;

		[SerializeField]
		private SkillOption _skill2;

		[Space]
		[SerializeField]
		private GameObject _skillDetailContainer;

		[SerializeField]
		private SkillOption _skillDetail;

		[Space]
		[SerializeField]
		private GameObject _skill2DetailContainer;

		[SerializeField]
		private SkillOption _skill1Detail;

		[SerializeField]
		private SkillOption _skill2Detail;

		public void Set(Weapon weapon)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			_thumnailIcon.enabled = true;
			_thumnailIcon.sprite = weapon.thumbnail;
			_thumnailIcon.transform.localScale = Vector3.one * 3f;
			_thumnailIcon.SetNativeSize();
			_name.text = weapon.displayName;
			_rarity.text = Lingua.GetLocalizedString(string.Format("{0}/{1}/{2}", "label", "Rarity", weapon.rarity));
			_category.text = weapon.categoryDisplayName;
			_flavor.text = (weapon.hasFlavor ? weapon.flavor : string.Empty);
			_passive.text = weapon.description;
			_category.gameObject.SetActive(true);
			if (weapon.currentSkills.Count == 1)
			{
				_skillContainer.gameObject.SetActive(true);
				_skill2Container.gameObject.SetActive(false);
				_skillDetailContainer.gameObject.SetActive(true);
				_skill2DetailContainer.gameObject.SetActive(false);
				_skillSwapKey.SetActive(false);
				_skill.Set(weapon.currentSkills[0]);
				_skillDetail.Set(weapon.currentSkills[0]);
			}
			else
			{
				_skillContainer.gameObject.SetActive(false);
				_skill2Container.gameObject.SetActive(true);
				_skillDetailContainer.gameObject.SetActive(false);
				_skill2DetailContainer.gameObject.SetActive(true);
				_skillSwapKey.SetActive(true);
				_skill1.Set(weapon.currentSkills[0]);
				_skill2.Set(weapon.currentSkills[1]);
				_skill1Detail.Set(weapon.currentSkills[0]);
				_skill2Detail.Set(weapon.currentSkills[1]);
			}
			_swapSkillNameSimple.text = weapon.activeName;
			_swapSkillNameDetail.text = weapon.activeName;
			_swapSkillDescription.text = weapon.activeDescription;
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
