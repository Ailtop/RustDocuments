using System.Collections;
using System.Collections.Generic;
using Characters.Gear.Weapons;
using FX.Connections;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class ConnectToTarget : TargetedCharacterOperation
	{
		private class ConnectionCoroutine
		{
			public Character target { get; private set; }

			public Connection connection { get; private set; }

			public Coroutine coroutine { get; private set; }

			public ConnectionCoroutine(MonoBehaviour coroutineReference, Character target, Connection connection, float duration)
			{
				this.target = target;
				this.connection = connection;
				coroutine = coroutineReference.StartCoroutine(CRun(target.chronometer.master, duration));
			}

			private IEnumerator CRun(Chronometer chronometer, float duration)
			{
				float elapsed = 0f;
				while (elapsed < duration && connection.connecting && target.liveAndActive)
				{
					elapsed += chronometer.deltaTime;
					yield return null;
				}
				if (connection.connecting)
				{
					connection.Disconnect();
				}
			}
		}

		[SerializeField]
		[GetComponentInParent(false)]
		private Weapon _weapon;

		[SerializeField]
		private ConnectionPool _connectionPool;

		[SerializeField]
		private float _duration;

		private List<ConnectionCoroutine> _connectings = new List<ConnectionCoroutine>();

		private MonoBehaviour coroutineReference => _connectionPool;

		private void OnDisable()
		{
			_connectings.Clear();
		}

		public override void Run(Character owner, Character target)
		{
			DisconnectIfConnected(target);
			Connection connection = _connectionPool.GetConnection();
			Vector2 endOffset = new Vector2(0f, owner.collider.bounds.size.y * 0.5f);
			Vector2 startOffset = new Vector2(0f, target.collider.bounds.size.y * 0.5f);
			connection.Connect(target.transform, startOffset, owner.transform, endOffset);
			ConnectionCoroutine connectionCoroutine = new ConnectionCoroutine(coroutineReference, target, connection, _duration);
			AddDisconnectAction(connectionCoroutine);
			_connectings.Add(connectionCoroutine);
		}

		private void DisconnectIfConnected(Character target)
		{
			ConnectionCoroutine result;
			if (TryGetConnectionCoroutine(target, out result))
			{
				result.connection.Disconnect();
			}
		}

		private void AddDisconnectAction(ConnectionCoroutine connectionCoroutine)
		{
			_003C_003Ec__DisplayClass10_0 _003C_003Ec__DisplayClass10_ = new _003C_003Ec__DisplayClass10_0();
			_003C_003Ec__DisplayClass10_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass10_.connectionCoroutine = connectionCoroutine;
			_003C_003Ec__DisplayClass10_.connectionCoroutine.connection.OnDisconnect += _003C_003Ec__DisplayClass10_._003CAddDisconnectAction_003Eg__OnDisconnect_007C0;
		}

		private bool TryGetConnectionCoroutine(Character target, out ConnectionCoroutine result)
		{
			foreach (ConnectionCoroutine connecting in _connectings)
			{
				if (connecting.target == target)
				{
					result = connecting;
					return true;
				}
			}
			result = null;
			return false;
		}
	}
}
