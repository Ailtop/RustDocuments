using System;
using UnityEngine;

namespace Rust.Water5;

[Serializable]
public struct SpectrumSettings
{
	[Range(0f, 1f)]
	public float scale;

	public float windSpeed;

	public float fetch;

	[Range(0f, 1f)]
	public float spreadBlend;

	[Range(0f, 1f)]
	public float swell;

	public float peakEnhancement;

	public float shortWavesFade;
}
