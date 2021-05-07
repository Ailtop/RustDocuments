using System.Collections;
using Characters.Actions;
using Characters.AI.Adventurer;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Cleric
{
	public class Reinforce : Behaviour
	{
		[SerializeField]
		private Action _readyMotion;

		[SerializeField]
		private Action _loopMotion;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(AttachAbility))]
		private AttachAbility _attachAbility;

		private Commander _commander;

		private void Awake()
		{
			_commander = GetComponentInParent<Commander>();
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_readyMotion.TryStart();
			while (_readyMotion.running)
			{
				yield return null;
			}
			Character character = controller.character;
			AdventurerController randomOne = _commander.GetRandomOne(controller);
			if (!(randomOne == null))
			{
				_attachAbility.Run(randomOne.character);
				_loopMotion.TryStart();
				while (_loopMotion.running)
				{
					yield return null;
				}
				_attachAbility.Stop();
				base.result = Result.Done;
			}
		}
	}
}
