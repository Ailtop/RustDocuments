using System;
using UnityEngine;

namespace FX
{
	[Serializable]
	public class PositionNoise
	{
		public enum Method
		{
			None,
			InsideCircle,
			Horizontal,
			Vertical
		}

		[SerializeField]
		private Method _method;

		[SerializeField]
		private float _value;

		public Vector3 Evaluate()
		{
			switch (_method)
			{
			case Method.InsideCircle:
				return UnityEngine.Random.insideUnitCircle * _value;
			case Method.Horizontal:
				return new Vector3(UnityEngine.Random.Range(0f - _value, _value), 0f);
			case Method.Vertical:
				return new Vector3(0f, UnityEngine.Random.Range(0f - _value, _value));
			default:
				return Vector3.zero;
			}
		}

		public Vector2 EvaluateAsVector2()
		{
			switch (_method)
			{
			case Method.InsideCircle:
				return UnityEngine.Random.insideUnitCircle * _value;
			case Method.Horizontal:
				return new Vector2(UnityEngine.Random.Range(0f - _value, _value), 0f);
			case Method.Vertical:
				return new Vector2(0f, UnityEngine.Random.Range(0f - _value, _value));
			default:
				return Vector2.zero;
			}
		}
	}
}
