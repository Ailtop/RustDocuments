using System.Collections;

namespace Characters.AI.Hero
{
	public interface IComboable
	{
		IEnumerator CTryContinuedCombo(AIController controller, ComboSystem comboSystem);
	}
}
