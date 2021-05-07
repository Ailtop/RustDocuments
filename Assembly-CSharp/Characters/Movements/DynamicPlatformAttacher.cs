using Level;
using UnityEngine;

namespace Characters.Movements
{
	[RequireComponent(typeof(CharacterController2D))]
	public class DynamicPlatformAttacher : MonoBehaviour
	{
		private CharacterController2D _controller;

		private void Awake()
		{
			_controller = GetComponent<CharacterController2D>();
			_controller.collisionState.belowCollisionDetector.OnEnter += OnBelowCollisionEnter;
			_controller.collisionState.belowCollisionDetector.OnExit += OnBelowCollisionExit;
		}

		private void OnDestroy()
		{
			_controller.collisionState.belowCollisionDetector.OnEnter -= OnBelowCollisionEnter;
			_controller.collisionState.belowCollisionDetector.OnExit -= OnBelowCollisionExit;
		}

		private void OnBelowCollisionEnter(RaycastHit2D hit)
		{
			DynamicPlatform component = hit.collider.GetComponent<DynamicPlatform>();
			if (!(component == null))
			{
				component.Attach(_controller);
			}
		}

		private void OnBelowCollisionExit(RaycastHit2D hit)
		{
			if (!(hit.collider == null))
			{
				DynamicPlatform component = hit.collider.GetComponent<DynamicPlatform>();
				if (!(component == null))
				{
					component.Detach(_controller);
				}
			}
		}
	}
}
