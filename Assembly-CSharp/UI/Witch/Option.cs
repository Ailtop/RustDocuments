using Characters;
using Data;
using TMPro;
using UnityEngine;

namespace UI.Witch
{
	public class Option : MonoBehaviour
	{
		private static readonly Color _darkQuartzColor = new Color(29f / 51f, 89f / 255f, 73f / 85f);

		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _level;

		[SerializeField]
		private TMP_Text _description;

		[SerializeField]
		private TMP_Text _nextLevelDescription;

		[SerializeField]
		private GameObject _nextLevelContainer;

		[SerializeField]
		private TMP_Text _cost;

		private WitchBonus.Bonus _bonus;

		public void Set(WitchBonus.Bonus bonus)
		{
			_bonus = bonus;
			UpdateTexts();
		}

		public void UpdateTexts()
		{
			string empty = string.Empty;
			_name.text = _bonus.displayName;
			_level.text = $"Lv. {_bonus.level}/{_bonus.maxLevel}";
			_description.text = _bonus.GetDescription(_bonus.level);
			if (_bonus.level < _bonus.maxLevel)
			{
				_nextLevelContainer.SetActive(true);
				_cost.text = _bonus.levelUpCost.ToString();
				_nextLevelDescription.text = _bonus.GetDescription(_bonus.level + 1);
			}
			else
			{
				_nextLevelContainer.SetActive(false);
			}
		}

		private void Update()
		{
			if (_bonus != null && _bonus.level != _bonus.maxLevel)
			{
				_cost.color = (GameData.Currency.darkQuartz.Has(_bonus.levelUpCost) ? _darkQuartzColor : Color.red);
			}
		}
	}
}
