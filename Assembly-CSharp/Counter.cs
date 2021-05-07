using System;
using UnityEngine;

[Serializable]
public class Counter
{
	public int maxCount;

	public float uniqueTime;

	public float refreshTime;

	private float _lastCount;

	private ChronometerTime _time;

	public int count { get; private set; }

	private float time
	{
		get
		{
			if (_time == null)
			{
				return Time.time;
			}
			return _time.time;
		}
	}

	public event Action onArrival;

	public void Initialize(ChronometerTime time)
	{
		_time = time;
	}

	public void Count()
	{
		float num = time - _lastCount;
		if (!(num < uniqueTime))
		{
			if (num > refreshTime)
			{
				count = 0;
			}
			_lastCount = time;
			count++;
			if (count >= maxCount)
			{
				count = 0;
				this.onArrival?.Invoke();
			}
		}
	}
}
