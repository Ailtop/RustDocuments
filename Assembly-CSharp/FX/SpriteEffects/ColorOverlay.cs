using UnityEngine;

namespace FX.SpriteEffects
{
	public class ColorOverlay : SpriteEffect
	{
		private readonly MaterialPropertyBlock _propertyBlock;

		private readonly Color _color;

		private readonly float _duration;

		private float _time;

		public ColorOverlay(int priority, Color color, float duration)
			: base(priority)
		{
			_propertyBlock = new MaterialPropertyBlock();
			_color = color;
			if (duration == 0f)
			{
				duration = float.PositiveInfinity;
			}
			_duration = duration;
			_time = 0f;
		}

		internal override void Apply(Renderer renderer)
		{
			renderer.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetColor(SpriteEffect._overlayColor, _color);
			renderer.SetPropertyBlock(_propertyBlock);
		}

		internal override bool Update(float deltaTime)
		{
			_time += deltaTime;
			return _time < _duration;
		}

		internal override void Expire()
		{
			_time = _duration;
		}
	}
}
