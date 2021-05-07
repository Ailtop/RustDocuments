using System.Collections;

namespace Characters.AI.Pope
{
	public interface ISequence
	{
		IEnumerator CRun(AIController controller);
	}
}
