using System.Collections.Generic;
using UnityEngine;

namespace FX.Connections
{
	public class ConnectionPool : MonoBehaviour
	{
		private Queue<Connection> _pool = new Queue<Connection>();

		private List<Connection> _connectings = new List<Connection>();

		private void Awake()
		{
			Initialize();
		}

		private void OnDisable()
		{
			DisconnectAll();
		}

		private void Initialize()
		{
			Connection[] componentsInChildren = GetComponentsInChildren<Connection>(true);
			foreach (Connection connect in componentsInChildren)
			{
				_pool.Enqueue(connect);
				connect.OnConnect += delegate
				{
					_connectings.Add(connect);
				};
				connect.OnDisconnect += delegate
				{
					_connectings.Remove(connect);
					_pool.Enqueue(connect);
				};
			}
		}

		public Connection GetConnection()
		{
			if (_pool.Count == 0)
			{
				DisconnectFirstConnecting();
			}
			return _pool.Dequeue();
		}

		private void DisconnectFirstConnecting()
		{
			_connectings[0].Disconnect();
		}

		private void DisconnectAll()
		{
			while (_connectings.Count > 0)
			{
				DisconnectFirstConnecting();
			}
		}
	}
}
