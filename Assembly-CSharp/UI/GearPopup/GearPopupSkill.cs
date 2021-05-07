using Characters.Gear.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GearPopup
{
	public class GearPopupSkill : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _cooldown;

		[SerializeField]
		private TMP_Text _description;

		public void Set(SkillInfo skillInfo)
		{
			string cooldown = string.Empty;
			if (skillInfo.action.cooldown.time != null)
			{
				cooldown = skillInfo.action.cooldown.time.cooldownTime.ToString("0");
			}
			Set(skillInfo.GetIcon(), skillInfo.displayName, cooldown, skillInfo.description);
		}

		public void Set(Sprite icon, string name, string cooldown, string description)
		{
			_name.text = name;
			_description.text = description;
			_icon.sprite = icon;
		}
	}
}
