using UnityEngine;

namespace Rust.Interpolation;

public struct FloatSnapshot : Interpolator<FloatSnapshot>.ISnapshot
{
	public float value;

	public float Time { get; set; }

	public FloatSnapshot(float time, float value)
	{
		Time = time;
		this.value = value;
	}

	public void MatchValuesTo(FloatSnapshot entry)
	{
		value = entry.value;
	}

	public void Lerp(FloatSnapshot prev, FloatSnapshot next, float delta)
	{
		value = Mathf.Lerp(prev.value, next.value, delta);
	}
}
