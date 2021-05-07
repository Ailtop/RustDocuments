using UnityEngine;

namespace FX.Connections
{
	public class SimpleConnector : MonoBehaviour
	{
		[SerializeField]
		private Connection _connection;

		[SerializeField]
		private Transform _start;

		[SerializeField]
		private Vector2 _startOffset;

		[SerializeField]
		private Transform _end;

		[SerializeField]
		private Vector2 _endOffset;

		private void Awake()
		{
			_connection.Connect(_start, _startOffset, _end, _endOffset);
		}
	}
}
