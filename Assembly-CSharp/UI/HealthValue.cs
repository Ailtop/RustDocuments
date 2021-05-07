using System;
using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class HealthValue : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI _text;

		[SerializeField]
		private Image _healthImage;

		private Health _health;

		private Shield _shield;

		private const string shieldColor = "#53DEFF";

		private const string highHealthColor = "#ffffff";

		private const string lowHealthColor = "#ff0000";

		public void Initialize(Health health, Shield shield)
		{
			_health = health;
			_shield = shield;
		}

		private void Update()
		{
			if (!(_health == null))
			{
				string text = ((_shield == null || !(_shield.amount > 0.0)) ? string.Empty : string.Format("<color={0}>(+{1})</color>", "#53DEFF", _shield.amount));
				string text2;
				if (_health.percent < 0.30000001192092896)
				{
					text2 = "#ff0000";
					_healthImage.color = Color.red;
				}
				else
				{
					text2 = "#ffffff";
					_healthImage.color = Color.white;
				}
				_text.text = $"<color={text2}>{Math.Ceiling(_health.currentHealth)} / {Math.Ceiling(_health.maximumHealth)}</color>  {text}";
			}
		}
	}
}
