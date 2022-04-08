using System;

namespace UnityEngine.Rendering.PostProcessing
{
	public enum ScreenSpaceReflectionResolution
	{
		Downsampled = 0,
		FullSize = 1,
		Supersampled = 2
	}
	[Serializable]
	public sealed class ScreenSpaceReflectionResolutionParameter : ParameterOverride<ScreenSpaceReflectionResolution>
	{
	}
}
