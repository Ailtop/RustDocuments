using System;
using UnityEngine;

namespace FX.SpriteEffects
{
	public sealed class GenericSpriteEffect : SpriteEffect
	{
		[Serializable]
		public class ColorOverlay
		{
			[SerializeField]
			private bool _enabled;

			[SerializeField]
			private Color _startColor = Color.white;

			[SerializeField]
			private Color _endColor = new Color(1f, 1f, 1f, 0f);

			[SerializeField]
			private Curve _curve;

			public bool enabled => _enabled;

			public Color startColor => _startColor;

			public Color endColor => _endColor;

			public Curve curve => _curve;

			public float duration => _curve.duration;

			internal void Apply(Renderer renderer, MaterialPropertyBlock propertyBlock, float time)
			{
				if (_enabled)
				{
					propertyBlock.SetColor(SpriteEffect._overlayColor, Color.LerpUnclamped(_startColor, _endColor, _curve.Evaluate(time)));
				}
			}
		}

		[Serializable]
		public class ColorBlend
		{
			[SerializeField]
			private bool _enabled;

			[SerializeField]
			private Color _startColor = Color.white;

			[SerializeField]
			private Color _endColor = new Color(1f, 1f, 1f, 0f);

			[SerializeField]
			private Curve _curve;

			public bool enabled => _enabled;

			public Color startColor => _startColor;

			public Color endColor => _endColor;

			public Curve curve => _curve;

			public float duration => _curve.duration;

			internal void Apply(Renderer renderer, MaterialPropertyBlock propertyBlock, float time)
			{
				if (_enabled)
				{
					propertyBlock.SetColor(SpriteEffect._baseColor, Color.LerpUnclamped(_startColor, _endColor, _curve.Evaluate(time)));
				}
			}
		}

		[Serializable]
		public class Outline
		{
			[SerializeField]
			private bool _enabled;

			[SerializeField]
			private Color _color = Color.white;

			[SerializeField]
			[Range(1f, 10f)]
			private float _brightness = 2f;

			[SerializeField]
			[Range(1f, 6f)]
			private int _width = 1;

			[SerializeField]
			[FrameTime]
			private float _duration;

			public bool enabled => _enabled;

			public Color color => _color;

			public float brightness => _brightness;

			public int width => _width;

			public float duration => _duration;

			internal void Apply(Renderer renderer, MaterialPropertyBlock propertyBlock, float time)
			{
				if (_enabled)
				{
					propertyBlock.SetFloat(SpriteEffect._outlineEnabled, 1f);
					Color value = _color * _brightness;
					value.a = _color.a;
					propertyBlock.SetColor(SpriteEffect._outlineColor, value);
					propertyBlock.SetFloat(SpriteEffect._outlineSize, _width);
					propertyBlock.SetFloat(SpriteEffect._alphaThreshold, 0.01f);
				}
			}
		}

		private readonly MaterialPropertyBlock _propertyBlock;

		private readonly ColorOverlay _colorOverlay;

		private readonly ColorBlend _colorBlend;

		private readonly Outline _outline;

		private readonly float _duration;

		private readonly float _speed;

		private float _time;

		public GenericSpriteEffect(int priority, float duration, float speed, ColorOverlay colorOverlay, ColorBlend colorBlend, Outline outline)
			: base(priority)
		{
			_propertyBlock = new MaterialPropertyBlock();
			_duration = duration;
			_speed = speed;
			_colorOverlay = colorOverlay;
			_colorBlend = colorBlend;
			_outline = outline;
		}

		internal override void Apply(Renderer renderer)
		{
			renderer.GetPropertyBlock(_propertyBlock);
			_colorOverlay.Apply(renderer, _propertyBlock, _time);
			_colorBlend.Apply(renderer, _propertyBlock, _time);
			_outline.Apply(renderer, _propertyBlock, _time);
			renderer.SetPropertyBlock(_propertyBlock);
		}

		internal override bool Update(float deltaTime)
		{
			_time += deltaTime * _speed;
			return _time < _duration;
		}

		internal override void Expire()
		{
			_time = _duration;
		}
	}
}
