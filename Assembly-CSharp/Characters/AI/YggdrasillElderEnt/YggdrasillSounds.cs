using FX;
using Singletons;
using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class YggdrasillSounds : MonoBehaviour
	{
		[Header("Intro")]
		[Space]
		[SerializeField]
		private SoundInfo _appearance_Sign;

		[SerializeField]
		private SoundInfo _appearance_Impact;

		[SerializeField]
		private SoundInfo _appearance_Appearance;

		[SerializeField]
		private SoundInfo _appearance_Roar;

		[Header("Fist Slam")]
		[Space]
		[SerializeField]
		private SoundInfo _fistSlam_Intro1;

		[SerializeField]
		private SoundInfo _fistSlam_Intro2;

		[SerializeField]
		private SoundInfo _fistSlam_Sign;

		[SerializeField]
		private SoundInfo _fistSlam_Impact1;

		[SerializeField]
		private SoundInfo _fistSlam_Impact2;

		[SerializeField]
		private SoundInfo _fistSlam_Recovery_Sign;

		[SerializeField]
		private SoundInfo _fistSlam_Recovery;

		[SerializeField]
		private SoundInfo _fistSlam_Outro;

		[Header("Sweep")]
		[Space]
		[SerializeField]
		private SoundInfo _sweeping_Intro1;

		[SerializeField]
		private SoundInfo _sweeping_Intro2;

		[SerializeField]
		private SoundInfo _sweeping_Ready;

		[SerializeField]
		private SoundInfo _sweeping_Sweeping1;

		[SerializeField]
		private SoundInfo _sweeping_Sweeping2;

		[SerializeField]
		private SoundInfo _sweeping_Outro;

		[Header("EnergyBomb")]
		[Space]
		[SerializeField]
		private SoundInfo _laser_Intro_Impact;

		[SerializeField]
		private SoundInfo _laser_Sign;

		[SerializeField]
		private SoundInfo _energyBomb_Fire;

		[Header("Groggy")]
		[Space]
		[SerializeField]
		private SoundInfo _groggy_Intro;

		[SerializeField]
		private SoundInfo _groggy_groggy;

		[SerializeField]
		private SoundInfo _groggy_impact;

		[SerializeField]
		private SoundInfo _groggy_recovery;

		[Header("Dead")]
		[Space]
		[SerializeField]
		private SoundInfo _dead_Intro;

		[SerializeField]
		private SoundInfo _dead_DarkQuartz_Intro;

		[SerializeField]
		private SoundInfo _dead_DarkQuartz_Explosion;

		[SerializeField]
		private SoundInfo _dead_Normalize;

		public void PlaySignSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_appearance_Sign, base.transform.position);
		}

		public void PlayImpactSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_appearance_Impact, base.transform.position);
		}

		public void PlayAppearanceSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_appearance_Appearance, base.transform.position);
		}

		public void PlayRoarSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_appearance_Roar, base.transform.position);
		}

		public void PlaySlamIntroSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlam_Intro1, base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlam_Intro2, base.transform.position);
		}

		public void PlaySlamSignSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlam_Sign, base.transform.position);
		}

		public void PlaySlamImpactSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlam_Impact1, base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlam_Impact2, base.transform.position);
		}

		public void PlaySlamRecoverySignSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlam_Recovery_Sign, base.transform.position);
		}

		public void PlaySlamRecoverySound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlam_Recovery, base.transform.position);
		}

		public void PlaySlamOutroSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlam_Outro, base.transform.position);
		}

		public void PlaySweepingIntroSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sweeping_Intro1, base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sweeping_Intro2, base.transform.position);
		}

		public void PlaySweepingReadySound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sweeping_Ready, base.transform.position);
		}

		public void PlaySweepingSweepingSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sweeping_Sweeping1, base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sweeping_Sweeping2, base.transform.position);
		}

		public void PlaySweepingOutroSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sweeping_Outro, base.transform.position);
		}

		public void PlayEnergyBombImpactSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_laser_Intro_Impact, base.transform.position);
		}

		public void PlayEnergyBombSignSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_laser_Sign, base.transform.position);
		}

		public void PlayEnergyBombFireSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_energyBomb_Fire, base.transform.position);
		}

		public void PlayGroggyIntroSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_groggy_Intro, base.transform.position);
		}

		public void PlayGroggyGroggySound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_groggy_groggy, base.transform.position);
		}

		public void PlayGroggyImpctSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_groggy_impact, base.transform.position);
		}

		public void PlayGroggyRecoverySound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_groggy_recovery, base.transform.position);
		}

		public void PlayDeadIntroSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_dead_Intro, base.transform.position);
		}

		public void PlayDeadDarkQuartzIntroSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_dead_DarkQuartz_Intro, base.transform.position);
		}

		public void PlayDeadDarkQuartzExplosionSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_dead_DarkQuartz_Explosion, base.transform.position);
		}

		public void PlayDeadNormalizeSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_dead_Normalize, base.transform.position);
		}
	}
}
