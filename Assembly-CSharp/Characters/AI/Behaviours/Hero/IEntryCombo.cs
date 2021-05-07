using System.Collections;
using Characters.AI.Hero;

namespace Characters.AI.Behaviours.Hero
{
	public interface IEntryCombo
	{
		IEnumerator CTryEntryCombo(AIController controller, ComboSystem comboSystem);
	}
}
