using Facepunch;
using System;
using System.Collections.Generic;

namespace CompanionServer
{
	public class SubscriberList<TKey, TTarget, TMessage> where TKey : IEquatable<TKey> where TTarget : class
	{
		private readonly object _syncRoot;

		private readonly Dictionary<TKey, HashSet<TTarget>> _subscriptions;

		private readonly IBroadcastSender<TTarget, TMessage> _sender;

		public SubscriberList(IBroadcastSender<TTarget, TMessage> sender)
		{
			_syncRoot = new object();
			_subscriptions = new Dictionary<TKey, HashSet<TTarget>>();
			_sender = sender;
		}

		public void Add(TKey key, TTarget value)
		{
			lock (_syncRoot)
			{
				HashSet<TTarget> value2;
				if (_subscriptions.TryGetValue(key, out value2))
				{
					value2.Add(value);
				}
				else
				{
					value2 = new HashSet<TTarget>
					{
						value
					};
					_subscriptions.Add(key, value2);
				}
			}
		}

		public void Remove(TKey key, TTarget value)
		{
			lock (_syncRoot)
			{
				HashSet<TTarget> value2;
				if (_subscriptions.TryGetValue(key, out value2))
				{
					value2.Remove(value);
					if (value2.Count == 0)
					{
						_subscriptions.Remove(key);
					}
				}
			}
		}

		public void Clear(TKey key)
		{
			lock (_syncRoot)
			{
				HashSet<TTarget> value;
				if (_subscriptions.TryGetValue(key, out value))
				{
					value.Clear();
				}
			}
		}

		public void Send(TKey key, TMessage message)
		{
			List<TTarget> obj;
			lock (_syncRoot)
			{
				HashSet<TTarget> value;
				if (!_subscriptions.TryGetValue(key, out value))
				{
					return;
				}
				obj = Pool.GetList<TTarget>();
				foreach (TTarget item in value)
				{
					obj.Add(item);
				}
			}
			_sender.BroadcastTo(obj, message);
			Pool.FreeList(ref obj);
		}
	}
}
