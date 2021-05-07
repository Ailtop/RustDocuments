using InControl;
using Steamworks;
using UnityEngine;

namespace Services
{
	public class Steam : MonoBehaviour
	{
		public void Initialize()
		{
			if (SteamManager.Initialized)
			{
				Debug.Log("Steam connected : " + SteamFriends.GetPersonaName());
				Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
			}
			else
			{
				Application.Quit();
			}
		}

		private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
		{
			if (pCallback.m_bActive != 0)
			{
				Debug.Log("Steam Overlay has been activated");
				Chronometer.global.AttachTimeScale(this, 0f);
				InputManager.Enabled = false;
			}
			else
			{
				Debug.Log("Steam Overlay has been closed");
				Chronometer.global.DetachTimeScale(this);
				InputManager.Enabled = true;
			}
		}
	}
}
