using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;
using Characters.Gear.Weapons;
using CutScenes;
using Data;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Achievements.Tracker
{
	public class PlayerAchievementTracker : MonoBehaviour
	{
		[SerializeField]
		private Character _player;

		private void Awake()
		{
			_player.health.onDied += OnPlayerDied;
			_player.health.onTookDamage += OnTookDamage;
			Singleton<Service>.Instance.levelManager.onMapChangedAndFadedIn += TrackMapAchievement;
			_player.playerComponents.inventory.weapon.onChanged += TrackHeadLootAchievement;
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Singleton<Service>.Instance.levelManager.onMapChangedAndFadedIn -= TrackMapAchievement;
			}
		}

		private void OnPlayerDied()
		{
			Achievement.SetAchievement(Achievement.Type.TheLegendBegins);
		}

		private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (!(_player.health.currentHealth > 0.0))
			{
				bool flag = tookDamage.attacker.trap != null;
				if (tookDamage.attacker.character != null && tookDamage.attacker.character.type == Character.Type.Trap)
				{
					flag = true;
				}
				if (flag)
				{
					Achievement.SetAchievement(Achievement.Type.Concentration);
				}
			}
		}

		private void TrackMapAchievement(Map old, Map @new)
		{
			if (Singleton<Service>.Instance.levelManager.currentChapter.type == Chapter.Type.Castle && GameData.Progress.cutscene.GetData(CutScenes.Key.ending))
			{
				Achievement.SetAchievement(Achievement.Type.GoHome);
			}
		}

		private void TrackHeadLootAchievement(Weapon old, Weapon @new)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			if (@new == null || (int)@new.rarity != 3)
			{
				return;
			}
			string[] names = EnumValues<Achievement.Type>.Names;
			int num = -1;
			for (int i = 0; i < names.Length; i++)
			{
				if (@new.name.IndexOf(names[i], StringComparison.OrdinalIgnoreCase) >= 0)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				Debug.Log("There is no achievement for Legendary Head " + @new.name + ".");
			}
			else
			{
				Achievement.SetAchievement(EnumValues<Achievement.Type>.Values[num]);
			}
		}
	}
}
