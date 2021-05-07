using System;
using Achievements;
using Data;
using UnityEngine;

namespace Characters.Player
{
	public class PlayerKillCounter : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		private void Awake()
		{
			Character character = _character;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character.onKilled, new Character.OnKilledDelegate(CountKill));
		}

		private void CountKill(ITarget target, ref Damage damage)
		{
			if (target.character == null)
			{
				return;
			}
			switch (target.character.type)
			{
			case Character.Type.TrashMob:
			case Character.Type.Boss:
				GameData.Progress.kills++;
				break;
			case Character.Type.Adventurer:
				GameData.Progress.kills++;
				GameData.Progress.eliteKills++;
				GameData.Progress.totalAdventurerKills++;
				if (GameData.Progress.totalAdventurerKills >= 100)
				{
					Achievement.SetAchievement(Achievement.Type.HeroSlayer);
				}
				break;
			case Character.Type.Named:
				break;
			}
		}
	}
}
