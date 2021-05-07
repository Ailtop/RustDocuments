using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace Achievements.Tracker
{
	public class BossClearTracker : MonoBehaviour
	{
		[SerializeField]
		private Achievement.Type _normalAchievement;

		[SerializeField]
		private Achievement.Type _perfectAchievement;

		[SerializeField]
		private Character _boss;

		private bool _tookDamage;

		private void Start()
		{
			Singleton<Service>.Instance.levelManager.player.health.onTookDamage += OnTookDamage;
			_boss.health.onDied += OnTargetDied;
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Singleton<Service>.Instance.levelManager.player.health.onTookDamage -= OnTookDamage;
				_boss.health.onDied -= OnTargetDied;
			}
		}

		private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (tookDamage.attackType != 0 && !(damageDealt < 1.0))
			{
				_tookDamage = true;
				Singleton<Service>.Instance.levelManager.player.health.onTookDamage -= OnTookDamage;
			}
		}

		private void OnTargetDied()
		{
			_boss.health.onDied -= OnTargetDied;
			Achievement.SetAchievement(_normalAchievement);
			if (!_tookDamage)
			{
				Achievement.SetAchievement(_perfectAchievement);
			}
		}
	}
}
