using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

namespace UI
{
	public class ScreenLetterBox : MonoBehaviour
	{
		[SerializeField]
		private CanvasScaler _scaler;

		[SerializeField]
		private PixelPerfectCamera _pixelPerfectCamera;

		[SerializeField]
		private RectTransform _canvas;

		[SerializeField]
		private RectTransform _top;

		[SerializeField]
		private RectTransform _bottom;

		[SerializeField]
		private RectTransform _left;

		[SerializeField]
		private RectTransform _right;

		private float _heightCache;

		private float _widthCache;

		private bool _verticalLetterBox;

		private int _screenWidth;

		private int _screenHeight;

		private void Update()
		{
			bool force = _screenWidth != Screen.width || _screenHeight != Screen.height;
			UpdateLetterBox(force);
		}

		public void UpdateLetterBox(bool force)
		{
			float num = (_canvas.sizeDelta.y - _scaler.referenceResolution.y) / 2f;
			float num2 = ((!_verticalLetterBox) ? 0f : ((_canvas.sizeDelta.x - _scaler.referenceResolution.x) / 2f));
			if (force || _heightCache != num || _widthCache != num2)
			{
				_heightCache = num;
				_widthCache = num2;
				Vector2 sizeDelta = _top.sizeDelta;
				sizeDelta.y = num;
				_top.sizeDelta = sizeDelta;
				Vector2 sizeDelta2 = _top.sizeDelta;
				sizeDelta2.y = num;
				_bottom.sizeDelta = sizeDelta2;
				Vector2 sizeDelta3 = _left.sizeDelta;
				sizeDelta3.x = num2;
				_left.sizeDelta = sizeDelta3;
				Vector2 sizeDelta4 = _right.sizeDelta;
				sizeDelta4.x = num2;
				_right.sizeDelta = sizeDelta4;
			}
		}

		public void SetVerticalLetterBox(bool verticalLetterBox)
		{
			_verticalLetterBox = verticalLetterBox;
			_pixelPerfectCamera.cropFrameY = _verticalLetterBox;
			UpdateLetterBox(true);
		}
	}
}
