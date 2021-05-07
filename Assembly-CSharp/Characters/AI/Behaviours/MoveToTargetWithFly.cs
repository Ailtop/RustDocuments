using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class MoveToTargetWithFly : Move
	{
		public enum RotateMethod
		{
			Constant,
			Lerp,
			Slerp
		}

		[SerializeField]
		private RotateMethod _rotateMethod;

		[SerializeField]
		private float _rotateSpeed = 2f;

		private Quaternion _rotation;

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			Character target = controller.target;
			base.result = Result.Doing;
			while (base.result == Result.Doing)
			{
				if ((bool)controller.FindClosestPlayerBody(controller.stopTrigger))
				{
					base.result = Result.Fail;
					break;
				}
				if (controller.target == null)
				{
					base.result = Result.Fail;
					break;
				}
				yield return null;
				Vector3 vector = target.collider.bounds.center - character.collider.bounds.center;
				if (vector.magnitude < 0.1f || LookAround(controller))
				{
					yield return idle.CRun(controller);
					base.result = Result.Success;
					break;
				}
				float angle = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				switch (_rotateMethod)
				{
				case RotateMethod.Constant:
					_rotation = Quaternion.RotateTowards(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), _rotateSpeed * 100f * Time.deltaTime);
					break;
				case RotateMethod.Lerp:
					_rotation = Quaternion.Lerp(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), _rotateSpeed * Time.deltaTime);
					break;
				case RotateMethod.Slerp:
					_rotation = Quaternion.Slerp(_rotation, Quaternion.AngleAxis(angle, Vector3.forward), _rotateSpeed * Time.deltaTime);
					break;
				}
				Vector3 eulerAngle = _rotation.eulerAngles;
				Vector3 vector2 = _rotation * Vector2.right;
				controller.character.movement.move = vector2;
			}
		}
	}
}
