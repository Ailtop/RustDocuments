using UnityEngine;

namespace Level
{
	public class PositionCache : MonoBehaviour
	{
		[SerializeField]
		private Transform _transform;

		private Vector2 _position;

		private void Awake()
		{
			if (_transform == null)
			{
				_transform = base.transform;
			}
		}

		public Vector2 Load()
		{
			return _position;
		}

		public void Save()
		{
			_position = _transform.position;
		}
	}
}
