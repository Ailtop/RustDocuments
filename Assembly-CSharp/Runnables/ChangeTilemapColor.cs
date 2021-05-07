using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Runnables
{
	public class ChangeTilemapColor : CRunnable
	{
		[SerializeField]
		private Tilemap _tilemap;

		[SerializeField]
		private Color _color;

		[SerializeField]
		private Curve _curve;

		public override IEnumerator CRun()
		{
			Color startColor = _tilemap.color;
			float elapsed = 0f;
			while (elapsed < _curve.duration)
			{
				elapsed += Chronometer.global.deltaTime;
				_tilemap.color = Color.Lerp(startColor, _color, _curve.Evaluate(elapsed));
				yield return null;
			}
			_tilemap.color = _color;
		}
	}
}
