using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.GearPopup
{
	public class ItemSelectElement : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		[SerializeField]
		private Image _icon;

		public Action onSelected;

		public void OnSelect(BaseEventData eventData)
		{
			onSelected?.Invoke();
		}

		public void SetIcon(Sprite sprite)
		{
			_icon.enabled = true;
			_icon.sprite = sprite;
			_icon.SetNativeSize();
		}
	}
}
