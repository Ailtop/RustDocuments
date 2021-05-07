using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class NpcName : MonoBehaviour
	{
		[SerializeField]
		private TextHolderSizer _sizer;

		[SerializeField]
		private Image _textField;

		[SerializeField]
		private TextMeshProUGUI _text;

		[SerializeField]
		private int _minSize = 97;

		[SerializeField]
		private int _maxSize = 305;

		[SerializeField]
		private int _deltaSize = 20;

		private float _originX;

		public string text
		{
			get
			{
				return _text.text;
			}
			set
			{
				_text.text = value;
				if (string.IsNullOrWhiteSpace(text))
				{
					base.gameObject.SetActive(false);
					return;
				}
				base.gameObject.SetActive(true);
				_sizer.UpdateSize();
			}
		}

		private void Start()
		{
			_originX = _textField.rectTransform.rect.width;
		}
	}
}
