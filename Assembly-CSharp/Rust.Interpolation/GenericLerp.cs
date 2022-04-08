using System;
using UnityEngine;

namespace Rust.Interpolation
{
	public class GenericLerp<T> : IDisposable where T : Interpolator<T>.ISnapshot, new()
	{
		private Interpolator<T> interpolator;

		private IGenericLerpTarget<T> target;

		private static float TimeOffset;

		private float timeOffset0 = float.MaxValue;

		private float timeOffset1 = float.MaxValue;

		private float timeOffset2 = float.MaxValue;

		private float timeOffset3 = float.MaxValue;

		private int timeOffsetCount;

		private float extrapolatedTime;

		private int TimeOffsetInterval => PositionLerp.TimeOffsetInterval;

		private float LerpTime => PositionLerp.LerpTime;

		public GenericLerp(IGenericLerpTarget<T> target, int listCount)
		{
			this.target = target;
			interpolator = new Interpolator<T>(listCount);
		}

		public void Tick()
		{
			if (target != null)
			{
				float extrapolationTime = target.GetExtrapolationTime();
				float interpolationDelay = target.GetInterpolationDelay();
				float interpolationSmoothing = target.GetInterpolationSmoothing();
				Interpolator<T>.Segment segment = interpolator.Query(LerpTime, interpolationDelay, extrapolationTime, interpolationSmoothing);
				if (segment.next.Time >= interpolator.last.Time)
				{
					extrapolatedTime = Mathf.Min(extrapolatedTime + Time.deltaTime, extrapolationTime);
				}
				else
				{
					extrapolatedTime = Mathf.Max(extrapolatedTime - Time.deltaTime, 0f);
				}
				if (extrapolatedTime > 0f && extrapolationTime > 0f && interpolationSmoothing > 0f)
				{
					float delta = Time.deltaTime / (extrapolatedTime / extrapolationTime * interpolationSmoothing);
					segment.tick.Lerp(target.GetCurrentState(), segment.tick, delta);
				}
				target.SetFrom(segment.tick);
			}
		}

		public void Snapshot(T snapshot)
		{
			float interpolationDelay = target.GetInterpolationDelay();
			float interpolationSmoothing = target.GetInterpolationSmoothing();
			float num = interpolationDelay + interpolationSmoothing + 1f;
			float lerpTime = LerpTime;
			timeOffset0 = Mathf.Min(timeOffset0, lerpTime - snapshot.Time);
			timeOffsetCount++;
			if (timeOffsetCount >= TimeOffsetInterval / 4)
			{
				timeOffset3 = timeOffset2;
				timeOffset2 = timeOffset1;
				timeOffset1 = timeOffset0;
				timeOffset0 = float.MaxValue;
				timeOffsetCount = 0;
			}
			TimeOffset = Mathx.Min(timeOffset0, timeOffset1, timeOffset2, timeOffset3);
			lerpTime = (snapshot.Time += TimeOffset);
			interpolator.Add(snapshot);
			interpolator.Cull(lerpTime - num);
		}

		public void SnapTo(T snapshot)
		{
			interpolator.Clear();
			Snapshot(snapshot);
			target.SetFrom(snapshot);
		}

		public void SnapToNow(T snapshot)
		{
			snapshot.Time = LerpTime;
			interpolator.last = snapshot;
			Wipe();
		}

		public void SnapToEnd()
		{
			float interpolationDelay = target.GetInterpolationDelay();
			Interpolator<T>.Segment segment = interpolator.Query(LerpTime, interpolationDelay, 0f, 0f);
			target.SetFrom(segment.tick);
			Wipe();
		}

		public void Dispose()
		{
			target = null;
			interpolator.Clear();
			timeOffset0 = float.MaxValue;
			timeOffset1 = float.MaxValue;
			timeOffset2 = float.MaxValue;
			timeOffset3 = float.MaxValue;
			extrapolatedTime = 0f;
			timeOffsetCount = 0;
		}

		private void Wipe()
		{
			interpolator.Clear();
			timeOffsetCount = 0;
			timeOffset0 = float.MaxValue;
			timeOffset1 = float.MaxValue;
			timeOffset2 = float.MaxValue;
			timeOffset3 = float.MaxValue;
		}
	}
}
