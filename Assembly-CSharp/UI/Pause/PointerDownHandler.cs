using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Pause
{
	internal class PointerDownHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
	{
		internal Action onPointerDown;

		public void OnPointerDown(PointerEventData eventData)
		{
			onPointerDown?.Invoke();
		}
	}
}
