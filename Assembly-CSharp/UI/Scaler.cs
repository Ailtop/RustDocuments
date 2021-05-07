using UnityEngine;

namespace UI
{
	public class Scaler : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _canvas;

		[SerializeField]
		private RectTransform _content;

		[SerializeField]
		private ScreenLetterBox _letterBox;

		private bool _verticalLetterBox;

		private Vector2 _contentSize;

		private void Awake()
		{
			_contentSize = _content.sizeDelta;
		}

		public void SetVerticalLetterBox(bool verticalLetterBox)
		{
			_verticalLetterBox = verticalLetterBox;
			_letterBox.SetVerticalLetterBox(verticalLetterBox);
			if (_verticalLetterBox)
			{
				_content.sizeDelta = _contentSize;
			}
			else
			{
				_content.sizeDelta = new Vector2(_canvas.sizeDelta.x, _contentSize.y);
			}
		}
	}
}
