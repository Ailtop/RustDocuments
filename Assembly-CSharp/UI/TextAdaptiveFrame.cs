using TMPro;
using UnityEngine;

namespace UI
{
	public class TextAdaptiveFrame : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private RectTransform _rectTransform;

		[SerializeField]
		private TMP_Text _text;

		public string text
		{
			get
			{
				return _text.text;
			}
			set
			{
				_text.text = value;
			}
		}

		private void OnEnable()
		{
			UpdateSize();
		}

		public void UpdateSize()
		{
			float size = _text.preferredWidth * 0.2f + 40f;
			_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		}
	}
}
