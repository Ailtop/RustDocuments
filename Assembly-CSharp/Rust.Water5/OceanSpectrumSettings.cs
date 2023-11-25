using UnityEngine;

namespace Rust.Water5;

[CreateAssetMenu(fileName = "New Spectrum Settings", menuName = "Water5/Spectrum Settings")]
public class OceanSpectrumSettings : ScriptableObject
{
	public OceanSettings oceanSettings;

	[Header("Deep Wave Settings")]
	public float g;

	public float beaufort;

	public float depth;

	public SpectrumSettings local;

	public SpectrumSettings swell;

	[Header("Material Settings")]
	public Color color;

	public Color specColor;

	public float smoothness;

	public Color waterColor;

	public Color waterExtinction;

	public float scatteringCoefficient;

	public Color subSurfaceColor;

	public float subSurfaceFalloff;

	public float subSurfaceBase;

	public float subSurfaceSun;

	public float subSurfaceAmount;

	public float foamAmount;

	public float foamScale;

	public Color foamColor;

	public Color baseFoamColor;

	[Button("Update Spectrum")]
	public void UpdateSpectrum()
	{
		WaterSystem.Instance?.Refresh();
	}
}
