using System;
using UnityEngine;

public class SoundModulation : MonoBehaviour, IClientComponent
{
	public enum Parameter
	{
		Gain = 0,
		Pitch = 1,
		Spread = 2,
		MaxDistance = 3
	}

	[Serializable]
	public class Modulator
	{
		public Parameter param;

		public float value = 1f;
	}

	private const int parameterCount = 4;
}
