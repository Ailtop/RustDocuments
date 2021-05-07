using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class TeleportBehind : Behaviour
	{
		[UnityEditor.Subcomponent(typeof(Teleport))]
		[SerializeField]
		private Teleport _teleport;

		[SerializeField]
		private Transform _destinationTransform;

		[Information("Hide의 최소 시간 이하", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private float _destinationSettingDelay;

		[SerializeField]
		[MinMaxSlider(-10f, 10f)]
		private Vector2 _distance;

		[SerializeField]
		private bool _lastStandingCollider = true;

		[SerializeField]
		private LayerMask _groundMask = Layers.groundMask;

		public override IEnumerator CRun(AIController controller)
		{
			StartCoroutine(SetDestination(controller));
			yield return _teleport.CRun(controller);
		}

		private IEnumerator SetDestination(AIController controller)
		{
			Character target = controller.target;
			float amount = Random.Range(_distance.x, _distance.y);
			yield return controller.character.chronometer.master.WaitForSeconds(_destinationSettingDelay);
			float num = ((target.lookingDirection == Character.LookingDirection.Right) ? (amount * -1f) : amount);
			float num2 = target.transform.position.x + num;
			Collider2D collider;
			Bounds bounds = (_lastStandingCollider ? target.movement.controller.collisionState.lastStandingCollider.bounds : ((!target.movement.TryGetClosestBelowCollider(out collider, _groundMask)) ? controller.character.movement.controller.collisionState.lastStandingCollider.bounds : collider.bounds));
			if (num2 <= bounds.min.x + 0.5f && target.lookingDirection == Character.LookingDirection.Right)
			{
				num2 = bounds.min.x + 0.5f;
			}
			else if (num2 >= bounds.max.x - 0.5f && target.lookingDirection == Character.LookingDirection.Left)
			{
				num2 = bounds.max.x - 0.5f;
			}
			_destinationTransform.position = new Vector3(num2, bounds.max.y + controller.character.collider.size.y);
		}
	}
}
