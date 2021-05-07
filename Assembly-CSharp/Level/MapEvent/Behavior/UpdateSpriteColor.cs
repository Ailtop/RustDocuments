using System.Collections;
using UnityEngine;

namespace Level.MapEvent.Behavior
{
	public class UpdateSpriteColor : Behavior
	{
		[SerializeField]
		private SpriteRenderer _sprite;

		[SerializeField]
		private Color _color;

		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private float _duration;

		public override void Run()
		{
			if (_duration <= 0f)
			{
				_sprite.color = _color;
			}
			else
			{
				StartCoroutine(CRun(_curve, _color, _duration));
			}
		}

		private IEnumerator CRun(Curve curve, Color color, float duration)
		{
			Color from = _sprite.color;
			Color to = _color;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Chronometer.global.deltaTime;
				_sprite.color = from + (to - from) * curve.Evaluate(elapsed / duration);
				yield return null;
			}
			_sprite.color = _color;
		}
	}
}
