using Characters.Player;
using FX;
using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.Shots.Events
{
	public class PlayAdventurerMusic : Event
	{
		[SerializeField]
		private MusicInfo _musicInfo;

		[SerializeField]
		private MusicInfo _rockstarMusicInfo;

		public override void Run()
		{
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
