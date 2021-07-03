using System;
using UnityEngine;

public class AudioVisualisationEntityLight : AudioVisualisationEntity
{
	[Serializable]
	public struct LightColourSet
	{
		[ColorUsage(true, true)]
		public Color LightColor;

		[ColorUsage(true, true)]
		public Color SecondaryLightColour;

		[ColorUsage(true, true)]
		public Color EmissionColour;
	}

	public Light TargetLight;

	public Light SecondaryLight;

	public MeshRenderer[] TargetRenderer;

	public LightColourSet RedColour;

	public LightColourSet GreenColour;

	public LightColourSet BlueColour;

	public LightColourSet YellowColour;

	public LightColourSet PinkColour;

	public Vector2[] LightIntensities = new Vector2[3]
	{
		new Vector2(0.05f, 15f),
		new Vector2(0.05f, 20f),
		new Vector2(0.05f, 25f)
	};
}
