using Characters.Player;
using CutScenes;
using Data;
using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Level.Adventurer
{
	public class AdventurerSoundController : MonoBehaviour
	{
		[SerializeField]
		private MusicInfo _musicInfo;

		[SerializeField]
		private MusicInfo _rockstarMusicInfo;

		[SerializeField]
		[GetComponent]
		private Collider2D _trigger;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			PlaySound();
			_trigger.enabled = false;
		}

		private void PlaySound()
		{
			if (!GameData.Progress.cutscene.GetData(Key.rookieHero))
			{
				PersistentSingleton<SoundManager>.Instance.StopBackGroundMusic();
				return;
			}
			WeaponInventory weapon = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon;
			if (weapon.Has("RockStar") || weapon.Has("RockStar_2"))
			{
				PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(_rockstarMusicInfo);
			}
			else
			{
				PersistentSingleton<SoundManager>.Instance.PlayBackgroundMusic(_musicInfo);
			}
		}
	}
}
