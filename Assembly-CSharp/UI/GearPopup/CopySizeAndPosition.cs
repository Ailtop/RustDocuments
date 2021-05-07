using UnityEngine;

namespace UI.GearPopup
{
	public class CopySizeAndPosition : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _targetTransform;

		private void Update()
		{
			_rectTransform.sizeDelta = _targetTransform.sizeDelta;
			_rectTransform.position = _targetTransform.position;
		}
	}
}
