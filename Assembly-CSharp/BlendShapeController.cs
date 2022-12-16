using System;
using UnityEngine;

public class BlendShapeController : MonoBehaviour
{
	public enum BlendMode
	{
		Idle = 0,
		Happy = 1,
		Angry = 2
	}

	[Serializable]
	public struct BlendState
	{
		[Range(0f, 100f)]
		public float[] States;

		public BlendMode Mode;
	}

	public SkinnedMeshRenderer TargetRenderer;

	public BlendState[] States;

	public float LerpSpeed = 0.25f;

	public BlendMode CurrentMode;
}
