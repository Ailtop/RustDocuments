using UnityEngine;

namespace Level
{
	public class Block : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Collider2D _collider2D;

		public void Activate()
		{
			_collider2D.enabled = true;
		}

		public void Deactivate()
		{
			_collider2D.enabled = false;
		}
	}
}
