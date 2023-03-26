using System;
using System.Collections.Generic;
using System.Diagnostics;
using Facepunch;
using Network;

namespace CompanionServer;

public class SubscriberList<TKey, TTarget, TMessage> where TKey : IEquatable<TKey> where TTarget : class
{
	private readonly object _syncRoot;

	private readonly Dictionary<TKey, Dictionary<TTarget, double>> _subscriptions;

	private readonly IBroadcastSender<TTarget, TMessage> _sender;

	private readonly double? _timeoutSeconds;

	private readonly Stopwatch _lastCleanup;

	public SubscriberList(IBroadcastSender<TTarget, TMessage> sender, double? timeoutSeconds = null)
	{
		_syncRoot = new object();
		_subscriptions = new Dictionary<TKey, Dictionary<TTarget, double>>();
		_sender = sender;
		_timeoutSeconds = timeoutSeconds;
		_lastCleanup = Stopwatch.StartNew();
	}

	public void Add(TKey key, TTarget value)
	{
		lock (_syncRoot)
		{
			if (!_subscriptions.TryGetValue(key, out var value2))
			{
				value2 = new Dictionary<TTarget, double>();
				_subscriptions.Add(key, value2);
			}
			value2[value] = TimeEx.realtimeSinceStartup;
		}
		CleanupExpired();
	}

	public void Remove(TKey key, TTarget value)
	{
		lock (_syncRoot)
		{
			if (_subscriptions.TryGetValue(key, out var value2))
			{
				value2.Remove(value);
				if (value2.Count == 0)
				{
					_subscriptions.Remove(key);
				}
			}
		}
		CleanupExpired();
	}

	public void Clear(TKey key)
	{
		lock (_syncRoot)
		{
			if (_subscriptions.TryGetValue(key, out var value))
			{
				value.Clear();
			}
		}
	}

	public void Send(TKey key, TMessage message)
	{
		double realtimeSinceStartup = TimeEx.realtimeSinceStartup;
		List<TTarget> obj;
		lock (_syncRoot)
		{
			if (!_subscriptions.TryGetValue(key, out var value))
			{
				return;
			}
			obj = Pool.GetList<TTarget>();
			foreach (KeyValuePair<TTarget, double> item in value)
			{
				if (!_timeoutSeconds.HasValue || realtimeSinceStartup - item.Value < _timeoutSeconds.Value)
				{
					obj.Add(item.Key);
				}
			}
		}
		_sender.BroadcastTo(obj, message);
		Pool.FreeList(ref obj);
	}

	public bool HasAnySubscribers(TKey key)
	{
		double realtimeSinceStartup = TimeEx.realtimeSinceStartup;
		lock (_syncRoot)
		{
			if (!_subscriptions.TryGetValue(key, out var value))
			{
				return false;
			}
			foreach (KeyValuePair<TTarget, double> item in value)
			{
				if (!_timeoutSeconds.HasValue || realtimeSinceStartup - item.Value < _timeoutSeconds.Value)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasSubscriber(TKey key, TTarget target)
	{
		double realtimeSinceStartup = TimeEx.realtimeSinceStartup;
		lock (_syncRoot)
		{
			if (!_subscriptions.TryGetValue(key, out var value) || !value.TryGetValue(target, out var value2))
			{
				return false;
			}
			if (!_timeoutSeconds.HasValue || realtimeSinceStartup - value2 < _timeoutSeconds.Value)
			{
				return true;
			}
		}
		return false;
	}

	private void CleanupExpired()
	{
		if (!_timeoutSeconds.HasValue || _lastCleanup.Elapsed.TotalMinutes < 2.0)
		{
			return;
		}
		_lastCleanup.Restart();
		double realtimeSinceStartup = TimeEx.realtimeSinceStartup;
		List<(TKey, TTarget)> obj = Pool.GetList<(TKey, TTarget)>();
		lock (_syncRoot)
		{
			foreach (KeyValuePair<TKey, Dictionary<TTarget, double>> subscription in _subscriptions)
			{
				foreach (KeyValuePair<TTarget, double> item in subscription.Value)
				{
					if (realtimeSinceStartup - item.Value >= _timeoutSeconds.Value)
					{
						obj.Add((subscription.Key, item.Key));
					}
				}
			}
			foreach (var (key, value) in obj)
			{
				Remove(key, value);
			}
		}
		Pool.FreeList(ref obj);
	}
}
