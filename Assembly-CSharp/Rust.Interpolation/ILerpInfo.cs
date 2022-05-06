namespace Rust.Interpolation;

public interface ILerpInfo
{
	float GetExtrapolationTime();

	float GetInterpolationDelay();

	float GetInterpolationSmoothing();
}
