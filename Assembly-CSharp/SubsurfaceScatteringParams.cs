using System;

[Serializable]
public struct SubsurfaceScatteringParams
{
	public enum Quality
	{
		Low = 0,
		Medium = 1,
		High = 2
	}

	public bool enabled;

	public Quality quality;

	public bool halfResolution;

	public float radiusScale;

	public static SubsurfaceScatteringParams Default = new SubsurfaceScatteringParams
	{
		enabled = true,
		quality = Quality.Medium,
		halfResolution = true,
		radiusScale = 1f
	};
}
