using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Dash : Behaviour
	{
		[SerializeField]
		[MinMaxSlider(0f, 20f)]
		private Vector2 _minMaxDistance;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _moveToDestination;

		[SerializeField]
		private Action _motion;

		protected void Start()
		{
			_childs = new List<Behaviour> { _moveToDestination };
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character target = controller.target;
			Character character = controller.character;
			float num = Mathf.Abs(character.transform.position.x - target.transform.position.x);
			float x = _minMaxDistance.x;
			float y = _minMaxDistance.y;
			_motion.TryStart();
			base.result = Result.Doing;
			if (num >= x && num < y)
			{
				controller.destination = target.transform.position;
				yield return _moveToDestination.CRun(controller);
			}
			else if (num >= y)
			{
				if (character.transform.position.x < target.transform.position.x)
				{
					controller.destination = new Vector2(character.transform.position.x + y, character.transform.position.y);
				}
				else
				{
					controller.destination = new Vector2(character.transform.position.x - y, character.transform.position.y);
				}
				yield return _moveToDestination.CRun(controller);
			}
			character.CancelAction();
			base.result = Result.Done;
		}
	}
}
