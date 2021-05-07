using Characters;
using Scenes;
using UI.GearPopup;
using UnityEngine;

namespace Level.BlackMarket
{
	public class ChefsFoodDisplay : DroppedGear
	{
		private ChefsFood _dish;

		public void Initialize(ChefsFood dish)
		{
			_dish = dish;
		}

		public override void OpenPopupBy(Character character)
		{
			Vector3 position = base.transform.position;
			Vector3 position2 = character.transform.position;
			position.x = position2.x + ((position.x > position2.x) ? _popupUIOffset.x : (0f - _popupUIOffset.x));
			position.y += _popupUIOffset.y;
			GearPopupCanvas gearPopupCanvas = Scene<GameBase>.instance.uiManager.gearPopupCanvas;
			gearPopupCanvas.gearPopup.Set(_dish.displayName, _dish.description);
			gearPopupCanvas.gearPopup.SetInteractionLabel(this);
			gearPopupCanvas.Open(position);
		}

		public override void ClosePopup()
		{
			base.ClosePopup();
			Scene<GameBase>.instance.uiManager.gearPopupCanvas.Close();
		}

		public void Initialize()
		{
		}
	}
}
