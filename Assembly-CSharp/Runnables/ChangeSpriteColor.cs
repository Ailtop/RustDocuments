using System.Collections;
using UnityEngine;

namespace Runnables
{
	public class ChangeSpriteColor : CRunnable
	{
		[SerializeField]
		private SpriteRenderer _sprite;

		[SerializeField]
		private Color _color;

		[SerializeField]
		private Curve _curve;

		public override IEnumerator CRun()
		{
			Color startColor = _sprite.color;
			float elapsed = 0f;
			while (elapsed < _curve.duration)
			{
				elapsed += Chronometer.global.deltaTime;
				_sprite.color = Color.Lerp(startColor, _color, _curve.Evaluate(elapsed));
				yield return null;
			}
			_sprite.color = _color;
		}
	}
}
