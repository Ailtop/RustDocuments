using System.Collections.Generic;

namespace Rust.Interpolation;

public interface IGenericLerpTarget<T> : ILerpInfo where T : ISnapshot<T>, new()
{
	void SetFrom(T snapshot);

	T GetCurrentState();

	void DebugInterpolationState(Interpolator<T>.Segment segment, List<T> entries);
}
