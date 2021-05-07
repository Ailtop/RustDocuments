using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(RectTransform), typeof(ILayoutElement))]
	public class SizeSetter : MonoBehaviour
	{
		private enum SizingMethod
		{
			None,
			Min,
			Preferred,
			Flexible
		}

		[SerializeField]
		private SizingMethod _widthSizing;

		[SerializeField]
		private SizingMethod _heightSizing;

		private RectTransform _rectTransform;

		private ILayoutElement _layoutElement;

		private void OnEnable()
		{
			_rectTransform = GetComponent<RectTransform>();
			_layoutElement = GetComponent<ILayoutElement>();
		}

		private void Update()
		{
			Vector2 sizeDelta = _rectTransform.sizeDelta;
			switch (_widthSizing)
			{
			case SizingMethod.Min:
				sizeDelta.x = _layoutElement.minWidth;
				break;
			case SizingMethod.Preferred:
				sizeDelta.x = _layoutElement.preferredWidth;
				break;
			case SizingMethod.Flexible:
				sizeDelta.x = _layoutElement.flexibleWidth;
				break;
			}
			switch (_heightSizing)
			{
			case SizingMethod.Min:
				sizeDelta.y = _layoutElement.minHeight;
				break;
			case SizingMethod.Preferred:
				sizeDelta.y = _layoutElement.preferredHeight;
				break;
			case SizingMethod.Flexible:
				sizeDelta.y = _layoutElement.flexibleHeight;
				break;
			}
			_rectTransform.sizeDelta = sizeDelta;
		}
	}
}
