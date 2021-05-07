using System.Collections;
using Services;
using Singletons;

namespace Characters.AI.Adventurer
{
	public class Combat : ITimeCombat
	{
		private float _expireTime = 30f;

		public Commander commander { get; set; }

		public AdventurerController who { get; set; }

		public bool terminated { get; private set; }

		public bool expired { get; set; }

		public Combat(Commander commander, AdventurerController controller)
		{
			who = controller;
			this.commander = commander;
		}

		public IEnumerator CheckCombatTime()
		{
			expired = false;
			yield return Chronometer.global.WaitForSeconds(_expireTime);
			expired = true;
		}

		public IEnumerator CProcess(Strategy strategy)
		{
			terminated = false;
			while (!expired && !terminated && !who.dead)
			{
				yield return who.CRunSequence(strategy);
			}
			terminated = true;
		}

		public void SetTargetToPlayer()
		{
			who.target = Singleton<Service>.Instance.levelManager.player;
		}
	}
}
