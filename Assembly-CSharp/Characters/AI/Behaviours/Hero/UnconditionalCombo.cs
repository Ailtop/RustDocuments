using System.Collections;
using Characters.Actions;
using Characters.AI.Hero;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public abstract class UnconditionalCombo : Behaviour, IComboable, IEntryCombo
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Action _startAction;

		public IEnumerator CTryContinuedCombo(AIController controller, ComboSystem comboSystem)
		{
			yield return CRun(controller);
			yield return comboSystem.CNext(controller);
		}

		public IEnumerator CTryEntryCombo(AIController controller, ComboSystem comboSystem)
		{
			_startAction.TryStart();
			while (_startAction.running)
			{
				yield return null;
			}
			comboSystem.Start();
			yield return CTryContinuedCombo(controller, comboSystem);
		}
	}
}
