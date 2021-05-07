using System;
using UnityEngine;

public abstract class Wave : MonoBehaviour
{
	public enum State
	{
		Waiting,
		Spawned,
		Cleared,
		Stopped
	}

	protected Action _onClear;

	protected Action _onSpawn;

	public State state { get; protected set; }

	public event Action onClear
	{
		add
		{
			_onClear = (Action)Delegate.Combine(_onClear, value);
		}
		remove
		{
			_onClear = (Action)Delegate.Remove(_onClear, value);
		}
	}

	public event Action onSpawn
	{
		add
		{
			_onSpawn = (Action)Delegate.Combine(_onSpawn, value);
		}
		remove
		{
			_onSpawn = (Action)Delegate.Remove(_onSpawn, value);
		}
	}

	public abstract void Initialize();

	public void Stop()
	{
		state = State.Stopped;
		StopAllCoroutines();
	}
}
