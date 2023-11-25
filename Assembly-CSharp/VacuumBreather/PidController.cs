using System;
using UnityEngine;

namespace VacuumBreather;

public class PidController
{
	private const float MaxOutput = 1000f;

	private float _integralMax;

	private float _integral;

	private float _kp;

	private float _ki;

	private float _kd;

	public float Kp
	{
		get
		{
			return _kp;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("value", "Kp must be a non-negative number.");
			}
			_kp = value;
		}
	}

	public float Ki
	{
		get
		{
			return _ki;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("value", "Ki must be a non-negative number.");
			}
			_ki = value;
			_integralMax = 1000f / Ki;
			_integral = Mathf.Clamp(_integral, 0f - _integralMax, _integralMax);
		}
	}

	public float Kd
	{
		get
		{
			return _kd;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("value", "Kd must be a non-negative number.");
			}
			_kd = value;
		}
	}

	public PidController(float kp, float ki, float kd)
	{
		if (kp < 0f)
		{
			throw new ArgumentOutOfRangeException("kp", "kp must be a non-negative number.");
		}
		if (ki < 0f)
		{
			throw new ArgumentOutOfRangeException("ki", "ki must be a non-negative number.");
		}
		if (kd < 0f)
		{
			throw new ArgumentOutOfRangeException("kd", "kd must be a non-negative number.");
		}
		Kp = kp;
		Ki = ki;
		Kd = kd;
		_integralMax = 1000f / Ki;
	}

	public float ComputeOutput(float error, float delta, float deltaTime)
	{
		_integral += error * deltaTime;
		_integral = Mathf.Clamp(_integral, 0f - _integralMax, _integralMax);
		float num = delta / deltaTime;
		return Mathf.Clamp(Kp * error + Ki * _integral + Kd * num, -1000f, 1000f);
	}
}
