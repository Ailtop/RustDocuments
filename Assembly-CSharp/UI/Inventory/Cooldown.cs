using Characters.Cooldowns;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
	public class Cooldown : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TextMeshProUGUI _text;

		public void Set(CooldownSerializer cooldown)
		{
			float num = ((cooldown.type != CooldownSerializer.Type.Time) ? 0f : cooldown.time.cooldownTime);
			if (_icon != null)
			{
				_icon.enabled = true;
			}
			_text.enabled = true;
			_text.text = num.ToString();
		}
	}
}
