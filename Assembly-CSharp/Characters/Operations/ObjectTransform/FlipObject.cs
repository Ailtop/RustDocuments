using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class FlipObject : Operation
	{
		[SerializeField]
		private Transform _object;

		[SerializeField]
		private bool _flipX;

		[SerializeField]
		private bool _flipY;

		public override void Run()
		{
			float x = (_flipX ? (0f - _object.localScale.x) : _object.localScale.x);
			float y = (_flipY ? (0f - _object.localScale.y) : _object.localScale.y);
			_object.localScale = new Vector2(x, y);
		}
	}
}
