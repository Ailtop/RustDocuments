using FX;
using Singletons;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class PlaySoundOnSelected : EventTrigger
	{
		private static GameObject _lastSelected;

		public SoundInfo soundInfo;

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (!(_lastSelected == eventData.selectedObject))
			{
				_lastSelected = eventData.selectedObject;
				PersistentSingleton<SoundManager>.Instance.PlaySound(soundInfo, Vector3.zero);
			}
		}
	}
}
