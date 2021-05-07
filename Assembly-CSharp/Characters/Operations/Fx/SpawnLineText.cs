using UnityEngine;

namespace Characters.Operations.Fx
{
	public class SpawnLineText : CharacterOperation
	{
		[SerializeField]
		private string _textKey;

		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private float _duration = 2f;

		[SerializeField]
		private float _coolTime = 8f;

		[SerializeField]
		private bool _force;

		private LineText _lineText;

		private Character _owner;

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
			if (_force || _lineText.finished)
			{
				string[] localizedStringArray = Lingua.GetLocalizedStringArray(_textKey);
				if (localizedStringArray.Length != 0)
				{
					string text = localizedStringArray.Random();
					Vector2 vector = ((_spawnPosition == null) ? GetDefaultPosition(owner.collider.bounds) : ((Vector2)_spawnPosition.position));
					_lineText.transform.position = vector;
					_lineText.Display(text, _duration);
				}
			}
		}

		private Vector2 GetDefaultPosition(Bounds bounds)
		{
			return new Vector2(bounds.center.x, bounds.max.y + 0.5f);
		}
	}
}
