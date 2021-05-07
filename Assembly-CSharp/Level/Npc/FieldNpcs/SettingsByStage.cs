using System;
using UnityEngine;

namespace Level.Npc.FieldNpcs
{
	[Serializable]
	public class SettingsByStage
	{
		[Header("DarkPriest")]
		[SerializeField]
		private int[] _darkPriestGoldCosts;

		[Header("SleepySkeleton")]
		[SerializeField]
		[Range(0f, 100f)]
		private int _sleepySekeletonHealthPercentCost;

		[SerializeField]
		private RarityPossibilities _sleepySekeletonHeadPossibilities;

		[Header("Plebby")]
		[SerializeField]
		private int _plebbyGoldCost;

		[SerializeField]
		private RarityPossibilities _plebbyItemPossibilities;

		[Header("Harpy Warrior")]
		[SerializeField]
		private int _harpyWarriorBones;

		public int[] darkPriestGoldCosts => _darkPriestGoldCosts;

		public int sleepySekeletonHealthPercentCost => _sleepySekeletonHealthPercentCost;

		public RarityPossibilities sleepySekeletonHeadPossibilities => _sleepySekeletonHeadPossibilities;

		public int plebbyGoldCost => _plebbyGoldCost;

		public RarityPossibilities plebbyItemPossibilities => _plebbyItemPossibilities;

		public int harpyWarriorBones => _harpyWarriorBones;
	}
}
