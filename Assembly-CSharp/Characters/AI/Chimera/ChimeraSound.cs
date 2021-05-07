using FX;
using Singletons;
using UnityEngine;

namespace Characters.AI.Chimera
{
	public class ChimeraSound : MonoBehaviour
	{
		[Header("Intro")]
		[Space]
		[SerializeField]
		private SoundInfo _impactSound;

		[Header("Stomp")]
		[Space]
		[SerializeField]
		private SoundInfo _fistSlamSound;

		[SerializeField]
		private SoundInfo _fistSlamReadySound;

		[Header("Bite")]
		[Space]
		[SerializeField]
		private SoundInfo _sweepingReadySound;

		[SerializeField]
		private SoundInfo _sweepingSound;

		[Header("VenomBall")]
		[Space]
		[SerializeField]
		private SoundInfo _energyBombReadySound;

		[SerializeField]
		private SoundInfo _energyBombSound;

		[Header("VenomCannon")]
		[Space]
		[SerializeField]
		private SoundInfo _groggySound;

		[SerializeField]
		private SoundInfo _groundImpactSound;

		[Header("VenomBreath")]
		[Space]
		[SerializeField]
		private SoundInfo _venomBreathSound;

		[Header("Roar")]
		[Space]
		[SerializeField]
		private SoundInfo _roarSound;

		[Header("In")]
		[Space]
		[SerializeField]
		private SoundInfo _inSound;

		[Header("Out")]
		[Space]
		[SerializeField]
		private SoundInfo _outSound;

		[Header("BigStomp")]
		[Space]
		[SerializeField]
		private SoundInfo _bicStompSound;

		[Header("Outro")]
		[Space]
		[SerializeField]
		private SoundInfo _dieSound;

		[SerializeField]
		private SoundInfo _dieShoutSound;

		public void PlayRoarSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_roarSound, base.transform.position);
		}

		public void PlayImpactSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_impactSound, base.transform.position);
		}

		public void PlaySlamReadySound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlamReadySound, base.transform.position);
		}

		public void PlaySlamSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_fistSlamSound, base.transform.position);
		}

		public void PlaySweepReadySound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sweepingReadySound, base.transform.position);
		}

		public void PlaySweepAttackSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_sweepingSound, base.transform.position);
		}

		public void PlayEnergyBombSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_energyBombSound, base.transform.position);
		}

		public void PlayEnergyBombReadySound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_energyBombReadySound, base.transform.position);
		}

		public void PlayGroggyStartSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_groggySound, base.transform.position);
		}

		public void PlayGroggyOnGroundSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_groundImpactSound, base.transform.position);
		}

		public void PlayDieSound()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_dieSound, base.transform.position);
			PersistentSingleton<SoundManager>.Instance.PlaySound(_dieShoutSound, base.transform.position);
		}
	}
}
