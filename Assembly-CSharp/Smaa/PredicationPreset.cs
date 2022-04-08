using System;
using UnityEngine;

namespace Smaa;

[Serializable]
public class PredicationPreset
{
	[Min(0.0001f)]
	public float Threshold = 0.01f;

	[Range(1f, 5f)]
	public float Scale = 2f;

	[Range(0f, 1f)]
	public float Strength = 0.4f;
}
