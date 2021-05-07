using UnityEngine;

namespace Characters.Operations
{
	public class SpawnRawLineText : CharacterOperation
	{
		[SerializeField]
		private string _text;

		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private float _duration = 1.5f;

		[SerializeField]
		private float _coolTime = 8f;

		[SerializeField]
		private bool _force;

		private LineText _lineText;

		public override void Run(Character owner)
		{
			if (_lineText == null)
			{
				_lineText = owner.GetComponentInChildren<LineText>();
				if (_lineText == null)
				{
					return;
				}
			}
			if ((_force || _lineText.finished) && _text.Length > 0)
			{
				Vector2 vector = ((_spawnPosition == null) ? GetDefaultPosition(owner.collider.bounds) : ((Vector2)_spawnPosition.position));
				_lineText.transform.position = vector;
				_lineText.Display(_text, _duration);
			}
		}

		private Vector2 GetDefaultPosition(Bounds bounds)
		{
			return new Vector2(bounds.center.x, bounds.max.y + 0.5f);
		}
	}
}
