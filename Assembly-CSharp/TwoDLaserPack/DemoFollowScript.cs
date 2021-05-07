using System.Collections.Generic;
using UnityEngine;

namespace TwoDLaserPack
{
	public class DemoFollowScript : MonoBehaviour
	{
		public Transform target;

		public float speed;

		public bool shouldFollow;

		public bool isHomeAndShouldDeactivate;

		public bool movingToDeactivationTarget;

		private Vector3 newPosition;

		public List<Transform> acquiredTargets;

		private void Start()
		{
			isHomeAndShouldDeactivate = false;
			movingToDeactivationTarget = false;
			acquiredTargets = new List<Transform>();
			if (target == null)
			{
				Debug.Log("No target found for the FollowScript on: " + base.gameObject.name);
			}
		}

		private void OnEnable()
		{
		}

		private void Update()
		{
			if (shouldFollow && target != null)
			{
				newPosition = Vector2.Lerp(base.transform.position, target.position, Time.deltaTime * speed);
				base.transform.position = new Vector3(newPosition.x, newPosition.y, base.transform.position.z);
			}
		}

		private void OnDisable()
		{
		}
	}
}
