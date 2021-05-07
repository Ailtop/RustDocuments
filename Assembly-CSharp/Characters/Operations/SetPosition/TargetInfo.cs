using System;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	[Serializable]
	public class TargetInfo
	{
		[SerializeField]
		private CustomFloat _customOffsetX;

		[SerializeField]
		private CustomFloat _customOffsetY;

		[SerializeField]
		[Policy.Subcomponent(true)]
		private Policy _policy;

		public Vector2 GetPosition()
		{
			Vector2 position = _policy.GetPosition();
			return new Vector2(position.x + _customOffsetX.value, position.y + _customOffsetY.value);
		}
	}
}
