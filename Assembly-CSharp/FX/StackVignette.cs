using UnityEngine;
using UnityEngine.UI;

namespace FX
{
	public class StackVignette : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Image _image;

		[SerializeField]
		[MinMaxSlider(0f, 100f)]
		private Vector2Int _stackRange;

		public void UpdateStack(float stack)
		{
			if (stack <= (float)_stackRange.x)
			{
				UpdateAlpha(0f);
			}
			else
			{
				UpdateAlpha(stack / (float)_stackRange.y);
			}
		}

		private void UpdateAlpha(float a)
		{
			Color color = _image.color;
			color.a = a;
			_image.color = color;
		}

		public void Hide()
		{
			UpdateAlpha(0f);
		}
	}
}
