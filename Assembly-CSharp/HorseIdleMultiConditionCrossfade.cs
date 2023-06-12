using System;
using UnityEngine;

public class HorseIdleMultiConditionCrossfade : StateMachineBehaviour
{
	[Serializable]
	public struct Condition
	{
		public enum CondtionOperator
		{
			GreaterThan = 0,
			LessThan = 1
		}

		public int FloatParameter;

		public CondtionOperator Operator;

		public float Value;
	}

	public string TargetState = "breathe";

	public float NormalizedTransitionDuration = 0.1f;
}
