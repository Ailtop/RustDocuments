using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class TeleportInRangeWithFly : Behaviour
	{
		[SerializeField]
		private Action _teleportStart;

		[SerializeField]
		private Action _teleportEnd;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Hide))]
		private Hide _hide;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Idle))]
		private Idle _idle;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _distance;

		public override IEnumerator CRun(AIController controller)
		{
			Vector3 amount = Random.insideUnitSphere;
			float num = Random.Range(_distance.x, _distance.y);
			amount *= num;
			base.result = Result.Doing;
			float num2 = controller.target.transform.position.x - controller.character.transform.position.x;
			controller.character.lookingDirection = ((!(num2 > 0f)) ? Character.LookingDirection.Left : Character.LookingDirection.Right);
			if (_teleportStart.TryStart())
			{
				while (_teleportStart.running)
				{
					yield return null;
				}
				yield return _hide.CRun(controller);
				controller.character.transform.position = controller.target.transform.position + amount;
				_teleportEnd.TryStart();
				while (_teleportEnd.running)
				{
					yield return null;
				}
				yield return _idle.CRun(controller);
				base.result = Result.Success;
			}
			else
			{
				base.result = Result.Fail;
			}
		}
	}
}
