using UnityEngine;

namespace Characters.Operations.Customs
{
	public class ResizeBoundsX : Operation
	{
		[SerializeField]
		private BoxCollider2D _collider;

		[SerializeField]
		private Transform _start;

		[SerializeField]
		private Transform _end;

		[SerializeField]
		private CustomFloat _extraSizeX = new CustomFloat(0f);

		[SerializeField]
		private CustomFloat _extraSizeY = new CustomFloat(0f);

		public override void Run()
		{
			float num = Mathf.Abs(_end.position.x - _start.position.x);
			float value = _extraSizeX.value;
			float value2 = _extraSizeY.value;
			_collider.size = new Vector2(num + value, _collider.size.y + value2);
			_collider.offset = new Vector2((0f - num) / 2f, _collider.offset.y);
		}
	}
}
