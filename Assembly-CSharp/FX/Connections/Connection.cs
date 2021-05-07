using System;
using UnityEngine;

namespace FX.Connections
{
	public abstract class Connection : MonoBehaviour
	{
		private Transform _start;

		private Transform _end;

		private Vector3 _startOffset = new Vector3(0f, 0f);

		private Vector3 _endOffset = new Vector3(0f, 0f);

		protected Vector3 startPosition => _start.position + _startOffset;

		protected Vector3 endPosition => _end.position + _endOffset;

		public bool connecting { get; private set; }

		public virtual bool lostConnection
		{
			get
			{
				if (!(_start == null))
				{
					return _end == null;
				}
				return true;
			}
		}

		public event Action OnConnect;

		public event Action OnDisconnect;

		public void Connect(Transform start, Vector2 startOffset, Transform end, Vector2 endOffset)
		{
			connecting = true;
			_start = start;
			_end = end;
			_startOffset = startOffset;
			_endOffset = endOffset;
			Show();
			this.OnConnect?.Invoke();
		}

		public void Disconnect()
		{
			connecting = false;
			Hide();
			this.OnDisconnect?.Invoke();
		}

		protected abstract void Show();

		protected abstract void Hide();
	}
}
