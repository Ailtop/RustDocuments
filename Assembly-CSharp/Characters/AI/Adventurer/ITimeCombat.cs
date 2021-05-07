using System.Collections;

namespace Characters.AI.Adventurer
{
	public interface ITimeCombat
	{
		bool terminated { get; }

		IEnumerator CheckCombatTime();
	}
}
