using UnityEngine;

namespace Characters.Operations
{
	public class TakeAimObject : CharacterOperation
	{
		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Transform _centerAxisPosition;

		[SerializeField]
		private bool _platformTarget;

		public override void Run(Character owner)
		{
			float y;
			if (_platformTarget)
			{
				Collider2D collider2D = FindTargetPlatform();
				if (collider2D == null)
				{
					return;
				}
				y = collider2D.bounds.max.y;
			}
			else
			{
				y = _target.transform.position.y;
			}
			Vector3 vector = new Vector3(_target.transform.position.x, y) - _centerAxisPosition.transform.position;
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			_centerAxisPosition.rotation = Quaternion.Euler(0f, 0f, z);
		}

		private Collider2D FindTargetPlatform()
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(_target.position, Vector2.down, float.PositiveInfinity, Layers.groundMask);
			if ((bool)raycastHit2D)
			{
				return raycastHit2D.collider;
			}
			return null;
		}
	}
}
