using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace Runnables
{
	public sealed class TakeHealth : Runnable
	{
		[SerializeField]
		private Target _target;

		[SerializeField]
		private CustomFloat _amount;

		public override void Run()
		{
			Character character = _target.character;
			float value = _amount.value;
			Singleton<Service>.Instance.floatingTextSpawner.SpawnPlayerTakingDamage(value, character.transform.position);
			character.health.TakeHealth(value);
		}
	}
}
