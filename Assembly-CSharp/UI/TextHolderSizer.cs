using System;
using TMPro;
using UnityEngine;

namespace UI
{
	public class TextHolderSizer : MonoBehaviour
	{
		[Flags]
		public enum Mode
		{
			Width = 0x1,
			Height = 0x2,
			Both = 0x3
		}

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		[EnumFlag]
		private Mode _mode;

		[SerializeField]
		private Vector2 _minSize = Vector2.zero;

		[SerializeField]
		private Vector2 _maxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		[SerializeField]
		private Vector2 _padding = Vector2.zero;

		[SerializeField]
		private Vector2 _multiplier = Vector2.one;

		private void Update()
		{
			UpdateSize();
		}

		public void UpdateSize()
		{
			Vector2 vector = new Vector2(_text.preferredWidth * _multiplier.x, _text.preferredHeight * _multiplier.y) + _padding;
			Vector2 sizeDelta = _rectTransform.sizeDelta;
			if (_mode.HasFlag(Mode.Width))
			{
				sizeDelta.x = Mathf.Clamp(_minSize.x, vector.x, _maxSize.x);
			}
			if (_mode.HasFlag(Mode.Height))
			{
				sizeDelta.y = Mathf.Clamp(_minSize.y, vector.y, _maxSize.y);
			}
			_rectTransform.sizeDelta = sizeDelta;
		}
	}
}
