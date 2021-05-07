using System.Collections;
using Scenes;
using UnityEngine;

namespace SkulStories
{
	public class ChangeScene : Sequence
	{
		[SerializeField]
		private Sprite _sequence;

		[SerializeField]
		private float _speed;

		[SerializeField]
		private float _delay;

		[SerializeField]
		private Vector2 _pivot = new Vector2(0f, 1f);

		[SerializeField]
		private bool _overlay;

		[SerializeField]
		private bool _top;

		private NarrationScene _narrationScene;

		private void Start()
		{
			_narrationScene = Scene<GameBase>.instance.uiManager.narrationScene;
		}

		public override IEnumerator CRun()
		{
			if (!_narration.skipped)
			{
				yield return CWaitForTime(_delay);
				if (_top)
				{
					_narrationScene.scene.rectTransform.SetAsLastSibling();
				}
				_narrationScene.SetPivot(_pivot);
				_narrationScene.Change(_sequence, _overlay);
				yield return _narrationScene.CFadeIn(_speed);
			}
		}

		private IEnumerator CWaitForTime(float length)
		{
			float elapsed = 0f;
			while (length > elapsed)
			{
				elapsed += Chronometer.global.deltaTime;
				yield return null;
				if (_narration.skipped || !_narration.sceneVisible)
				{
					break;
				}
			}
			_delay = 0f;
		}
	}
}
