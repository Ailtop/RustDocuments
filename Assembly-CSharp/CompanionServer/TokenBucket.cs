using System;
using Network;

namespace CompanionServer;

public class TokenBucket
{
	private double _lastUpdate;

	private double _tokens;

	public ITokenBucketSettings Settings;

	public bool IsFull
	{
		get
		{
			Update();
			return _tokens >= Settings.MaxTokens;
		}
	}

	public bool IsNaughty
	{
		get
		{
			Update();
			return _tokens <= -10.0;
		}
	}

	public void Reset()
	{
		_lastUpdate = TimeEx.realtimeSinceStartup;
		_tokens = Settings?.MaxTokens ?? 0.0;
	}

	public bool TryTake(double requestedTokens)
	{
		Update();
		if (requestedTokens > _tokens)
		{
			_tokens -= 1.0;
			return false;
		}
		_tokens -= requestedTokens;
		return true;
	}

	private void Update()
	{
		double realtimeSinceStartup = TimeEx.realtimeSinceStartup;
		double num = realtimeSinceStartup - _lastUpdate;
		_lastUpdate = realtimeSinceStartup;
		double num2 = num * Settings.TokensPerSec;
		_tokens = Math.Min(_tokens + num2, Settings.MaxTokens);
	}
}
