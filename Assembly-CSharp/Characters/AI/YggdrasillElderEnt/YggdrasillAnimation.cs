using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class YggdrasillAnimation : MonoBehaviour
	{
		public enum Tag
		{
			P1_Appearance = 0,
			P1_Dead = 1,
			P1_EnergyBomb = 2,
			P1_FistSlam = 3,
			P1_FistSlam_Intro = 4,
			P1_FistSlam_Outro = 5,
			P1_Groggy = 6,
			P1_Idle = 7,
			P1_Idle_CutScene = 8,
			P1_Laser = 9,
			P1_Laser_Intro = 10,
			P1_Sleep = 11,
			P1_Sweeping_Intro = 12,
			P1_Sweeping_Left = 13,
			P1_Sweeping_Right = 14,
			P1_Sweeping_Outro = 0xF,
			P2_Awakening = 100,
			P2_BothFistPowerSlam = 101,
			P2_BothFistPowerSlam_Intro = 102,
			P2_BothFistPowerSlam_Outro = 103,
			P2_EnergyCorps = 104,
			P2_EnergyCorps_Intro = 105,
			P2_FistPowerSlam = 106,
			P2_FistPowerSlam_Intro = 107,
			P2_FistPowerSlam_Outro = 108,
			P2_Groggy = 109,
			P2_Idle = 110,
			P2_SweepingCombo_Intro = 111,
			P2_SweepingComob_Left = 112,
			P2_SweepingComob_Right = 113,
			P2_SweepingComob_Outro = 114
		}

		[SerializeField]
		private Tag _tag;

		[SerializeField]
		private CharacterAnimationController.AnimationInfo _info;

		public Tag tag => _tag;

		public CharacterAnimationController.AnimationInfo info => _info;
	}
}
