using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hud
{
	public class AbilityIcon : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private Image _fill;

		[SerializeField]
		private TMP_Text _stackText;

		private int _stack;

		public Sprite icon
		{
			get
			{
				return _icon.sprite;
			}
			set
			{
				_icon.sprite = value;
			}
		}

		public float fillAmount
		{
			get
			{
				return _fill.fillAmount;
			}
			set
			{
				_fill.fillAmount = value;
			}
		}

		public bool clockwise
		{
			get
			{
				return _fill.fillClockwise;
			}
			set
			{
				_fill.fillClockwise = value;
			}
		}

		public int stacks
		{
			get
			{
				return _stack;
			}
			set
			{
				_stack = value;
				_stackText.text = ((value > 0) ? value.ToString() : string.Empty);
			}
		}
	}
}
