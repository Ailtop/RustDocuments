using Unity.Mathematics;
using UnityEngine;

namespace UI.GearPopup
{
	[RequireComponent(typeof(RectTransform))]
	public class GearPopupCanvas : MonoBehaviour
	{
		private const float _width = 474f;

		private const float _minViewportY = 0.4f;

		private const float _maxViewportY = 0.6f;

		private const float _padding = 5f;

		[SerializeField]
		private GearPopup _gearPopup;

		[SerializeField]
		private RectTransform _container;

		[SerializeField]
		private RectTransform _content;

		[SerializeField]
		private RectTransform _canvas;

		public GearPopup gearPopup => _gearPopup;

		private void Awake()
		{
			Close();
		}

		private void Update()
		{
			Vector2 vector = _content.sizeDelta / 2f;
			vector.x = 474f;
			vector.x *= _container.lossyScale.x;
			vector.y *= _container.lossyScale.y;
			vector.x += 5f;
			vector.y += 5f;
			float num = _canvas.sizeDelta.x * _canvas.localScale.x;
			float num2 = _canvas.sizeDelta.y * _canvas.localScale.y;
			Vector3 position = _container.position;
			position.x = math.clamp(position.x, vector.x, num - vector.x);
			position.y = math.clamp(position.y, vector.y, num2 - vector.y);
			_container.position = position;
		}

		public void Open(Vector3 position)
		{
			_container.gameObject.SetActive(true);
			Vector3 vector = Camera.main.WorldToViewportPoint(position);
			vector.y = Mathf.Clamp(vector.y, 0.4f, 0.6f);
			Vector2 sizeDelta = _canvas.sizeDelta;
			sizeDelta.x *= _canvas.localScale.x;
			sizeDelta.y *= _canvas.localScale.y;
			Vector2 vector2 = new Vector2(vector.x * sizeDelta.x, vector.y * sizeDelta.y);
			_container.position = vector2;
		}

		public void Close()
		{
			_container.gameObject.SetActive(false);
		}
	}
}
