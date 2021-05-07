using Characters.Gear.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
	public class SkillOption : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _name;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TextMeshProUGUI _description;

		[SerializeField]
		private Cooldown _cooldown;

		public void Set(SkillInfo skillDescInfo)
		{
			_name.text = skillDescInfo.displayName;
			_icon.sprite = skillDescInfo.cachedIcon;
			_icon.SetNativeSize();
			if (_description != null)
			{
				_description.text = skillDescInfo.description;
			}
			if (_cooldown != null)
			{
				_cooldown.Set(skillDescInfo.action.cooldown);
			}
		}
	}
}
