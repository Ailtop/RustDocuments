using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public sealed class Landing : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Action _jump;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Action _action;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Action _end;

		public override IEnumerator CRun(AIController controller)
		{
			_jump.TryStart();
			while (_jump.running)
			{
				yield return null;
			}
			_action.TryStart();
			while (_action.running)
			{
				yield return null;
			}
			_end.TryStart();
			while (_end.running)
			{
				yield return null;
			}
		}
	}
}
