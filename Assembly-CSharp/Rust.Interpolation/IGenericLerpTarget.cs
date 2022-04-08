using System.Collections.Generic;

namespace Rust.Interpolation;

public interface IGenericLerpTarget<T> : ILerpInfo where T : Interpolator<T>.ISnapshot, new()
{
	void SetFrom(T snapshot);

	T GetCurrentState();

	void DebugInterpolationState(Interpolator<T>.Segment segment, List<T> entries);
}
