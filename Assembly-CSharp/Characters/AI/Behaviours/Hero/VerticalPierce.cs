using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public class VerticalPierce : SequentialCombo
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Action _readyAction;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Action _jumpAction;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Action _attackAction;

		public override IEnumerator CRun(AIController controller)
		{
			_readyAction.TryStart();
			while (_readyAction.running)
			{
				yield return null;
			}
			_jumpAction.TryStart();
			while (_jumpAction.running)
			{
				yield return null;
			}
			_attackAction.TryStart();
			while (_attackAction.running)
			{
				yield return null;
			}
		}
	}
}
