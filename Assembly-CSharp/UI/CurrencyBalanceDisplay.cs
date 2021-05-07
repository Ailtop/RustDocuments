using Data;
using TMPro;
using UnityEngine;

namespace UI
{
	public class CurrencyBalanceDisplay : MonoBehaviour
	{
		[SerializeField]
		private GameData.Currency.Type _type;

		[SerializeField]
		private TextMeshProUGUI _label;

		[SerializeField]
		private TextMeshProUGUI _text;

		[SerializeField]
		private bool _colored;

		private int _balanceCache;

		private EnumArray<GameData.Currency.Type, string> _colorOpenByCurrency => new EnumArray<GameData.Currency.Type, string>(Lingua.GetLocalizedString(Lingua.Key.colorOpenGold), Lingua.GetLocalizedString(Lingua.Key.colorOpenDarkQuartz), Lingua.GetLocalizedString(Lingua.Key.colorOpenBone));

		private string _colorClose => Lingua.GetLocalizedString("cc");

		private void Awake()
		{
			UpdateText(true);
		}

		private void Update()
		{
			UpdateText(false);
		}

		private void UpdateText(bool force)
		{
			int balance = GameData.Currency.currencies[_type].balance;
			if (force || _balanceCache != balance)
			{
				_balanceCache = balance;
				if (_colored)
				{
					_text.text = $"{_colorOpenByCurrency[_type]}{balance}{_colorClose}";
				}
				else
				{
					_text.text = balance.ToString();
				}
			}
		}

		public void SetType(GameData.Currency.Type type)
		{
			_type = type;
			switch (type)
			{
			case GameData.Currency.Type.Gold:
				_label.text = Lingua.GetLocalizedString("label/balance/goldBalance");
				break;
			case GameData.Currency.Type.DarkQuartz:
				_label.text = Lingua.GetLocalizedString("label/balance/darkQuartzBalance");
				break;
			case GameData.Currency.Type.Bone:
				_label.text = Lingua.GetLocalizedString("label/balance/boneBalance");
				break;
			}
			UpdateText(true);
		}
	}
}
