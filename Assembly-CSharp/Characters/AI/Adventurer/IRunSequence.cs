using System.Collections;

namespace Characters.AI.Adventurer
{
	public interface IRunSequence
	{
		IEnumerator CRunSequence(Strategy strategy);
	}
}
