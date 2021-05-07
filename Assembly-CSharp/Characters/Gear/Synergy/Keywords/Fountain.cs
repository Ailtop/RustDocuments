using System.Collections;
using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Fountain : Keyword
	{
		[SerializeField]
		private SoundInfo _healSound;

		[SerializeField]
		private double[] _healPercentByLevel = new double[4] { 0.0, 0.05, 0.1, 0.2 };

		public override Key key => Key.Fountain;

		protected override IList valuesByLevel => _healPercentByLevel;

		protected override void Initialize()
		{
		}

		protected override void UpdateBonus()
		{
		}

		protected override void OnAttach()
		{
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn += OnMapLoadedAndFadedIn;
		}

		protected override void OnDetach()
		{
			if (!Service.quitting)
			{
				Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn -= OnMapLoadedAndFadedIn;
			}
		}

		private void OnMapLoadedAndFadedIn()
		{
			double amount = (base.character.health.maximumHealth - base.character.health.currentHealth) * _healPercentByLevel[base.level] * 0.01;
			if (amount < 1.0)
			{
				amount = 1.0;
			}
			base.character.health.Heal(ref amount);
			if (!(amount < 1.0))
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_healSound, base.character.transform.position);
			}
		}
	}
}
