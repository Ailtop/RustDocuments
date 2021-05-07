using System.Collections.Generic;
using System.Linq;
using Characters;
using Level;
using UnityEngine;

namespace Runnables
{
	public class KillAllEnemy : Runnable
	{
		[SerializeField]
		private List<Character> _excepts;

		public override void Run()
		{
			(from enemy in Map.Instance.waveContainer.GetAllEnemies()
				where !_excepts.Contains(enemy)
				select enemy).ToList().ForEach(delegate(Character enemy)
			{
				enemy.health.Kill();
			});
		}
	}
}
