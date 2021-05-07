using Steamworks;
using UnityEngine;

namespace Achievements
{
	public static class Achievement
	{
		public enum Type
		{
			TheLegendBegins,
			RookieWelcome,
			Concentration,
			GoHome,
			DemonCastleRestoration,
			SavedElderEnt,
			SavedElderEnt_2,
			TheendofGoldManeKnights,
			TheendofGoldManeKnights_2,
			FakeGoddessGone,
			FakeGoddessGone_2,
			LeoniasFall,
			LeoniasFall_2,
			FirstHeroLastBattle,
			FirstHeroLastBattle_2,
			HeroSlayer,
			SkeletonKing,
			ColdBlood,
			DwarfChandelier,
			Manners,
			WhattoBuy,
			BrainPoolOperation,
			Chatterbox,
			StockManagement,
			Skeleton_Sword,
			Skeleton_Spear,
			Skeleton_Shield,
			EntSkul,
			WereWolf,
			Thief,
			Recruit,
			Gargoyle,
			Clown,
			Ghoul,
			GhostRider,
			Minotaurus,
			Mummy,
			GlacialSkull,
			BombSkul,
			AquaSkull,
			Alchemist,
			Warrior,
			Genie,
			Hunter,
			RockStar,
			Ninja,
			DarkPaladin,
			HighWarlock,
			LivingArmor,
			Berserker,
			Samurai,
			Predator,
			ArchLich,
			Yaksha,
			Fighter,
			GrimReaper,
			Gambler
		}

		public static bool GetAchievement(Type type)
		{
			if (!SteamManager.Initialized)
			{
				return false;
			}
			bool pbAchieved;
			if (!SteamUserStats.GetAchievement(type.ToString(), out pbAchieved))
			{
				return false;
			}
			return pbAchieved;
		}

		public static void SetAchievement(Type type)
		{
			Debug.Log($"Set Achievement : {type}");
			if (SteamManager.Initialized && !GetAchievement(type) && SteamUserStats.SetAchievement(type.ToString()))
			{
				SteamUserStats.StoreStats();
			}
		}
	}
}
