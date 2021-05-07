using Characters.Gear.Synergy.Keywords;
using Services;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
	public class KeywordOption : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _level;

		[SerializeField]
		private TMP_Text _description;

		public void Set(Keyword.Key keyword)
		{
			_icon.sprite = keyword.GetIcon();
			_name.text = keyword.GetName();
			Keyword keyword2 = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.synergy.keywordComponents[keyword];
			_level.text = $"{keyword2.count}/{keyword2.maxLevel}";
			if (_description != null)
			{
				_description.text = keyword2.GetCurrentDescription();
			}
		}
	}
}
