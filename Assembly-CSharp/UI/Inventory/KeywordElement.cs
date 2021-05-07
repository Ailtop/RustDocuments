using Characters.Gear.Synergy.Keywords;
using Services;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
	public class KeywordElement : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _level;

		private Keyword.Key _keyword;

		private Keyword _keywordComponent;

		public void Set(Keyword.Key keyword)
		{
			_keyword = keyword;
			_keywordComponent = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.synergy.keywordComponents[keyword];
			_icon.sprite = keyword.GetIcon();
			_name.text = keyword.GetName();
			if (_keywordComponent.count >= _keywordComponent.maxLevel)
			{
				_level.text = $"<color=#FFD86F>{_keywordComponent.count}</color>";
			}
			else
			{
				_level.text = _keywordComponent.count.ToString();
			}
		}
	}
}
