using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class SelectionSpriteSwapper : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		[SerializeField]
		private Image _image;

		[SerializeField]
		private Sprite _normal;

		[SerializeField]
		private Sprite _selected;

		private void Awake()
		{
			_image.sprite = _normal;
		}

		private void OnDisable()
		{
			_image.sprite = _normal;
		}

		public void OnSelect(BaseEventData eventData)
		{
			_image.sprite = _selected;
		}

		public void OnDeselect(BaseEventData eventData)
		{
			_image.sprite = _normal;
		}
	}
}
