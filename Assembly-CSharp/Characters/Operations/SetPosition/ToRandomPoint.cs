using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToRandomPoint : Policy
	{
		[SerializeField]
		private Transform[] _transforms;

		public override Vector2 GetPosition()
		{
			return _transforms.Random().position;
		}
	}
}
