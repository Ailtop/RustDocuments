using System;
using UnityEngine;

namespace Characters
{
	[Serializable]
	public class Force
	{
		public enum Method
		{
			LookingDirection,
			Constant
		}

		[SerializeField]
		private CustomFloat _angle = new CustomFloat(0f);

		[SerializeField]
		private CustomFloat _power = new CustomFloat(0f);

		[SerializeField]
		private Method _method;

		public Vector2 Evaluate(Character character, float extraPower = 0f)
		{
			float value = _angle.value;
			Vector2 result = new Vector2(Mathf.Cos(value * ((float)Math.PI / 180f)), Mathf.Sin(value * ((float)Math.PI / 180f))) * (_power.value + extraPower);
			if (_method == Method.LookingDirection && character.lookingDirection != 0)
			{
				result.x *= -1f;
			}
			return result;
		}
	}
}
