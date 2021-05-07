using Characters.Gear.Synergy.Keywords;
using Services;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GearPopup
{
	public class KeywordElement : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _level;

		[SerializeField]
		private TMP_Text _description;

		private Keyword.Key _keyword;

		private Keyword _keywordComponent;

		public void Set(Keyword.Key keyword, int delta = 0)
		{
			_keyword = keyword;
			_keywordComponent = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.synergy.keywordComponents[keyword];
			if (_icon != null)
			{
				_icon.sprite = keyword.GetIcon();
			}
			if (_name != null)
			{
				_name.text = keyword.GetName();
			}
			UpdateLevel(delta);
			if (_description != null)
			{
				_description.text = _keywordComponent.GetCurrentDescription();
			}
		}

		public void UpdateLevel(int delta = 0)
		{
			if (!(_level == null))
			{
				string format = ((delta > 0) ? "<color=#5FED64>{0}</color>/{1}" : ((delta != 0) ? "<color=#FF4D4D>{0}</color>/{1}" : "{0}/{1}"));
				_level.text = string.Format(format, _keywordComponent.count + delta, _keywordComponent.maxLevel);
			}
		}
	}
}
