using System.Collections;
using System.Collections.Generic;
using Runnables;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CutScenes.Shots.Events.Customs
{
	public class ChangeTilemapColor : CRunnable
	{
		[SerializeField]
		private Tilemap _tilemap;

		[SerializeField]
		private Color _color;

		[SerializeField]
		private Curve _curve;

		private static Dictionary<Tilemap, ChangeTilemapColor> _changes = new Dictionary<Tilemap, ChangeTilemapColor>();

		private bool _interrupt;

		private static void StopSameChanges(Tilemap tilemap)
		{
			ChangeTilemapColor value;
			if (_changes.TryGetValue(tilemap, out value))
			{
				_changes.Remove(tilemap);
				value.Stop();
			}
		}

		private static void StartChange(Tilemap tilemap, ChangeTilemapColor changeTilemapColor)
		{
			_changes.Add(tilemap, changeTilemapColor);
		}

		public void Stop()
		{
			_interrupt = true;
		}

		public override IEnumerator CRun()
		{
			StopSameChanges(_tilemap);
			StartChange(_tilemap, this);
			Color start = _tilemap.color;
			float elapsed = 0f;
			_interrupt = false;
			while (elapsed < _curve.duration && !_interrupt)
			{
				elapsed += Chronometer.global.deltaTime;
				_tilemap.color = Color.Lerp(start, _color, _curve.Evaluate(elapsed));
				yield return null;
			}
			_tilemap.color = _color;
		}
	}
}
