using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Level.MapEvent.Behavior
{
	public class UpdateTilemapColor : Behavior
	{
		[SerializeField]
		private Tilemap _tilemap;

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
				_tilemap.color = _color;
			}
			else
			{
				StartCoroutine(CRun(_curve, _color, _duration));
			}
		}

		private IEnumerator CRun(Curve curve, Color color, float duration)
		{
			Color from = _tilemap.color;
			Color to = _color;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Chronometer.global.deltaTime;
				_tilemap.color = from + (to - from) * curve.Evaluate(elapsed / duration);
				yield return null;
			}
			_tilemap.color = _color;
		}
	}
}
