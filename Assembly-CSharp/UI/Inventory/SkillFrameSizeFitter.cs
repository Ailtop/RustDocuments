using UnityEngine;

namespace UI.Inventory
{
	public class SkillFrameSizeFitter : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _targetRectTransform;

		[SerializeField]
		private float _startHeight;

		private float _scale;

		private float _defaultHeight;

		private void Awake()
		{
			_defaultHeight = _rectTransform.sizeDelta.y;
			_scale = base.transform.localScale.y;
		}

		private void Update()
		{
			float y = _targetRectTransform.sizeDelta.y;
			float num = 0f;
			if (y > _startHeight)
			{
				num = (y - _startHeight) / _scale;
			}
			Vector2 sizeDelta = _rectTransform.sizeDelta;
			sizeDelta.y = _defaultHeight + num;
			_rectTransform.sizeDelta = sizeDelta;
		}
	}
}
